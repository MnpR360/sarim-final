#if UNITY_EDITOR

using System.Collections;
using System;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using ambiens.archtoolkit.atmaterials.texturestructure;
using UnityEngine.Rendering;
using ambiens.archtoolkit.atmaterials.TexPacker;

namespace ambiens.archtoolkit.atmaterials.utils
{
    [ExecuteInEditMode]
    public class Downloadcc0TexturesHandler : MonoBehaviour
    {
        public enum RenderPipelineType
        {
            Builtin,
            HDRP,
            URP,
        }

        public enum MaterialType
        {
            Standard,
            StandardSpecular,
            Transparent
        }

        public static RenderPipelineType CurrentRenderPipeline = RenderPipelineType.Builtin;
        public static Dictionary<
            RenderPipelineType,
            Dictionary<MaterialType, string>
        > materialMaps = new Dictionary<RenderPipelineType, Dictionary<MaterialType, string>>()
        {
            {
                RenderPipelineType.Builtin,
                new Dictionary<MaterialType, string>()
                {
                    { MaterialType.Standard, "Standard" },
                    { MaterialType.StandardSpecular, "Standard (Specular setup)" }
                }
            },
            {
                RenderPipelineType.HDRP,
                new Dictionary<MaterialType, string>()
                {
                    { MaterialType.Standard, "HDRP/Lit" },
                    { MaterialType.StandardSpecular, "HDRP/Lit" }
                }
            },
            {
                RenderPipelineType.URP,
                new Dictionary<MaterialType, string>()
                {
                    { MaterialType.Standard, "Universal Render Pipeline/Lit" },
                    { MaterialType.StandardSpecular, "Universal Render Pipeline/Lit" }
                }
            }
        };

        private float maxDistance = 20;

        public static void GetAndUnZip(
            DownloadsItem downloadsItem,
            string url,
            string relativeAssetPath,
            string assetPath,
            string name,
            Action<float> OnProgress,
            Action<string, DownloadsItem> onComplete
        )
        {
            var gameObject = new GameObject("Download Zip handler");

            var tHandler = gameObject.AddComponent<Downloadcc0TexturesHandler>();

            EditorApplication.update += tHandler.Update;

            tHandler.StartCoroutine(
                tHandler.DownloadZip(
                    downloadsItem,
                    url,
                    relativeAssetPath,
                    assetPath,
                    name,
                    OnProgress,
                    onComplete
                )
            );
        }

        public static void GetTextureFromSet(
            DownloadsItem downloadsItem,
            string startingUrl,
            List<string> set,
            string relativeAssetPath,
            string assetPath,
            string name,
            Action<float> OnProgress,
            Action<string, DownloadsItem> onComplete
        )
        {
            var gameObject = new GameObject("Download textures handler");

            var tHandler = gameObject.AddComponent<Downloadcc0TexturesHandler>();

            EditorApplication.update += tHandler.Update;

            tHandler.StartCoroutine(
                tHandler.DownloadTexture(
                    downloadsItem,
                    startingUrl,
                    set,
                    relativeAssetPath,
                    assetPath,
                    name,
                    OnProgress,
                    onComplete
                )
            );
        }

        private void Update()
        {
            if (this.transform.position.x >= this.maxDistance)
            {
                this.transform.position = Vector3.zero;
                return;
            }

            this.transform.Translate(new Vector3(0.1f, 0, 0));
        }

        private void OnDestroy()
        {
            EditorApplication.update -= this.Update;
        }

        private IEnumerator DownloadTexture(
            DownloadsItem downloadsItem,
            string startingUrl,
            List<string> set,
            string relativeAssetPath,
            string assetPath,
            string name,
            Action<float> onProgress,
            Action<string, DownloadsItem> onComplete
        )
        {
            List<string> relativeAssetsPath = new List<string>();

            foreach (var url in set)
            {
                string remoteUrl = string.Empty;

                if (!url.StartsWith("https") || !url.StartsWith("http")) // if it not start with http, means that url must be composed
                    remoteUrl = startingUrl + url;
                else
                    remoteUrl = url;

                using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return www.SendWebRequest();

                    yield return www.isDone;

                    if (string.IsNullOrEmpty(www.error))
                    {
                        if (!Directory.Exists(assetPath))
                        {
                            Directory.CreateDirectory(assetPath);
                        }
                        var b = www.downloadHandler.data;

                        var filepath =
                            assetPath
                            + Path.GetFileNameWithoutExtension(remoteUrl)
                            + Path.GetExtension(remoteUrl);
                        if (File.Exists(filepath))
                        {
                            File.Delete(filepath);
                        }
                        using (
                            FileStream file = File.Open(
                                filepath,
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.None
                            )
                        )
                        {
                            file.Write(b, 0, b.Length);
                            file.Close();
                        }

                        AssetDatabase.Refresh();

                        relativeAssetsPath.Add(relativeAssetPath);
                    }
                }
            }

