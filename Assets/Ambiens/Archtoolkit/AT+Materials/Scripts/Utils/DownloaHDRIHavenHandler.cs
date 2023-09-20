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
    public class DownloadHDRIHavenHandler : MonoBehaviour
    {
        public enum RenderPipelineType
        {
            Builtin,
            HDRP,
            URP,
        }

        public static RenderPipelineType CurrentRenderPipeline = RenderPipelineType.Builtin;
        public static Dictionary<RenderPipelineType, string> materialMaps = new Dictionary<
            RenderPipelineType,
            string
        >()
        {
            { RenderPipelineType.Builtin, "Skybox/Panoramic" },
            { RenderPipelineType.HDRP, "Skybox/Panoramic" },
            { RenderPipelineType.URP, "Skybox/Panoramic" }
        };

        private float maxDistance = 20;

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

        public static void Get(
            DownloadsItem downloadsItem,
            string url,
            string relativeAssetPath,
            string assetPath,
            string name,
            Action<float> OnProgress,
            Action<string, DownloadsItem> onComplete
        )
        {
            var gameObject = new GameObject("Download HDRI handler");

            var tHandler = gameObject.AddComponent<DownloadHDRIHavenHandler>();

            EditorApplication.update += tHandler.Update;

            tHandler.StartCoroutine(
                tHandler.DownloadTexture(
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

        public IEnumerator DownloadTexture(
            DownloadsItem downloadsItem,
            string url,
            string relativeAssetPath,
            string assetPath,
            string name,
            Action<float> onProgress,
            Action<string, DownloadsItem> onComplete
        )
        {
            string filepath = "";

            if (string.IsNullOrEmpty(url))
            {
                if (onComplete != null)
                    onComplete("empty url", downloadsItem);

                yield return null;
            }
            else
            {
#if !UNITY_2018_3_OR_NEWER

                using (WWW www = new WWW(url))
                {
                    onProgress(www.progress);
                    yield return www;

                    foreach (var h in www.responseHeaders)
                    {
                        Debug.Log(h.Key + " " + h.Value);
                    }

                    if (!Directory.Exists(assetPath))
                    {
                        Directory.CreateDirectory(assetPath);
                    }
                    var b = www.bytes;

                    filepath =
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
                    try
                    {
                        AssetDatabase.Refresh();
                    }
                    catch (Exception e)
                    {
                        EditorUtility.DisplayDialog("Error", e.Message, "Ok!");
                    }

                    relativeAssetsPath.Add(relativeAssetPath);
                }

#else
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

                        filepath = assetPath + Path.GetFileNameWithoutExtension(url) + Path.GetExtension(url);
                        if (File.Exists(filepath))
                        {
                            File.Delete(filepath);
                        }
                        using (FileStream file = File.Open(filepath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            file.Write(b, 0, b.Length);
                            file.Close();
                        }

                        AssetDatabase.Refresh();

                    }
                    else{
                        Debug.Log("<color=red>AT+MATERIALS Error</color> Error Loading the file: "+www.error);
                        Debug.Log("<color=red>AT+MATERIALS Error</color> Download Handler Error: "+www.downloadHandler.error);
                    }
                }
#endif
            }

            yield return null;

            downloadsItem.materialLoaded = GetMaterial(
                relativeAssetPath + Path.GetFileNameWithoutExtension(url) + Path.GetExtension(url)
            );

            if (onComplete != null)
                onComplete(url, downloadsItem);

            DestroyImmediate(this.gameObject);
        }

        public static Material GetMaterial(string textureRelativePath)
        {
            var hdri = AssetDatabase.LoadAssetAtPath<Texture2D>(textureRelativePath);

            RefreshMaterialNames();

            Material mat = new Material(Shader.Find(materialMaps[CurrentRenderPipeline]));
            if (CurrentRenderPipeline != RenderPipelineType.HDRP)
            {
                mat.SetTexture("_MainTex", hdri);
            }
            else
            {
#if ATM_HDRP_SUPPORT
                TextureImporter textureImporter = AssetImporter.GetAtPath(textureRelativePath) as TextureImporter;
                textureImporter.textureShape = TextureImporterShape.TextureCube;
                AssetDatabase.ImportAsset(textureRelativePath);
#endif
            }

            return mat;
        }

        public static void RefreshMaterialNames()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                if (GraphicsSettings.renderPipelineAsset.name.ToLower().Contains("hd"))
                {
                    CurrentRenderPipeline = RenderPipelineType.HDRP;
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
                }
            }
        }
    }
}
#endif
