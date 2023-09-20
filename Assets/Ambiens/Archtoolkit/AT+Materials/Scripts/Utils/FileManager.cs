using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Networking;

namespace ambiens.archtoolkit.atmaterials.utils
{
    public static class FileAssetCache
    {

        public static string PluginCachePath()
        {
            return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmbiensPlugins"), "ATMaterials");
        }
        public static string GetCacheName(string url)
        {

            var split = Path.GetExtension(url).Split('?');
            var realUrl = url.Split('?')[0];
            var ext = split[0];
            //var cache = (split.Length > 1) ? split[1] : "";
            return Path.Combine(Path.Combine(PluginCachePath(), "txtcache"), ("c_" + md5(realUrl) + ext));

        }
        public static string md5(string input)
        {
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static bool AlreadyCached(string onlineUrl)
        {
            string fname = GetCacheName(onlineUrl);
            
            if (fname != "" && System.IO.File.Exists(fname))
            {
                return true;
            }

            return false;
        }

        public static void SaveCacheAssetFromUrl(string localUrl, string onlineUrl,
            Action OnStart,
            Action<string> OnComplete,
            Action<string> OnError,
            Action<float> OnProgress,
            MonoBehaviour caller)
        {
            string fname = GetCacheName(onlineUrl);

            if (fname != "" && System.IO.File.Exists(fname))
            {
                
                OnComplete(fname);
            }
            else
            {
                caller.StartCoroutine(FileAssetCache.fetchAssetFromUrl(localUrl, OnStart, OnComplete, OnProgress, OnError, fname));
            }
        }

        public static void FetchCachedAssetFromUrl(string url,
            Action OnStart,
            Action<string> OnComplete,
            Action<string> OnError,
            Action<float> OnProgress,
            MonoBehaviour caller, bool createCache = true)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string fname = (createCache) ? GetCacheName(url) : "";

                if (fname != "" && System.IO.File.Exists(fname))
                {
                    if (OnComplete != null)
                    {
                        OnComplete(fname);
                    }
                }
                else
                {
                    //Debug.Log("Getting file From url " + url);
                    caller.StartCoroutine(FileAssetCache.fetchAssetFromUrl(url, OnStart, OnComplete, OnProgress, OnError, fname));
                }
            }
            else
            {
                if (OnError != null)
                {
                    OnError("FileNameIsNull!");
                }
            }
        }

        private static IEnumerator fetchAssetFromUrl(string url,
            Action OnStart, Action<string> OnComplete, Action<float> OnProgress, Action<string> OnError, string filename = "")
        {

            UnityWebRequest webRequest = new UnityWebRequest(url);
            webRequest.downloadHandler = new DownloadHandlerFile(filename);

            OnStart();

            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                OnProgress(operation.progress);

                yield return null;
            }

            if (!string.IsNullOrEmpty(webRequest.error))
            {
                Debug.Log("<color=red>AT+MATERIALS Error</color> "+url+" "+webRequest.error);
                OnError(webRequest.error);
            }
            else
            {
                try
                {
                    if (OnComplete != null)
                    {
                        if (filename != "")
                            OnComplete(filename);
                        else
                            OnComplete(url.Replace("file://", ""));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    OnError(ex.Message);
                }
            }
            
        }



        private static byte[] ReadAllBytes(string filePath)
        {
            byte[] buffer = null;
            if (File.Exists(filePath))
            {
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                try
                {
                    int length = (int)fileStream.Length;  // get file length
                    buffer = new byte[length];            // create buffer
                    int count;                            // actual number of bytes read
                    int sum = 0;                          // total number of bytes read

                    // read until Read method returns 0 (end of the stream has been reached)
                    while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    {
                        sum += count;  // sum is a buffer offset for next reading
                    }
                }
                catch (Exception e)
                {
#if DEBUG
                    Debug.LogWarning("ReadAllBytes: exception on read file " + filePath + ", " + e.Message);
#endif
                }
                finally
                {
                    fileStream.Close();
                }
            }
            else
            {
#if DEBUG
                Debug.LogWarning("ReadAllBytes: can't read file " + filePath + ", it doesn't exists");
#endif
            }
            return buffer;
        }

        private static void WriteAllBytes(string filePath, byte[] b)
        {
            if (b != null)
            {
                Debug.Log(filePath);
                string path = filePath.Replace(@"\" + Path.GetFileName(filePath), "");
                Debug.Log(path);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (FileStream file = File.Open(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    file.Write(b, 0, b.Length);
                    file.Close();
                }
            }
            else
            {
                Debug.LogWarning("WriteAllBytes: can't save file " + filePath + ", byte[] is empty");
            }
        }
    }
}