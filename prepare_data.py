"""
prepare_data.py
===============

Run this ONCE before training. It does two things:

  1. Downloads Google Speech Commands v0.02 (English)
     — ~2.3 GB download, ~105,000 WAV files
     — Already has "up", "down", "left", "right", silence, and unknown words
     — Already 16kHz mono WAV, exactly the format we need

  2. Generates Arabic direction words using free Microsoft Edge TTS
     — Uses 5 different Arabic voices for variety
     — Generates ~250 samples per direction word (فوق/تحت/يسار/يمين)
     — No internet needed after install, no API key, completely free

After this script finishes, just run train_voice_commands.py.

Usage:
    pip install edge-tts soundfile numpy --break-system-packages
    python prepare_data.py

Output:
    voice_data/
        up/         English "up" + Arabic "فوق"
        down/       English "down" + Arabic "تحت"
        left/       English "left" + Arabic "يسار"
        right/      English "right" + Arabic "يمين"
        silence/    Background noise from Google dataset
        unknown/    Other English words (used to reject non-commands)
"""

import os
import sys
import asyncio
import random
import tarfile
import urllib.request
import shutil
import struct
import math

# ── Configuration ──────────────────────────────────────────────────────────────
OUTPUT_DIR = "./voice_data"

# How many files to take per class from Google Speech Commands.
# The dataset has 3,000-4,000 per word — we cap it to keep things manageable.
MAX_ENGLISH_PER_CLASS = 2000

# How many TTS samples to generate per Arabic word.
# More = better but slower. 250 is a good balance.
ARABIC_SAMPLES_PER_WORD = 250

# Words from Google Speech Commands to use as "unknown" (teach model to reject them)
UNKNOWN_WORDS = ["yes", "no", "on", "off", "stop", "go", "one", "two", "three"]
MAX_UNKNOWN_FILES = 600

SPEECH_COMMANDS_URL = "http://download.tensorflow.org/data/speech_commands_v0.02.tar.gz"
ARCHIVE_PATH = "./speech_commands_v0.02.tar.gz"
EXTRACT_PATH = "./speech_commands_raw"

# Arabic words mapped to English folder names
ARABIC_WORDS = {
    "up":    "فوق",
    "down":  "تحت",
    "left":  "يسار",
    "right": "يمين",
}

# 5 Microsoft Edge TTS Arabic voices — variety of accents and genders.
# This is what makes TTS-generated data work: multiple different voices.
ARABIC_VOICES = [
    "ar-SA-ZariyahNeural",    # Saudi, female
    "ar-SA-HamedNeural",      # Saudi, male
    "ar-EG-SalmaNeural",      # Egyptian, female
    "ar-EG-ShakirNeural",     # Egyptian, male
    "ar-AE-FatimaNeural",     # UAE, female
]


# ── Step 1: Download & extract Google Speech Commands ─────────────────────────

def download_with_progress(url, dest):
    print(f"\nDownloading Google Speech Commands (~2.3 GB)...")
    print("This will take a few minutes depending on your internet speed.\n")

    def report(block_num, block_size, total_size):
        downloaded = block_num * block_size
        if total_size > 0:
            pct = min(downloaded / total_size * 100, 100)
            mb_done = downloaded / 1_000_000
            mb_total = total_size / 1_000_000
            bar = "#" * int(pct / 2) + "-" * (50 - int(pct / 2))
            print(f"\r  [{bar}] {pct:.1f}%  {mb_done:.0f}/{mb_total:.0f} MB", end="", flush=True)

    urllib.request.urlretrieve(url, dest, reporthook=report)
    print()


