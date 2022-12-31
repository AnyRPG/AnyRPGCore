# Unity-Skybox-Blender
A tool to blend skyboxes in Unity with realtime environment lighting and reflections updates.

## How to use

1. Add the SkyboxBlender.cs script to an object in your scene.
2. Assign the two input skybox materials to blend to the SkyboxBlender.
2. Create a new material with the SkyboxBlender/BlendedSkybox shader.
3. Assign the newly created material to the SkyboxBlender and to your scene skybox.
5. Blend !

## Useful information

- To update the reflection probe resolution after changing it, use the Update Probe button.
- The script executes in edit mode to visualize the blend result in the editor, you can disable it by removing the [ExecuteInEditMode] tag in the SkyboxBlender script.
- The input skyboxes materials are expected to use the Unity built-in Skybox/6-sided shader.

## Known Issues

- The reflection update does not always work properly when not updating every frames.
