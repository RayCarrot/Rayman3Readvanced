# TODO
This document contains a list of planned features for Rayman 3 Readvanced, in no particular order. Besides this there are also various TODO comments in the code which should be resolved. I'll gladly accept any help anyone would be willing to provide for this project!

## 📃 General
- Use Game Jolt API for achievements and time attack leaderboards.
- If the screen resolution is not 16:9 then there should be an additional resolution option matching that aspect ratio.
- Add license file with licenses for all libraries used.
- Improve readme. Make it more user-friendly, showcase new features like J2ME and achievements etc. Also for more technical details include build instructions (specifically for the native library which gets built to a nuget package).

## 💬 Localization
- If you change the button mapping then the in-game tutorial texts are wrong, such as when Murfy or Ly explain how to perform a move. Find a way to replace this. Perhaps instead of showing the input as text we show an icon of the key/button?
- Translate new text to the available languages. Currently all new strings are hard-coded, so it should be moved to a new text bank.
- Make sure all menus work in all languages, so the text doesn't overflow.

## 🧑‍💻 Code
- Move hard-coded primitive values to constant fields. For actors this should be in ActorName.Consts.cs and will also help reduce allocations when boxing objects.
- Move hard-coded data to config files, such as song tables, achievements, Mode7 camera values and Rayman 3 joypad mapping

## 🧪 Unit tests
- Add tests for core engine functionality
- Add test for engine libraries
- Add tests for data loading
- Add tests for game behavior

## 🎮 Multiplayer
Implementing local multiplayer, using multiple game instances (through named pipes) or through LAN, shouldn't be too hard. The game's multiplayer code is very simple, with it usually just sending a single 16-bit value between clients each frame.

It might also be possible to implement split-screen multiplayer where each player has its own Scene2D instance and viewport. The HUD should be re-done though and static things like sounds need to be correctly handled.

However online multiplayer would be much more complicated. The game expects the communication between clients every frame, which would require very low latency (probably around 16 ms?). If we can get it working then this library would be a potential option: https://github.com/RevenantX/LiteNetLib

## ⚙️ Options
### ⌚ Performance
- Option to pre-load all textures in animations and tiles when initializing a new Frame instance to avoid lazy loading.
- Options for if cache should be cleared when loading new frame? Or maybe not since it'd cause huge memory usage?

### ✨ Optional improvements
- Extend backgrounds so that they can render in the modern widescreen resolution without scaling:
    - BossMachine ✔️
    - BossRockAndLava
    - BossFinal_M1
    - BossFinal_M2
    - Power2
    - Power5
    - Power6
    - Worldmap
    - Check multiplayer, menus etc. and check N-Gage
- Make 60fps transitions optional. A lot of the ones in the game run at a lower framerate (by checking `GameTime.ElapsedFrames`) and currently we have it hard-coded to run them all in 60fps instead.

## ⭐ Bonus
### Interactive Playground
Unlocks once you get all achievements and is inspired by the Insomniac Museum areas. It's a big level which showcases unused features, bugs and other interesting details about the game. It has Murfy stones you walk up to for conversations with Murfy about the different things being shown.

### Mods
Allow you to install mods to the game by creating a folder for each mod which can they contain replaced textures, text, sounds etc. as well as new languages for fan-translations.

### J2ME version
Potentially include a remade version of the J2ME game as a bonus. It's a simple game to decompile since one of the versions has full debug symbols.

### Cheat codes
It might be fun to include some cheat codes. These can be triggered by holding the select button while typing on the keyboard. Ideas:
- Type "PAL" to change the framerate to 50.
- Type something to make the 3D Mode7 walls comically tall.

## 🐞 Bugs
- An exception gets thrown if closing the game while it's loading the rom.
- Saved window position and size might become invalid, such as becoming negative. Auto fix if wrong.
- When running on integrated graphics in a WindowsDX build it seems to have minor graphics glitches like screen tearing (noticeable in the scrolling clouds background of the first level).
- Some backgrounds scroll too much, making it rather dizzying for the player. For example `SanctuaryOfStoneAndFire_M3` and `GameCube_Bonus2`.
- Changing settings on the current audio device causes a bug where sounds don't play. Any new sounds get stuck on 0 and never advance, meaning sound instances keep building up and the game soft-locks when waiting for if a sound has finished. One solution is to call `deinit` and then `init` on the SoLoud instance when this bug occurs (perhaps detect it if a sound stream position has not changed from the previous frame), however this also invalidates all sound instances meaning we have to restart the music. Another option is to switch audio backend from the default `MiniAudio` one to `SDL2` as this does not have the bug. However the WindowsDX builds do not come with the SDL dll file, so we need to include that then.

## 🧪 Testing
- Make sure audio panning works as intended for all actors.
- Test Readvanced alongside the original game. Perhaps even creating recording files to play back between both games.
