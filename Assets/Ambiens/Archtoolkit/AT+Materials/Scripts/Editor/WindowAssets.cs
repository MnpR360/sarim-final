using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ambiens.archtoolkit.atmaterials.utils;
using System.Linq;
using System;
using ambiens.archtoolkit.atmaterials.texturestructure;

namespace ambiens.archtoolkit.atmaterials.editor
{
    public enum WindowAssetsStatus
    {
        Materials = 0,
        Info = 1
    }

    [System.Serializable]
    public class WindowAssets : EditorWindow
    {
        private string error;
        private JsonSources jsonSources = new JsonSources();
        private bool jsonDownloaded = false;
        private Dictionary<string, Texture2D> imgTexCache = new Dictionary<string, Texture2D>();

        private List<string> currentLoadingTex = new List<string>();
        private List<string> currentLoadingMat = new List<string>();

        private List<DownloadsItem> LoadedMat = new List<DownloadsItem>();

        private Texture2D defaultIcon;
        private int selectedSourceIndex = 0;
        private TextureSource selectedSource;
        private int selectedCategoryIndex = 0;
        private Categories selectedCategory;
        private Vector2 scrollView = Vector2.zero;
        private Texture sourceLogo = null;
        private int toolbarSelected = 0;
        private string[] toolbar = new string[] { "Materials", "Info" };
        private WindowAssetsStatus currentStatus = WindowAssetsStatus.Materials;
        private JsonSourcesSO sourcesSO;

        private Action OnSourceChanged;
        private Action OnCategoryChanged;

        public static string AssetLocation =
            "Assets/Ambiens/Archtoolkit/AT+Materials/SourceMaterial.asset";
        public static string IconsLocation = "Assets/Ambiens/Archtoolkit/AT+Materials/Icons/";

        private ATMatSocialReferements[] socialClasses = new ATMatSocialReferements[]
        {
            //new SocialReferements ("UI/Documentation", "", "Click here to open documentation","Documentation"), // Docs
            //new ATMatSocialReferements ("UI/unityLogo", "", "Check our unity forum post","Support"), // Support
            //new ATMatSocialReferements("UI/Reddit","","Check the Reddit post","Reddit"),
            new ATMatSocialReferements(
                "UI/YouTube.png",
                "https://www.youtube.com/channel/UCkqMEDTMuARl75aCK5T5ryA",
                "Tutorial playlist",
                "Tutorials"
            ), // Youtube
            new ATMatSocialReferements(
                "UI/discord_logo.png",
                "https://discord.com/invite/Dph5fymmjY",
                "Discord Server",
                "Support server"
            ), // Discord
            //new ATMatSocialReferements ("UI/Facebook", "https://www.facebook.com/groups/152547958672484/", "Facebook page","Facebook official page"), // Facebook
            new ATMatSocialReferements(
                "UI/Instagram.png",
                "https://www.instagram.com/ambiensvr/",
                "Instagram page",
                "Instagram official page"
            ), // Instagram
            //new ATMatSocialReferements ("UI/ATIcon", "https://www.archtoolkit.com", "Archtoolkit web page, for stay up to date","Archtoolkit web page"), // Archtoolkit site
            new ATMatSocialReferements(
                "UI/Almbiens.png",
                "https://www.ambiensvr.com",
                "Our company site",
                "AmbiensVR web page"
            ) // Ambiens site
        };

        private static WindowAssets Instance;

        [MenuItem("Tools/Ambiens/ArchToolkit/AT+Materials")]
        public static void OpenWindow()
        {
            Instance = EditorWindow.GetWindow<WindowAssets>(false, "AT+Materials", true);

            Instance.maxSize = new Vector2(350, 650);
            Instance.minSize = new Vector2(200, 550);
            Instance.Show();

            EditorUtility.SetDirty(Instance);
        }

