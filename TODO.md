# TODO
This document contains a list of planned features for Rayman 3 Readvanced, in no particular order. Besides this there are also various TODO comments in the code which should be resolved and the main progress is documented over at [Progress](PROGRESS.MD). I'll gladly accept any help anyone would be willing to provide for this project!

## 📃 General
- The camera doesn't work as well on N-Gage when playing in widescreen due to it being optimized for a portrait aspect ratio. Fix by having a setting for the camera where it's either in GBA, N-Gage or Adaptive mode.
- Add option in the menu to convert save files between GBA and N-Gage as well as importing/exporting between Readvanced and emulators. The save data is the same, so should be easy.
- Some animations, like the cage icon and Scaleman shadow, have a gap between sprites that end up being less than 1 pixel and thus not normally visible in the original game. However due to how the scaling works here it becomes visible in higher resolutions. Find a way to fix this.
- Make sure the gameplay is the same for things like multiplayer, time trials etc. so that actor cycles and randomization stay consistent. Also disable debug features to prevent cheating.
- First time launching the game, before the title screen, should show options like language, controls and option preset.
- Add controller support. Maybe also haptic feedback on things like camera shake and bomb damage?
- Press A to skip menu transition like in Rayman M.

## 💬 Localization
- If you change the button mapping then the in-game tutorial texts are wrong, such as when Murfy or Ly explain how to perform a move. Find a way to replace this.
- Translate new text to the available languages. Currently all strings are hard-coded.

## 🧑‍💻 Code
- Move hard-coded primitive values to constant fields.
- Properly set up cross-platform support.

## 🎮 Multiplayer
Implementing local multiplayer, using multiple game instances (through named pipes) or through LAN, shouldn't be too hard. The game's multiplayer code is very simple, with it usually just sending a single 16-bit value between clients each frame.

It might also be possible to implement split-screen multiplayer where each player has its own Scene2D instance and viewport. The HUD should be re-done though and static things like sounds need to be correctly handled.

However online multiplayer would be much more complicated. The game expects the communication between clients every frame, which would require very low latency (probably around 16 ms?). If we can get it working then this library would be a potential option: https://github.com/RevenantX/LiteNetLib

## ⚙️ Options
### ⌚ Performance
- Option to pre-load all textures in animations and tiles when initializing a new Frame instance to avoid lazy loading.
- Option not to cache serialized data from the ROM. Currently it always does that.

### ✨ Optional improvements
- Buffered inputs for specific things like jumping (including for the walking shell).
- N-Gage specific features that could be enabled in the GBA version:
    - Allow skipping intro early.
    - After pausing in multiplayer it shows the countdown again.
    - Final boss missile and Hoodstormer move slower.
    - TimerBar sounds during last 10 seconds.
- Auto-pause if the game window is inactive.
- Fix the helico animation hitbox for Rayman.
- Move faster in worldmap (when holding down button?).
- Do not show N-Gage button prompts in the corners.
- Remove stray pixels and fix bad tiling in levels.
- More tutorial boxes from Murfy, explain strafing for race levels, how to damage Scaleman boss, blue lums not being usable yet etc.
- Standardized button mapping. This makes the menus use standardized buttons, and for the intro you can press any button to continue and not just the start button.
- Restore 2 hp from red lums instead of 1, name it "restore double hit-points".
- Disable flashing lights option. Use for race levels and lightning effect.
- Press the select button while in a hub world to bring up level info bars for every level for that hub in a vertical, scrollable, list, with you selecting one to teleport to that level curtain.
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

## ⭐ Bonus
### Achievements
Rewarded for things such as:
- Game progress:
    - Unlock new world.
    - Defeat boss.
    - All lums/cages in a world.
    - Beat the game.
    - Get the 1000th lum.
- Stats:
    - Defeat X number of X enemy.
    - Jump X number of times.
    - Move X distance in total.
- Specifics:
    - Complete level without defeating any enemies.
    - Complete level without taking any damage.
    - Ride a keg backwards.
    - Defeat an enemy with a keg or sphere.
    - Defeat Rocky without using the blue lum.
    - Collect all yellow lums in race during a single lap.
    - Beat Marshes of Awakening 1 without moving to the side (so only jumping).

### Time trials
List of time trials which you can play from different pre-selected levels. Probably not the entire levels since that doesn't sound fun. Have it be more like Rayman Origins where it's only part of a level.
- While in a level you have ways of freezing the timer. This can be special time freeze items we add, or it's from collecting lums and cages. This adds risk vs reward as you might want to go out of your way to find these. Perhaps collecting them all gives you a time bonus at the end, like how Crash Team Racing does?
- Finishing a time trial gives you either bronze, silver or gold. Show these next to each time trial in the list. You want to get all gold!
- Finishing a time trial saves your ghost (we can do that by saving Rayman's state, pos, anim etc., each frame). Upon replaying you can show the ghost. Perhaps the bronze, silver and gold requirements each also have ghosts which are pre-recorded?

### Interactive Playground
Unlocks once you get all achievements and is inspired by the Insomniac Museum areas. It's a big level which showcases unused features, bugs and other interesting details about the game. It has Murfy stones you walk up to for conversations with Murfy about the different things being shown.

### Mods
Allow you to install mods to the game by creating a folder for each mod which can they contain replaced textures, text, sounds etc. as well as new languages for fan-translations.

### Level editor
A level editor could be created where you can create your own levels using tilesets from the existing levels, or add your own tilesets. You could even just use a static texture for the level map, but collision has to always be tile-based.

The actors could be selected from a list, where each actor has a list of valid initial states it can be in.

### Cheat codes
It might be fun to include some cheat codes. These can be triggered by holding the select button while typing on the keyboard. Ideas:
- Type "PAL" to change the framerate to 50.
- Type something to make the 3D Mode7 walls comically tall.

## 🐞 Bugs
- Is keg collision wrong when flying? See Mega Havoc 2. It seems more strict than the original game.
- An exception gets thrown if closing the game while it's loading the rom.
- In levels where the cycles of objects matters, like the flying keg ones, it doesn't line up well if all objects are always loaded, making some parts impossible to beat