def setup_english_data():
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    if not os.path.exists(ARCHIVE_PATH):
        download_with_progress(SPEECH_COMMANDS_URL, ARCHIVE_PATH)
    else:
        print("Archive already downloaded, skipping download.")

    if not os.path.exists(EXTRACT_PATH):
        print("Extracting... (this takes a minute)")
        with tarfile.open(ARCHIVE_PATH, "r:gz") as tar:
            tar.extractall(EXTRACT_PATH)
        print("Extracted.")
    else:
        print("Already extracted, skipping.")

    print("\nCopying English files into voice_data/...")

    # Direction words -> into our class folders
    for word in ["up", "down", "left", "right"]:
        src = os.path.join(EXTRACT_PATH, word)
        dst = os.path.join(OUTPUT_DIR, word)
        os.makedirs(dst, exist_ok=True)

        files = [f for f in os.listdir(src) if f.endswith(".wav")]
        files = random.sample(files, min(MAX_ENGLISH_PER_CLASS, len(files)))
        for i, fname in enumerate(files):
            shutil.copy(
                os.path.join(src, fname),
                os.path.join(dst, f"en_{word}_{i:04d}.wav")
            )
        print(f"  {word}/ : {len(files)} English files")

    # Silence -> from _background_noise_ (long files, we slice them into 1-sec clips)
    silence_dir = os.path.join(OUTPUT_DIR, "silence")
    os.makedirs(silence_dir, exist_ok=True)
    bg_dir = os.path.join(EXTRACT_PATH, "_background_noise_")
    if os.path.isdir(bg_dir):
        bg_files = [f for f in os.listdir(bg_dir) if f.endswith(".wav")]
        count = 0
        for bg_file in bg_files:
            clips = slice_wav_to_1sec(os.path.join(bg_dir, bg_file))
            for j, clip in enumerate(clips[:30]):  # max 30 clips per source file
                write_wav(
                    os.path.join(silence_dir, f"silence_{bg_file}_{j:03d}.wav"),
                    clip, 16000
                )
                count += 1
        print(f"  silence/ : {count} clips from background noise files")

    # Unknown words -> sample from other Google Speech Commands words
    unknown_dir = os.path.join(OUTPUT_DIR, "unknown")
    os.makedirs(unknown_dir, exist_ok=True)
    count = 0
    per_word = MAX_UNKNOWN_FILES // len(UNKNOWN_WORDS)
    for word in UNKNOWN_WORDS:
        src = os.path.join(EXTRACT_PATH, word)
        if not os.path.isdir(src):
            continue
        files = [f for f in os.listdir(src) if f.endswith(".wav")]
        files = random.sample(files, min(per_word, len(files)))
        for fname in files:
            shutil.copy(
                os.path.join(src, fname),
                os.path.join(unknown_dir, f"unk_{word}_{fname}")
            )
            count += 1
    print(f"  unknown/ : {count} files from other English words")


# ── Step 2: Generate Arabic TTS samples ────────────────────────────────────────

async def generate_arabic_tts():
    try:
        import edge_tts
    except ImportError:
        print("\n[!] edge-tts not installed. Run:")
        print("    pip install edge-tts --break-system-packages")
        print("Skipping Arabic generation.")
        return

    print("\nGenerating Arabic TTS samples...")
    print(f"  {ARABIC_SAMPLES_PER_WORD} samples per word × 4 words = "
          f"{ARABIC_SAMPLES_PER_WORD * 4} total Arabic files\n")

    # Speed variations (0.9 = slightly slower, 1.3 = slightly faster)
    # This simulates different natural speaking speeds
    rates = ["-10%", "+0%", "+0%", "+10%", "+20%"]

    for english_name, arabic_word in ARABIC_WORDS.items():
        out_dir = os.path.join(OUTPUT_DIR, english_name)
        os.makedirs(out_dir, exist_ok=True)

        samples_per_voice = ARABIC_SAMPLES_PER_WORD // len(ARABIC_VOICES)
        count = 0

        for voice in ARABIC_VOICES:
            for i in range(samples_per_voice):
                rate = random.choice(rates)
                out_path = os.path.join(
                    out_dir,
                    f"ar_{english_name}_{voice.split('-')[1]}_{i:04d}.mp3"
                )
                tmp_wav = out_path.replace(".mp3", ".wav")

                try:
                    communicate = edge_tts.Communicate(arabic_word, voice, rate=rate)
                    await communicate.save(out_path)

                    # Convert MP3 to 16kHz mono WAV using edge_tts output
                    mp3_to_wav_16k(out_path, tmp_wav)
                    os.remove(out_path)  # remove mp3

                    count += 1
                except Exception as e:
                    print(f"  Warning: skipped {voice} sample {i}: {e}")

        print(f"  {english_name}/ ({arabic_word}) : added {count} Arabic TTS files")


def mp3_to_wav_16k(mp3_path, wav_path):
    """Convert MP3 to 16kHz WAV without needing ffmpeg, using the built-in audioop."""
    try:
        import audioop
    except ImportError:
        audioop = None

    # Try to use pydub if available (best quality)
    try:
        from pydub import AudioSegment
        audio = AudioSegment.from_mp3(mp3_path)
        audio = audio.set_frame_rate(16000).set_channels(1).set_sample_width(2)
        audio.export(wav_path, format="wav")
        return
    except ImportError:
        pass

    # Fallback: use ffmpeg if available
    import subprocess
    result = subprocess.run(
        ["ffmpeg", "-y", "-i", mp3_path, "-ar", "16000", "-ac", "1",
         "-sample_fmt", "s16", wav_path],
        capture_output=True
    )
    if result.returncode != 0:
        raise RuntimeError(
            "ffmpeg not found and pydub not installed. "
            "Install one of them:\n"
            "  pip install pydub --break-system-packages\n"
            "  or download ffmpeg from https://ffmpeg.org/download.html"
        )


# ── WAV helpers (no external dependencies) ────────────────────────────────────

