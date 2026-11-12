"""
train_voice_commands.py
========================

Trains a small CNN keyword-spotting model that recognizes the spoken
commands "up / down / left / right" — said in EITHER English or Arabic —
and exports it to ONNX so it can run inside Unity via the Inference Engine
(formerly Sentis) package, fully offline, on any platform.

Each class folder should contain recordings of BOTH languages mixed
together, so the model learns "this means move up" regardless of which
language was spoken:

    data/
      up/        -> recordings of "up" AND "فوق"
      down/      -> recordings of "down" AND "تحت"
      left/      -> recordings of "left" AND "يسار"
      right/     -> recordings of "right" AND "يمين"
      silence/   -> room noise / silence (mic open, nobody talking)
      unknown/   -> any other word/sound, so the model learns to reject it

All .wav files should ideally be mono, 16-bit PCM, 16kHz. If they aren't,
this script will resample them, but recording at 16kHz directly gives the
best quality.

Usage:
    pip install numpy scipy soundfile torch matplotlib --break-system-packages
    python train_voice_commands.py --data_dir ./data --epochs 30

Outputs (in --out_dir, default "./export"):
    voice_commands.onnx   -> the model, import this into Unity
    labels.txt            -> one class name per line, in the exact output
                              order the model uses (index 0, 1, 2, ...)
    accuracy_curve.jpg    -> training/validation accuracy (%) per epoch
    loss_curve.jpg        -> training/validation loss per epoch
"""

import os
import json
import random
import argparse

import numpy as np
import soundfile as sf
import torch
import torch.nn as nn
from torch.utils.data import Dataset, DataLoader
import matplotlib
matplotlib.use("Agg")  # no display available on most training machines/servers
import matplotlib.pyplot as plt

# ──────────────────────────────────────────────────────────────────────
# Audio / feature constants — these MUST match MelSpectrogram.cs exactly.
# If you change any of these, update the C# side too.
# ──────────────────────────────────────────────────────────────────────
SAMPLE_RATE = 16000
CLIP_SAMPLES = 16000      # 1.0 second clips
FRAME_LENGTH = 400        # 25ms
FRAME_STEP = 160          # 10ms
NFFT = 512
N_MELS = 40
FMIN = 20.0
FMAX = 8000.0
NUM_FRAMES = 1 + (CLIP_SAMPLES - FRAME_LENGTH) // FRAME_STEP  # = 98


# ──────────────────────────────────────────────────────────────────────
# Audio loading helpers
# ──────────────────────────────────────────────────────────────────────
def load_wav_mono16k(path):
    samples, sr = sf.read(path, dtype="float32", always_2d=False)
    if samples.ndim > 1:
        samples = samples.mean(axis=1)  # downmix to mono
    if sr != SAMPLE_RATE:
        # simple linear resample - fine for speech, but recording at 16kHz
        # directly will sound cleaner than relying on this.
        duration = len(samples) / sr
        new_len = int(round(duration * SAMPLE_RATE))
        old_idx = np.linspace(0, len(samples) - 1, num=len(samples))
        new_idx = np.linspace(0, len(samples) - 1, num=new_len)
        samples = np.interp(new_idx, old_idx, samples).astype(np.float32)
    return samples


def pad_or_center_trim(samples, length=CLIP_SAMPLES):
    if len(samples) == length:
        return samples
    if len(samples) < length:
        pad_total = length - len(samples)
        pad_left = pad_total // 2
        pad_right = pad_total - pad_left
        return np.pad(samples, (pad_left, pad_right))
    # longer than needed: keep the loudest 1-second window
    energy = samples ** 2
    window_sum = np.convolve(energy, np.ones(length), mode="valid")
    start = int(np.argmax(window_sum))
    return samples[start:start + length]


# ──────────────────────────────────────────────────────────────────────
# Feature extraction — log-mel spectrogram (must match MelSpectrogram.cs)
# ──────────────────────────────────────────────────────────────────────
def hz_to_mel(f):
    return 2595.0 * np.log10(1.0 + f / 700.0)


