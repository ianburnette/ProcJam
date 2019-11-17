# To use:
- Open Scenes/Main.scene
- Select the SpriteGenerator game object in the hierarchy 
- (optionally, press play for animation to play at full speed and for saving functionality to work)
- Click the "Generate" button in the "Controls" behavior in the Unity inspector
- Click a sprite to save it to "Assets/Exported Sprites" (in play mode)

# Presets
- To override the current configuration, select a Preset from the dropdown
- Outside of play mode, click the "Save Config as New Preset" to save the current configuration as a new preset (see bug note below)
- In order to make this preset selectable in the dropdown, you must also add it to the "Presets" enum in Controls.cs

# Known Issues
- If you generate a sprite that is is too large (keep it under 128px if you're generating more than one at a time), has too many frames of animation (keep it under 4 if over 32x32), the program will hang for a long time while it processes.