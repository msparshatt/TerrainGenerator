# System tab

## New
Controls for creating a new terrain

- **Flat** will create a flat terrain at the middle height
- **Heightmap** load either a raw or png heightmap file
- **procedural** procedurally generate a terrain. See [Procedural Generation](#procedural-generation)

## Load/Save
Loading and saving old terrain files

## Export
Export the terrain

- **Object** export the terrain as a wavefront obj file. This will create 3 files. *filename*.obj *filename*.mtl and *filename*.png
- **Scale** Increase the size of the exported mesh
- **Heightmap** Exports a raw heightmap file

## Other

- **Reset** Resets the terrain and any settings to the default
- **Help** Opens up a help window
- **About** Opens up the about window
- **Settings** Opens the settings window

# Materials tab
This allows you to mix up to 5 materials based on the material settings

## Settings

- **Tiling** Adjusts the scale of all 5 materials
- **Ambient Occlusion** Will turn on the materials' ambient occlusion texture

## Base material
Select the base material

## Material 2 - 5
Select the materials to mix with the base material. The materials are mixed in numerical order. So material 3 will cover material 4

### Material Dropdown
Set how the material will be mixed

- **Top** Applies the material from the top downwards
- **Steep** Applies the material to steep areas
- **Bottom** Applies the material from the bottom upwards
- **Shallow** Applies the material to shallow areas
- **Peaks** Applies the material to peaks (Experimental)
- **Valleys** Applies the material to valleys (Experimental)
- **Random** Applies the material based on a simplified perlin noise

### Factor
How much of the material to mix in

### Random seed
Used when using the **Random** option to provide an offset for the perlin noise function


# Sculpt tab
Use this mode to sculpt the terrain.

- **Radius** The radius of the sculpting brush (Can be controlled using the mousewheel)
- **Strength** Controls the sculpting strength (Can be controlled with shift and the mousewheel)
- **Rotation** Controls the rotation of the sculpt brush (Can be controledd with control and the mousewheel)

The sculpt controls are
- Left click - Raise the terrain
- Shift + Left click - Lower the terrain
- Control + Left Click - flatten the terrain

# Paint Tab
Paint textures over the terrain

- **Radius** The radius of the paint brush (Can be controlled using the mousewheel)
- **Strength** Controls the painting strength (Can be controlled with shift and the mousewheel)
- **Rotation** Controls the rotation of the paint brush (Can be controledd with control and the mousewheel)

- **Textures** Choose the texture to paint
- **Clear** Clears any painted textures
- **Tiling** Will scale the texture to be painted
- **Reset** Resets the tiling value to 1

The paint controls are
- Left click - Paint
- Shift + Left click - Erase

# Procedural Generation

Todo

# Importing brushes, materials and textures

Todo

# Controls

Todo
