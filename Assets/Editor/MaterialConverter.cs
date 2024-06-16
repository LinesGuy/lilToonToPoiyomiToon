using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;

public class LinesLiltoonToPoiyomi : EditorWindow {
    [MenuItem("Tools/Convert liltoon material to Poiyomi", false)]
    private static void ConvertMaterial() {
        if (Selection.objects.Length == 0) return;

        // See if we have poi pro installed
        bool isPro = Shader.Find(".poiyomi/Poiyomi Pro") != null;
        // If not, see if we have poi toon installed at all
        if (!isPro && Shader.Find(".poiyomi/Poiyomi Toon") == null) {
            EditorUtility.DisplayDialog("Convert liltoon to poiyomi", "Looks like you don't have Poiyomi Toon 9 or later installed, boo womp, please install it before running this script!", "OK");
            return;
        }

        List<string> unsupportedMaterials = new List<string>();
        for (int i = 0; i < Selection.objects.Length; i++) {
            if (Selection.objects[i] is Material material) {
                if (IsLiltoon(material)) {
                    CreatePoiyomiMaterial(ref material, isPro);
                } else {
                    string path = AssetDatabase.GetAssetPath(material);
                    if (!string.IsNullOrEmpty(path)) unsupportedMaterials.Add(path);
                    continue;
                }
            }
        }
        if (unsupportedMaterials.Count > 0) EditorUtility.DisplayDialog("Convert liltoon to poiyomi", "Skipped conversion of materials using unsupported shaders." + "\r\n" + string.Join("\r\n", unsupportedMaterials), "OK");
        AssetDatabase.SaveAssets();
    }
    /*
    [MenuItem("Tools/Convert liltoon material to Poiyomi", true)]
    private static bool CheckMaterialFormat() {
        if (Selection.activeGameObject == null) return false;
        return true; // AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(".mat", StringComparison.OrdinalIgnoreCase);
    }
    */

    private static bool IsLiltoon(Material material) {
        return material.shader.name.Contains("lilToon");
    }