def mel_to_hz(m):
    return 700.0 * (10.0 ** (m / 2595.0) - 1.0)


def build_mel_filterbank(sample_rate=SAMPLE_RATE, nfft=NFFT, n_mels=N_MELS,
                          fmin=FMIN, fmax=FMAX):
    n_bins = nfft // 2 + 1
    mel_min, mel_max = hz_to_mel(fmin), hz_to_mel(fmax)
    mel_points = np.linspace(mel_min, mel_max, n_mels + 2)
    hz_points = mel_to_hz(mel_points)
    bin_points = np.floor((nfft + 1) * hz_points / sample_rate).astype(int)

    filterbank = np.zeros((n_mels, n_bins), dtype=np.float32)
    for m in range(1, n_mels + 1):
        f_left, f_center, f_right = bin_points[m - 1], bin_points[m], bin_points[m + 1]
        if f_center > f_left:
            for k in range(f_left, f_center):
                if 0 <= k < n_bins:
                    filterbank[m - 1, k] = (k - f_left) / (f_center - f_left)
        if f_right > f_center:
            for k in range(f_center, f_right):
                if 0 <= k < n_bins:
                    filterbank[m - 1, k] = (f_right - k) / (f_right - f_center)
    return filterbank


_MEL_FILTERBANK = build_mel_filterbank()
_HANN_WINDOW = np.array(
    [0.5 - 0.5 * np.cos(2 * np.pi * n / (FRAME_LENGTH - 1)) for n in range(FRAME_LENGTH)],
    dtype=np.float32,
)


def compute_log_mel(samples):
    feats = np.zeros((NUM_FRAMES, N_MELS), dtype=np.float32)
    for i in range(NUM_FRAMES):
        start = i * FRAME_STEP
        frame = samples[start:start + FRAME_LENGTH] * _HANN_WINDOW
        padded = np.zeros(NFFT, dtype=np.float32)
        padded[:FRAME_LENGTH] = frame
        spectrum = np.fft.rfft(padded, n=NFFT)
        power = (spectrum.real ** 2 + spectrum.imag ** 2) / NFFT
        mel_energy = _MEL_FILTERBANK.dot(power)
        feats[i] = np.log(mel_energy + 1e-6)
    return feats  # shape (NUM_FRAMES, N_MELS)


# ──────────────────────────────────────────────────────────────────────
# Augmentation — makes the model robust to different mics/rooms/speakers
# ──────────────────────────────────────────────────────────────────────
def augment(samples):
    out = samples.copy()

    # random time shift
    shift = random.randint(-1600, 1600)  # up to 100ms
    out = np.roll(out, shift)

    # random gain
    out = out * random.uniform(0.7, 1.3)

    # light additive white noise (simulates room hiss / mic noise floor)
    if random.random() < 0.7:
        noise_level = random.uniform(0.0, 0.02)
        out = out + np.random.normal(0, noise_level, size=out.shape).astype(np.float32)

    return np.clip(out, -1.0, 1.0).astype(np.float32)


# ──────────────────────────────────────────────────────────────────────
# Dataset
# ──────────────────────────────────────────────────────────────────────
class VoiceCommandDataset(Dataset):
    def __init__(self, data_dir, labels, file_list, train=True):
        self.data_dir = data_dir
        self.labels = labels
        self.files = file_list  # list of (filepath, label_index)
        self.train = train

    def __len__(self):
        return len(self.files)

    def __getitem__(self, idx):
        path, label_idx = self.files[idx]
        samples = load_wav_mono16k(path)
        samples = pad_or_center_trim(samples)
        if self.train:
            samples = augment(samples)
        feats = compute_log_mel(samples)
        x = torch.from_numpy(feats).unsqueeze(0)  # (1, NUM_FRAMES, N_MELS)
        return x, label_idx


