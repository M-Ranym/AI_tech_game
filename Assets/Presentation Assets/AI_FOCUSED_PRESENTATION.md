# Teaching Artificial Intelligence to Children
### Design and Development of the "AI Learning Journey" Module — within Ferid Around the World
Full slide text, AI-centered, 43 slides across 10 chapters — including Méthodologie and Conception,
matching the fuller structure you pointed to from Yassine's deck. This is the actual content to type
onto each slide — trim only if a slide visually overflows.

---

### SLIDE 1 — Title
**Teaching Artificial Intelligence to Children**
Design and Development of the "AI Learning Journey" Module within Ferid Around the World

By Ranym Moussa
Defended on [date] before the jury — President / Examiner / Supervisor
Host Company: Horizon Education — Industrial Supervisor: Mouadh Mkadmi
National Diploma of Computer Engineering — Academic Year 2024/2025

---

### SLIDE 2 — Plan
1. General Context
2. Problem Statement
3. Proposed Solution
4. Requirements Specification
5. Méthodologie
6. Conception
7. System & AI Architecture
8. Implementation — 7 Mini-Games, 7 AI Concepts
9. The Real AI: Voice Command Recognition
10. Conclusion & Perspectives

---

## 1. General Context

### SLIDE 3 — Chapter divider
**1. General Context**

### SLIDE 4 — 1.1 Host Company & Platform
- Horizon Education — Tunisian ed-tech startup, Sousse, founded 2020
- Specializes in educational games for children
- Flagship product: **Ferid Around the World** — a mobile learning app where the robot mascot Ferid guides children through maths, languages, science, geography, life skills
- This project adds a brand-new module to that platform: **AI literacy**

### SLIDE 5 — 1.2 Why Teach AI to Children?
- AI already shapes healthcare, transport, communication, education
- But nearly all AI-education material assumes prior programming or math knowledge
- Gap: no accessible way for a 5–10 year-old to grasp *how a machine actually learns*
- Serious games + implicit learning = a way to teach the workflow of AI without teaching code

### SLIDE 6 — 1.3 Existing Solutions & Their Limits
| Solution | What it teaches | Limit |
|---|---|---|
| Machine Learning for Kids | Build simple ML models via visual programming | Needs prior coding knowledge, older kids |
| Code.org — AI for Oceans | Classification through an ocean-cleanup game | Covers one AI technique only |
| Google Quick, Draw! | Image recognition demo | Fun, but doesn't teach the pipeline |

**None of them show the complete journey — from raw data to an autonomous decision.**

---

## 2. Problem Statement

### SLIDE 7 — Chapter divider
**2. Problem Statement**

### SLIDE 8 — Problem Statement
- Children have no intuitive, age-appropriate path through the **full AI development pipeline**
- Existing tools each teach one isolated fragment (a technique, a demo) — never the workflow that connects them
- Result: children can play with "AI-flavored" games without ever understanding what an AI system actually *does*, step by step, to go from data to a decision

---

## 3. Proposed Solution

### SLIDE 9 — Chapter divider
**3. Proposed Solution**

### SLIDE 10 — An AI Curriculum, Disguised as a Game
- **AI Learning Journey**: 7 connected mini-games, ages 5–10
- Each mini-game = one **real stage** of how an AI system gets built, translated into an age-appropriate interaction
- A world map is the throughline: complete one stage, unlock the next
- The goal is not to teach programming or math — it's to teach **the workflow of AI** through play: collect → clean → recognize → validate → debug → plan → decide

### SLIDE 11 — The AI Pipeline the Child Walks Through
*(Visual: `diagrams/03-ai-learning-architecture.svg`)*

| Stage | Mini-Game | AI Concept Taught |
|---|---|---|
| 1. Data Preparation | Game 0 — Data Gathering | Data Collection |
| 1. Data Preparation | Game 1 — Data Cleaning | Data Cleaning |
| 2. Computer Vision | Game 2 — Image Recognition Eval. | Image Recognition |
| 2. Computer Vision | Game 3 — Face Detection | Face Detection |
| 3. AI Validation | Game 4 — AI Agent Debugging | System Debugging |
| 4. Intelligent Robotics | Game 5 — Robot Path Planning | Path Planning |
| 4. Intelligent Robotics | Game 6 — AI Decision Making | Decision Making |

---

## 4. Requirements Specification

