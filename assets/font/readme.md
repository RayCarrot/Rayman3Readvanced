# Rayman 3 Font

### Steps
Here is a list of steps taken to produce the Rayman 3 font. It's rather convoluted as it was done to simulate the low-resolution font used in the original game.

#### XNA Font Texture Generator
- Use [XNA Font Texture Generator](https://github.com/yariker/xfontgen), but with the clear color modified to be transparent
- Set the following properties and then export texture:
    - Font: Rayman 3
	- Size: 200
	- Color: FFC500FF / D5C583FF
	- Char: 32-255
	- Antialiased: off
	- Kerning: 100
	- Leading: 50
	- Width: 3000

#### Paint.net
- Resize canvas to 3200x4433 (extend 100 in each direction)
- Use [outline plugin](https://forums.getpaint.net/topic/118140-mccreerys-plugins-texture-tools-color-ramp-outline-and-erode-april-23-2021/) to set an outline with radius 20 and color (65, 8, 0)
- Resize to a width of 220 using Fant
- Posterize alpha to 0
- Use magic wand in global mode with a tolerance of 35% to remove outlines
- Add outlines again, with a radius of 1
- Copy yellow version as layer to white version, select everything transparent and delete on white layer
- Manually fix some letters
