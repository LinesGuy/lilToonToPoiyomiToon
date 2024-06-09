# lilToon to PoiyomiToon
Unity script to create Poiyomi Toon materials from lilToon materials

![image](https://github.com/LinesGuy/lilToonToPoiyomiToon/assets/60029482/19034527-e683-4e4e-bba9-ac9fac5708c6)

# How to use
1. Install the .UnityPackage from [releases](https://github.com/LinesGuy/lilToonToPoiyomiToon/releases/download/Meow/LinesMaterialConverter.unitypackage)
2. Select one or more liltoon materials in your project
   
![image](https://github.com/LinesGuy/lilToonToPoiyomiToon/assets/60029482/a8e13d37-3e14-4021-a7bb-295a23530005)

3. Click on the convert button under tools

![image](https://github.com/LinesGuy/lilToonToPoiyomiToon/assets/60029482/ad91085b-7399-4487-b971-86f32ade80f9) 

You should now have Poiyomi Toon materials

![image](https://github.com/LinesGuy/lilToonToPoiyomiToon/assets/60029482/581e2d07-b8a6-42ed-8302-29d5017240c9)

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
- Normal map
- Backlight
- Matcaps
- Rim light
- Outline
- Emissions

# What doesn't work 

- Glitter (soontm)
- 2nd normal map / Detail normal (soontm)
- Fur materials (lil/poi fur use completely different methods)
- Main texture UV setting
- Reflections and specular (I can't find a way to consistently convert or approximate the values)
- Rim Shade (I've never seen someone use this so I'm not sure how to use it)
- Audiolink (I've never used liltoon audiolink)
- Refraction, Blur, Gem shaders
- Tesselation
- Stencils
- Probably some other stuff, DM on twitter @LinesTheCat or discord @linesnya (say you're from my converter thing)
