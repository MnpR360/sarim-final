using System.Collections.Generic;
using UnityEngine;

namespace ambiens.archtoolkit.atmaterials.texturestructure
{
    [System.Serializable]
    public class JsonSourcesSO : ScriptableObject
    {
        public JsonSources jsonSources;
        private List<string> toRemoveFilter = new List<string>() { "substance" };

        public List<ATMaterialItem> Search(string search)
        {
            if (jsonSources == null)
                return new List<ATMaterialItem>();
            if (search.Length > 2)
            {
                var toreturn = new List<ATMaterialItem>();

                search = search.ToLower();

                foreach (var s in this.jsonSources.sources)
                {
                    foreach (var c in s.categories)
                    {
                        if (c.name.ToLower().Contains(search))
                        {
                            toreturn.AddRange(filterMaterials(c.materials));
                        }
                        else
                        {
                            foreach (var m in c.materials)
                            {
                                foreach (var t in m.labels)
                                {
                                    if (t.ToLower().Contains(search) && !toreturn.Contains(m))
                                    {
                                        if (filterSingleMaterial(m))
                                            toreturn.Add(m);
                                    }
                                }
                            }
                        }
                    }
                }

                return toreturn;
            }
            else
                return new List<ATMaterialItem>();
        }

        string auxStrName = "";
        bool foundFilterName = false;

        public List<ATMaterialItem> filterMaterials(List<ATMaterialItem> toFilter)
        {
            var toreturn = new List<ATMaterialItem>();

            foreach (var m in toFilter)
            {
                if (filterSingleMaterial(m))
                    toreturn.Add(m);
            }
            return toreturn;
        }

        private bool filterSingleMaterial(ATMaterialItem mat)
        {
            foundFilterName = false;
            auxStrName = mat.name.ToLower();
            foreach (var f in toRemoveFilter)
            {
                if (auxStrName.Contains(f))
                {
                    foundFilterName = true;
                }
            }
            return !foundFilterName;
        }
    }
}
