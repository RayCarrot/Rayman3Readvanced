# TODO
This document contains a list of planned features for Rayman 3 Readvanced, in no particular order. Besides this there are also various TODO comments in the code which should be resolved and the main progress is documented over at [Progress](PROGRESS.MD). I'll gladly accept any help anyone would be willing to provide for this project!

## 📃 General
- If you change the button mapping then the in-game tutorial texts are wrong, such as when Murfy or Ly explain how to perform a move. Find a way to replace this.
- The camera doesn't work as well on N-Gage when playing in widescreen due to it being optimized for a portrait aspect ratio. Fix by having a setting for the camera where it's either in GBA, N-Gage or Adaptive mode.
- Add option in the menu to convert save files between GBA and N-Gage as well as importing/exporting between Readvanced and emulators. The save data is the same, so should be easy.
- Move hard-coded values to constants. There are a lot of these in the game!
- Add a cheat menu?
- When rendering in a resolution that's not a factor of the internal resolution then the sprites in animations don't always align correctly and there may be a 1 pixel gap. Fix by always rendering to the highest possible factor of the internal resolution and then scaling to the actual resolution?
- Translate new text to the available languages.
- Make sure the gameplay is the same for things like multiplayer, time trials etc. so that actor cycles and such stay the consistent. Also disable debug features then to prevent cheating.

## 🎮 Multiplayer
Implementing local multiplayer, using multiple game instances (through named pipes) or through LAN, shouldn't be too hard. The game's multiplayer code is very simple, with it usually just sending a single 16-bit value between clients each frame.

It might also be possible to implement split-screen multiplayer where each player has its own Scene2D instance and viewport. The HUD should be re-done though and static things like sounds need to be correctly handled.

However online multiplayer would be much more complicated. The game expects the communication between clients every frame, which would require very low latency (probably around 16 ms?). If we can get it working then this library would be a potential option: https://github.com/RevenantX/LiteNetLib

## ⚙️ Options
### ⌚ Performance
- Option to pre-load all textures in animations and tiles when initializing a new Frame instance to avoid lazy loading.
- Option not to cache serialized data from the ROM. Currently it always does that.

### ✨ Optional improvements
- Buffered inputs.
- N-Gage specific features that could be enabled in the GBA version:
    - Allow skipping intro early.
    - After pausing in multiplayer it shows the countdown again.
    - Final boss missile and Hoodstormer move slower.
    - TimerBar sounds during last 10 seconds.
- Fix the helico animation hitbox for Rayman.
- Move faster in worldmap (when holding down button?).
- Do not show N-Gage button prompts in the corners.
- Remove stray pixels and fix bad tiling in levels.
- More tutorial boxes from Murfy, explain strafing for race levels etc.
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
- Game progress (finish world, beating boss, unlocking new power etc.).
- Completion (all lums/cages in word, 100% game etc.).
- Gameplay (defeat enemies, performing common actions like jumping etc.).
- Specifics (complete a level in a special way, such as not defeating any enemies or not using purple lums, riding a keg backwards, defeating an enemy with a keg/sphere etc.).

### Time trials
List of time trials which you can play from different pre-selected levels. Probably not the entire levels since that doesn't sound fun. Have it be more like Rayman Origins where it's only part of a level.
- While in a level you have ways of freezing the timer. This can be special time freeze items we add, or it's from collecting lums and cages. This adds risk vs reward as you might want to go out of your way to find these. Perhaps collecting them all gives you a time bonus at the end, like how Crash Team Racing does?
- Finishing a time trial gives you either bronze, silver or gold. Show these next to each time trial in the list. You want to get all gold!
- Finishing a time trial saves your ghost (we can do that by saving Rayman's state, pos, anim etc., each frame). Upon replaying you can show the ghost. Perhaps the bronze, silver and gold requirements each also have ghosts which are pre-recorded?

### Challenges
List of challenges you can play. These put you into a level and has you attempting to beat the challenge. Some ideas:
- Beat level without taking any hits (have this for harder levels, such as Prickly Passage, or bosses).
- Beat level with Dark Rayman chasing you (this would work well for levels where you have to change direction a lot, such as first section of Vertigo Wastes).
- Beat level with darkness effect from Rayman 1.
- Beat level in reverse (start at the end and make your way back to the beginning).
- Beat Rock and Lava Boss without using blue lum (using damage boosts).
- Beat level while playing as Murfy (new gameplay style where you fly).
- Collect all yellow lums in race during one lap.
- Beat Marshes of Awakening 1 without moving to the side (so only jumping).
- Find hidden collectible in level.

### Mods
Allow you to install mods to the game by creating a folder for each mod which can they contain replaced textures, text, sounds etc. as well as new languages for fan-translations.

### Level editor
A level editor could be created where you can create your own levels using tilesets from the existing levels, or add your own tilesets. You could even just use a static texture for the level map, but collision has to always be tile-based.

The actors could be selected from a list, where each actor has a list of valid initial states it can be in.

## 🐞 Bugs
- Is keg collision wrong when flying? See Mega Havoc 2. It seems more strict than the original game.
- An exception gets thrown if closing the game while it's loading the rom.
- In levels where the cycles of objects matters, like the flying keg ones, it doesn't line up well if all objects are always loaded, making some parts impossible to beat
