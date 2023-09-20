#if UNITY_EDITOR
using System.Collections;
using System;
using UnityEngine;
using ambiens.archtoolkit.atmaterials.texturestructure;
using UnityEngine.Networking;

namespace ambiens.archtoolkit.atmaterials.utils
{
    [ExecuteInEditMode]
    public class DownloaderJsonHandler : MonoBehaviour
    {
        public const string URL = "https://www.ambiensvr.com/archtoolkit/atmaterials1.2/sources.json";

        public void Init(Action<JsonSources,string> onComplete)
        {
            UnityEditor.EditorApplication.update += this.Update;

            StartCoroutine(this.DownloadJson(URL,onComplete));
        }

        private void OnDestroy()
        {
            UnityEditor.EditorApplication.update -= this.Update;
        }

        private void Update()
        {
            this.transform.Translate(new Vector3(1,0,0));
        }

        private IEnumerator DownloadJson(string url,Action<JsonSources,string> onComplete)
        {

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                yield return www.isDone;

                if (string.IsNullOrEmpty(www.error))
                {
                    var json = www.downloadHandler.text;
                    var content = JsonUtility.FromJson<JsonSources>(json);
                    
                    if (onComplete != null)
                        onComplete(content,"");
                }
                else
                {
                    Debug.LogError("Error " + www.error);

                    if (onComplete != null)
                        onComplete(null,www.error);
                }

                DestroyImmediate(this.gameObject);
            }
        }

    }
}
#endif