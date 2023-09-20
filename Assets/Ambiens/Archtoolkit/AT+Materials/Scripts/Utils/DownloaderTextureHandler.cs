#if UNITY_EDITOR
using System.Collections;
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Networking;
#endif

namespace ambiens.archtoolkit.atmaterials.utils
{
    [ExecuteInEditMode]
    public class DownloaderTextureHandler : MonoBehaviour
    {
        private float maxDistance = 20;

        public static void Get(string url, int w, int h, Action<string, Texture2D> onComplete)
        {
            var gameObject = new GameObject("Download Image handler");

            var tHandler = gameObject.AddComponent<DownloaderTextureHandler>();

            EditorApplication.update += tHandler.Update;

            //tHandler.StartCoroutine(tHandler.DownloadTexture(url, w, h, onComplete));
            tHandler.DownloadTexture(url, w, h, onComplete);
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

        private void DownloadTexture(string url, int w, int h, Action<string, Texture2D> onComplete)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (onComplete != null)
                    onComplete("url empty", null);
                DestroyImmediate(this.gameObject);
            }
            else
            {
                Texture2D tex;
                tex = new Texture2D(w, h, TextureFormat.RGB24, false);

                FileAssetCache.FetchCachedAssetFromUrl(
                    url,
                    () => { },
                    (string localCache) =>
                    {
                        //StartCoroutine(GetFromFile("file:///"+localCache, onComplete));
                        try
                        {
                            tex.LoadImage(File.ReadAllBytes(localCache)); //ImageCache.ReadAllBytes(fname));
                            tex.name = Path.GetFileName(localCache);
                            onComplete(string.Empty, tex);

                            DestroyImmediate(this.gameObject);
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("<color=red>AT+MATERIALS Error</color> " + ex.Message);
                            DestroyImmediate(this.gameObject);
                        }
                    },
                    (string error) =>
                    {
                        DestroyImmediate(this.gameObject);
                    },
                    (float p) => { },
                    this
                );
            }
        }
    }
}
#endif