        public void DownloadJson()
        {
            if (this.defaultIcon == null)
                this.defaultIcon = WindowAssets.GetIcon("UI/ATIcon");

            var go = new GameObject("JsonHandlerDownloader");
            this.sourceLogo = null;

            this.sourcesSO = AssetDatabase.LoadAssetAtPath<JsonSourcesSO>(AssetLocation);

            if (this.sourcesSO == null)
            {
                this.sourcesSO = ScriptableObject.CreateInstance<JsonSourcesSO>();

                AssetDatabase.CreateAsset(this.sourcesSO, AssetLocation);
            }

            if (this.sourcesSO.jsonSources == null || this.sourcesSO.jsonSources.sources.Count <= 0)
            {
                go.AddComponent<DownloaderJsonHandler>()
                    .Init(
                        (JsonSources jsonSources, string error) =>
                        {
                            this.selectedSourceIndex = 0;
                            this.selectedCategoryIndex = 0;
                            this.jsonSources = jsonSources;
                            this.sourcesSO.jsonSources = this.jsonSources;
                            this.error = error;
                            this.jsonDownloaded = true;
                            this.Focus();

                            this.UpdateJson();
                        }
                    );
            }
            else
            {
                this.jsonSources = this.sourcesSO.jsonSources;
                this.jsonDownloaded = true;
                GameObject.DestroyImmediate(go);
            }
        }

        private void OnEnable()
        {
            foreach (var social in this.socialClasses)
            {
                if (social == null)
                    continue;

                social.SetTextureImage();
            }
            this.OnSourceChanged += this.SourceChanged;

            if (this.jsonDownloaded == false)
            {
                this.DownloadJson();
            }
        }

        private void SourceChanged()
        {
            this.selectedCategoryIndex = 0;
        }

        private void OnGUI()
        {
            if (
                EditorApplication.isCompiling
                || EditorApplication.isPlaying
                || EditorApplication.isPaused
            )
                return;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            this.toolbarSelected = GUILayout.Toolbar(
                this.toolbarSelected,
                this.toolbar,
                GUILayout.Height(30)
            );

            this.currentStatus = (WindowAssetsStatus)this.toolbarSelected;

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            //TODO->Reorganize this!
            DownloadHDRIHavenHandler.RefreshMaterialNames();

            if (
                DownloadHDRIHavenHandler.CurrentRenderPipeline
                == DownloadHDRIHavenHandler.RenderPipelineType.HDRP
            )
            {
#if !ATM_HDRP_SUPPORT
                AddHDRPSupport();
#endif
            }

            switch (this.currentStatus)
            {
                case WindowAssetsStatus.Materials:
                    this.MaterialsStatusGUI();
                    break;
                case WindowAssetsStatus.Info:
                    this.InfoStatusGUI();
                    break;
            }

            GUILayout.EndVertical();
        }