def build_file_list(data_dir, labels):
    files = []
    for idx, label in enumerate(labels):
        folder = os.path.join(data_dir, label)
        if not os.path.isdir(folder):
            continue
        for fname in os.listdir(folder):
            if fname.lower().endswith(".wav"):
                files.append((os.path.join(folder, fname), idx))
    return files


# ──────────────────────────────────────────────────────────────────────
# Model
# ──────────────────────────────────────────────────────────────────────
class VoiceCNN(nn.Module):
    def __init__(self, num_classes):
        super().__init__()
        self.conv1 = nn.Conv2d(1, 8, kernel_size=(10, 4), stride=(2, 1))
        self.conv2 = nn.Conv2d(8, 16, kernel_size=(5, 3), stride=(2, 1))
        self.relu = nn.ReLU()
        self.dropout = nn.Dropout(0.3)

        with torch.no_grad():
            dummy = torch.zeros(1, 1, NUM_FRAMES, N_MELS)
            flat_size = self._forward_conv(dummy).numel()

        self.fc1 = nn.Linear(flat_size, 64)
        self.fc2 = nn.Linear(64, num_classes)

    def _forward_conv(self, x):
        x = self.relu(self.conv1(x))
        x = self.relu(self.conv2(x))
        return x

    def forward(self, x):
        x = self._forward_conv(x)
        x = x.flatten(1)
        x = self.relu(self.fc1(x))
        x = self.dropout(x)
        return self.fc2(x)  # raw logits — softmax is applied on the Unity side


# ──────────────────────────────────────────────────────────────────────
# Plots — saved as JPGs alongside the exported model
# ──────────────────────────────────────────────────────────────────────
def save_training_curves(history, out_dir):
    epochs = range(1, len(history["train_loss"]) + 1)

    # Accuracy curve (%)
    plt.figure(figsize=(8, 5))
    plt.plot(epochs, [a * 100 for a in history["train_acc"]], label="Training Accuracy")
    plt.plot(epochs, [a * 100 for a in history["val_acc"]], label="Validation Accuracy")
    plt.xlabel("Epoch")
    plt.ylabel("Accuracy (%)")
    plt.title("Training / Validation Accuracy")
    plt.legend()
    plt.grid(True, alpha=0.3)
    plt.tight_layout()
    acc_path = os.path.join(out_dir, "accuracy_curve.jpg")
    plt.savefig(acc_path, format="jpg", dpi=150)
    plt.close()
    print(f"Saved accuracy plot: {acc_path}")

    # Loss curve
    plt.figure(figsize=(8, 5))
    plt.plot(epochs, history["train_loss"], label="Training Loss")
    plt.plot(epochs, history["val_loss"], label="Validation Loss")
    plt.xlabel("Epoch")
    plt.ylabel("Loss")
    plt.title("Training / Validation Loss")
    plt.legend()
    plt.grid(True, alpha=0.3)
    plt.tight_layout()
    loss_path = os.path.join(out_dir, "loss_curve.jpg")
    plt.savefig(loss_path, format="jpg", dpi=150)
    plt.close()
    print(f"Saved loss plot: {loss_path}")


