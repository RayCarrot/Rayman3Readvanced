# TODO
This document contains a list of planned features for Rayman 3 Readvanced, in no particular order. Besides this there are also various TODO comments in the code which should be resolved and the main progress is documented over at [Progress](PROGRESS.MD). I'll gladly accept any help anyone would be willing to provide for this project!

## 📃 General
- Show icon with animation in the screen corner when saving.
- If you change the button mapping then the in-game tutorial texts are wrong, such as when Murfy or Ly explain how to perform a move. Can we automatically replace that so it works for all languages?
- The camera doesn't work as well on N-Gage when playing in widescreen due to the different values being used - have it work like on GBA instead and use those values?
- Option to convert save file between GBA and N-Gage. The save data is the same, so should be easy.
- Option to export/import saves to/from emulator saves.
- Run single frame when changing graphics to see changes? We can add an IsEnabled property to JoyPad which is false when the menu is showing to prevent inputs from registering on that frame.
- Move hard-coded values to constants. There are a lot of these in the game!
- Add a cheat menu?

## 🎥 Mode7
Mode7 is going to be very complicated to re-implement. The way it originally works on the GBA is that the background layer and sprites have a different affine transform set on each scanline of the screen. This produces the "depth" effect even though the GBA can normally only do linear transformations. Ideally we'd implement this by creating a matrix or shader which produces a similar effect. We could re-implement the same logic as the GBA uses, but it would probably be more complicated and wouldn't scale as nicely in higher resolution.

Some potential resources:

- https://github.com/MattDrivenDev/MonoGameMode7/blob/main/src/Mode7/Mode7Camera.cs (using code from this article: https://www.c-sharpcorner.com/uploadfile/8c85cf/game-components-affine-and-non-affine-transforms-in-windows-phone-7)
- https://www.coranac.com/tonc/text/mode7.htm (code and math tables available at: https://github.dev/devkitPro/libtonc/blob/master/include/tonc_math.h)
- https://discussions.unity.com/t/hedgehog-arts-gpu-mode7-previously-hard-coded-mode7-effect/606919
- https://gist.github.com/Jellonator/0686c6e74d06745957de5a96fa00ec6c

## 🎮 Multiplayer
Implementing local multiplayer, using multiple game instances or through LAN, shouldn't be too hard. The game's multiplayer code is very simple, with it usually just sending a single 16-bit value between clients each frame.

However online multiplayer would be much more complicated. The game expects the communication between clients every frame, which would require very low latency (probably around 16 ms?). If we can get it working then this library would be a potential option: https://github.com/RevenantX/LiteNetLib

## 🔊 Audio
The sound code currently uses the sound engine built-in to MonoGame. However it has several limitations, such as not supporting loop points, making it not viable for this projects. Because of this a new audio library has to be found, which supports the following features:

- WAV and XM playback
- Loop points
- Fading in/out
- Adjustable pitch
- Adjustable pan

Potential resources:
- https://community.monogame.net/t/2022-audio-libraries/18068/5
- https://solhsa.com/soloud
- https://www.ambiera.com/irrklang
- https://github.com/naudio/NAudio

## ⚙️ Options
### 📃 General
- Have option presets, such as "Modern" and "Original".
- Show a tooltip for each option when it is selected. Some say "A custom value can be set in the config.json file" as to make that more clear.
- Option to enable debug features. This allows you to toggle the debug mode, use debug cheats etc. The collision tileset should only be loaded if this option is enabled.
- Make clear internal resolution is not the resolution the game renders at. We use high res rotation, subpixel positions etc. which means the game takes advantage of a higher rendering resolution. It also doesn't effect menus and such which are forced to the original resolution.

### ⌚ Performance
- Option not to clear cache between Frame instances.
- Option to pre-load all textures in animations and tiles when initializing a new Frame instance.
- Option not to cache serialized data from the ROM. Currently it always does that.
- Option to pre-load all levels asynchronously during intro sequence.
- Use single texture sheets for AnimatedObject and maybe tiles too as to avoid creating too many textures.

### ✨ Optional improvements
The following are ideas for optional improvements which the player can toggle on to enhance/modernize the game experience:

- Play unused level start animation.
- Infinite lives.
- Fix the helico animation hitbox for Rayman.
- Slightly increase hitbox width for moving platforms.
- Move faster in worldmap (when holding down button?).
- Press the select button while in a hub world to bring up level info bars for every level for that hub in a vertical, scrollable, list, with you selecting one to teleport to that level curtain.
- Kyote time, allow jumping after a few frames (game already has a system for it, but only used for specific cases such as moving platforms that burn up).
- Have enemies, such as the helico bombs in the waterski levels, not instakill you.
- Have tiles, such as the spikes in the cave of bad dreams, not insta-kill you. Allow you to stand on them when on i-frames?
- Restore original Rayman 2 level names.
- Option to check for buffered inputs? Pass in buffer length in the JoyPad check methods? For jumps, attack etc. as to avoid it feeling like inputs get lost.

## ⭐ Bonus
### Achievements
Rewarded for things such as:
- Game progress (finish world, beating boss, unlocking new power etc.).
- Completion (all lums/cages in word, 100% game etc.).
- Gameplay (defeat enemies, performing common actions like jumping etc.).
- Specifics (complete a level in a special way, such as not defeating any enemies or not using purple lums etc.).

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

### Mods
Allow you to install mods to the game by creating a folder for each mod which can they contain replaced textures, text, sounds etc.

### Level editor
A level editor could be created where you can create your own levels using tilesets from the existing levels, or add your own tilesets. You could even just use a static texture for the level map, but collision has to always be tile-based.

The actors could be selected from a list, where each actor has a list of valid initial states it can be in.

## 🐞 Bugs
- Is keg collision wrong when flying? See Mega Havoc 2. It seems more strict than the original game.
