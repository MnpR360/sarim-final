using UnityEngine;
using System;

namespace ambiens.archtoolkit.atmaterials.editor
{
    [Serializable]
    public class ATMatSocialReferements
    {
        public string title;
        public string textureUrl;
        public string siteUrl;
        public string description;
        public Texture textureImage;

        public void SetTextureImage()
        {
            this.textureImage = WindowAssets.GetIcon(this.textureUrl);//Resources.Load<Texture>(this.textureUrl);
        }

        public ATMatSocialReferements(string textureUrl, string siteUrl, string description, string title)
        {
            this.textureUrl = textureUrl;
            this.description = description;
            this.siteUrl = siteUrl;
            this.title = title;
        }
    }
}