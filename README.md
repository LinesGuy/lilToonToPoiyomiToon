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
- Normal map + 2nd normal map (detail normal)
- Backlight
- Matcaps
- Rim light
- Outline
- Emissions

# What doesn't work 

- Reflections and specular (I can't find a way to consistently convert or approximate the values), if your material looks dark or is missing some light then it's probably this
- Glitter (soontm)
- Fur materials (lil/poi fur use completely different methods)
- Main texture UV setting
- Rim Shade (I've never seen someone use this so I'm not sure how to use it)
- Audiolink (I've never used liltoon audiolink)
- Refraction, Blur, Gem shaders
- Tesselation
- Stencils
- Probably some other stuff, DM on twitter @LinesTheCat or discord @linesnya (say you're from my converter thing)
