using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ambiens.archtoolkit.atmaterials.TexPacker
{
    public enum TextureChannel
    {
        ChannelRed,
        ChannelGreen,
        ChannelBlue,
        ChannelAlpha
    }

    public class TextureChannelInput
    {
        public bool enabled;
        public TextureChannel output;

        public TextureChannelInput() { }
        public TextureChannelInput(TextureChannel output, bool enabled = false)
        {
            this.output = output;
            this.enabled = enabled;
        }
    }
    public enum TextureFormat
    {
        JPG,
        PNG,
        EXR
    }
    public class TextureInput
    {
        public Texture2D texture;

        private Dictionary<TextureChannel, TextureChannelInput> _inputs = new Dictionary<TextureChannel, TextureChannelInput>();

        public TextureInput()
        {
            _inputs[TextureChannel.ChannelRed] = new TextureChannelInput();
            _inputs[TextureChannel.ChannelGreen] = new TextureChannelInput();
            _inputs[TextureChannel.ChannelBlue] = new TextureChannelInput();
            _inputs[TextureChannel.ChannelAlpha] = new TextureChannelInput();
        }

        public TextureChannelInput GetChannelInput(TextureChannel channel)
        {
            return _inputs[channel];
        }

        public void SetChannelInput(TextureChannel channel, TextureChannelInput channelInput)
        {
            _inputs[channel] = channelInput;
        }
    }
    public class TexturePacker
    {
        private readonly string _shaderName = "Hidden/TexturePacker";
        private Material _material;

        private List<TextureInput> _texInputs = new List<TextureInput>();
        public List<TextureInput> texInputs
        {
            get { return _texInputs; }
        }

        public int resolution = 2048;

        public void Initialize()
        {
            if (_material == null)
            {
                _material = new Material(Shader.Find(_shaderName));
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        public void Add(TextureInput entry)
        {
            _texInputs.Add(entry);
        }

        public void Remove(TextureInput input)
        {
            _texInputs.Remove(input);
        }

        private string GetPropertyName(int i, string param)
        {
            return string.Format("_Input0{0}{1}", i, param);
        }

        public void ClearProperties()
        {
            for (int i = 0; i < 6; ++i)
            {
                _material.SetTexture(GetPropertyName(i, "Tex"), Texture2D.blackTexture);
                _material.SetVector(GetPropertyName(i, "In"), Vector4.zero);
            }
        }

        private Vector4 GetInputs(TextureInput texInput)
        {
            Vector4 states = Vector4.zero;

            for (int i = 0; i < 4; ++i)
            {
                var state = texInput.GetChannelInput((TextureChannel)i).enabled;
                states[i] = state ? 1f : 0f;
            }

            return states;
        }

        private Matrix4x4 GetOutputs(TextureInput texInput)
        {
            Matrix4x4 m = Matrix4x4.zero;

            for (int i = 0; i < 4; ++i)
            {
                Vector4 inChannel = Vector4.zero;
                var output = texInput.GetChannelInput((TextureChannel)i).output;
                inChannel[(int)output] = 1f;
                m.SetRow(i, inChannel);
            }

            return m;
        }

        public Texture2D Create()
        {
            int idx = 0;
            foreach (var input in _texInputs)
            {
                var Tex = input.texture;
                _material.SetTexture(GetPropertyName(idx, "Tex"), Tex);

                var In = GetInputs(input);
                _material.SetVector(GetPropertyName(idx, "In"), In);

                var Out = GetOutputs(input);
                _material.SetMatrix(GetPropertyName(idx, "Out"), Out);
                ++idx;
            }

            var texture = TextureUtility.GenerateTexture(resolution, resolution, _material);

            return texture;
        }
    }

    public static class TextureUtility
    {
        public static Texture2D GenerateTexture(int width, int height, Material mat)
        {
            RenderTexture tempRT = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(Texture2D.blackTexture, tempRT, mat);

            Texture2D output = new Texture2D(tempRT.width, tempRT.height, UnityEngine.TextureFormat.RGBA32, false);
            RenderTexture.active = tempRT;

            output.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            output.Apply();
            output.filterMode = FilterMode.Bilinear;

            RenderTexture.ReleaseTemporary(tempRT);
            RenderTexture.active = null;

            return output;
        }

        public static Texture2D InvertTexture(Texture2D original)
        {
            var mat = new Material(Shader.Find("Custom/InvertTexture"));
            mat.SetTexture("_MainTex", original);

            RenderTexture tempRT = RenderTexture.GetTemporary(2048, 2048);

            Graphics.Blit(null, tempRT, mat);

            Texture2D output = new Texture2D(tempRT.width, tempRT.height, UnityEngine.TextureFormat.RGBA32, false);
            RenderTexture.active = tempRT;

            output.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            output.Apply();
            output.filterMode = FilterMode.Bilinear;

            RenderTexture.ReleaseTemporary(tempRT);
            RenderTexture.active = null;

            return output;
        }

        public static Texture2D GetSmoothnessFromRoughness(Texture2D original, string path)
        {
#if UNITY_EDITOR
            var smoothness = InvertTexture(original);

            smoothness.alphaIsTransparency = true;
           
            File.WriteAllBytes(path, smoothness.EncodeToPNG());
            AssetDatabase.Refresh();
            var localPath = path.Replace(Application.dataPath.Replace("Assets", ""), "");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(localPath);

            return texture;
#else
            return null;
#endif

        }
    }
}