### SLIDE 12 — Chapter divider
**4. Requirements Specification**

### SLIDE 13 — Requirements
**Functional (selected):**
- Display a world map with progressive unlocking
- Interactive tutorial before every new mechanic
- Evaluate the player's actions with immediate visual/audio feedback
- Save progress and resume across sessions

**Non-Functional:**
- Usability for ages 5–10 · Performance (smooth transitions) · Reliability
- Maintainability (modular) · Scalability (new games without rearchitecting)
- Compatibility (Android/Unity) · Accessibility (visual + audio guidance)

(Use case diagram now shown in full in Chapter 6 — Conception, not repeated here)

---

## 5. Méthodologie

### SLIDE 14 — Chapter divider
**5. Méthodologie** — Agile Scrum, how the project was actually managed over 8 sprints

### SLIDE 15 — Agile Scrum: Roles & Events
- Scrum divides the project into short **Sprints**; each delivers a tested, functional increment before the next begins
- **Roles:**

| Role | Team Member | Responsibility |
|---|---|---|
| Product Owner | Mouadh Mkadmi | Vision, backlog priority, validates features |
| Scrum Master | Yassine Alaoui | Process, technical guidance, unblocking |
| Development Team | Ranym Moussa | Design, implementation, testing, integration |

- **Events:** Sprint Planning · Daily Scrum · Sprint Review · Sprint Retrospective
- **Tools:** ClickUp (task tracking) · Slack (team communication) · Git/GitHub (version control)

### SLIDE 16 — Product Backlog & Sprint Planning
8 sprints, 16 weeks — one mini-game delivered as a tested increment per sprint.

| Sprint | Goal | Weeks |
|---|---|---|
| 1 | Data Gathering | 1–2 |
| 2 | Data Cleaning | 3–4 |
| 3 | Image Recognition Evaluation | 5–6 |
| 4 | Face Detection | 7–8 |
| 5 | AI Agent Debugging | 9–10 |
| 6 | Robot Path Planning | 11–12 |
| 7 | AI Decision Making | 13–14 |
| 8 | Game Map + full integration | 15–16 |

---

## 6. Conception

### SLIDE 17 — Chapter divider
**6. Conception** — UML modeling: how the player interacts with the system, and how the classes underneath are organized

### SLIDE 18 — General Use Case Diagram
*(Visual: `report-figures/Fig1.3_...` — already correct in your report, proper `<<include>>`/`<<extend>>` UML notation)*
- Single actor: the child **Player** — launches the game, views the map, selects and plays a mini-game, views its tutorial, receives feedback, and unlocks the next activity

### SLIDE 19 — Sequence & Class Diagram (Face Detection / AI Agent Debugging / Robot Path Planning group)
*(Visual: `report-figures/Fig4.14_...` sequence diagram + `Fig4.15_...` class diagram — verified correct, no swap issue in this pair)*
- Shown here since this group includes Robot Path Planning — the mini-game the voice-recognition module (Chapter 9) plugs into
- Key classes: `MazeGenerator`, `MazeCell`, `RobotController` drive the maze logic; all activity managers report progress to the shared `GameProgressManager` singleton

---

## 7. System & AI Architecture

### SLIDE 20 — Chapter divider
**7. System & AI Architecture**

### SLIDE 21 — Overall System Architecture
*(Visual: `diagrams/02-overall-system-architecture.svg`)*
- **Game Map** — central navigation hub
- **Progress Manager** — singleton, tracks locked / unlocked / finished levels
- **Save System** — Unity PlayerPrefs, persists across sessions
- Each of the 7 mini-games is an independent module — modular, easy to extend with new AI concepts later

### SLIDE 22 — Technology Stack
- **Unity** + **C#** — engine and gameplay logic
- **PyTorch** — training the voice-recognition CNN
- **Unity Sentis** — running that trained model offline, inside the game
- **Krita** — graphical assets · **Git/GitHub** — version control
*(Scrum/ClickUp/Slack already covered in Chapter 5 — Méthodologie, not repeated here)*

---

## 8. Implementation — 7 Mini-Games, 7 AI Concepts

### SLIDE 23 — Chapter divider
**8. Implementation — 7 Mini-Games, 7 AI Concepts**

