# 🃏 Scripts of Tribute – Unity GUI Client

A Unity-based graphical client for playing **Scripts of Tribute** – This tool is designed to:

- play against your own C# bots compiled as DLLs,
- play against older legacy bots,
- connect to external bots over gRPC (e.g., written in Python),
- visualize and debug matches with a rich UI.

Game view:
![](Docs\screenshots\GameView.png)

Choice panel view:
![](Docs\screenshots\ChoicePanel.png)

Summary at the end of the game:
![](Docs\screenshots\GameEnd.png)

---

## ⚠️ Note: Missing Required Assets

This repository **does not include** all assets required to run the game in Unity.

These missing assets include:
- Music by Monument Studios
- GUI assets by Layer Lab
- many more

These are under a paid license I own and **will not be redistributed**. If you are a collaborator or reviewer, please contact me directly and confirm license ownership before requesting access.

---

## 🤖 Adding Your Own Bot (C#)

1. Implement a bot class inheriting from `AI`, and override all the necessary methods

2. Compile your bot as a `.dll` file.

3. Drop the DLL into the following folder: `Scripts of Tribute 2.0_Data/StreamingAssets/Bots`

4. It will appear in the Unity client’s bot selection panel.

![Panel](Docs\screenshots\GameSetup.png)

---

## 🔌 Connecting a Python Bot (via gRPC)

1. Install the official Python bot [package](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Python)
2. Launch the python bot in separate process, in terminal on client port 50000 and server port 49000 (default settings)
3. In Unity, select GrpcBot as your bot.
4. The client communicates with your bot using an intermediate process (GrpcBotProxy.exe) which you can get [here](https://github.com/Ematerasu/GrpcProxyHost) and MessagePack-encoded game states. Either download executable file from Releases section or compile the project yourself.
5. The executable GrpcProxy should be placed in `Scripts of Tribute 2.0_Data/StreamingAssets`

## ⚙️ Features
* Drafting phase and full 1v1 match flow

* Tavern, Hand, Agent zones, and all game visuals

* Card choice panels and interactive UI

* Sound, music, and screen resolution options

* Card tooltips and Patron icons

* Visual diffs between game states for smooth updates

* Manual seed control for reproducible AI matches (deterministic testing)

## 🧪 For Bot Developers
* Use AI Move or AI Move Whole Turn buttons to test bot decisions.

* View step-by-step breakdowns of each move and resulting state.

* Debug logs and diffs let you trace visual changes with precision.

## 📌 License
This is a development/debug tool intended for **educational and experimental use only**. This project is **non-commercial** and is not distributed for profit. All rights to Tales of Tribute and related assets belong to ZeniMax Online Studios / Bethesda.

## 🔗 Useful Links
[ScriptsOfTribute C# Engine](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Core)

[gRPC Python Bot Package](https://github.com/ScriptsOfTribute/ScriptsOfTribute-Python)

[Game Rules (EsoHub)](https://eso-hub.com/en/guides/tales-of-tribute-guide)
