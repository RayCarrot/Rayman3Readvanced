## Ubisoft Montreal
### Rayman 3
The source code structure for Rayman 3 are separated by their build date. This includes all roms from the prototype leak, but does not include the earlier preview build that was dumped due to it not having a known date.

### Tom Clancy's Splinter Cell
Splinter Cell only has one prototype, but its date is sadly unknown. It uses a later build of the engine than in Rayman 3 and the main libraries have been renamed as follows:
- `GbaStdLib` has been merged with `GbaSdk`
- `Gba2DPlatform` has been renamed to `Gba2d`
- `GbaAnimationPlayer` has been renamed to `GbaAnimation`
- `GbaTileGraphics` has been renamed to `GbaPlayfield`

## Ubisoft Milan
### Tom Clancy's Rainbow Six Rogue Spear
This includes a single prototype dated 2001-10-25. This uses a separate branch of the engine and thus has many changes to it.