# This script is now defunct

Poiyomi Toon Shaders now includes its own material translation script, making this tool obsolete. The script may work for now but if/when it breaks it won't get updated. Grab the latest version of Poiyomi [here](https://github.com/poiyomi/PoiyomiToonShader/releases), and use it by right clicking a liltoon material (or an avatar in the hierarchy) and translating it like so:

![Screenshot 2025-03-22 010602](https://github.com/user-attachments/assets/a5f9f5eb-3e82-4a8e-8ebb-9eecd098672e)

# lilToon to PoiyomiToon
Unity script to create Poiyomi Toon materials from lilToon materials

![image](https://github.com/LinesGuy/lilToonToPoiyomiToon/assets/60029482/8ad916ca-27dd-4a7c-a8d8-622bcfd0d814)


# How to use
0. Ensure you have [Poiyomi Toon 9](https://github.com/poiyomi/PoiyomiToonShader/releases/latest) installed (Poiyomi 8 not supported and probably won't work)
1. Install the .UnityPackage from [releases](https://github.com/LinesGuy/lilToonToPoiyomiToon/releases/)
2. Select one or more liltoon materials in your project
   
![image](https://github.com/LinesGuy/lilToonToPoiyomiToon/assets/60029482/a8e13d37-3e14-4021-a7bb-295a23530005)

3. Click on the convert button under tools

![image](https://github.com/LinesGuy/lilToonToPoiyomiToon/assets/60029482/ad91085b-7399-4487-b971-86f32ade80f9) 

Backups of the original lilToon materials will be created, and the original materials should now be using Poiyomi shaders, no need to apply the materials

![image](https://github.com/user-attachments/assets/15476b20-c743-4d7f-81c9-a846c1a5c9b2)


# What works

- Opaque/Cutout/Transparent/TwoPass mode
- Setting of Culling, flipped normals, ZWrite, Render queue
- Shading mode (flat/multilayer math)
- Lighting settings
- Main texture
- Hue shift + saturation
- 2nd and 3rd main texture (converted to poiyomi decal 0 and 1)
- Alpha mask
- Shadow layers + border
- AO map (shadow map)
- Normal map + 2nd normal map (detail normal)
- Backlight
- Matcaps
- Rim light
- Rim Shade (converted to 2nd rim light in Multiply mode)
- Outline
- Emissions
- Refraction/Blur (intensity might be a bit off, though)

# What doesn't work 
- Reflections and specular and lilToon and Poiyomi use very different methods, if your material looks dark or is missing some light then it's probably this
- Transparent materials will occasionaly look buggy
- Glitter
- Fur materials (lil/poi fur use completely different methods)
- Main texture UV setting
- Audiolink (I've never used liltoon audiolink)
- Gem shaders
- Tesselation
- Stencils
- Probably some other stuff, DM on twitter @LinesTheCat or discord @linesnya (say you're from my converter thing)