            yield return null;

            if (relativeAssetsPath.Count == set.Count)
            {
                downloadsItem.materialLoaded = GetMaterial(relativeAssetsPath[0]);

                if (onComplete != null)
                    onComplete(set[0], downloadsItem);
            }

            DestroyImmediate(this.gameObject);
        }

        private IEnumerator DownloadZip(
            DownloadsItem downloadsItem,
            string url,
            string relativeAssetPath,
            string assetPath,
            string name,
            Action<float> onProgress,
            Action<string, DownloadsItem> onComplete
        )
        {
            if (string.IsNullOrEmpty(url))
            {
                if (onComplete != null)
                    onComplete("empty url", downloadsItem);

                yield return null;
            }
            else
            {
                using (UnityWebRequest www = UnityWebRequest.Get(url))
                {
                    yield return www.SendWebRequest();

                    yield return www.isDone;

                    if (string.IsNullOrEmpty(www.error))
                    {
                        if (!Directory.Exists(assetPath))
                        {
                            Directory.CreateDirectory(assetPath);
                        }
                        var b = www.downloadHandler.data;
                        string filePath = assetPath + name + ".zip";
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        using (
                            FileStream file = File.Open(
                                filePath,
                                FileMode.CreateNew,
                                FileAccess.Write,
                                FileShare.None
                            )
                        )
                        {
                            file.Write(b, 0, b.Length);
                            file.Close();
                        }

                        AssetDatabase.Refresh();

                        ZipUtil.Unzip(filePath, assetPath);

                        File.Delete(filePath);

                        AssetDatabase.Refresh();

                        downloadsItem.materialLoaded = GetMaterial(relativeAssetPath);
                        if (onComplete != null)
                            onComplete(url, downloadsItem);
                    }
                    else
                    {
                        Debug.Log(
                            "<color=red>AT+MATERIALS Error</color> Download of file "
                                + url
                                + " failed! => "
                                + www.error
                        );
                    }
                }

                DestroyImmediate(this.gameObject);
            }
        }

