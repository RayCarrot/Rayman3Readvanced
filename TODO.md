# TODO
This document contains a list of planned features for Rayman 3 Readvanced, in no particular order. Besides this there are also various TODO comments in the code which should be resolved. I'll gladly accept any help anyone would be willing to provide for this project!

## 📃 General
- The camera doesn't work as well on N-Gage when playing in widescreen due to it being optimized for a portrait aspect ratio. Fix by having a setting for the camera where it's either in GBA, N-Gage or Adaptive mode.
- First time launching the game, before the title screen, should show options like language, controls and option preset.
- Use Game Jolt API for achievements and leaderboards.
- If the screen resolution is not 16:9 then there should be an additional resolution option matching that aspect ratio.
- Instead of downscaling the menu font we should create separate, smaller, version. Same in other places. This is to keep sprites rendering in the same resolution for an as authentic feel as possible.

## 💬 Localization
- If you change the button mapping then the in-game tutorial texts are wrong, such as when Murfy or Ly explain how to perform a move. Find a way to replace this.
- Translate new text to the available languages. Currently all strings are hard-coded.
- Make sure all menus work in all languages, so the text doesn't overflow

## 🧑‍💻 Code
- Move hard-coded primitive values to constant fields.
- Optimize BinarySerializer more. Pointers should ideally be structs instead of classes in order to reduce allocations. We could also serialize animation channels as a `ushort[]` which saves on a lot of allocations. Analyze with VS profiler to see where allocations happen and check with BenchmarkDotNet. 
- Try and reduce the number of allocations per frame as much as possible.

## 🎮 Multiplayer
Implementing local multiplayer, using multiple game instances (through named pipes) or through LAN, shouldn't be too hard. The game's multiplayer code is very simple, with it usually just sending a single 16-bit value between clients each frame.

It might also be possible to implement split-screen multiplayer where each player has its own Scene2D instance and viewport. The HUD should be re-done though and static things like sounds need to be correctly handled.

However online multiplayer would be much more complicated. The game expects the communication between clients every frame, which would require very low latency (probably around 16 ms?). If we can get it working then this library would be a potential option: https://github.com/RevenantX/LiteNetLib

## ⚙️ Options
### ⌚ Performance
- Option to pre-load all textures in animations and tiles when initializing a new Frame instance to avoid lazy loading.
- Option not to cache serialized data from the ROM. Currently it always does that.

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

### Level editor
A level editor could be created where you can create your own levels using tilesets from the existing levels, or add your own tilesets. You could even just use a static texture for the level map, but collision has to always be tile-based.

The actors could be selected from a list, where each actor has a list of valid initial states it can be in.

### J2ME version
Potentially include a remade version of the J2ME game as a bonus. It's a simple game to decompile since one of the versions has full debug symbols.

### Cheat codes
It might be fun to include some cheat codes. These can be triggered by holding the select button while typing on the keyboard. Ideas:
- Type "PAL" to change the framerate to 50.
- Type something to make the 3D Mode7 walls comically tall.

## 🐞 Bugs
- An exception gets thrown if closing the game while it's loading the rom.
- In levels where the cycles of objects matters, like the flying keg ones, it doesn't line up well if all objects are always loaded, making some parts impossible to beat
- When all object are active it causes issues with cycles being different. This is noticeable for the walking shell sections, the precipice and the flying keg sections. Possibly for the waterski levels too.
- Saved window position and size might become invalid, such as becoming negative. Auto fix if wrong.
- When running on integrated graphics in a WindowsDX build it seems to have minor graphics glitches like screen tearing (noticeable in the scrolling clouds background of the first level).