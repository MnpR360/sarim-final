#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace ambiens.archtoolkit.atmaterials.utils
{

    public static class ATMatStringUtils
    {
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }

        public static string CleanFileName(string fileName)
        {
            var stringClean = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

            // This is because textureHeaven has special character 
            if (stringClean.Contains("&sdot"))
            {
                int index = stringClean.IndexOf("&sdot");
                if (index > 0)
                    stringClean = stringClean.Substring(0, index);

                stringClean = stringClean.Replace("&sdot", "");
            }

            return stringClean;
        }

        public static T[] GetAtPath<T>(string path)
        {
            //var pathWithoutAsset = path.Replace("Assets", "");
            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles(Application.dataPath.Replace("Assets", "") + path);
            foreach (string fileName in fileEntries)
            {
                int index = fileName.LastIndexOf("/");

                string localPath = path;

                if (index > 0)
                    localPath += fileName.Substring(index).Replace("/", "");

                var ext = Path.GetExtension(localPath);
                if (ext == ".meta")
                    continue;

                var t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                if (t != null)
                    al.Add(t);
            }
            T[] result = new T[al.Count];
            for (int i = 0; i < al.Count; i++)
                result[i] = (T)al[i];

            return result;
        }
    }
}
#endif