### SLIDE 24 — Game 0: Data Gathering
- **Gameplay**: drag animals into their correct categories
- **The real AI concept**: every AI system starts with data — and a model is only as good as the data it's trained on ("garbage in, garbage out"). Sorting animals *is* the data-collection step of a real ML pipeline, made physical.
- Visual: `report-figures/Fig3.3_...` (gameplay) + `Fig3.5_...` (success panel)

### SLIDE 25 — Game 1: Data Cleaning
- **Gameplay**: sort objects across levels of increasing difficulty; the game deliberately starts with some items mis-sorted
- **The real AI concept**: raw collected data is noisy and partly mislabeled — it must be reviewed and cleaned *before* it can train a model. The deliberately-wrong starting arrangement mirrors real messy datasets.
- Visual: `report-figures/Fig3.8_...` + `Fig3.9_Wrong_classification...`

### SLIDE 26 — Game 2: Image Recognition Evaluation
- **Gameplay**: the robot labels images; the child judges whether each prediction is right or wrong
- **The real AI concept**: you never blindly trust a model's output — you evaluate its predictions against ground truth. That's literally what an ML engineer does with a validation set. The robot's simulated accuracy score visibly rises or falls based on the child's own judgement.
- Visual: `report-figures/Fig3.12_...` + `Fig3.13_Evaluation_screen`

### SLIDE 27 — Game 3: Face Detection
- **Gameplay**: the robot draws a box around what it thinks is a face; the child confirms or drags it to the correct spot
- **The real AI concept**: object localization — a model must first *find* something in an image before it can recognize or analyze it. Correcting a wrong box mirrors human-in-the-loop labeling, one of the real techniques used to improve production AI systems.
- Visual: `report-figures/Fig4.2_...` + `Fig4.4_Correcting_robot_s_answer`

### SLIDE 28 — Game 4: AI Agent Debugging
- **Gameplay**: a grid of AI agents, some faulty; the child bins the faulty ones and keeps the good ones
- **The real AI concept**: AI systems fail silently and need active auditing — you can't assume a deployed model is correct. Scoring rewards catching real faults more than it penalizes false alarms (0.20 vs 0.15 weighting), mirroring how debugging prioritizes recall over caution.
- Visual: `report-figures/Fig4.6_...` + `Fig4.7_Indicate_missed_agents`

### SLIDE 29 — Game 5: Robot Path Planning
- **Gameplay**: build a movement program (drag blocks or **speak commands**) to guide the robot through a maze
- **The real AI concept**: planning a sequence of actions to reach a goal under constraints — a classic AI search/planning problem. This is also where the project's *real* trained neural network plugs in (Chapter 9).
- Visual: `report-figures/Fig5.2_...` + `Fig5.3_Action_cards_arrangement` *(these are correctly the Robot Path Planning screenshots — see note below)*

### SLIDE 30 — Game 6: AI Decision Making
- **Gameplay**: place the right action card (Jump / Climb / Break Rock / Zipline) at each decision point along a fixed path
- **The real AI concept**: the same "path" requires *different* actions depending on context — this is context-aware action selection, the same idea behind a policy function in a decision-making AI system.
- Visual: `report-figures/Fig4.10_...` / `Fig4.12_...` *(these are correctly the AI Decision Making screenshots — see note below)*

> ⚠️ **Report note:** in `report test.docx`, the figures captioned "Robot Path Planning" (4.9–4.12) and
> "AI Decision Making" (5.1–5.3) are swapped — verified by opening all seven images. The references above
> already point you to the correct file for each concept; fix the captions in the report itself too.

### SLIDE 31 — The Game Map: Tying It Together
- Central navigation: an animated drone travels a fixed path, one stop per mini-game
- Finishing a game → reward panel → drone flies to next stop → lock fades → progress saved
- Visual: `report-figures/Fig5.7_...`

---

## 9. The Real AI: Voice Command Recognition

### SLIDE 32 — Chapter divider
**9. The Real AI: Voice Command Recognition**
*Everything so far simulates AI concepts for pedagogy. This is a real trained neural network, in production, in the game.*

### SLIDE 33 — Where This Fits
*(Visual: `diagrams/04-ai-hierarchy.svg`)*
- Artificial Intelligence → Machine Learning → Deep Learning → Automatic Speech Recognition → **Voice Command Classification**
- A Convolutional Neural Network (CNN), trained in PyTorch, lets the child control the Robot Path Planning robot with **spoken English commands** instead of dragging blocks
- Supervised learning: the network is trained on labelled audio and learns the mapping from speech patterns to commands

