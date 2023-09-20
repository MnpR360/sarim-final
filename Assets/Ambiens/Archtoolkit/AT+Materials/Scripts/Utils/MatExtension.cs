using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ambiens.archtoolkit.atmaterials.utils
{

    public static class MaterialsExtensions
    {


        public static List<string> MainTextureNames = new List<string> { "_BaseMap", "_BaseColorMap", "BaseColorMap", "_BaseColorTex", "_MainTex", "Albedo" };
        public static List<string> BumpTextureNames = new List<string> { "_BumpMap" };
        public static List<string> MetallicMapNames = new List<string> { "_MetallicGlossMap" };
        public static List<string> NormalMapNames = new List<string> { /*"_ParallaxMap", */"_BumpMap", "Normal Map", "_NormalMap", "NormalMap" };

        public static List<string> OcclusionMapNames = new List<string> { "_OcclusionMap" };

        public static List<string> DisplacementMapNames = new List<string> { "_HeightMap" };

        public static List<string> SmoothnessNames = new List<string> { "_Shininess", "_Glossiness", "_RoughnessFactor", "_EmissionGain", "_Cutoff" };
        public static List<string> MetallicNames = new List<string> { "_Metallic", "_MetallicFactor" };
        public static List<string> OtherProperties = new List<string> { "_RimIntensity", "_RimPower", "_Jitter", "_Frequency" };
        public static List<string> ColorProperties = new List<string> { "_BaseColor", "_Color", "_color", "_BaseColorFactor", "_TintColor" };
        public static List<string> EmissionProperties = new List<string> { "_EmissionColor" };
        public static List<string> TilingProprierties = new List<string> { "" };
        public static List<string> OffsetProperties = new List<string> { "" };

        public static string GetAlbedoProperty(this Material mat)
        {
            foreach (var name in MainTextureNames)
            {
                var property = mat.HasProperty(name);
                if (property)
                {
                    return name;
                }
            }
            return null;
        }
        public static string GetMetallicMapProperty(this Material mat)
        {
            foreach (var name in MetallicMapNames)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }
        public static string GetDisplacementMapProperty(this Material mat)
        {
            foreach (var name in DisplacementMapNames)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }
        public static string GetOcclusionMapProperty(this Material mat)
        {
            foreach (var name in OcclusionMapNames)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }
        public static string GetNormalMapProperty(this Material mat)
        {
            foreach (var name in NormalMapNames)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }
        public static string GetEmissionProperty(this Material mat)
        {
            foreach (var name in EmissionProperties)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }

        public static string GetTilingProprierty(this Material mat)
        {
            foreach (var name in TilingProprierties)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }

        public static string GetOffsetProprierty(this Material mat)
        {
            foreach (var name in OffsetProperties)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }

        public static string GetBumpProperty(this Material mat)
        {
            foreach (var name in BumpTextureNames)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }

        public static string GetSmoothnessProperty(this Material mat)
        {
            foreach (var name in SmoothnessNames)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }

        public static string GetMetallicProperty(this Material mat)
        {
            foreach (var name in MetallicNames)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }

        public static string GetColorProperty(this Material mat)
        {
            foreach (var name in ColorProperties)
            {
                var property = mat.HasProperty(name);
                //Debug.Log(mat.name + " " + name + " " + property);

                if (property)
                    return name;
            }
            return null;
        }

        public static string GetGenericProperty(this Material mat)
        {
            foreach (var name in OtherProperties)
            {
                var property = mat.HasProperty(name);

                if (property)
                    return name;
            }
            return null;
        }
    }

}