        public static Material GetMaterial(string texturesPath)
        {
            var textures = ATMatStringUtils.GetAtPath<Texture>(texturesPath);

            RefreshMaterialNames();

            var tList = textures.ToList();

            Texture albedo = tList.Find(
                t =>
                    t.name.ContainsAny(
                        new string[]
                        {
                            "col",
                            "Albedo",
                            "albedo",
                            "alb",
                            "Base_Color",
                            "Color",
                            "Base",
                            "diff",
                            "diffuse",
                            "Diffuse"
                        }
                    )
            );
            Texture normalMap = tList.Find(
                t =>
                    t.name.ContainsAny(
                        new string[]
                        {
                            "nMap",
                            "nrm",
                            "NormalMap",
                            "Normal",
                            "normal",
                            "nor",
                            "nrml",
                            "Nor"
                        }
                    )
            );
            Texture occlusion = tList.Find(
                t => t.name.ContainsAny(new string[] { "AO", "occ", "ao", "AmbientOcclusion" })
            );
            Texture heightMap = tList.Find(
                t =>
                    t.name.ContainsAny(
                        new string[]
                        {
                            "Disp",
                            "disp",
                            "Displacement",
                            "heightmap",
                            "Height",
                            "bump",
                            "bumpmap"
                        }
                    )
            );
            Texture roughness = tList.Find(
                t => t.name.ContainsAny(new string[] { "roughness", "rgh", "Roughness", "rough" })
            );
            Texture metallic = tList.Find(
                t =>
                    t.name.ContainsAny(
                        new string[] { "Metallic", "metallic", "metal", "Metalness" }
                    )
            );

            Texture opacity = tList.Find(
                t => t.name.ContainsAny(new string[] { "Opacity", "opacity" })
            );

            Material mat = null;

            if (metallic != null)
            {
                mat = new Material(
                    Shader.Find(materialMaps[CurrentRenderPipeline][MaterialType.Standard])
                );

                if (CurrentRenderPipeline == RenderPipelineType.URP)
                {
                    mat.DisableKeyword("_SPECULAR_SETUP");
                    mat.SetFloat("_WorkflowMode", 1);
                }
            }
            else if (roughness != null)
            {
                mat = new Material(
                    Shader.Find(materialMaps[CurrentRenderPipeline][MaterialType.Standard])
                );
            }

            if (mat == null)
                mat = new Material(
                    Shader.Find(materialMaps[CurrentRenderPipeline][MaterialType.Standard])
                );

            if (opacity != null && albedo != null)
            {
                TexturePacker texPacker = new TexturePacker();
                texPacker.Initialize();

                opacity = SetSingleChannelSettings(opacity);
                Debug.Log(opacity);
                var iO = new TextureInput();
                iO.texture = (Texture2D)opacity;
                iO.SetChannelInput(
                    TextureChannel.ChannelRed,
                    new TextureChannelInput(TextureChannel.ChannelAlpha, true)
                );

                texPacker.Add(iO);

                var iA = new TextureInput();
                iA.texture = (Texture2D)albedo;
                iA.SetChannelInput(
                    TextureChannel.ChannelRed,
                    new TextureChannelInput(TextureChannel.ChannelRed, true)
                );
                iA.SetChannelInput(
                    TextureChannel.ChannelGreen,
                    new TextureChannelInput(TextureChannel.ChannelGreen, true)
                );
                iA.SetChannelInput(
                    TextureChannel.ChannelBlue,
                    new TextureChannelInput(TextureChannel.ChannelBlue, true)
                );

                texPacker.Add(iA);

                var output = texPacker.Create();
                output.alphaIsTransparency = true;
                var maskPath = Path.Combine(
                    Application.dataPath.Replace("Assets", "") + texturesPath,
                    "Albedo_Opaque.png"
                );
                File.WriteAllBytes(maskPath, output.EncodeToPNG());
                AssetDatabase.Refresh();
                var localPath = maskPath.Replace(Application.dataPath.Replace("Assets", ""), "");
                Debug.Log(localPath);
                albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(localPath);

                mat.SetFloat("_SurfaceType", 1);
                mat.SetFloat("_AlphaCutoffEnable", 1);
                mat.SetFloat("_AlphaCutoff", 0.5f);

                var keywords =
                    "_BLENDMODE_ALPHA _BLENDMODE_PRESERVE_SPECULAR_LIGHTING _DISPLACEMENT_LOCK_TILING_SCALE _ENABLE_FOG_ON_TRANSPARENT _HEIGHTMAP _MASKMAP _NORMALMAP _NORMALMAP_TANGENT_SPACE _SURFACE_TYPE_TRANSPARENT _VERTEX_DISPLACEMENT _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE";

                foreach (var k in keywords.Split(' '))
                {
                    mat.EnableKeyword(k);
                }
            }

            //Create Mask Map
            if (CurrentRenderPipeline == RenderPipelineType.HDRP)
            {
                TexturePacker texPacker = new TexturePacker();
                texPacker.Initialize();

                if (roughness != null)
                {
                    Debug.Log("ROUGHNESS -> " + roughness);
                    //roughness= SetSingleChannelSettings(roughness);
                    var smoothPath = Path.Combine(
                        Application.dataPath.Replace("Assets", "") + texturesPath,
                        "Smoothness.png"
                    );
                    var smoothness = TextureUtility.GetSmoothnessFromRoughness(
                        roughness as Texture2D,
                        smoothPath
                    );

                    var i = new TextureInput();
                    i.texture = (Texture2D)smoothness;
                    i.SetChannelInput(
                        TextureChannel.ChannelRed,
                        new TextureChannelInput(TextureChannel.ChannelAlpha, true)
                    );

                    texPacker.Add(i);

                    var keywords =
                        "_DISPLACEMENT_LOCK_TILING_SCALE _HEIGHTMAP _MASKMAP _NORMALMAP _NORMALMAP_TANGENT_SPACE _VERTEX_DISPLACEMENT _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE";

                    foreach (var k in keywords.Split(' '))
                    {
                        mat.EnableKeyword(k);
                    }

                    mat.SetFloat("_SmoothnessRemapMax", 0.7f);
                    mat.SetFloat("_SmoothnessRemapMin", 0.3f);
                }

                if (occlusion != null)
                {
                    occlusion = SetSingleChannelSettings(occlusion);
                    //Debug.Log("AO -> "+occlusion);
                    var i = new TextureInput();
                    i.texture = (Texture2D)occlusion;
                    var c = new TextureChannelInput(TextureChannel.ChannelGreen, true);
                    i.SetChannelInput(TextureChannel.ChannelRed, c);
                    texPacker.Add(i);
                }
                else
                {
                    mat.SetFloat("_AORemapMax", 1);
                    mat.SetFloat("_AORemapMin", 0.6f);
                }
                if (metallic != null)
                {
                    metallic = SetSingleChannelSettings(metallic);

                    //Debug.Log(metallic);
                    var i = new TextureInput();
                    i.texture = (Texture2D)metallic;
                    i.SetChannelInput(
                        TextureChannel.ChannelRed,
                        new TextureChannelInput(TextureChannel.ChannelRed, true)
                    );
                    texPacker.Add(i);

                    mat.SetFloat("_Metallic", 1);
                }

                var output = texPacker.Create();
                output.alphaIsTransparency = true;
                var maskPath = Path.Combine(
                    Application.dataPath.Replace("Assets", "") + texturesPath,
                    "Mask.png"
                );
                File.WriteAllBytes(maskPath, output.EncodeToPNG());
                AssetDatabase.Refresh();
                var localPath = maskPath.Replace(Application.dataPath.Replace("Assets", ""), "");
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(localPath);
                mat.SetTexture("_MaskMap", texture);
            }
            else if (CurrentRenderPipeline == RenderPipelineType.URP)
            {
                TexturePacker texPacker = new TexturePacker();
                texPacker.Initialize();

                if (roughness != null)
                {
                    //roughness= SetSingleChannelSettings(roughness);
                    var smoothPath = Path.Combine(
                        Application.dataPath.Replace("Assets", "") + texturesPath,
                        "Smoothness.png"
                    );
                    var smoothness = TextureUtility.GetSmoothnessFromRoughness(
                        roughness as Texture2D,
                        smoothPath
                    );

                    var i = new TextureInput();
                    i.texture = (Texture2D)smoothness;
                    i.SetChannelInput(
                        TextureChannel.ChannelRed,
                        new TextureChannelInput(TextureChannel.ChannelAlpha, true)
                    );

                    texPacker.Add(i);

                    var keywords =
                        "_DISPLACEMENT_LOCK_TILING_SCALE _HEIGHTMAP _MASKMAP _NORMALMAP _NORMALMAP_TANGENT_SPACE _VERTEX_DISPLACEMENT _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE";

                    foreach (var k in keywords.Split(' '))
                    {
                        mat.EnableKeyword(k);
                    }

                    mat.SetFloat("_Smoothness", 1);
                }

                var output = texPacker.Create();
                output.alphaIsTransparency = true;
                var maskPath = Path.Combine(
                    Application.dataPath.Replace("Assets", "") + texturesPath,
                    "MetallicGloss.png"
                );
                File.WriteAllBytes(maskPath, output.EncodeToPNG());
                AssetDatabase.Refresh();
                var localPath = maskPath.Replace(Application.dataPath.Replace("Assets", ""), "");
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(localPath);
                mat.SetTexture("_MetallicGlossMap", texture);
            }

            if (albedo != null)
            {
                mat.SetTexture(mat.GetAlbedoProperty(), albedo);
            }

            if (normalMap != null)
            {
                var path = AssetDatabase.GetAssetPath(normalMap.GetInstanceID());

                if (!string.IsNullOrEmpty(path))
                {
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    importer.textureType = TextureImporterType.NormalMap;
                    AssetDatabase.WriteImportSettingsIfDirty(path);
                }

                mat.EnableKeyword("_NORMALMAP");
                //mat.SetTexture("_BumpMap", normalMap);
                mat.SetTexture(mat.GetNormalMapProperty(), normalMap);
            }

            if (occlusion != null && mat.GetOcclusionMapProperty() != null)
            {
                mat.SetTexture(mat.GetOcclusionMapProperty(), occlusion);
            }

            if (heightMap != null && mat.GetDisplacementMapProperty() != null)
            {
                mat.SetTexture(mat.GetDisplacementMapProperty(), heightMap);
                if (CurrentRenderPipeline == RenderPipelineType.HDRP)
                {
                    mat.SetFloat("_DisplacementMode", 1);

                    mat.SetFloat("_HeightMapParametrization", 0);
                    mat.SetFloat("_HeightMax", 0.005f);
                    mat.SetFloat("_HeightMin", 0);
                    mat.SetFloat("_HeightAmplitude", 0);
                    mat.SetFloat("_HeightCenter", 0.005f);
                }
            }

            if (CurrentRenderPipeline == RenderPipelineType.Builtin)
            {
                if (roughness != null)
                {
                    var smoothNessPath = Path.Combine(
                        Application.dataPath.Replace("Assets", "") + texturesPath,
                        "Smoothness.png"
                    );

                    var smoothness = TextureUtility.GetSmoothnessFromRoughness(
                        roughness as Texture2D,
                        smoothNessPath
                    );

                    TexturePacker texPacker = new TexturePacker();
                    texPacker.Initialize();

                    var i = new TextureInput();
                    i.texture = (Texture2D)smoothness;
                    i.SetChannelInput(
                        TextureChannel.ChannelRed,
                        new TextureChannelInput(TextureChannel.ChannelAlpha, true)
                    );

                    texPacker.Add(i);

                    var output = texPacker.Create();
                    output.alphaIsTransparency = true;
                    var maskPath = Path.Combine(
                        Application.dataPath.Replace("Assets", "") + texturesPath,
                        "Metallic.png"
                    );
                    File.WriteAllBytes(maskPath, output.EncodeToPNG());
                    AssetDatabase.Refresh();
                    var localPath = maskPath.Replace(
                        Application.dataPath.Replace("Assets", ""),
                        ""
                    );
                    var MetallicMap = AssetDatabase.LoadAssetAtPath<Texture2D>(localPath);

                    mat.SetTexture("_MetallicGlossMap", MetallicMap);
                }
            }

            /*
            if (metallic != null && mat.GetMetallicMapProperty()!=null)
            {
                mat.SetTexture(mat.GetMetallicMapProperty(), metallic);
            }
            */
            return mat;
        }