def read_wav_samples(path):
    """Read a WAV file and return (samples_float32, sample_rate)."""
    with open(path, "rb") as f:
        data = f.read()

    if data[:4] != b"RIFF" or data[8:12] != b"WAVE":
        return [], 16000

    offset = 12
    sample_rate = 16000
    num_channels = 1
    bits_per_sample = 16
    audio_data = b""

    while offset < len(data) - 8:
        chunk_id = data[offset:offset+4]
        chunk_size = struct.unpack_from("<I", data, offset+4)[0]
        chunk_data = data[offset+8:offset+8+chunk_size]

        if chunk_id == b"fmt ":
            num_channels = struct.unpack_from("<H", chunk_data, 2)[0]
            sample_rate = struct.unpack_from("<I", chunk_data, 4)[0]
            bits_per_sample = struct.unpack_from("<H", chunk_data, 14)[0]
        elif chunk_id == b"data":
            audio_data = chunk_data

        offset += 8 + chunk_size + (chunk_size % 2)

    if not audio_data or bits_per_sample != 16:
        return [], sample_rate

    n = len(audio_data) // (2 * num_channels)
    samples = []
    for i in range(n):
        val = 0
        for c in range(num_channels):
            s = struct.unpack_from("<h", audio_data, (i * num_channels + c) * 2)[0]
            val += s
        samples.append((val / num_channels) / 32768.0)

    return samples, sample_rate


def write_wav(path, samples_float, sample_rate=16000):
    """Write a list of float samples to a 16kHz mono 16-bit WAV file."""
    pcm = bytes()
    for s in samples_float:
        clamped = max(-1.0, min(1.0, s))
        pcm += struct.pack("<h", int(clamped * 32767))

    with open(path, "wb") as f:
        data_size = len(pcm)
        f.write(b"RIFF")
        f.write(struct.pack("<I", 36 + data_size))
        f.write(b"WAVE")
        f.write(b"fmt ")
        f.write(struct.pack("<I", 16))
        f.write(struct.pack("<H", 1))           # PCM
        f.write(struct.pack("<H", 1))           # mono
        f.write(struct.pack("<I", sample_rate))
        f.write(struct.pack("<I", sample_rate * 2))  # byte rate
        f.write(struct.pack("<H", 2))           # block align
        f.write(struct.pack("<H", 16))          # bits per sample
        f.write(b"data")
        f.write(struct.pack("<I", data_size))
        f.write(pcm)


def resample_linear(samples, old_rate, new_rate):
    """Simple linear interpolation resample."""
    if old_rate == new_rate:
        return samples
    ratio = old_rate / new_rate
    new_len = int(len(samples) / ratio)
    resampled = []
    for i in range(new_len):
        pos = i * ratio
        low = int(pos)
        high = min(low + 1, len(samples) - 1)
        frac = pos - low
        resampled.append(samples[low] * (1 - frac) + samples[high] * frac)
    return resampled


def slice_wav_to_1sec(path, target_sr=16000):
    """Slice a long WAV file into 1-second clips at 16kHz."""
    samples, sr = read_wav_samples(path)
    if not samples:
        return []
    if sr != target_sr:
        samples = resample_linear(samples, sr, target_sr)
    chunk = target_sr
    clips = []
    for i in range(0, len(samples) - chunk, chunk):
        clips.append(samples[i:i+chunk])
    return clips


# ── Main ───────────────────────────────────────────────────────────────────────

def print_summary():
    print("\n" + "="*55)
    print("  Data ready! Summary:")
    print("="*55)
    for folder in ["up", "down", "left", "right", "silence", "unknown"]:
        path = os.path.join(OUTPUT_DIR, folder)
        if os.path.isdir(path):
            count = len([f for f in os.listdir(path) if f.endswith(".wav")])
            arabic = ARABIC_WORDS.get(folder, "")
            label = f"{folder}/ ({arabic})" if arabic else f"{folder}/"
            print(f"  {label:<20} {count:>5} files")
    print("="*55)
    print("\nNext step — run training:")
    print("  python train_voice_commands.py --data_dir ./voice_data --epochs 30\n")


if __name__ == "__main__":
    print("="*55)
    print("  Voice data preparation")
    print("="*55)

    # Check Python version
    if sys.version_info < (3, 8):
        print("Python 3.8+ required.")
        sys.exit(1)

    # Step 1 — English data from Google Speech Commands
    print("\n[Step 1/2] English data (Google Speech Commands)")
    setup_english_data()

    # Step 2 — Arabic TTS
    print("\n[Step 2/2] Arabic TTS generation")
    try:
        asyncio.run(generate_arabic_tts())
    except RuntimeError as e:
        if "no running event loop" in str(e).lower():
            loop = asyncio.new_event_loop()
            asyncio.set_event_loop(loop)
            loop.run_until_complete(generate_arabic_tts())
        else:
            raise

    print_summary()