        public static void AddHDRPSupport()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup
            );
            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.Add("ATM_HDRP_SUPPORT");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray())
            );
        }

        private void UpdateJson()
        {
            if (this.selectedSource == null)
                return;

            if (this.selectedCategory == null)
                return;

            foreach (var source in this.jsonSources.sources)
            {
                if (source.name != this.selectedSource.name)
                    continue;

                foreach (var cat in source.categories)
                {
                    if (cat.name != this.selectedCategory.name)
                        continue;

                    foreach (var mat in cat.materials)
                    {
                        //This is just to order everything the first time
                        mat.GetDownloads();
                        for (int i = 0; i < mat.downloads.Count; i++)
                        {
                            var download = mat.downloads[i];

                            var d = this.LoadedMat.Find(m => m.url == download.url);

                            if (d == null)
                                continue;

                            if (!string.IsNullOrEmpty(d.localPath))
                            {
                                mat.downloads[i].materialLoaded = d.materialLoaded;
                                mat.downloads[i].localPath = d.localPath;
                            }
                        }
                    }
                }
            }

            this.sourcesSO.jsonSources = this.jsonSources;
        }

        private void InfoStatusGUI()
        {
            GUI.DrawTexture(
                new Rect(0, 50, this.position.width, 120),
                WindowAssets.GetIcon("UI/AT.png"),
                ScaleMode.ScaleToFit
            );

            GUILayout.BeginArea(new Rect(0, 150, this.position.width, this.position.height));

            var welcomeTextStyle = new GUIStyle();

            welcomeTextStyle.wordWrap = true;

            welcomeTextStyle.normal.textColor = Color.black;

            GUILayout.BeginArea(new Rect(0, 10, this.position.width, 120), GUI.skin.textArea);

            EditorGUILayout.LabelField(
                "AT+Materials is part of the Archtoolkit family, a series of plugins made for architects and designers. Check our Unity",
                welcomeTextStyle
            );

            if (GUILayout.Button("Asset Store page (Click here)", this.GetBtnStyle()))
            {
                Help.BrowseURL(
                    "https://assetstore.unity.com/packages/tools/utilities/archtoolkit-beta-137202"
                );
            }

            GUILayout.EndArea();

            GUILayout.BeginVertical();

            var customButton = new GUIStyle(GUI.skin.button);

            customButton.imagePosition = ImagePosition.ImageOnly;
            customButton.alignment = TextAnchor.MiddleLeft;
            customButton.normal.background = null;

            var customLabel = new GUIStyle(GUI.skin.label);
            customLabel.fontSize = 10;
            customLabel.wordWrap = true;
            customLabel.alignment = TextAnchor.LowerLeft;

            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 12;

            GUILayout.Space(150);

            foreach (var social in this.socialClasses)
            {
                GUILayout.BeginHorizontal();

                if (social == null)
                    continue;

                if (
                    GUILayout.Button(
                        social.textureImage,
                        customButton,
                        GUILayout.Width(50),
                        GUILayout.Height(50)
                    )
                )
                    Help.BrowseURL(social.siteUrl);

                GUILayout.BeginVertical();

                GUILayout.Space(10);

                GUILayout.Label(social.title, titleStyle);

                GUILayout.Label(social.description, customLabel);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.EndArea();
        }

        GUIStyle GetBtnStyle()
        {
            var s = new GUIStyle();
            var b = s.border;
            s.fontStyle = FontStyle.Italic;
            b.left = 0;
            b.top = 0;
            b.right = 0;
            b.bottom = 0;
            return s;
        }

        private void MaterialsStatusGUI()
        {
            if (this.jsonDownloaded)
            {
                if (string.IsNullOrEmpty(this.error))
                {
                    this.SearchBoxGUI();

                    if (string.IsNullOrEmpty(this.search))
                    {
                        this.SelectSourceGUI();

                        if (this.selectedSource != null)
                        {
                            // Make source logo
                            if (!string.IsNullOrEmpty(this.selectedSource.img))
                            {
                                if (this.sourceLogo == null)
                                {
                                    DownloaderTextureHandler.Get(
                                        this.selectedSource.img,
                                        128,
                                        128,
                                        (string error, Texture2D t) =>
                                        {
                                            if (string.IsNullOrEmpty(error))
                                            {
                                                this.sourceLogo = t;
                                            }
                                            else
                                            {
                                                this.sourceLogo = GetIcon("UI/AT.png");
                                            }
                                        }
                                    );
                                }

                                if (this.sourceLogo != null)
                                    GUI.DrawTexture(
                                        new Rect(0, 70, this.position.width, 120),
                                        this.sourceLogo,
                                        ScaleMode.ScaleToFit
                                    );

                                GUILayout.Space(110);
                            }

                            this.SelectCategoryGUI();

                            if (this.selectedCategory != null)
                                this.ShowMaterialsGUI();
                        }
                    }
                    else
                    {
                        this.ShowMaterialsGUI();
                    }
                }
                else
                {
                    this.Repaint();
                    GUILayout.Label("Error " + this.error);
                }
            }
        }

        private string search = "";

        private void SearchBoxGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("SEARCH");
            search = GUILayout.TextField(search);

            GUILayout.EndHorizontal();
        }

        private void SelectSourceGUI()
        {
            GUILayout.BeginHorizontal();
            var arr = this.jsonSources.sources.Select(s => s.name).ToArray();
            var popupStyle = new GUIStyle(EditorStyles.popup);

            popupStyle.fixedHeight = 20;
            popupStyle.imagePosition = ImagePosition.ImageAbove;

            var selectedSourceTemp = this.selectedSource;
            selectedSourceIndex = EditorGUILayout.Popup(selectedSourceIndex, arr, popupStyle);
            if (
                selectedSourceIndex != -1
                && (this.jsonSources.sources != null && this.jsonSources.sources.Count > 0)
            )
                selectedSource = this.jsonSources.sources[selectedSourceIndex];
            else
                selectedSource = null;

            if (selectedSourceTemp != this.selectedSource)
            {
                this.sourceLogo = null;

                if (this.OnSourceChanged != null)
                    this.OnSourceChanged();
            }
            // var refreshStyleBtn = new GUIStyle(GUI.skin.button);

            if (
                GUILayout.Button(
                    GetIcon(
                        ambiens.archtoolkit.atmaterials.utils.ArchMaterialDataPaths.REFRESH_IMAGE
                    ),
                    GUILayout.Width(20),
                    GUILayout.Height(20)
                )
            )
            {
                this.DownloadJson();
            }

            GUILayout.EndHorizontal();
        }

        private void SelectCategoryGUI()
        {
            GUILayout.BeginHorizontal();

            var popupStyle = new GUIStyle(EditorStyles.popup);

            popupStyle.fixedHeight = 20;
            popupStyle.imagePosition = ImagePosition.ImageAbove;

            var filteredCategories = this.selectedSource.categories.FindAll(
                c => c.materials.Count > 0
            );

            var arr = filteredCategories
                .Select(s => s.name + " (" + s.materials.Count + ")")
                .ToArray();
            selectedCategoryIndex = EditorGUILayout.Popup(selectedCategoryIndex, arr, popupStyle);
            if (
                selectedCategoryIndex != -1
                && (
                    this.selectedSource.categories != null
                    && this.selectedSource.categories.Count > 0
                )
            )
                selectedCategory = filteredCategories[selectedCategoryIndex];
            else
                selectedCategory = null;

            // GUIStyle btnStyle = new GUIStyle(GUI.skin.box);
            if (selectedCategory != null && !string.IsNullOrEmpty(selectedCategory.url))
            {
                if (
                    GUILayout.Button(
                        GetIcon(
                            ambiens
                                .archtoolkit
                                .atmaterials
                                .utils
                                .ArchMaterialDataPaths
                                .REDIRECT_IMAGE
                        ),
                        GUILayout.Width(20),
                        GUILayout.Height(20)
                    )
                )
                {
                    if (selectedSource.name.Contains("CC0Textures"))
                    {
                        var cat = selectedCategory.url.Remove(0, 1).Remove(0, 1);
                        var url = "www." + this.selectedSource.name + ".com" + cat;

                        Help.BrowseURL(url);
                        return;
                    }
                    Help.BrowseURL(selectedCategory.url);
                }
            }

            GUILayout.EndHorizontal();
        }

        private int imageSize = 200;

        private void ShowMaterialsGUI()
        {
            this.scrollView = GUILayout.BeginScrollView(scrollView, false, false);

            List<ATMaterialItem> materials = new List<ATMaterialItem>();
            if (string.IsNullOrEmpty(this.search))
            {
                materials = selectedCategory.materials;
            }
            else
            {
                materials = this.sourcesSO.Search(this.search);
            }

            foreach (var mat in materials)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);

                if (this.imgTexCache.ContainsKey(mat.img))
                {
                    var imageCache = this.imgTexCache[mat.img];
                    if (imageCache != null)
                    {
                        GUILayout.Box(
                            imageCache,
                            GUILayout.Width(imageSize),
                            GUILayout.Height(imageSize)
                        );
                    }
                    else
                    {
                        GUILayout.Box(
                            defaultIcon,
                            GUILayout.Width(imageSize),
                            GUILayout.Height(imageSize)
                        );
                    }
                }
                else
                {
                    GUILayout.Box(
                        this.defaultIcon,
                        GUILayout.Width(imageSize),
                        GUILayout.Height(imageSize)
                    );

                    if (!this.currentLoadingTex.Contains(mat.img))
                    {
                        this.currentLoadingTex.Add(mat.img);
                        DownloaderTextureHandler.Get(
                            mat.img,
                            512,
                            512,
                            (string url, Texture2D text) =>
                            {
                                this.currentLoadingTex.Remove(mat.img);
                                if (text != null)
                                {
                                    if (!this.imgTexCache.ContainsKey(mat.img))
                                        this.imgTexCache.Add(mat.img, text);

                                    this.Repaint();
                                }
                            }
                        );
                    }
                }

                //Vertical layout at the right side of the image
                GUILayout.BeginVertical();

                GUILayout.Label(mat.name);

                //Horizontal layout under the material label
                GUILayout.BeginHorizontal();
                // GUILayout.Space(5);
                string l = "";
                //This is just to order everything the first time
                mat.GetDownloads();
                int rowSplit = mat.downloads.Count / 3;
                if (rowSplit < 2)
                    rowSplit = 2;

                for (int i = 0; i < mat.downloads.Count; i++)
                {
                    if (i % rowSplit == 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                    var dLink = mat.downloads[i];

                    l = dLink.type
                        .Replace("<i class=\"fas fa-file-archive\"></i> ", "")
                        .Replace("<i class=\"fas fa-wrench\"></i> ", "");

                    l = ambiens.archtoolkit.atmaterials.utils.ATMatStringUtils.CleanFileName(l);

                    dLink.type = l;

                    if (this.currentLoadingMat.Contains(dLink.url) || this.AllSetDownloaded(dLink))
                    {
                        GUILayout.Label("Loading...");
                        if (this.CheckAlreadyDownloaded(mat, ref dLink))
                        {
                            this.currentLoadingMat.Remove(dLink.url);
                            foreach (var item in dLink.set)
                            {
                                this.currentLoadingMat.Remove(item);
                            }
                        }
                    }
                    else
                    {
                        bool alreadyDownloaded = false;
                        if (string.IsNullOrEmpty(dLink.url)) // Usa set
                        {
                            //foreach (var text in dLink.set)
                            //{
                            alreadyDownloaded = this.CheckAlreadyDownloaded(mat, ref dLink);
                            if (alreadyDownloaded)
                            {
                                l = "[D] " + l;
                            }
                            //}
                        }
                        else
                        {
                            alreadyDownloaded = this.CheckAlreadyDownloaded(mat, ref dLink);
                            if (alreadyDownloaded)
                            {
                                l = "[D] " + l;
                            }
                        }

                        var relativePath = this.GetMaterialRelativePathPath(mat);

                        var assetPath = this.GetMaterialAbsolutePathPath(mat);

                        if (GUILayout.Button(l))
                        {
                            if (alreadyDownloaded)
                            {
                                this.SelectMaterial(
                                    dLink.localPath /*"Assets" + relativePath + mat.name + ".mat"*/
                                );
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(dLink.url))
                                {
                                    this.currentLoadingMat.Add(dLink.url);

                                    if (dLink.url.Contains(".zip"))
                                    {
                                        Downloadcc0TexturesHandler.GetAndUnZip(
                                            dLink,
                                            dLink.url,
                                            "Assets" + relativePath,
                                            assetPath,
                                            mat.name + "_" + l,
                                            (float p) =>
                                            {
                                                Debug.Log(p);
                                            },
                                            (string url, DownloadsItem dLinked) =>
                                            {
                                                this.currentLoadingMat.Remove(dLinked.url);

                                                if (dLinked.materialLoaded != null)
                                                {
                                                    dLinked.localPath =
                                                        "Assets"
                                                        + relativePath
                                                        + mat.name
                                                        + dLinked.type
                                                        + ".mat";

                                                    AssetDatabase.CreateAsset(
                                                        dLinked.materialLoaded,
                                                        dLinked.localPath
                                                    );

                                                    AssetDatabase.Refresh();

                                                    this.SelectMaterial(dLinked.localPath);

                                                    this.UpdateJson();
                                                }
                                            }
                                        );
                                    }
                                    else if (
                                        dLink.url.Contains(".exr") || dLink.url.Contains(".hdr")
                                    )
                                    {
                                        DownloadHDRIHavenHandler.Get(
                                            dLink,
                                            dLink.url,
                                            "Assets" + relativePath,
                                            assetPath,
                                            mat.name + "_" + l,
                                            (float p) =>
                                            {
                                                Debug.Log(p);
                                            },
                                            (string url, DownloadsItem dLinked) =>
                                            {
                                                this.currentLoadingMat.Remove(dLinked.url);

                                                if (dLinked.materialLoaded != null)
                                                {
                                                    dLinked.localPath =
                                                        "Assets"
                                                        + relativePath
                                                        + mat.name
                                                        + dLinked.type
                                                        + ".mat";

                                                    AssetDatabase.CreateAsset(
                                                        dLinked.materialLoaded,
                                                        dLinked.localPath
                                                    );

                                                    AssetDatabase.Refresh();

                                                    this.SelectMaterial(dLinked.localPath);

                                                    RenderSettings.skybox = dLinked.materialLoaded;

                                                    this.UpdateJson();
                                                }
                                            }
                                        );
                                    }
                                }
                                else
                                {
                                    foreach (var text in dLink.set)
                                    {
                                        this.currentLoadingMat.Add(text);
                                    }
                                    var startingUrl = "www." + this.selectedSource.name + ".com";

                                    Downloadcc0TexturesHandler.GetTextureFromSet(
                                        dLink,
                                        startingUrl,
                                        dLink.set,
                                        "Assets" + relativePath,
                                        assetPath,
                                        mat.name + "_" + l,
                                        (float p) =>
                                        {
                                            Debug.Log(p);
                                        },
                                        (string url, DownloadsItem dLinked) =>
                                        {
                                            this.currentLoadingMat.Remove(url);

                                            if (dLinked.materialLoaded != null)
                                            {
                                                dLinked.localPath =
                                                    "Assets"
                                                    + relativePath
                                                    + mat.name
                                                    + dLinked.type
                                                    + ".mat";

                                                AssetDatabase.CreateAsset(
                                                    dLinked.materialLoaded,
                                                    dLinked.localPath
                                                );

                                                AssetDatabase.Refresh();

                                                this.SelectMaterial(dLinked.localPath);

                                                this.UpdateJson();
                                            }
                                        }
                                    );
                                }
                            }
                        }
                    }
                }

                GUILayout.EndHorizontal(); //End horizontal layout of download buttons

                GUILayout.Space(5);

                if (
                    GUILayout.Button(
                        GetIcon(ArchMaterialDataPaths.REDIRECT_IMAGE),
                        GUILayout.Width(20),
                        GUILayout.Height(20)
                    )
                )
                {
                    Help.BrowseURL(mat.url);
                }

                GUILayout.EndVertical(); //End vertical layout at the right side of the image

                GUILayout.EndHorizontal(); //End of material container
            }
            GUILayout.EndScrollView();
        }

        private void RefillDictionary(ATMaterialItem mat, ref DownloadsItem downloadsItem)
        {
            var relative = this.GetMaterialRelativePathPath(mat);
            var objectPath = "Assets" + relative + mat.name + downloadsItem.type + ".mat";
            var obj = AssetDatabase.LoadAssetAtPath<Material>(objectPath);

            if (obj != null)
            {
                downloadsItem.localPath = objectPath;
                downloadsItem.materialLoaded = obj as Material;
            }
            else
            {
                downloadsItem.localPath = string.Empty;
                downloadsItem.materialLoaded = null;
            }
        }

        private string GetMaterialRelativePathPath(ATMaterialItem mat)
        {
            string relativePath =
                "/AT+Materials/"
                + this.selectedSource.name
                + "/"
                + this.selectedCategory.name
                + "/"
                + mat.name
                + "/";
            string assetPath = Application.dataPath;
            assetPath += relativePath;

            return relativePath;
        }

        private string GetMaterialAbsolutePathPath(ATMaterialItem mat)
        {
            string relativePath =
                "/AT+Materials/"
                + this.selectedSource.name
                + "/"
                + this.selectedCategory.name
                + "/"
                + mat.name
                + "/";
            string assetPath = Application.dataPath;
            assetPath += relativePath;

            return assetPath;
        }

        private void SelectMaterial(string path)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (obj == null)
                return;

            Selection.activeObject = obj;

            EditorGUIUtility.PingObject(obj);
        }

        private bool AllSetDownloaded(DownloadsItem downloadsItem)
        {
            var allDownloaded = false;
            foreach (var item in downloadsItem.set)
            {
                allDownloaded = this.currentLoadingMat.Contains(item);
            }

            return allDownloaded;
        }

        public void ChangeStatusTo(WindowAssetsStatus status)
        {
            this.currentStatus = status;
        }

        private bool CheckAlreadyDownloaded(ATMaterialItem mat, ref DownloadsItem downloadsItem)
        {
            this.RefillDictionary(mat, ref downloadsItem);
            //Aggiungere anche check sulla cartella!

            if (string.IsNullOrEmpty(downloadsItem.localPath))
                return false;

            return true;
        }

        public static Dictionary<string, Texture2D> icons_cache =
            new Dictionary<string, Texture2D>();

        public static Texture2D GetIcon(string path)
        {
            string p = System.IO.Path.Combine(IconsLocation, path);

            if (icons_cache.ContainsKey(p) && icons_cache[p] != null)
                return icons_cache[p];
            else
            {
                if (!icons_cache.ContainsKey(p))
                    icons_cache.Add(p, AssetDatabase.LoadAssetAtPath<Texture2D>(p));
                else
                    icons_cache[p] = AssetDatabase.LoadAssetAtPath<Texture2D>(p);

                return icons_cache[p];
            }
        }
    }
}
