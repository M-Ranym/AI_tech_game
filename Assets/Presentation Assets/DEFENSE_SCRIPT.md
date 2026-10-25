# PFE Defense Presentation — Script & Visual Guide
### "AI Learning Journey" module for Ferid Around the World — Ranym Moussa

~28 slides, condensed for a 20–25 min defense. Text below is close to what should
appear on each slide (keep bullets short — this is more than you'd actually put on
the slide itself, trim to fit).

**Visual sourcing key**, used throughout:
- 📷 `report-figures/FigX.Y_...` → an image ALREADY in your `report test.docx`. Open the
  report, find the figure, right-click → Copy, paste into the slide. I've also extracted
  copies into `Assets/Presentation Assets/report-figures/` so you can grab them directly
  without opening Word.
- 🖼 `diagrams/NN-...svg` → a new diagram I drew to fill a real gap (your report doesn't
  have an image for it). In `Assets/Presentation Assets/diagrams/`. PowerPoint (365/2021+)
  can insert `.svg` directly: Insert → Pictures → This Device.
- 📋 table → paste as a **native PowerPoint table**, not an image (it stays editable and
  matches your theme's fonts/colors automatically — better than a flattened picture).
- 🎥 screen recording → the one thing I can't produce for you; record it yourself.

---

## Section A — Title & Agenda

**Slide 1 — Title**
- Development of an AI-Literacy Educational Game — "AI Learning Journey" — within Ferid Around the World
- By Ranym Moussa
- Defended on [date] before the jury: President / Examiner / Supervisor
- Host company: Horizon Education · Industrial Supervisor: Mouadh Mkadmi
- Academic Year 2024/2025 — National Diploma of Computer Engineering
- Visual: 📷 `Fig1.1_Horizon_Education_logo` small, bottom corner (optional)

**Slide 2 — Agenda**
1. General Context
2. Proposed Solution
3. Requirements Specification
4. System Architecture & Technologies
5. Implementation (mini-games)
6. The AI Voice-Recognition Module
7. Conclusion & Perspectives

---

## Section B — General Context

**Slide 3 — Section divider:** "General Context"

**Slide 4 — Host Company**
- Horizon Education — Tunisian ed-tech startup, Sousse, founded 2020
- Specializes in educational games for children
- Flagship product: **Ferid Around the World**, a mobile learning app for young children (maths, languages, science, geography, life skills…)
- Visual: 📷 `Fig1.1_Horizon_Education_logo` + 📷 `Fig1.2_Ferid_Around_the_World_Application`

**Slide 5 — The AI Learning Journey (this project)**
- New educational module added to Ferid Around the World
- A world map guides children (age 5–10) through **7 mini-games**, each teaching one stage of how AI systems are built
- 📋 table (Table 1.1, condensed):

| Mini-Game | AI Concept |
|---|---|
| Game 0 | Data Gathering |
| Game 1 | Data Cleaning |
| Game 2 | Image Recognition |
| Game 3 | Face Detection |
| Game 4 | AI System Debugging |
| Game 5 | Robot Path Planning |
| Game 6 | AI Decision Making |

**Slide 6 — Existing Solutions & Their Limits**
- Machine Learning for Kids — visual programming, but needs prior coding knowledge, targets older kids
- Code.org "AI for Oceans" — engaging, but covers only classification, one narrow slice of AI
- Google Quick, Draw! — shows image recognition well, but no view of the wider AI pipeline
- **Gap:** none present the *complete* AI workflow to young, non-technical children

**Slide 7 — Problem Statement**
- Existing AI-education tools are not kid-friendly: too complex, no real tutorials, no Arabic support
- Children have no intuitive path from "what is data" to "how does a robot decide" — the full pipeline is never shown end-to-end in an age-appropriate way

---

## Section C — Proposed Solution & Requirements

**Slide 8 — Section divider:** "Proposed Solution"

**Slide 9 — Proposed Solution**
- An educational game integrated into Ferid Around the World
- 7 interconnected mini-games covering the full AI workflow: data → vision → validation → robotics/decision-making
- Emphasis on **conceptual understanding through play**, not programming or math — tutorials, animations, visual feedback, progressive difficulty

**Slide 10 — Requirements (condensed)**
- 📋 table, pick ~5 functional + all 7 non-functional, e.g.:

| Functional (selected) | Non-Functional |
|---|---|
| Display world map & unlock progression | Usability — intuitive for ages 5–10 |
| Interactive tutorial before each activity | Performance — smooth scene transitions |
| Evaluate player actions, give immediate feedback | Reliability — stable, uninterrupted sessions |
| Validate level completion & save progress | Maintainability — modular architecture |
| Animations, sound, instructions during play | Scalability — new mini-games without rearchitecting |
| | Compatibility — Android via Unity |
| | Accessibility — visual + audio guidance |

**Slide 11 — General Use Case Diagram**
- Single actor: the child Player — launch, view map, select mini-game, view tutorial, play, receive feedback, unlock next
- Visual: 📷 `Fig1.3_General_Use_Case_Diagram` (already clean UML with `<<include>>`/`<<extend>>` — reuse as-is)

---

## Section D — System Architecture

**Slide 12 — Section divider:** "System Architecture & Technologies"

**Slide 13 — Overall System Architecture**
- Central **Game Map** (navigation hub) + **Progress Manager** (singleton, tracks locked/unlocked/finished) + **Save System** (Unity PlayerPrefs)
- Game Map gives access to the 7 independent mini-game modules — modular, easy to extend
- Visual: 📷 `Fig2.1_Overall_System_Architecture`

**Slide 14 — AI Learning Architecture (the pipeline)**
- Data Preparation → Computer Vision → AI System Validation → Intelligent Robotics
- Each stage maps directly to 1–2 mini-games (see Slide 5 table)
- Visual: 📷 `Fig2.2_AI_Learning_Architecture` (nice tree diagram, orange theme — reuse as-is)

**Slide 15 — Technologies Used**
- Unity (engine) · C# (language) · Visual Studio (IDE) · Krita (art) · Git/GitHub (VCS) · ClickUp (task tracking) · Slack (comms)
- Visual: grid of 📷 `Fig2.3` through `Fig2.10` logos (8 small logos, 2 rows of 4)

**Slide 16 — Agile Scrum & Team**
- 8 sprints, 16 weeks, 2 weeks/sprint, one mini-game (or the Game Map) delivered per sprint
- 📋 table (Table 2.2, condensed):

| Role | Member | Responsibility |
|---|---|---|
| Product Owner | Mouadh Mkadmi | Vision, backlog priority, validates features |
| Scrum Master | Yassine Alaoui | Process, technical guidance, unblocking |
| Development Team | Ranym Moussa | Design, implementation, testing, integration |

**Slide 17 — Sprint Planning Overview**
- 📋 table (Table 2.4, condensed) — one row per sprint, Goal + Weeks. Consider a simple 8-block timeline graphic instead of a full table if space is tight.

---

## Section E — Implementation

**Slide 18 — Section divider:** "Implementation"

**Slide 19 — Mini-Games Overview (1/2): the Data & Vision stage**
- **Data Gathering** — drag animals into correct categories (teaches: data collection matters)
- **Data Cleaning** — sort objects across levels/sub-levels of increasing difficulty (teaches: data must be cleaned before use)
- **Image Recognition Evaluation** — judge whether the robot's predictions are correct, watch its accuracy improve (teaches: verifying AI predictions)
- Visual: one screenshot each — 📷 `Fig3.3_Data_Gathering_gameplay_interface`, 📷 `Fig3.8_Data_Cleaning_gameplay_interface`, 📷 `Fig3.12_Image_Recognition_gameplay_interface`

**Slide 20 — Mini-Games Overview (2/2): Validation & Robotics**
- **Face Detection** — evaluate/correct the robot's face-detection square, watch it improve over 6 rounds (teaches: object localization)
- **AI Agent Debugging** — spot and bin faulty AI agents while keeping good ones (teaches: AI systems need debugging)
- **AI Decision Making** — place the right action card (Jump/Climb/Break Rock/Zipline) at each decision point (teaches: context-aware action selection)
- Visual: 📷 `Fig4.2_Face_Detection_gameplay_interface`, 📷 `Fig4.6_AI_Agent_Debugging_gameplay_interface`, 📷 `Fig4.12_Submit_selection_The_completed_program_r` ← ⚠️ filename says "Robot Path Planning" but this is actually the correct AI Decision Making screenshot (jump/climb/zipline cards + robot on the rope bridge) — see swap note

**Slide 21 — Flagship Deep-Dive: Robot Path Planning**
- The most complete mini-game: teaches path planning AND doubles as the demo surface for the voice-recognition module
- 4 levels × 2 rounds (first procedurally DFS-generated, second fixed) = 8 distinct mazes
- Learner builds a program by **dragging direction blocks or speaking commands**; robot "rolls until blocked" rather than moving cell by cell
- Visual: 📷 `Fig5.1_Activity_Workflow_AI_Decision_Making_A_f` + 📷 `Fig5.2_AI_Decision_Making_gameplay_interface` + 📷 `Fig5.3_Action_cards_arrangement` ← ⚠️ all three filenames say "AI Decision Making" but all three are actually the correct Robot Path Planning content (maze, up/down/left/right blocks, mic button, "Level 2" / "Play now") — see swap note

**Slide 22 — The Game Map: tying it all together**
- Central navigation: drone travels a fixed path, stops = mini-games
- Finishing a game → reward panel → drone flies to next stop → lock fades → progress saved (PlayerPrefs) → resumes exactly where left off next session
- Visual: 📷 `Fig5.7_Game_Map_interface_The_Game_Map_interfac` + 📷 `Fig5.5_Locked_and_unlocked_activities`

---

## Section F — The AI Voice-Recognition Module

**Slide 23 — Section divider + Intro**
- Beyond the simulated mini-games: a **real** AI component
- A CNN trained in PyTorch recognizes spoken English navigation commands, lets the child control the robot by voice in Robot Path Planning
- Positions the module: AI → Machine Learning → Deep Learning → Automatic Speech Recognition → Voice Command Classification (this project)
- Visual: 📷 `Fig2.11_Artificial_Intelligence_hierarchy_adopte`

**Slide 24 — VoiceCNN Architecture & Data**
- Input: 1-second clip, 16 kHz → log-mel spectrogram (98×40)
- Conv layers (feature extraction) → ReLU + Dropout → Fully Connected → 6-class softmax (up/down/left/right/unknown/silence)
- Trained on Google Speech Commands v0.02; CNN chosen over RNN/LSTM/Transformer for its real-time, low-compute fit with Unity
- Visual: 🖼 `diagrams/05-voicecnn-architecture.svg` (fills the missing Fig 2.12 — also drop this into the report) + 🖼 `diagrams/06-audio-preprocessing-pipeline.svg` (Fig 2.13, already exists too — 📷 `Fig2.13_Audio_preprocessing_pipeline` if you prefer your original)

**Slide 25 — Training & Deployment in Unity**
- PyTorch training (Adam, Cross-Entropy Loss) → best-validation-accuracy model retained → exported to **ONNX**
- Loaded via **Unity Sentis** (CPU backend) — fully offline inference, no network/GPU dependency
- Same mel-spectrogram pipeline re-implemented in C# (`MelSpectrogram.cs`) for train/inference parity
- Visual: 📷 `Fig2.14_Model_training_workflow` + 🖼 `diagrams/08-unity-integration-workflow.svg` (fills the missing Fig 2.15 — also drop this into the report)

**Slide 26 — Live Demo**
- Show: pick up a direction block by voice in Robot Path Planning, run the maze
- Backup: 🎥 record a 20–30s screen capture in advance in case of mic/room-noise issues during the defense

---

## Section G — Conclusion

**Slide 27 — Conclusion**
- Delivered a modular, 7-mini-game educational platform teaching the AI pipeline end-to-end to children 5–10
- Agile Scrum across 8 sprints / 16 weeks; Unity + C# + PyTorch/Sentis stack
- Real AI integration (not just simulated): CNN-based voice control, deployed fully offline

**Slide 28 — Future Perspectives**
- Multilingual speech recognition (Arabic, French)
- Larger/more diverse voice dataset, better noisy-environment robustness
- New gadgets/mechanics (per your Ferid Factory reference deck: elevator, laser-style additions could map here too if relevant)
- Child progress tracking; level creation/sharing tools

**Slide 29 — Thank You / Questions**

---

## Action items before you build the deck

1. **The entire Robot Path Planning ↔ AI Decision Making figure blocks are swapped in `report test.docx`.** Confirmed by opening every image in both blocks:
   - Figures captioned **4.9–4.12 ("Robot Path Planning")** actually show **AI Decision Making** content (workflow with decision points/action-card check; jump/climb/zipline platformer cards). Note Fig 4.10 and Fig 4.11 appear to be the exact same screenshot inserted twice.
   - Figures captioned **5.1–5.3 ("AI Decision Making")** actually show **Robot Path Planning** content (maze-generation/wall-collision workflow; up/down/left/right blocks + mic button in a maze scene).
   - Fix: re-caption or re-order these seven images so each block matches its real content — this is a factual error an examiner could easily catch since the body text right next to each figure describes the *correct* activity in detail.
2. **Insert the two genuinely missing figures (2.12, 2.15) into the report**, using the SVGs I made or your own redraw — right now the "List of Figures" promises them but no image was ever inserted for either.
3. Everything else in `Assets/Presentation Assets/report-figures/` is verified against its caption and ready to paste in as-is.
4. I verified ~13 of the 57 images directly (all logos/diagrams/use-case figures, the full Robot Path Planning / AI Decision Making block, and one screenshot each from three other mini-games) — the rest weren't individually opened, so give each a glance as you paste it in.
