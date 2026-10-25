# Defense Speech — Spoken Script (matches an updated 42-slide version of `Ai  teaching game presentation.pptx`)
### Simple words, short sentences, one idea per slide. Read it a few times, don't memorize word-for-word.

**Before you present, fix these things in the deck (your pptx is still the 43-slide version — match it to this):**
1. Delete the old "Why Teach AI to Children?" slide (was slide 5) — its content is now part of Problem Statement (new slide 7). Every slide after it moves down by one number.
2. Old slide 10 said "ages 5–10" — that text is gone now; the merged Problem Statement (slide 7) says "6–12" once, matching the other slides.
3. Slides 33 ("Validation Loss") and 34 ("Validation Accuracy") are charts with no numbers I can read — write in your real numbers in the two bracketed spots below before you rehearse, or you will hesitate on the one number the jury always asks for.

---

**[Slide 1 — Title]**
Good morning. I am Ranym Moussa. This is my final-year project — an AI game for learning, built as a new part of Horizon Education's app, Ferid Around the World.

**[Slide 2 — Plan]**
Ten parts: context, problem, solution, requirements, how we worked, system design, architecture, the seven mini-games, voice recognition, and conclusion.

**[Slide 3 — Chapter: General Context]**
Let's start with the context.

**[Slide 4 — Host Company]**
This project was built at Horizon Education, a Tunisian company in Sousse that makes learning apps. It started in 2020. Their app is called Ferid Around the World, made for children.

**[Slide 5 — Existing Solutions & Their Limits]**
Other tools are not enough: Machine Learning for Kids needs coding skills, AI for Oceans teaches only one thing, and Quick Draw shows recognition but not how it works. None show the full path from data to a decision.

**[Slide 6 — Chapter: Problem Statement]**
Which brings us to the problem.

**[Slide 7 — Problem Statement]**
AI is already used in health, transport, and education — but most learning material needs coding or math first. There is no easy path for a child aged 6 to 12 to understand the full process. Tools are separate, and none show the full path from data to a decision.

**[Slide 8 — Chapter: Proposed Solution]**
Here is how we solved it.

**[Slide 9 — An AI Curriculum, Disguised as a Game]**
With Horizon Education, we made the AI Learning Journey: seven mini-games. Each one is a real AI step, turned into play. A world map connects them — finish one, unlock the next. No code, no math, just the steps: collect, clean, recognize, check, debug, plan, decide.

**[Slide 10 — The AI Pipeline the Child Walks Through]**
This is the base of the project: four real steps to build AI — preparing data, computer vision, checking results, and robotics. Each mini-game belongs to one of these steps.

**[Slide 11 — Chapter: Requirements Specification]**
A quick word on requirements.

**[Slide 12 — Requirements]**
What it must do: a world map that unlocks step by step, tutorials, fast feedback, and saved progress. How it must feel: easy for young children to use, and built so more AI ideas can be added later.

**[Slide 13 — Chapter: Methodology]**
A word on how the project was run.

**[Slide 14 — Agile Scrum]**
The project used Agile Scrum — short work periods called sprints, each one adding a new, tested part of the game, with normal steps: planning, daily check-ins, review, and looking back to improve.

**[Slide 15 — Chapter: System Design]**
Before writing code, the system was planned using UML diagrams, so the design would still work well as more mini-games were added.

**[Slide 16 — General Use Case Diagram]**
There is one user: the child. The main action is picking and playing a mini-game, which always has a tutorial, feedback, and save/unlock steps. The six ways of playing are just different versions of that same main action, depending on which game is picked.

**[Slide 17 — General Class Diagram]**
Each of the seven mini-games is its own separate class, with no shared parent class — this was done on purpose, so each game could be built and tested alone. The one shared part is `GameFinishTrigger`, which tells a central `GameProgressManager` when a level is finished.

**[Slide 18 — Chapter: System & AI Architecture]**
So how is this actually built?

**[Slide 19 — Overall System Architecture]**
In the center is the Game Map, which handles moving between games. A Progress Manager keeps track of locked, unlocked, and finished levels, saved using Unity's PlayerPrefs. Every mini-game works on its own, but each one tells this same shared system when it is finished.

**[Slide 20 — Technology Stack]**
The game is built in Unity with C#. PyTorch was used to train the voice model. Unity Sentis runs it inside the game, without internet. Krita was used for the art, and Git to save code versions.

