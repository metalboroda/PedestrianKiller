#if UNITY_EDITOR

// Disable 'obsolete' warnings
#pragma warning disable 0618
#pragma warning disable 0612

using System;
using UnityEngine;

namespace UnityEditor
{
    public class DecaleryStandardPBRShaderGUI : ShaderGUI
    {
        public enum Mode
        {
            Default,
            NormalOnly,
            NormalAndOcclusion
        }

        public enum BakeryMode
        {
            None,
            SH,
            MonoSH,
            Volume
        }

        public enum ColorUV
        {
            UV1,
            UV3
        }

        public static readonly string[] modeNames = Enum.GetNames(typeof(Mode));
        public static readonly string[] bakeryModeNames = Enum.GetNames(typeof(BakeryMode));
        public static readonly string[] uvNames = Enum.GetNames(typeof(ColorUV));

        MaterialProperty pMainTex, pColor, pBumpMap, pGlossMap, pMetallicMap, pAOMap, pGlossiness, pMetallic, pNormalIntensity, pBias, pEnableNormalMap, pEnableSpecular, pEnableReflections, pSH, pMonoSH, pVolume, pNormalOnly, pNormalAndOcclusion, pBlendSrc, pBlendDst, pBlendSrcA, pBlendDstA, pBlendSrcE, pBlendDstE, pColorMask, pUV2;
        MaterialProperty pTexArray, pMainTexArray, pBumpMapArray, pGlossMapArray, pMetallicMapArray, pAOMapArray;
        MaterialEditor m_MaterialEditor;

