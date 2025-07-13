# Engine documentation
The original game uses [Ubisoft's GbaEngine](https://raymanpc.com/wiki/en/GbaEngine). It was originally developed by Ubisoft Milan for the GBA game *Rainbow Six - Rogue Spear* and written in C. The engine was based on their original GBC engine (which was written in assembly), with several of the tools from it initially being reused before new ones were developed. The engine ended up mainly being used by Ubisoft Milan, Ubisoft Montreal and Ubisoft Shanghai, with each of the different studios making their own changes to it. This has caused there to be three separate branches of the engine which got updated separately from each other. Rayman 3 uses Ubisoft Montreal's engine, which this document focuses on.

Several late-cycle GBA games of this engine were also released on the Nintendo DS in an updated version of the engine. Later games would be released exclusively on the Nintendo DS and also included full 3D support. Leftover developer text seems to indicate that this updated engine was part of the Onyx engine, although the connection to it is unclear.

## Engine structure
This is the structure of the original engine, as seen in the version of it used by Rayman 3. Later versions, and ones in different branches have several differences.

```
├───GbaCommon
│   ├── GbaSDK
│   └── GbaStdLib
├───GbaSimilar
│   ├── GbaAnimation
│   ├── GbaTileGraphics
│   ├── Gba2DPlatform
│   └── Gba3d
└───GbaSpecific
    └── Rayman2
```

Mockups of the source code file structures for all dated Rayman 3 prototypes, as well as other known prototypes for the same engine, have been made under [structure](structure).

### GbaSDK
This project has reusable code for any GBA game, such as display drivers, a graphics engine for allocating tiles in an optimized way and network code for multiplayer.

It also includes the storage system used by this engine. All of the main assets for a game are stored as resource blocks in a data table. Each resource has a list of dependencies, which are references to additional resources. [BinarySerializer.Ubisoft.GbaEngine](https://github.com/BinarySerializer/BinarySerializer.Ubisoft.GbaEngine) is being used to deserialize this data from the ROM.

Additionally the `FrameManager` is located here. This manages the main game loop, which in term is determined by the current `Frame` instance. A Frame is a class which provides a `Step` function, which is called once per frame. It also has the `Init` and `UnInit` functions for when switching the current Frame instance.

### GbaStdLib
This project mainly provides a memory manager for allocating data and sound code for playing audio.

### GbaAnimation
Here the animation player code is located, as well as classes for managing sprites in VRAM, such as `AnimationSpriteManager` and `AnimationPaletteManager`.

Each animation instance is managed by an `AnimatedObject` class instance. Additionally there are several variants of this class which provide different features, such as affine rendering, hit boxes and displacement vectors.

### GbaTileGraphics
This project provides the `Playfield` classes for rendering tile graphics, either in 2D (`TgxPlayfield2D`) or as Mode7 (`TgxPlayfieldMode7`). Each Playfield has a collection of game layers, which are either graphical tile layers or physical layers (for collision). Besides this, additional effects can be applied, such as animated tiles using an `AnimatedTileKit` and palette swapping. The latter is however not used in Rayman 3.

There also exists camera classes for 2D (`TgxCamera2D`) and Mode7 (`TgxCameraMode7`) which handle scrolling the game layers. In 2D the layers are grouped into `Clusters`, each with a different scroll speed, thus allowing layers to produce a parallax scrolling effect.

### Gba2DPlatform
This project contains components for a 2D platformer, such as scenes, actors, knots and more.

The main class is `Scene2D` which is generally used to contain a "level" in the game. This consists of a Playfield, a `CameraActor`, a collection of `GameObjects` and `Dialogs`. What follows is a description of each of these types.

#### GameObjects
Every `Scene` has a collection of GameObjects. These come in two different types, either as `Actors` or as `Captors`. 

An actor is an object which has an `AnimatedObject` and a finite-state machine (`FSM`) for managing its state. They can inherit from different base classes which give them different properties. For example, an actor inheriting from `ActionActor` can use `Actions` defined in the game data, which in turn indicate which animation is to be played and optionally has a `MechModel` with movement data. The movement data is however only used if the actor inherits from `MovableActor`.

Additionally, actors are grouped based on their life-time. `Normal actors` have defined activation zones, which in the game is determined by the `Knots`. These act as sectors, defining which objects are active where in the scene. `Always actors` are actors which remain active until manually disabled. These are usually used for objects which spawn from other objects.

Besides this there are also special types of actors. The `main actor` is always defined as the first actor in the scene (as an always actor since it's always active), or as the first four actors if in multiplayer. In this game it is usually the `Rayman` actor, unless the level has a different play style. `Projectile actors` are actors which are spawned by other actors. An example is the water splash from the piranhas. These are usually defined as always actors, but not all the time.

The other type of GameObject is a captor. This is an object which defines a collision box which triggers events when an actor collides with it. These usually involve enabling another object or playing a sound.

#### CameraActor
The `CameraActor` in each scene is responsible for managing the in-game camera and ultimately scroll the TgxCamera. There are different types of camera actors for different gameplay styles. For example the worldmap in Rayman 3 has a different implementation than the normal sidescroller levels. The same goes for the Mode7 levels.

#### Dialogs
Each scene has a collection of `Dialogs`. These are UI elements which display on top of the main game, such as the HUD, timers or the pause menu. When adding a dialog it can be set to display as a modal, thus deactivating the previously added ones. This is done for the pause menu.

#### States and messages
Most objects in the game, such as actors, dialogs and cameras, communicate by having a `ProcessMessage` function. This allows it to retrieve a message type, identified by an id, a sender and an optional parameter.

Most objects also contain a finite-state machine (`FSM`) which runs each frame. This manages the current state the object and which behavior it should perform.

### Gba3d
This project provides support for real 3D rendering on the GBA, through software (since the GBA has no hardware support for 3D). There is a basic animation system accompanying it and a hierarchy system for meshes. In Rayman 3 this is used to render the spinning wheel in the credits. Besides this it doesn't appear to have been used in any other games.

### Rayman2
This is where the game-specific code is located, such as the levels, actors, dialogs, game info and more. The project is internally named "Rayman 2" since that's what the game started development as. This port however uses the name "Rayman 3" to avoid confusion.

## Changes from the original code
What follows are the most notable code changes which were made in this port.

### Fewer singletons
The original game relies on a lot of global singletons for instances of types such as `Scene2D`, `TgxPlayfield` and `GameInfo`. In the N-Gage version these have been replaced by a factory system where singleton instances have an ID and are retrieved by calling a function.

In this port the way singletons are handled has been changed. The goal has been to reduce the amount of static properties to be able to separate the instance data between the different `Frame` instances. Making the classes static would also not always work due to inheritance (for example, `TgxPlayfield` can be of type `TgxPlayfield2D` or `TgxPlayfieldMode7`).

Because of this instances which were originally singletons have been changed in different ways depending on how the different types are used. `Scene2D` and `TgxPlayfield` are now owned by the current `Frame` and passed in to methods as needed. The `GameInfo` class is made static and `JoyPad` has an underlying singleton instance of `SimpleJoyPad` which is accessed through static methods.

### Floating-point arithmetic
The GBA has no support for floating-point arithmetic. The way the game works around this is by using 32-bit fixed-point arithmetic instead, with a point position of 16.

This has been changed in this port to instead use the `float` type in C#. This works better with MonoGame and allows for common types such as `Vector2` to be used. Additionally the game often has to cast its fixed-point values to standard integers which is not done in this port. Because of this there might be very slight gameplay changes due to value precision differences.

### Improved smoothness
Several minor changes have been made to improve the smoothness of animations and movements to make the game look better in a higher resolution.

For example, if the game moves a sprite by 1 pixel every two frames then this port has it changed to moving by 0.5 pixels every frame. If this is set to be rendered in the original GBA resolution then there won't be a difference, but if rendering in a higher resolution then this half-pixel will scale to be one or more real pixels which the sprite moves to on this in-between frame, making it appear smoother.

There are also other cases where the game limits the amount of work it does on a single frame to reduce lag. For example, when the game fades the screen by modifying the palettes it does so by every frame alternating between modifying the object and background palettes. In this port this is instead managed by modifying both palettes by half the original amount every frame.

### Rendering
Since the original game was developed for the GBA the rendering is handled very differently from how it would be in a modern engine. Data is loaded into VRAM, which can only hold a limited amount of data. Level objects have pre-calculated addresses for where they are to be loaded into VRAM during different parts of the level and graphics can be defined to either be loaded dynamically (when in use) or statically (once during load).

Additionally, the game takes advantage of several features on the GBA to produce some of its effects. Level transitions are handled using windows, the clouds scrolling at different speeds in the background is done using vsync callbacks which occur after each scanline is drawn etc.

Initially this port was set up to emulate the PPU on the GBA, and thus manually draw each frame pixel by pixel. This however came with major performance costs and would complicate high resolution rendering. Because of this it was later changed into rendering in a more conventional way using textures. Each GBA sprite is now a texture, most of which are created using lazy loading. In order to support changing palettes the textures are rendered in gray-scale with the colors being applied through a shader which gets them from a texture acting as the palette.

Certain effects might not be replicated exactly the same as on a GBA, such as affine sprite rendering and alpha blending, but the goal is to have it appear as similar as possible. In some cases, like when rotating sprites, this has the advantage of making them appear much clearer in higher resolutions. The color range is now also higher since everything is rendered in 32-bit as opposed to 15-bit on the GBA. For colors read from the palettes in the game data this doesn't change the colors displayed, however for dynamically set colors it can take advantage of the higher range.

Another example where there is a difference is for animations with palette cycling. Since the original game only has a single global palette in VRAM it means that multiple animations of the same type will share the same palette, and thus also modify the same one. Having multiple animations playing will then result in the palette cycling appear to animate faster since it shifts it multiple times per frame. This is most notable for the blue lum bar where the filled parts consists of several animations. In this port each animation is self-contained and modifies its own palette, resulting in the animations always modifying the palette at a constant rate.

### Mode7
The Mode7 rendering is handled very differently from how the original game does it. On the GBA it gets rendered by changing the rotation/scaling parameters of the map on each scanline using pre-calculated perspective values to correctly scale it. In this port it instead gets rendered in 3D using a perspective and view matrix. It's been attempted to recreate the original camera perspective, however it's not exactly the same due to how differently they work.

Also unlike the normal sidescrolling levels increasing the internal resolution does not increase the FOV of these levels. This is because increasing it just slightly will cause you to see way too far into the level, sometimes even past the horizon, due to the camera angle. However changing the internal resolution does still change the aspect ratio, allowing them to be played in widescreen.

Another thing that had to be changed was map wrapping. If you look outside of the main level map in the original game you would see leftover tiles in the VRAM repeated. It's not very noticeable due to you always moving in those levels, but it looks rather bad. So since the game has no proper method for handling wrapping this port instead defines a custom section of tiles to repeat outside of the map.

When it comes to the sprites they are also rendered differently. In the original game it uses some rather imprecise code to determine their screen position and scale. Due to its imprecision is causes sprites to not always stay fixed in place. It also doesn't correctly handle animations with multiple sprites, or where the sprites change their offsets. This is because the animation offsets are always in screen space rather than world space. It's most noticeable for the lum animations where the further away from the camera they are the more the sparkling animation causes the sprites to move. To solve these issues, and make rendering easier, the sprites are now rendered in 3D instead, using the camera's matrices, and rotated to always face the camera.

The new way of rendering sprites does cause some sprites to appear a bit differently. This is due to some actors not rendering their sprites with scaling applied, meaning they only get positioned on screen, but not scaled based on the distance. This causes some sprites to appear smaller in this port than they do in the original game where they're always rendered at the original size.

### 3D
The vast majority of content rendered in the original game is in 2D. However there is one thing that is rendered in true 3D, through software rendering, and that's the rotating wheel in the credits screen. Thanks to it being stored as a normal mesh with high resolution textures it can now be rendered in full 3D, making it much clearer in high resolution. The rotation has also been updated to interpolate between frames, making it appear much smoother.

### V-SYNC
In the original GBA version the game calls `vsync` whenever it waits for the frame to finish drawing on screen. This can be done at any point, but is in the majority of cases done inside of the Frame classes' `Init` and `Step` functions.

In the N-Gage port this behavior was changed so that v-sync is managed by the main game loop. Similarly in this port it relies on MonoGame's `Update` method in the game class. This all assumes that each frame has the game either initializing a new Frame class or stepping an existing one. This will match the original GBA in the vast majority of cases, but there are unique cases, like when the game loads heavy data, where v-sync is originally called and thus might cause there to be a 1-frame difference in this port. This will not effect the gameplay in any way.

### Multiplayer
*TODO: Document once multiplayer has been implemented*

### GameCube Link
In the original GBA version you can connect the game to a GameCube running Rayman 3 to download bonus maps and unlock Ly's Punch Challenge 3. The code for this has been re-implemented, with the actual connectivity part lacking an implementation. Instead you can now select a Rayman 3 GameCube ISO file from which the map files get extracted. Then once all those are completed you will unlock Ly's Punch Challenge 3, instead of being determined by the data transferred from the GameCube. 

### Naming conventions
Most types and members have the same name as in the original GBA version, but there are also several cases where it's been changed. For example all French names have been translated to English, something which is usually the case for the actor classes. Short names have also been expanded (for example `FrameMngr` is now named `FrameManager`). State functions have also been renamed from the pattern of `FsmXXXStateFunction` (where "XXX" is the state name) to `Fsm_XXX` to improve readability. For example, `FsmPreInitStateFunction` is now named `Fsm_PreInit`. Constructors for static classes have been renamed from `Ctor` to `Init`, with the destructors being renamed from `Dtor` to `UnInit` and removed when empty.

### Optional improvements
Besides the code related changes there are also optional improvements which the player can toggle in the port. Due to these changing how parts of the game works it also means there are added checks for these throughout the game's code.

### Other minor changes
Various minor changes have been made to the code to modernize it in C# and reduce duplicate code. For example there is now a struct for `PhysicalType` (tile collision) which has helper properties for checking common things like if it's solid. This avoids having to match the value against a constant each time.

Similarly there is now a class for each instance of a finite-state machine which handles things like changing the current state. This was most likely done using macros in the original GBA version. States can now also be null which avoids having to declare empty state functions.

## Sounds
### GBA
In the GBA version all sounds are stored as MIDI data using the MP2K sound engine. Thanks to Rayman 4 DS including the original sound files and projects we have a good idea of how the files were originally stored. Sound effects were sampled from .aif files while the music is fully sequenced from different instruments, also stored as .aif files. There is one MIDI file for each music track while sound effects can have multiple MIDI files using different settings, such as the pitch.

For this port we sadly can't play the sounds directly from the ROM, so the sounds have been extracted using agbplay to PCM16 .wav files with loop points (thanks to Miles!). They've then been named based on the original .mid file names from Rayman 4 DS. In the code there is then a table which matches the song resource id with the .wav files so they can be played.

### N-Gage
In the N-Gage version the sound effects and music are handled differently. The music is stored as .xm files, while the sound effects are stored as .wav files and appear to be converted from the original .aif sound samples. Because of this it lacks the multiple variants of sound effects which the GBA MIDI had. The file names, which here are only used for debugging purposes due to the sounds being read directly from the data, come from the Digiblast version thanks to them being stored in the same alphabetical order. The N-Gage version also has 4 additional sound effects which we sadly lack the names for.

## Decompiling
The engine is being recreated thanks to Ghidra allowing the original game's code to be decompiled. Both the GBA and N-Gage versions are being used to recreate the code, so that any differences between them can be correctly handled. The N-Gage version is also easier to decompile due to it having fewer compiler optimizations, such as no function inlining.

There also exists various prototypes of GBA games using this engine which were compiled with assertions in the code. These assertions contain debug strings for if the assertions fail which are incredibly useful as they contain function names, variable names, source file paths and more. Thanks to this information we have a very good idea of how the original engine was structured and how things were named.