**[Slide 21 — Chapter: Implementation]**
Now the main part of the project — seven mini-games, each one hiding a real AI idea.

**[Slide 22 — Data Gathering]**
The child drags animals into the right groups. This teaches that every AI system starts with data, and a model is only as good as the data used to train it: bad data in, bad results out.

**[Slide 23 — Data Cleaning]**
The child sorts objects, and each round starts with some items already in the wrong place. This is like real data — it has mistakes and must be cleaned before it can train a model.

**[Slide 24 — Image Recognition Evaluation]**
The robot names what it sees in pictures, and the child says if it is right or wrong. This is what checking a model's answers looks like in real machine learning. The robot's score goes up or down based on what the child says.

**[Slide 25 — Face Detection]**
The robot draws a box around what it thinks is a face. The child says yes, or moves the box to the right place. A model must first find something before it can know what it is — this step is called finding the object. Fixing the box is like a real method used in AI: a person helps correct the AI's mistakes.

**[Slide 26 — AI Agent Debugging]**
The child removes the broken AI agents from a group, and keeps the good ones. AI systems can fail without any warning, so someone must always check them. Missing a real problem counts as a bigger mistake than a false alarm.

**[Slide 27 — Robot Path Planning]**
The child guides a robot through a maze, either by dragging direction blocks or by saying the commands out loud. This teaches path planning, a well-known AI problem. This is also where the real, trained AI model is used — more on that next.

**[Slide 28 — AI Decision Making]**
The child places the right action card at each decision point on a fixed path. The same path can need different actions depending on the situation — this is the idea behind how a real AI system decides what to do.

**[Slide 29 — The Game Map]**
This connects everything: a moving drone travels to one stop for each mini-game. When a game is finished, the drone flies to the next stop, the lock disappears, and progress is saved by itself.

**[Slide 30 — Chapter: Voice Command Recognition]**
Everything shown so far only acts like AI, to teach the idea. What comes next is different — this is a real, trained AI model, running inside the game.

**[Slide 31 — Where This Fits]**
This is added into Robot Path Planning: instead of dragging blocks, the child can speak commands to control the robot with their voice.

**[Slide 32 — Why CNN]**
Three types of AI models were compared: CNN, LSTM, and Transformer. They were checked on accuracy, speed, how much data they need, and how much power they use. CNN was the best in all four — very important since this runs on a phone, inside a game, not on a big server.

**[Slide 33 — Validation Loss]**
This is the loss chart from training — [say your final loss number and what it shows, for example: "it goes down slowly and becomes flat, which means the model learned well"].

**[Slide 34 — Validation Accuracy]**
And this is the accuracy chart — [say your final accuracy number here].

**[Slide 35 — The Task & The Data]**
This is a six-word classification task: up, down, left, right, plus unknown and silence. These last two let the model say "I'm not sure" instead of guessing. It was trained using Google Speech Commands, a dataset with thousands of recordings from many different people.

**[Slide 36 — Training]**
The data was split into two parts: one to train the model, one to test it. Training used the Adam method and cross-entropy loss to measure mistakes. The best version of the model was saved and exported to ONNX format.

**[Slide 37 — Deployment in Unity]**
Inside Unity, the ONNX model runs using Sentis, fully offline, using only the CPU. The recorded voice is turned into the same picture format used during training, then checked by the model. If the model is not sure, the answer is thrown away. If it is sure, the command is added to the robot's steps, just like a block dragged by hand.

**[Slide 38 — Live Demo]**
Now, a live demo — a direction word said out loud, and the robot moves in the maze.

**[Slide 39 — Chapter: Conclusion & Perspectives]**
To finish.

**[Slide 40 — Conclusion]**
The goal was to give children a real, easy path through the full AI process, not just one small part — and the seven mini-games do that. This is backed by a real AI system that actually works: an offline voice model, built with a CNN, that a child can talk to.

**[Slide 41 — Future Perspectives]**
Next steps: adding Arabic and French to the voice recognition, using a bigger and more varied voice dataset so it works better with noise, and adding more AI ideas to grow the pipeline.

**[Slide 42 — Thank You / Questions]**
Thank you for your attention — I'd be happy to answer any questions.