    private static void CreatePoiyomiMaterial(ref Material lil, bool isPro) {
        // Check if we have poiyomi pro installed
        string shaderName = isPro ? ".poiyomi/Poiyomi Pro" : ".poiyomi/Poiyomi Toon";

        if (lil.shader.name.Contains("TwoPass")) {
            shaderName = isPro ? ".poiyomi/Poiyomi Pro Two Pass" : ".poiyomi/Poiyomi Toon Two Pass";
        }
        if (lil.shader.name.Contains("Refraction")) {
            shaderName = isPro ? ".poiyomi/Poiyomi Pro Grab Pass" : ".poiyomi/Poiyomi Toon Grab Pass";
        }

        Shader shader = Shader.Find(shaderName);
        Material poi = new Material(shader);

        // Rendering Mode -> Rendering Preset
        if (lil.shader.name.Contains("Cutout")) {
            poi.SetFloat("_Mode", 1);
            poi.SetFloat("_Cutoff", lil.GetFloat("_Cutoff"));
            poi.SetFloat("_AlphaForceOpaque", 0);
        }
        if (lil.shader.name.Contains("Transparent")) {
            poi.SetFloat("_Mode", 3);
            poi.SetFloat("_Cutoff", lil.GetFloat("_Cutoff"));
            poi.SetFloat("_AlphaForceOpaque", 0);

            poi.SetFloat("_AlphaPremultiply", 1);
            poi.SetFloat("_BlendOpAlpha", 4);
            poi.SetFloat("_DstBlend", 10);
            poi.SetFloat("_DstBlendAlpha", 1);
        }

        // Base Setting -> Rendering
        poi.SetFloat("_Cull", lil.GetFloat("_Cull"));
        poi.SetFloat("_ZWrite", lil.GetFloat("_ZWrite"));
        poi.renderQueue = lil.renderQueue;
        poi.SetFloat("_FlipBackfaceNormals", lil.GetFloat("_FlipNormal"));

        // Lighting -> Shading/Light Data
        poi.SetFloat("_LightingColorMode", 3); // OpenLit(lil toon)
        poi.SetFloat("_LightingMapMode", 1); // Normalized NDotL
        poi.SetFloat("_LightingDirectionMode", 4); // OpenLit(lil toon)

        poi.SetFloat("", lil.GetFloat(""));
        poi.SetFloat("_LightingMonochromatic", lil.GetFloat("_MonochromeLighting"));

        poi.SetFloat("_LightingMinLightBrightness", lil.GetFloat("_LightMinLimit"));
        poi.SetFloat("_LightingCap", lil.GetFloat("_LightMaxLimit"));

        // UV Setting is skipped because idk the poi equivalent of this :3

        // Main Color / Alpha
        poi.mainTexture = lil.mainTexture;
        poi.color = lil.color;
        poi.mainTextureOffset = lil.mainTextureOffset;
        poi.mainTextureScale = lil.mainTextureScale;
        poi.SetTexture("_MainColorAdjustTexture", lil.GetTexture("_MainColorAdjustMask"));
        if (lil.GetColor("_MainTexHSVG").r != 0) poi.SetFloat("_MainHueShiftToggle", 1);
        // If HSV / Gamma is changed, enable color adjust in poi
        if (lil.GetColor("_MainTexHSVG") != new Color(0, 1, 1, 1))
            poi.SetFloat("_MainColorAdjustToggle", 1);
        if (lil.GetColor("_MainTexHSVG").r != 0) {
            poi.SetFloat("_MainHueShiftToggle", 1);
            // hue shift [-0.5, 0.5] to [0, 1]
            float newHue = lil.GetColor("_MainTexHSVG").r;
            if (newHue < 0.5) newHue += 1;
            poi.SetFloat("_MainHueShift", newHue);
        }

        // saturation [0, 2] to [-1, 1]
        poi.SetFloat("_Saturation", lil.GetColor("_MainTexHSVG").g - 1);
        // idk how to set Value / Gamma :3

        // Main Color 2nd -> Decal 1
        if (lil.GetFloat("_UseMain2ndTex") == 1) {
            poi.SetFloat("_DecalEnabled", 1);
            if (lil.GetFloat("_Main2ndTexBlendMode") == 1) poi.SetFloat("_DecalBlendType", 8);
            if (lil.GetFloat("_Main2ndTexBlendMode") == 2) poi.SetFloat("_DecalBlendType", 6);
            if (lil.GetFloat("_Main2ndTexBlendMode") == 3) poi.SetFloat("_DecalBlendType", 2);
            if (lil.GetFloat("_Main2ndTexIsDecal") == 1) {
                //poi.SetColor("_DecalPosition1", new Color(1 / lil.GetTextureOffset("_Main2ndTex").x, 1 / lil.GetTextureOffset("_Main2ndTex").y, 0, 0));
                poi.SetColor("_DecalPosition", new Color((0.5f - lil.GetTextureOffset("_Main2ndTex").x) / lil.GetTextureScale("_Main2ndTex").x, (0.5f - lil.GetTextureOffset("_Main2ndTex").y) / lil.GetTextureScale("_Main2ndTex").y, 0, 0));
                poi.SetColor("_DecalScale", new Color(1 / lil.GetTextureScale("_Main2ndTex").x, 1 / lil.GetTextureScale("_Main2ndTex").y, 0, 0));

            } else {
                poi.SetTextureOffset("_DecalTexture", lil.GetTextureOffset("_Main2ndTex"));
                poi.SetTextureScale("_DecalTexture", lil.GetTextureScale("_Main2ndTex"));
            }
            poi.SetTexture("_DecalTexture", lil.GetTexture("_Main2ndTex"));
            poi.SetColor("_DecalColor", lil.GetColor("_Color2nd"));
        }

        // Main Color 3rd -> Decal 1
        if (lil.GetFloat("_UseMain3rdTex") == 1) {
            poi.SetFloat("_DecalEnabled1", 1);
            if (lil.GetFloat("_Main3rdTexBlendMode") == 1) poi.SetFloat("_DecalBlendType1", 8);
            if (lil.GetFloat("_Main3rdTexBlendMode") == 2) poi.SetFloat("_DecalBlendType1", 6);
            if (lil.GetFloat("_Main3rdTexBlendMode") == 3) poi.SetFloat("_DecalBlendType1", 2);
            if (lil.GetFloat("_Main3rdTexIsDecal") == 1) {
                //poi.SetColor("_DecalPosition1", new Color(1 / lil.GetTextureOffset("_Main3rdTex").x, 1 / lil.GetTextureOffset("_Main3rdTex").y, 0, 0));
                poi.SetColor("_DecalPosition1", new Color((0.5f - lil.GetTextureOffset("_Main3rdTex").x) / lil.GetTextureScale("_Main3rdTex").x, (0.5f - lil.GetTextureOffset("_Main3rdTex").y) / lil.GetTextureScale("_Main3rdTex").y, 0, 0));
                poi.SetColor("_DecalScale1", new Color(1 / lil.GetTextureScale("_Main3rdTex").x, 1 / lil.GetTextureScale("_Main3rdTex").y, 0, 0));

            } else {
                poi.SetTextureOffset("_DecalTexture1", lil.GetTextureOffset("_Main3rdTex"));
                poi.SetTextureScale("_DecalTexture1", lil.GetTextureScale("_Main3rdTex"));
            }
            poi.SetTexture("_DecalTexture1", lil.GetTexture("_Main3rdTex"));
            poi.SetColor("_DecalColor1", lil.GetColor("_Color3rd"));
        }

        // Alpha Mask -> Alpha Map
        poi.SetTexture("_AlphaMask", lil.GetTexture("_AlphaMask"));
        poi.SetFloat("_MainAlphaMaskMode", lil.GetFloat("_AlphaMaskMode"));
        poi.SetTextureOffset("_AlphaMask", lil.GetTextureOffset("_AlphaMask"));
        poi.SetTextureScale("_AlphaMask", lil.GetTextureScale("_AlphaMask"));
        if (lil.GetFloat("_AlphaMaskScale") == -1) {
            poi.SetFloat("_AlphaMaskInvert", 1);
            poi.SetFloat("_AlphaMaskValue", -(lil.GetFloat("_AlphaMaskValue") - 1));
        } else { // lil alpha mask scale = -1
            poi.SetFloat("_AlphaMaskInvert", 0);
            poi.SetFloat("_AlphaMaskValue", lil.GetFloat("_AlphaMaskValue"));
        }

        // Shadow -> Shading/Shading if enabled
        if (lil.GetFloat("_UseShadow") == 1.0f) {
            poi.SetFloat("_LightingMode", 1);

            poi.SetFloat("_ShadowStrength", lil.GetFloat("_ShadowStrength"));

            poi.SetTexture("_ShadowColorTex", lil.GetTexture("_ShadowColorTex"));
            poi.SetTextureScale("_ShadowColorTex", lil.GetTextureScale("_ShadowColorTex"));
            poi.SetTextureOffset("_ShadowColorTex", lil.GetTextureOffset("_ShadowColorTex"));
            poi.SetColor("_ShadowColor", lil.GetColor("_ShadowColor"));
            poi.SetFloat("_ShadowBorder", lil.GetFloat("_ShadowBorder"));
            poi.SetFloat("_ShadowBlur", lil.GetFloat("_ShadowBlur"));
            poi.SetFloat("_ShadowReceive", lil.GetFloat("_ShadowReceive"));

            poi.SetTexture("_Shadow2ndColorTex", lil.GetTexture("_Shadow2ndColorTex"));
            poi.SetTextureScale("_Shadow2ndColorTex", lil.GetTextureScale("_Shadow2ndColorTex"));
            poi.SetTextureOffset("_Shadow2ndColorTex", lil.GetTextureOffset("_Shadow2ndColorTex"));
            poi.SetColor("_Shadow2ndColor", lil.GetColor("_Shadow2ndColor"));
            poi.SetFloat("_Shadow2ndBorder", lil.GetFloat("_Shadow2ndBorder"));
            poi.SetFloat("_Shadow2ndBlur", lil.GetFloat("_Shadow2ndBlur"));
            poi.SetFloat("_Shadow2ndReceive", lil.GetFloat("_Shadow2ndReceive"));

            poi.SetTexture("_Shadow3rdColorTex", lil.GetTexture("_Shadow3rdColorTex"));
            poi.SetTextureScale("_Shadow3rdColorTex", lil.GetTextureScale("_Shadow3rdColorTex"));
            poi.SetTextureOffset("_Shadow3rdColorTex", lil.GetTextureOffset("_Shadow3rdColorTex"));
            poi.SetColor("_Shadow3rdColor", lil.GetColor("_Shadow3rdColor"));
            poi.SetFloat("_Shadow3rdBorder", lil.GetFloat("_Shadow3rdBorder"));
            poi.SetFloat("_Shadow3rdBlur", lil.GetFloat("_Shadow3rdBlur"));
            poi.SetFloat("_Shadow3rdReceive", lil.GetFloat("_Shadow3rdReceive"));

            poi.SetColor("_ShadowBorderColor", lil.GetColor("_ShadowBorderColor"));
            poi.SetFloat("_ShadowBorderRange", lil.GetFloat("_ShadowBorderRange"));

            poi.SetTexture("_LightingDetailShadowMaps", lil.GetTexture("_ShadowBorderMask"));

            poi.SetFloat("_LightingMulitlayerNonLinear", 0); // liltoon has no equivalent option so i assume this is disabled by default

            poi.SetFloat("_LightingAdditiveType", 1);
            poi.SetFloat("_LightingAdditiveGradientStart", 0.4f);
            poi.SetFloat("_LightingAdditiveGradientEnd", 0.6f);
        }

        // Normal map
        poi.SetTexture("_BumpMap", lil.GetTexture("_BumpMap"));
        poi.SetTextureOffset("_BumpMap", lil.GetTextureOffset("_BumpMap"));
        poi.SetTextureScale("_BumpMap", lil.GetTextureScale("_BumpMap"));
        poi.SetFloat("_BumpScale", lil.GetFloat("_BumpScale"));
        // Normap map 2nd
        if (lil.GetFloat("_UseBump2ndMap") == 1) {
            poi.SetFloat("_DetailEnabled", 1);
            poi.SetTexture("_DetailMask", lil.GetTexture("_Bump2ndScaleMask"));
            poi.SetTextureScale("_DetailMask", lil.GetTextureScale("_Bump2ndScaleMask"));
            poi.SetTextureOffset("_DetailMask", lil.GetTextureOffset("_Bump2ndScaleMask"));

            poi.SetTexture("_DetailNormalMap", lil.GetTexture("_Bump2ndMap"));
            poi.SetTextureOffset("_DetailNormalMap", lil.GetTextureOffset("_Bump2ndMap"));
            poi.SetTextureScale("_DetailNormalMap", lil.GetTextureScale("_Bump2ndMap"));
            poi.SetFloat("_DetailTexIntensity", lil.GetFloat("_Bump2ndScale"));
        }


        // Reflections
        if (lil.GetFloat("_UseReflection") == 1) {
            // Poiyomi Toon and lilToon use different methods for reflections so this is
            // currently disabled, the values are copied over anyway

            //poi.SetFloat("_MochieBRDF", 1);

            poi.SetFloat("_MochieMetallicMultiplier", lil.GetFloat("_Metallic"));
            poi.SetFloat("_MochieRoughnessMultiplier", lil.GetFloat("_Smoothness"));
            // TODO Smoothness, Metallic masks
            poi.SetColor("_MochieReflectionTint", lil.GetColor("_ReflectionColor"));
            poi.SetColor("_MochieSpecularTint", lil.GetColor("_ReflectionColor"));
            poi.SetFloat("_MochieReflectionStrength", lil.GetFloat("_Reflectance"));
            if (lil.GetFloat("_ApplyReflection") == 0) {
                poi.SetFloat("_MochieForceFallback", 1); // Forcing a fallback without a cubemap texture disables environment reflections
            } else {
                // TODO environment reflections
            }
            poi.SetFloat("_MochieSpecularStrength", 1);
        }

        if (lil.GetFloat("_UseBacklight") == 1) {
            poi.SetFloat("_BacklightEnabled", 1);
            poi.SetColor("_BacklightColor", lil.GetColor("_BacklightColor"));
            poi.SetFloat("_BacklightMainStrength", lil.GetFloat("_BacklightMainStrength"));
            poi.SetFloat("_BacklightReceiveShadow", lil.GetFloat("_BacklightReceiveShadow"));
            poi.SetFloat("_BacklightBackfaceMask", lil.GetFloat("_BacklightBackfaceMask"));
            poi.SetFloat("_BacklightNormalStrength", lil.GetFloat("_BacklightNormalStrength"));
            poi.SetFloat("_BacklightBorder", lil.GetFloat("_BacklightBorder"));
            poi.SetFloat("_BacklightBlur", lil.GetFloat("_BacklightBlur"));
            poi.SetFloat("_BacklightDirectivity", lil.GetFloat("_BacklightDirectivity"));
            poi.SetFloat("_BacklightViewStrength", lil.GetFloat("_BacklightViewStrength"));
        }

        // Matcap
        if (lil.GetFloat("_UseMatCap") == 1) {
            poi.SetFloat("_MatcapEnable", 1);
            poi.SetTexture("_Matcap", lil.GetTexture("_MatCapTex"));
            poi.SetTextureScale("_Matcap", lil.GetTextureScale("_MatCapTex"));
            poi.SetTextureOffset("_Matcap", lil.GetTextureOffset("_MatCapTex"));
            poi.SetColor("_MatcapColor", lil.GetColor("_MatCapColor"));
            poi.SetFloat("_MatcapBorder", 0.5f); // Poi uses 0.43 by default whereas lil uses 0.5
            poi.SetTexture("_MatcapMask", lil.GetTexture("_MatCapBlendMask"));
            poi.SetFloat("_MatcapReplace", 0);
            float matCapBlend = lil.GetFloat("_MatCapBlend");
            if (lil.GetFloat("_MatCapBlendMode") == 0) poi.SetFloat("_MatcapReplace", matCapBlend);
            else if (lil.GetFloat("_MatCapBlendMode") == 1) poi.SetFloat("_MatcapAdd", matCapBlend);
            else if (lil.GetFloat("_MatCapBlendMode") == 2) poi.SetFloat("_MatcapScreen", matCapBlend);
            else if (lil.GetFloat("_MatCapBlendMode") == 3) poi.SetFloat("_MatcapMultiply", matCapBlend);
            poi.SetFloat("_MatcapBaseColorMix", lil.GetFloat("_MatCapMainStrength"));
            poi.SetFloat("_MatcapNormal", lil.GetFloat("_MatCapNormalStrength"));
            if (lil.GetFloat("_MatCapLod") != 0) {
                poi.SetFloat("_MatcapSmoothnessEnabled", 1);
                var smoothness = Math.Max(0f, 1f - lil.GetFloat("_MatCapLod") / 10f);
                poi.SetFloat("_MatcapSmoothness", smoothness);
            }
            if (lil.GetFloat("_MatCapCustomNormal") == 1) {
                poi.SetFloat("_Matcap0CustomNormal", 1);
                poi.SetFloat("_MatcapNormal", 0);
                poi.SetTexture("_Matcap0NormalMap", lil.GetTexture("_MatCapBumpMap"));
                poi.SetTextureScale("_Matcap0NormalMap", lil.GetTextureScale("_MatCapBumpMap"));
                poi.SetTextureOffset("_Matcap0NormalMap", lil.GetTextureOffset("_MatCapBumpMap"));
                poi.SetFloat("_Matcap0NormalMapScale", (float)lil.GetFloat("_MatCapBumpScale"));
            }
        }
        // MatCap 2nd
        if (lil.GetFloat("_UseMatCap2nd") == 1) {
            poi.SetFloat("_Matcap2Enable", 1);
            poi.SetTexture("_Matcap2", lil.GetTexture("_MatCap2ndTex"));
            poi.SetTextureScale("_Matcap2", lil.GetTextureScale("_MatCap2ndTex"));
            poi.SetTextureOffset("_Matcap2", lil.GetTextureOffset("_MatCap2ndTex"));
            poi.SetColor("_Matcap2Color", lil.GetColor("_MatCap2ndColor"));
            poi.SetFloat("_Matcap2Border", 0.5f);
            poi.SetTexture("_Matcap2Mask", lil.GetTexture("_MatCap2ndBlendMask"));
            poi.SetFloat("_Matcap2Replace", 0);
            float matCap2ndBlend = lil.GetFloat("_MatCap2ndBlend");
            if (lil.GetFloat("_MatCap2ndBlendMode") == 0) poi.SetFloat("_Matcap2Replace", matCap2ndBlend);
            else if (lil.GetFloat("_MatCap2ndBlendMode") == 1) poi.SetFloat("_Matcap2Add", matCap2ndBlend);
            else if (lil.GetFloat("_MatCap2ndBlendMode") == 2) poi.SetFloat("_Matcap2Screen", matCap2ndBlend);
            else if (lil.GetFloat("_MatCap2ndBlendMode") == 3) poi.SetFloat("_Matcap2Multiply", matCap2ndBlend);
            poi.SetFloat("_Matcap2BaseColorMix", lil.GetFloat("_MatCap2ndMainStrength"));
            poi.SetFloat("_Matcap2Normal", lil.GetFloat("_MatCap2ndNormalStrength"));
            if (lil.GetFloat("_MatCap2ndLod") != 0) {
                poi.SetFloat("_Matcap2SmoothnessEnabled", 1);
                var smoothness2nd = Math.Max(0f, 1f - lil.GetFloat("_MatCap2ndLod") / 10f);
                poi.SetFloat("_Matcap2Smoothness", smoothness2nd);
            }
            if (lil.GetFloat("_MatCap2ndCustomNormal") == 1) {
                poi.SetFloat("_Matcap1CustomNormal", 1);
                poi.SetFloat("_Matcap2Normal", 0);
                poi.SetTexture("_Matcap1NormalMap", lil.GetTexture("_MatCap2ndBumpMap"));
                poi.SetTextureScale("_Matcap1NormalMap", lil.GetTextureScale("_MatCap2ndBumpMap"));
                poi.SetTextureOffset("_Matcap1NormalMap", lil.GetTextureOffset("_MatCap2ndBumpMap"));
                poi.SetFloat("_Matcap1NormalMapScale", (float)lil.GetFloat("_MatCap2ndBumpScale"));
            }
        }

        // Rim Light
        if (lil.GetFloat("_UseRim") == 1) {
            poi.SetFloat("_EnableRimLighting", 1);
            poi.SetFloat("_RimStyle", 2);
            poi.SetTexture("_RimColorTex", lil.GetTexture("_RimColorTex"));
            poi.SetTextureScale("_RimColorTex", lil.GetTextureScale("_RimColorTex"));
            poi.SetTextureOffset("_RimColorTex", lil.GetTextureOffset("_RimColorTex"));
            poi.SetColor("_RimColor", lil.GetColor("_RimColor"));

            poi.SetFloat("_RimMainStrength", lil.GetFloat("_RimMainStrength"));
            poi.SetFloat("_RimEnableLighting", lil.GetFloat("_RimEnableLighting"));
            poi.SetFloat("_RimShadowMask", lil.GetFloat("_RimShadowMask"));
            poi.SetFloat("_RimBackfaceMask", lil.GetFloat("_RimBackfaceMask"));
            poi.SetFloat("_RimBlendMode", lil.GetFloat("_RimBlendMode"));
            poi.SetFloat("_RimDirStrength", lil.GetFloat("_RimDirStrength"));
            poi.SetFloat("_RimDirRange", lil.GetFloat("_RimDirRange"));
            poi.SetFloat("_RimBorder", lil.GetFloat("_RimBorder"));
            poi.SetFloat("_RimBlur", lil.GetFloat("_RimBlur"));
            poi.SetFloat("_RimIndirRange", lil.GetFloat("_RimIndirRange"));
            poi.SetColor("_RimIndirColor", lil.GetColor("_RimIndirColor"));
            poi.SetFloat("_RimIndirBorder", lil.GetFloat("_RimIndirBorder"));
            poi.SetFloat("_RimIndirBlur", lil.GetFloat("_RimIndirBlur"));
            poi.SetFloat("_RimNormalStrength", lil.GetFloat("_RimNormalStrength"));
            poi.SetFloat("_RimFresnelPower", lil.GetFloat("_RimFresnelPower"));
            poi.SetFloat("_RimVRParallaxStrength", lil.GetFloat("_RimVRParallaxStrength"));
        }

        // RimShade -> Rim Lighting 1
        if (lil.GetFloat("_UseRimShade") == 1) {
            // RimShade seems to just be Rim Lighting in "Multiply" mode
            poi.SetFloat("_EnableRim2Lighting", 1);
            poi.SetFloat("_Rim2Style", 2);

            poi.SetTexture("_Rim2ColorTex", lil.GetTexture("_RimShadeMask"));
            poi.SetTextureScale("_Rim2ColorTex", lil.GetTextureScale("_RimShadeMask"));
            poi.SetTextureOffset("_Rim2ColorTex", lil.GetTextureOffset("_RimShadeMask"));

            // Seems that poiyomi's "rim shade" is ever so slightly brighter than liltoon's rimshade
            Color rimShadeColor = lil.GetColor("_RimShadeColor");
            rimShadeColor.r *= 0.922f;
            rimShadeColor.g *= 0.922f;
            rimShadeColor.b *= 0.922f;
            poi.SetColor("_Rim2Color", rimShadeColor);

            poi.SetFloat("_Rim2Border", lil.GetFloat("_RimShadeBorder"));
            poi.SetFloat("_Rim2Blur", lil.GetFloat("_RimShadeBlur"));
            poi.SetFloat("_Rim2FresnelPower", lil.GetFloat("_RimShadeFresnelPower"));
            poi.SetFloat("_Rim2ShadowMask", 0); // lil's rimshade seems to ignore shadows completely
            poi.SetFloat("_Rim2BlendMode", 3); // Multiply mode
        }

        // Outline
        if (lil.shader.name.Contains("Outline")) {
            poi.SetFloat("_EnableOutlines", 1);
            // Here I assume that wherever a creator puts any texture in the outline color texture, they're emulating maintex blend
            if (lil.GetTexture("_OutlineTex") != null) {
                poi.SetFloat("_OutlineTintMix", 1);
            }
            poi.SetColor("_LineColor", lil.GetColor("_OutlineColor"));
            poi.SetFloat("_LineWidth", lil.GetFloat("_OutlineWidth"));
            poi.SetTexture("_OutlineMask", lil.GetTexture("_OutlineWidthMask"));
            poi.SetFloat("_Offset_Z", lil.GetFloat("_OutlineZBias"));
            poi.SetFloat("_OutlineFixWidth", lil.GetFloat("_OutlineFixWidth"));
            poi.SetFloat("_OutlineShadowStrength", lil.GetFloat("_OutlineEnableLighting"));
            poi.SetFloat("_OutlineClipAtZeroWidth", lil.GetFloat("_OutlineDeleteMesh"));
        }

        // Emission
        if (lil.GetFloat("_UseEmission") == 1) {
            poi.SetFloat("_EnableEmission", 1);
            // Lil has a base color slider but poi only has a toggle
            if (lil.GetFloat("_EmissionMainStrength") > 0.5f) {
                poi.SetFloat("_EmissionBaseColorAsMap", 1);
            }
            // Lil Mask strength is equivalent to poi emission strength
            poi.SetFloat("_EmissionStrength", lil.GetFloat("_EmissionBlend"));
            // Many creators use the "Color" texture as the "Mask" texture and vice versa but seems these are interchangeable so whatevs
            poi.SetTexture("_EmissionMask", lil.GetTexture("_EmissionBlendMask"));
            poi.SetTextureScale("_EmissionMask", lil.GetTextureScale("_EmissionBlendMask"));
            poi.SetTextureOffset("_EmissionMask", lil.GetTextureOffset("_EmissionBlendMask"));
            poi.SetColor("_EmissionMaskPan", lil.GetColor("_EmissionBlendMask_ScrollRotate") * 20f);// Poiyomi panning is 20 times slower than liltoon scroll

            poi.SetTexture("_EmissionMap", lil.GetTexture("_EmissionMap"));
            poi.SetTextureScale("_EmissionMap", lil.GetTextureScale("_EmissionColor"));
            poi.SetTextureOffset("_EmissionMap", lil.GetTextureOffset("_EmissionColor"));
            poi.SetColor("_EmissionMapPan", lil.GetColor("_EmissionMap_ScrollRotate") * 20f);

            poi.SetColor("_EmissionColor", lil.GetColor("_EmissionColor"));
            if (lil.GetColor("_EmissionBlink").r > 0) {
                poi.SetFloat("_EmissionBlinkingEnabled", 1);
                poi.SetFloat("_EmissiveBlink_Velocity", lil.GetColor("_EmissionBlink").b);
                poi.SetFloat("_EmissionBlinkingOffset", lil.GetColor("_EmissionBlink").a);
            }
        }
        // Emission 2nd
        if (lil.GetFloat("_UseEmission2nd") == 1) {
            poi.SetFloat("_EnableEmission1", 1);
            poi.SetFloat("_Emission1BaseColorAsMap", lil.GetFloat("_Emission2ndMainStrength"));
            poi.SetFloat("_Emission1Strength", lil.GetFloat("_Emission2ndBlend"));
            poi.SetTexture("_EmissionMask1", lil.GetTexture("_Emission2ndMap"));
            poi.SetColor("_EmissionColor1", lil.GetColor("_Emission2ndColor"));
            poi.SetFloat("_EmissionStrength1", lil.GetFloat("_Emission2ndMainStrength"));
        }

        if (lil.shader.name.Contains("Refraction")) {
            // This is not at all a precise conversion but is better than nothing
            poi.SetFloat("_RefractionIndex", 1f + lil.GetFloat("_RefractionStrength"));
            poi.SetFloat("_RefractionFresnelPower", lil.GetFloat("_RefractionFresnelPower"));
            // Not sure how to transfer "Get color from main" or Color so these are left out
            if (lil.shader.name.Contains("Blur")) {
                poi.SetFloat("_EnableBlur", 1);
                // Kinda arbitrary values used here, looks fairly close to liltoon's blur
                poi.SetFloat("_GrabBlurDistance", 1);
                poi.SetFloat("_GrabBlurQuality", 6);
                poi.SetFloat("_GrabBlurDirections", 5);
            }
        }


        string newMaterialName = Path.GetDirectoryName(AssetDatabase.GetAssetPath(lil)) + "\\PoiConverted_" + lil.name + ".mat";
        AssetDatabase.CreateAsset(poi, newMaterialName);
    }
}