        private static Texture2D SetSingleChannelSettings(Texture texture)
        {
            var p = AssetDatabase.GetAssetPath(texture.GetInstanceID());
            TextureImporter importer = AssetImporter.GetAtPath(p) as TextureImporter;

            importer = AssetImporter.GetAtPath(p) as TextureImporter;
            var s = new TextureImporterSettings();
            importer.ReadTextureSettings(s);

            s.ApplyTextureType(TextureImporterType.SingleChannel);
            s.singleChannelComponent = TextureImporterSingleChannelComponent.Red;
            importer.SetTextureSettings(s);
            AssetDatabase.WriteImportSettingsIfDirty(p);

            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }

        public static void RefreshMaterialNames()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                if (GraphicsSettings.renderPipelineAsset.name.ToLower().Contains("hd"))
                {
                    CurrentRenderPipeline = RenderPipelineType.HDRP;
                    //Very brutal check for HDRP Preview material
                    var s = Shader.Find(materialMaps[CurrentRenderPipeline][MaterialType.Standard]);
                    if (s == null)
                    {
                        materialMaps[CurrentRenderPipeline][MaterialType.Standard] =
                            "HDRenderPipeline/Lit";
                        materialMaps[CurrentRenderPipeline][MaterialType.StandardSpecular] =
                            "HDRenderPipeline/Lit";
                    }
                }
                else if (
                    GraphicsSettings.renderPipelineAsset.name.ToLower().Contains("universal")
                    || GraphicsSettings.renderPipelineAsset.name.ToLower().Contains("urp")
                )
                {
                    CurrentRenderPipeline = RenderPipelineType.URP;
                }
                else if (GraphicsSettings.renderPipelineAsset.name.ToLower().Contains("lwrp"))
                {
                    CurrentRenderPipeline = RenderPipelineType.URP;
                    //Very brutal check for LWRP Preview material
                    var s = Shader.Find(materialMaps[CurrentRenderPipeline][MaterialType.Standard]);
                    if (s == null)
                    {
                        materialMaps[CurrentRenderPipeline][MaterialType.Standard] =
                            "Lightweight Render Pipeline/Lit";
                        materialMaps[CurrentRenderPipeline][MaterialType.StandardSpecular] =
                            "Lightweight Render Pipeline/Lit";
                    }
                }
            }
        }
    }
}
#endif
