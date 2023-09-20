using System.Collections.Generic;
using System;
using UnityEngine;

namespace ambiens.archtoolkit.atmaterials.texturestructure
{
    [Serializable]
    public class JsonSources
    {
        public List<TextureSource> sources = new List<TextureSource>();
    }

    [Serializable]
    public class TextureSource
    {
        public string name = "";
        public string img;
        public List<Categories> categories = new List<Categories>();
    }

    [Serializable]
    public class Categories
    {
        public string url;
        public string name;

        public List<ATMaterialItem> materials = new List<ATMaterialItem>();
    }

    [Serializable]
    public class ATMaterialItem
    {
        public string url;
        public string img;
        public string name;
        public List<string> labels = new List<string>();

        public List<DownloadsItem> downloads = new List<DownloadsItem>();
        private bool downloadsItemsAlreadyOrdered = false;

        private string auxstr1;
        private string auxstr2;

        public List<DownloadsItem> GetDownloads()
        {
            if (downloadsItemsAlreadyOrdered)
                return downloads;

            downloads.Sort(
                (a, b) =>
                {
                    auxstr1 = a.type.ToLower();
                    auxstr2 = b.type.ToLower();

                    if (auxstr1.Contains("k") && auxstr2.Contains("k"))
                    {
                        auxstr1 = auxstr1
                            .Replace("k", "")
                            .Replace("jpg", "")
                            .Replace("png", "")
                            .Replace("-", "");
                        auxstr2 = auxstr2
                            .Replace("k", "")
                            .Replace("jpg", "")
                            .Replace("png", "")
                            .Replace("-", "");
                        int af = Convert.ToInt32(auxstr1);
                        int bf = Convert.ToInt32(auxstr2);
                        return af.CompareTo(bf);
                    }
                    else
                        return string.Compare(a.type, b.type);
                }
            );
            downloadsItemsAlreadyOrdered = true;
            return downloads;
        }
    }

    [Serializable]
    public class DownloadsItem
    {
        public string type;
        public string url;
        public List<string> set;
        public string localPath;
        public Material materialLoaded;
    }
}