# ──────────────────────────────────────────────────────────────────────
# Training
# ──────────────────────────────────────────────────────────────────────
def train(args):
    labels = sorted([
        d for d in os.listdir(args.data_dir)
        if os.path.isdir(os.path.join(args.data_dir, d))
    ])
    if not labels:
        raise SystemExit(f"No class folders found inside {args.data_dir}")
    print(f"Classes (in this exact order — index 0, 1, 2 ...): {labels}")

    all_files = build_file_list(args.data_dir, labels)
    if len(all_files) < 20:
        print("⚠ Very little training data found. Results will be unreliable "
              "until you record more samples per class.")
    random.shuffle(all_files)

    split = int(len(all_files) * 0.85)
    train_files, val_files = all_files[:split], all_files[split:]

    train_ds = VoiceCommandDataset(args.data_dir, labels, train_files, train=True)
    val_ds = VoiceCommandDataset(args.data_dir, labels, val_files, train=False)

    train_loader = DataLoader(train_ds, batch_size=args.batch_size, shuffle=True)
    val_loader = DataLoader(val_ds, batch_size=args.batch_size, shuffle=False)

    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    model = VoiceCNN(num_classes=len(labels)).to(device)
    optimizer = torch.optim.Adam(model.parameters(), lr=args.lr)
    criterion = nn.CrossEntropyLoss()

    best_val_acc = 0.0
    best_state = None

    # history for the accuracy/loss curve JPGs saved at the end of training
    history = {"train_loss": [], "val_loss": [], "train_acc": [], "val_acc": []}

    for epoch in range(args.epochs):
        model.train()
        train_loss, train_correct, train_total = 0.0, 0, 0
        for x, y in train_loader:
            x, y = x.to(device), y.to(device)
            optimizer.zero_grad()
            logits = model(x)
            loss = criterion(logits, y)
            loss.backward()
            optimizer.step()
            train_loss += loss.item() * x.size(0)
            train_correct += (logits.argmax(dim=1) == y).sum().item()
            train_total += y.size(0)
        train_loss /= max(len(train_ds), 1)
        train_acc = train_correct / train_total if train_total else 0.0

        model.eval()
        val_loss, correct, total = 0.0, 0, 0
        with torch.no_grad():
            for x, y in val_loader:
                x, y = x.to(device), y.to(device)
                logits = model(x)
                loss = criterion(logits, y)
                val_loss += loss.item() * x.size(0)
                preds = logits.argmax(dim=1)
                correct += (preds == y).sum().item()
                total += y.size(0)
        val_loss /= max(len(val_ds), 1)
        val_acc = correct / total if total else 0.0

        history["train_loss"].append(train_loss)
        history["val_loss"].append(val_loss)
        history["train_acc"].append(train_acc)
        history["val_acc"].append(val_acc)

        print(f"epoch {epoch+1:3d}/{args.epochs}  train_loss={train_loss:.4f}  "
              f"val_loss={val_loss:.4f}  train_acc={train_acc:.3f}  val_acc={val_acc:.3f}")

        if val_acc >= best_val_acc:
            best_val_acc = val_acc
            best_state = {k: v.clone() for k, v in model.state_dict().items()}

    if best_state is not None:
        model.load_state_dict(best_state)
    print(f"Best validation accuracy: {best_val_acc:.3f}")

    os.makedirs(args.out_dir, exist_ok=True)

    save_training_curves(history, args.out_dir)

    # Export ONNX with a fixed input shape (best compatibility with Sentis /
    # Unity Inference Engine — avoid dynamic axes for a model this small).
    model.eval().cpu()
    dummy_input = torch.zeros(1, 1, NUM_FRAMES, N_MELS)
    onnx_path = os.path.join(args.out_dir, "voice_commands.onnx")
    torch.onnx.export(
        model, dummy_input, onnx_path,
        input_names=["input"], output_names=["logits"],
        opset_version=13,
    )
    print(f"Saved model: {onnx_path}")

    labels_path = os.path.join(args.out_dir, "labels.txt")
    with open(labels_path, "w", encoding="utf-8") as f:
        f.write("\n".join(labels))
    print(f"Saved labels: {labels_path}")
    print("\nCopy both files into your Unity project, then assign them on "
          "VoiceCommandRecognizer (modelAsset + labelsFile).")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--data_dir", required=True,
                         help="Folder containing one subfolder per class (up/down/left/right/silence/unknown)")
    parser.add_argument("--out_dir", default="./export")
    parser.add_argument("--epochs", type=int, default=30)
    parser.add_argument("--batch_size", type=int, default=32)
    parser.add_argument("--lr", type=float, default=1e-3)
    args = parser.parse_args()
    train(args)