        public void FindProperties(MaterialProperty[] props)
        {
            pMainTex = FindProperty("_MainTex", props);
            pColor = FindProperty("_Color", props);
            pBumpMap = FindProperty("_BumpMap", props);
            pGlossMap = FindProperty("_GlossMap", props);
            pMetallicMap = FindProperty("_MetallicMap", props);
            pAOMap = FindProperty("_AOMap", props);
            pGlossiness = FindProperty("_Glossiness", props);
            pMetallic = FindProperty("_Metallic", props);
            pNormalIntensity = FindProperty("_NormalIntensity", props);
            pBias = FindProperty("_Bias", props);
            pEnableNormalMap = FindProperty("_NORMALMAP", props);
            pEnableSpecular = FindProperty("_SPECULAR", props);
            pEnableReflections = FindProperty("_REFLECTIONS", props);
            pSH = FindProperty("_BAKERY_SH", props);
            pMonoSH = FindProperty("_BAKERY_MONOSH", props);
            pVolume = FindProperty("_BAKERY_VOLUME", props);
            pNormalOnly = FindProperty("_MODE_NORMALONLY", props);
            pNormalAndOcclusion = FindProperty("_MODE_NORMAL_AO_ONLY", props);
            pUV2 = FindProperty("_USEUV2", props);

            pBlendSrc = FindProperty("_BlendSrc", props);
            pBlendDst = FindProperty("_BlendDst", props);
            pBlendSrcA = FindProperty("_BlendSrcA", props);
            pBlendDstA = FindProperty("_BlendDstA", props);
            pBlendSrcE = FindProperty("_BlendSrcE", props);
            pBlendDstE = FindProperty("_BlendDstE", props);
            pColorMask = FindProperty("__ColorMask", props);

            pTexArray = FindProperty("_TEXARRAY", props);
            pMainTexArray = FindProperty("_MainTexArray", props);
            pBumpMapArray = FindProperty("_BumpMapArray", props);
            pGlossMapArray = FindProperty("_GlossMapArray", props);
            pMetallicMapArray = FindProperty("_MetallicMapArray", props);
            pAOMapArray = FindProperty("_AOMapArray", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props);
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            EditorGUI.BeginChangeCheck();
            {
                m_MaterialEditor.ShaderProperty(pBias, new GUIContent("Depth offset"));
                EditorGUILayout.Space();
                if (pTexArray.floatValue > 0)
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Color array (RGB), Opacity (A)"), pMainTexArray, pColor);
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Normal map array"), pBumpMapArray);
                }
                else
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Color (RGB), Opacity (A)"), pMainTex, pColor);
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Normal map"), pBumpMap);
                }
                m_MaterialEditor.ShaderProperty(pEnableNormalMap, new GUIContent("Normal map enabled"), 2);
                m_MaterialEditor.ShaderProperty(pNormalIntensity, new GUIContent("Normal map intensity"), 2);
                if (pTexArray.floatValue > 0)
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Gloss map array"), pGlossMapArray);
                }
                else
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Gloss map"), pGlossMap);
                }
                m_MaterialEditor.ShaderProperty(pGlossiness, new GUIContent("Glossiness"), 2);
                if (pTexArray.floatValue > 0)
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Metallic map array"), pMetallicMapArray);
                }
                else
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Metallic map"), pMetallicMap);
                }
                m_MaterialEditor.ShaderProperty(pMetallic, new GUIContent("Metalness"), 2);
                if (pTexArray.floatValue > 0)
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("AO map array"), pAOMapArray);
                }
                else
                {
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("AO map"), pAOMap);
                }
                EditorGUILayout.Space();
                m_MaterialEditor.ShaderProperty(pEnableSpecular, new GUIContent("Specular (forward)"));
                m_MaterialEditor.ShaderProperty(pEnableReflections, new GUIContent("Reflections (forward)"));
                m_MaterialEditor.ShaderProperty(pTexArray, new GUIContent("Use Texture Arrays"));
            }
            if (EditorGUI.EndChangeCheck())
            {
            }

            EditorGUI.BeginChangeCheck();
            int uvMode = EditorGUILayout.Popup(new GUIContent("Main UV channel", "Affects color, gloss, metallic and AO. Doesn't affect normal map or lightmap."), (int)pUV2.floatValue, uvNames);
            if (EditorGUI.EndChangeCheck())
            {
                pUV2.floatValue = (float)uvMode;
                if (uvMode == 0)
                {
                    material.DisableKeyword("USEUV2");
                }
                else
                {
                    material.EnableKeyword("USEUV2");
                }
            }

            EditorGUI.showMixedValue = pSH.hasMixedValue || pMonoSH.hasMixedValue || pVolume.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            BakeryMode bmode = BakeryMode.None;
            if (pSH.floatValue > 0)
            {
                bmode = BakeryMode.SH;
            }
            else if (pMonoSH.floatValue > 0)
            {
                bmode = BakeryMode.MonoSH;
            }
            else if (pVolume.floatValue > 0)
            {
                bmode = BakeryMode.Volume;
            }
            bmode = (BakeryMode)EditorGUILayout.Popup("Bakery mode", (int)bmode, bakeryModeNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (bmode == BakeryMode.SH)
                {
                    pSH.floatValue = 1.0f;
                    pMonoSH.floatValue = 0;
                    pVolume.floatValue = 0;
                    material.EnableKeyword("BAKERY_SH");
                    material.DisableKeyword("BAKERY_MONOSH");
                    material.DisableKeyword("BAKERY_VOLUME");
                }
                else if (bmode == BakeryMode.MonoSH)
                {
                    pSH.floatValue = 0;
                    pMonoSH.floatValue = 1.0f;
                    pVolume.floatValue = 0;
                    material.DisableKeyword("BAKERY_SH");
                    material.EnableKeyword("BAKERY_MONOSH");
                    material.DisableKeyword("BAKERY_VOLUME");
                }
                else if (bmode == BakeryMode.Volume)
                {
                    pSH.floatValue = 0;
                    pMonoSH.floatValue = 0;
                    pVolume.floatValue = 1.0f;
                    material.DisableKeyword("BAKERY_SH");
                    material.DisableKeyword("BAKERY_MONOSH");
                    material.EnableKeyword("BAKERY_VOLUME");
                }
                else
                {
                    pSH.floatValue = 0;
                    pMonoSH.floatValue = 0;
                    pVolume.floatValue = 0;
                    material.DisableKeyword("BAKERY_SH");
                    material.DisableKeyword("BAKERY_MONOSH");
                    material.DisableKeyword("BAKERY_VOLUME");
                }
            }
            EditorGUI.showMixedValue = false;

            EditorGUI.showMixedValue = pNormalOnly.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            Mode mode = Mode.Default;
            if (pNormalOnly.floatValue > 0)
            {
                mode = Mode.NormalOnly;
            }
            else if (pNormalAndOcclusion.floatValue > 0)
            {
                mode = Mode.NormalAndOcclusion;
            }
            mode = (Mode)EditorGUILayout.Popup("Deferred mode", (int)mode, modeNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (mode == Mode.NormalOnly)
                {
                    pNormalOnly.floatValue = 1.0f;
                    pNormalAndOcclusion.floatValue = 0;
                    material.EnableKeyword("MODE_NORMALONLY");
                    material.DisableKeyword("MODE_NORMAL_AO_ONLY");

                    pBlendSrc.floatValue = (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
                    pBlendDst.floatValue = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                    pBlendSrcA.floatValue = (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
                    pBlendDstA.floatValue = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                    pBlendSrcE.floatValue = (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
                    pBlendDstE.floatValue = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                    pColorMask.floatValue = 14;
                }
                else if (mode == Mode.NormalAndOcclusion)
                {
                    pNormalOnly.floatValue = 0;
                    pNormalAndOcclusion.floatValue = 1.0f;
                    material.DisableKeyword("MODE_NORMALONLY");
                    material.EnableKeyword("MODE_NORMAL_AO_ONLY");

                    pBlendSrc.floatValue = (int)UnityEngine.Rendering.BlendMode.Zero;
                    pBlendDst.floatValue = (int)UnityEngine.Rendering.BlendMode.One;
                    pBlendSrcA.floatValue = (int)UnityEngine.Rendering.BlendMode.DstAlpha;
                    pBlendDstA.floatValue = (int)UnityEngine.Rendering.BlendMode.Zero;
                    pBlendSrcE.floatValue = (int)UnityEngine.Rendering.BlendMode.DstColor;
                    pBlendDstE.floatValue = (int)UnityEngine.Rendering.BlendMode.Zero;
                    pColorMask.floatValue = 15;
                }
                else
                {
                    pNormalOnly.floatValue = 0;
                    pNormalAndOcclusion.floatValue = 0;
                    material.DisableKeyword("MODE_NORMALONLY");
                    material.DisableKeyword("MODE_NORMAL_AO_ONLY");

                    pBlendSrc.floatValue = (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
                    pBlendDst.floatValue = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                    pBlendSrcA.floatValue = (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
                    pBlendDstA.floatValue = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                    pBlendSrcE.floatValue = (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
                    pBlendDstE.floatValue = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                    pColorMask.floatValue = 14;
                }
            }
            EditorGUI.showMixedValue = false;

            int prevQueue = material.renderQueue;
            int newQueue = EditorGUILayout.IntField("Render Queue", prevQueue);//, 1000, 5000);
            if (prevQueue != newQueue)
            {
                material.renderQueue = newQueue;
            }
        }
    }
}

#endif