### SLIDE 34 — The Task & The Data
- **6-class classification**: up · down · left · right · unknown · silence
- The `unknown`/`silence` classes let the model *withhold* a prediction instead of guessing on unclear audio
- **Dataset**: Google Speech Commands v0.02 — thousands of labelled single-word recordings from many speakers
- Future work: extend the dataset with Arabic and French recordings for multilingual support

### SLIDE 35 — Audio Preprocessing Pipeline
*(Visual: `diagrams/06-audio-preprocessing-pipeline.svg`)*
1. Record 1s of audio @ 16 kHz → 2. Normalize → 3. Frame (400-sample / 25ms windows, 160-sample hop → 98 frames) → 4. Hann window + 512-point FFT → 5. Project onto 40 mel filters (20Hz–8kHz) → 6. Log-compress → **7. 98×40 log-mel spectrogram**
- This exact pipeline is re-implemented in **C# (`MelSpectrogram.cs`)** so Unity reproduces identical features at inference time — critical since Sentis has no access to Python audio libraries

### SLIDE 36 — VoiceCNN Architecture
*(Visual: `diagrams/05-voicecnn-architecture.svg`)*
- Input: 98×40×1 log-mel spectrogram
- Convolutional layers → acoustic feature extraction
- ReLU activations + Dropout regularization → generalization, avoids overfitting
- Fully connected layers → final classification → softmax over 6 classes
- **Why CNN over RNN/LSTM/Transformer?** Best balance of accuracy, compute cost, and real-time inference speed for a Unity-deployed, resource-constrained context — CNNs capture local spectral patterns without the sequential overhead of recurrent models

### SLIDE 37 — Training
*(Visual: `diagrams/07-model-training-workflow.svg` — or `report-figures/Fig2.14_...`)*
- Train/validation split of the labelled dataset
- **Adam optimizer**, **Cross-Entropy Loss**
- Best-validation-accuracy checkpoint retained
- Exported to **ONNX** for cross-platform deployment

### SLIDE 38 — Deployment in Unity
*(Visual: `diagrams/08-unity-integration-workflow.svg`)*
- Model loaded via **Unity Sentis** (`ModelLoader.Load`), **CPU backend** — fully offline, no network or GPU dependency
- Child holds the microphone button (records up to 3s) → padded/centered to the 1s training window
- Sentis inference → softmax computed manually in C# → **confidence threshold 60%**
- Below threshold, or predicted `silence`/`unknown` → prediction discarded, no action
- Otherwise → command mapped to a movement block, added to the robot's program panel exactly as if dragged manually

### SLIDE 39 — Live Demo
- Pick up a direction block **by voice** in Robot Path Planning, run the maze
- 🎥 Record a 20–30s backup screen capture in advance in case of mic/room-noise issues during the defense

---

## 10. Conclusion & Perspectives

### SLIDE 40 — Chapter divider
**10. Conclusion & Perspectives**

### SLIDE 41 — Conclusion
- Delivered a 7-mini-game platform that walks children 5–10 through the **entire AI development pipeline** — not an isolated technique, the full workflow
- Backed by a **real, deployed AI system**: a CNN-based offline speech recognizer, not just a simulated one
- Built with Unity/C#/PyTorch/Sentis, managed through Agile Scrum across 8 sprints / 16 weeks

### SLIDE 42 — Future Perspectives
- Multilingual speech recognition — Arabic, French
- Larger, more diverse voice dataset; improved robustness to background noise
- Child progress tracking for parents/educators
- Additional AI concepts and mini-games extending the pipeline further

### SLIDE 43 — Thank You / Questions

---

## Where every visual comes from
- `diagrams/02, 03, 04, 05, 06, 07, 08` — new diagrams I drew (05 and 08 fill real gaps in your report — Fig 2.12 and Fig 2.15 were never inserted; use them there too)
- `report-figures/Fig1.3, Fig2.1, Fig4.14, Fig4.15` — already correct in your report, reuse directly
- `report-figures/Fig3.x, Fig4.x, Fig5.x` gameplay screenshots — already correct **except** the Robot Path Planning / AI Decision Making block (4.9–4.12 ↔ 5.1–5.3), where I've pointed you to the correct file for each slide above
