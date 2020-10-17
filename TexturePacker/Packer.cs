#if !UNITY_2020 && !UNITY_2019 && !UNITY_2018 && !UNITY_2017 && !UNITY_5

using Newtonsoft.Json;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;

#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _TexturePacker.Lib;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Extensions;
using UnityEngine.Utils.TexturePackerTool;

namespace _TexturePacker
{
#if !UNITY_2020 && !UNITY_2019 && !UNITY_2018 && !UNITY_2017 && !UNITY_5

    /// <summary>
    /// Objects that performs the packing task. Takes a list of textures as input and generates a set of atlas textures/definition pairs
    /// </summary>
    public sealed class Packer
    {
        /// <summary>
        /// List of all the textures that need to be packed
        /// </summary>
        public List<TextureInfo> SourceTextures;

        /// <summary>
        /// Stream that recieves all the info logged
        /// </summary>
        public StringWriter Log;

        /// <summary>
        /// Stream that recieves all the error info
        /// </summary>
        public StringWriter Error;

        /// <summary>
        /// Number of pixels that separate textures in the atlas
        /// </summary>
        public int Padding;

        /// <summary>
        /// Size of the atlas in pixels. Represents one axis, as atlases are square
        /// </summary>
        public int AtlasSize;

        /// <summary>
        /// Toggle for debug mode, resulting in debug atlases to check the packing algorithm
        /// </summary>
        public bool DebugMode;

        /// <summary>
        /// Which heuristic to use when doing the fit
        /// </summary>
        public BestFitHeuristic FitHeuristic;

        public Packer()
        {
            SourceTextures = new List<TextureInfo>();
            Log = new StringWriter();
            Error = new StringWriter();
        }

        public void Process(OutputType _Type, string _OutName, string _SourceDir, string _Pattern, int _AtlasSize, int _Padding, bool _DebugMode, bool _SafeMode, bool _FullPath, bool _Save = true)
        {
            Padding = _Padding;
            AtlasSize = _AtlasSize;
            DebugMode = _DebugMode;

            //1: scan for all the textures we need to pack
            ScanForTextures(_SourceDir, _Pattern, _FullPath, out var sources);

            List<TextureInfo> textures = new List<TextureInfo>();
            textures = SourceTextures.ToList();

            bool areIcons = !_SafeMode || textures.All(tex => tex.Width == tex.Height)
                            && (int)textures.Average(tex => tex.Width * tex.Height) == textures[0].Width * textures[1].Height;

            if (AtlasSize == 1024 && areIcons)
            {
                int atlasSize = (int)Math.Ceiling(Math.Sqrt(textures.Count)) * textures[0].Width;

                if (atlasSize <= 8192)
                {
                    Console.WriteLine($@"Icon mode detected, resizing atlas to {AtlasSize} pixels.");
                    AtlasSize = atlasSize;
                }
                else
                    Console.WriteLine($@"Atlas exceeded max size of 8192 pixels ({atlasSize})");
            }

            //2: generate as many atlases as needed (with the latest one as small as possible)
            var atlases = new List<Atlas>();
            while (textures.Count > 0)
            {
                Atlas atlas = new Atlas();
                atlas.Width = _AtlasSize;
                atlas.Height = _AtlasSize;

                List<TextureInfo> leftovers = LayoutAtlas(textures, atlas);

                if (leftovers.Count == 0)
                {
                    // we reached the last atlas. Check if this last atlas could have been twice smaller
                    while (leftovers.Count == 0)
                    {
                        atlas.Width /= 2;
                        atlas.Height /= 2;
                        leftovers = LayoutAtlas(textures, atlas);
                    }
                    // we need to go 1 step larger as we found the first size that is to small
                    atlas.Width *= 2;
                    atlas.Height *= 2;
                    leftovers = LayoutAtlas(textures, atlas);
                }

                atlases.Add(atlas);

                textures = leftovers;
            }

            if (_Save)
                SaveAtlases(_Type, _DebugMode, _OutName, sources, atlases);
        }

        private void SaveAtlases(OutputType _Type, bool _Debug, string _Destination, List<string> _Sources, List<Atlas> _Atlases)
        {
            int atlasCount = 0;
            string prefix = Path.HasExtension(_Destination) ? Path.GetFileNameWithoutExtension(_Destination) : _Destination;

            //1: Save images
            foreach (Atlas atlas in _Atlases)
            {
                string atlasName = string.Format(prefix + "{0:000}" + ".png", atlasCount);

                using (var img = CreateAtlasImage(atlas, _Sources))
                    img.Save(atlasName, System.Drawing.Imaging.ImageFormat.Png);

                ++atlasCount;
            }

            switch (_Type)
            {
                case OutputType.TXT:
                    OutputTXT(_Atlases, prefix);
                    break;

                case OutputType.JSON:
                case OutputType.JMin:
                case OutputType.MinifiedJSON:
                    OutputJson(_Atlases, prefix, _Type != OutputType.JSON);
                    break;

                case OutputType.CSV:
                    throw new NotImplementedException();

                case OutputType.YAML:
                    throw new NotImplementedException();

                case OutputType.HTML:
                    throw new NotImplementedException();

                case OutputType.XML:
                    throw new NotImplementedException();
            }

            if (!_Debug)
                return;

            var tw = new StreamWriter(prefix + ".log");
            tw.WriteLine("--- LOG -------------------------------------------");
            tw.WriteLine(Log.ToString());
            tw.WriteLine("--- ERROR -----------------------------------------");
            tw.WriteLine(Error.ToString());
            tw.Close();
        }

        private void OutputJson(List<Atlas> _Atlases, string _AtlasName, bool _Minified)
        {
            var atlasBlock = new AtlasBlock(_Atlases);

            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(folderPath ?? throw new InvalidOperationException(), $"{Path.GetFileNameWithoutExtension(_AtlasName)}{(_Minified ? ".min" : string.Empty)}.json");

            File.WriteAllText(filePath, JsonConvert.SerializeObject(atlasBlock, _Minified ? Formatting.None : Formatting.Indented));
        }

        private static void OutputTXT(List<Atlas> _Atlases, string _AtlasName)
        {
            // TODO: Create one txt file only

            string filePath = !Path.HasExtension(_AtlasName) ? $"{_AtlasName}.txt" : _AtlasName;

            using (var tw = new StreamWriter(filePath))
            {
                tw.WriteLine("source_tex, atlas_tex, u, v, scale_u, scale_v");

                //2: save description in file
                foreach (Atlas atlas in _Atlases)
                    foreach (Node node in atlas.Nodes)
                    {
                        if (node.Texture != null)
                        {
                            tw.Write(node.Texture.Source + ", ");
                            tw.Write(_AtlasName + ", ");
                            tw.Write((float)node.Bounds.X / atlas.Width + ", ");
                            tw.Write((float)node.Bounds.Y / atlas.Height + ", ");
                            tw.Write((float)node.Bounds.Width / atlas.Width + ", ");
                            tw.WriteLine(((float)node.Bounds.Height / atlas.Height).ToString(CultureInfo.InvariantCulture));
                        }
                    }
            }
        }

        private void ScanForTextures(string _Path, string _Wildcard, bool _FullPath, out List<string> _Sources)
        {
            DirectoryInfo di = new DirectoryInfo(_Path);
            FileInfo[] files = di.GetFiles(_Wildcard, SearchOption.AllDirectories);
            _Sources = new List<string>();

            foreach (FileInfo fi in files)
            {
                Image img = Image.FromFile(fi.FullName);
                if (img.Width <= AtlasSize && img.Height <= AtlasSize)
                {
                    TextureInfo ti = new TextureInfo();

                    if (!_FullPath)
                        _Sources.Add(fi.FullName);

                    ti.Source = _FullPath ? fi.FullName : Path.GetFileNameWithoutExtension(fi.Name);
                    ti.Width = img.Width;
                    ti.Height = img.Height;

                    SourceTextures.Add(ti);

                    Log.WriteLine("Added " + fi.FullName);
                }
                else
                {
                    Error.WriteLine(fi.FullName + " is too large to fix in the atlas. Skipping!");
                }
            }
        }

        private void HorizontalSplit(Node _ToSplit, int _Width, int _Height, List<Node> _List)
        {
            Node n1 = new Node();
            n1.Bounds.X = _ToSplit.Bounds.X + _Width + Padding;
            n1.Bounds.Y = _ToSplit.Bounds.Y;
            n1.Bounds.Width = _ToSplit.Bounds.Width - _Width - Padding;
            n1.Bounds.Height = _Height;
            n1.SplitType = SplitType.Vertical;

            Node n2 = new Node();
            n2.Bounds.X = _ToSplit.Bounds.X;
            n2.Bounds.Y = _ToSplit.Bounds.Y + _Height + Padding;
            n2.Bounds.Width = _ToSplit.Bounds.Width;
            n2.Bounds.Height = _ToSplit.Bounds.Height - _Height - Padding;
            n2.SplitType = SplitType.Horizontal;

            if (n1.Bounds.Width > 0 && n1.Bounds.Height > 0)
                _List.Add(n1);
            if (n2.Bounds.Width > 0 && n2.Bounds.Height > 0)
                _List.Add(n2);
        }

        private void VerticalSplit(Node _ToSplit, int _Width, int _Height, List<Node> _List)
        {
            Node n1 = new Node();
            n1.Bounds.X = _ToSplit.Bounds.X + _Width + Padding;
            n1.Bounds.Y = _ToSplit.Bounds.Y;
            n1.Bounds.Width = _ToSplit.Bounds.Width - _Width - Padding;
            n1.Bounds.Height = _ToSplit.Bounds.Height;
            n1.SplitType = SplitType.Vertical;

            Node n2 = new Node();
            n2.Bounds.X = _ToSplit.Bounds.X;
            n2.Bounds.Y = _ToSplit.Bounds.Y + _Height + Padding;
            n2.Bounds.Width = _Width;
            n2.Bounds.Height = _ToSplit.Bounds.Height - _Height - Padding;
            n2.SplitType = SplitType.Horizontal;

            if (n1.Bounds.Width > 0 && n1.Bounds.Height > 0)
                _List.Add(n1);
            if (n2.Bounds.Width > 0 && n2.Bounds.Height > 0)
                _List.Add(n2);
        }

        private TextureInfo FindBestFitForNode(Node _Node, List<TextureInfo> _Textures)
        {
            TextureInfo bestFit = null;

            float nodeArea = _Node.Bounds.Width * _Node.Bounds.Height;
            float maxCriteria = 0.0f;

            foreach (TextureInfo ti in _Textures)
            {
                switch (FitHeuristic)
                {
                    // Max of Width and Height ratios
                    case BestFitHeuristic.MaxOneAxis:
                        if (ti.Width <= _Node.Bounds.Width && ti.Height <= _Node.Bounds.Height)
                        {
                            float wRatio = (float)ti.Width / (float)_Node.Bounds.Width;
                            float hRatio = (float)ti.Height / (float)_Node.Bounds.Height;
                            float ratio = wRatio > hRatio ? wRatio : hRatio;
                            if (ratio > maxCriteria)
                            {
                                maxCriteria = ratio;
                                bestFit = ti;
                            }
                        }
                        break;

                    // Maximize Area coverage
                    case BestFitHeuristic.Area:

                        if (ti.Width <= _Node.Bounds.Width && ti.Height <= _Node.Bounds.Height)
                        {
                            float textureArea = ti.Width * ti.Height;
                            float coverage = textureArea / nodeArea;
                            if (coverage > maxCriteria)
                            {
                                maxCriteria = coverage;
                                bestFit = ti;
                            }
                        }
                        break;
                }
            }

            return bestFit;
        }

        private List<TextureInfo> LayoutAtlas(List<TextureInfo> _Textures, Atlas _Atlas)
        {
            List<Node> freeList = new List<Node>();
            List<TextureInfo> textures = new List<TextureInfo>();

            _Atlas.Nodes = new List<Node>();

            textures = _Textures.ToList();

            Node root = new Node();
            root.Bounds.Size = new Size(_Atlas.Width, _Atlas.Height);
            root.SplitType = SplitType.Horizontal;

            freeList.Add(root);

            while (freeList.Count > 0 && textures.Count > 0)
            {
                Node node = freeList[0];
                freeList.RemoveAt(0);

                TextureInfo bestFit = FindBestFitForNode(node, textures);
                if (bestFit != null)
                {
                    if (node.SplitType == SplitType.Horizontal)
                    {
                        HorizontalSplit(node, bestFit.Width, bestFit.Height, freeList);
                    }
                    else
                    {
                        VerticalSplit(node, bestFit.Width, bestFit.Height, freeList);
                    }

                    node.Texture = bestFit;
                    node.Bounds.Width = bestFit.Width;
                    node.Bounds.Height = bestFit.Height;

                    textures.Remove(bestFit);
                }

                _Atlas.Nodes.Add(node);
            }

            return textures;
        }

        private Image CreateAtlasImage(Atlas _Atlas, List<string> _Sources)
        {
            Image img = new Bitmap(_Atlas.Width, _Atlas.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img);

            if (DebugMode)
            {
                g.FillRectangle(Brushes.Green, new Rectangle(0, 0, _Atlas.Width, _Atlas.Height));
            }

            for (var index = 0; index < _Atlas.Nodes.Count; index++)
            {
                Node node = _Atlas.Nodes[index];
                string source = _Sources?[index];

                if (node.Texture != null)
                {
                    Image sourceImg = Image.FromFile(source ?? node.Texture.Source);
                    g.DrawImage(sourceImg, node.Bounds);

                    if (DebugMode)
                    {
                        string label = Path.GetFileNameWithoutExtension(node.Texture.Source);
                        SizeF labelBox = g.MeasureString(label, SystemFonts.MenuFont, new SizeF(node.Bounds.Size));
                        RectangleF rectBounds = new Rectangle(node.Bounds.Location,
                            new Size((int)labelBox.Width, (int)labelBox.Height));
                        g.FillRectangle(Brushes.Black, rectBounds);
                        g.DrawString(label, SystemFonts.MenuFont, Brushes.White, rectBounds);
                    }
                }
                else
                {
                    g.FillRectangle(Brushes.DarkMagenta, node.Bounds);

                    if (DebugMode)
                    {
                        string label = node.Bounds.Width + "x" + node.Bounds.Height;
                        SizeF labelBox = g.MeasureString(label, SystemFonts.MenuFont, new SizeF(node.Bounds.Size));
                        RectangleF rectBounds = new Rectangle(node.Bounds.Location,
                            new Size((int)labelBox.Width, (int)labelBox.Height));
                        g.FillRectangle(Brushes.Black, rectBounds);
                        g.DrawString(label, SystemFonts.MenuFont, Brushes.White, rectBounds);
                    }
                }
            }

            return img;
        }
    }

#else

    /// <summary>
    ///     The Packer class
    /// </summary>
    public class Packer
    {
        /// <summary>
        ///     The atlas
        /// </summary>
        private MinifiedAtlas<UnityMinifiedNode> m_atlas;

        /// <summary>
        ///     The dictionary
        /// </summary>
        private Dictionary<string, Rect> m_dict;

        /// <summary>
        ///     The loaded textures
        /// </summary>
        private readonly Dictionary<string, Texture2D> m_loadedTextures;

        /// <summary>
        ///     The mono
        /// </summary>
        private readonly MonoBehaviour m_mono;

        /// <summary>
        ///     The resource path
        /// </summary>
        private readonly string m_resourcePath;

        /// <summary>
        ///     Prevents a default instance of the <see cref="Packer" /> class from being created.
        /// </summary>
        private Packer()
        {
            m_dict = new Dictionary<string, Rect>();
            m_loadedTextures = new Dictionary<string, Texture2D>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Packer" /> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="monoBehaviour">The mono behaviour.</param>
        /// <param name="callback">The callback.</param>
        public Packer(string folder, MonoBehaviour monoBehaviour, Action callback = null)
            : this()
        {
            m_mono = monoBehaviour;
            m_resourcePath = folder;

            m_mono.StartCoroutine(LoadAtlas(callback));
        }

        /// <summary>
        ///     Gets the atlas.
        /// </summary>
        /// <value>
        ///     The atlas.
        /// </value>
        public Texture2D Atlas { get; private set; }

        /// <summary>
        ///     Loads the atlas.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        private IEnumerator LoadAtlas(Action callback)
        {
            string json;

            var textRequest = Resources.LoadAsync<TextAsset>($"{m_resourcePath}/atlas");
            var textureRequest = Resources.LoadAsync<Texture2D>($"{m_resourcePath}/atlas");

            yield return new WaitUntil(() => textRequest.isDone && textureRequest.isDone);

            json = (textRequest.asset as TextAsset).text;
            m_atlas = JsonConvert.DeserializeObject<MinifiedAtlas<UnityMinifiedNode>>(json);

            Atlas = textureRequest.asset as Texture2D;

            //m_dict = m_atlas.Nodes.ToDictionary(t => t.Name,
            //            t => t.Rectangle);

            m_dict = new Dictionary<string, Rect>();

            // Fixed: Checking for items with the same key
            foreach (var node in m_atlas.Nodes)
                if (!m_dict.ContainsKey(node.Name))
                    m_dict.Add(node.Name, node.Rectangle);
            //else
            //    Debug.LogError($"Item '{node.Name}' with the same key already added!");

            callback?.Invoke();
        }

        /// <summary>
        ///     Gets the texture.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Texture2D GetTexture(string name)
        {
            if (m_loadedTextures.ContainsKey(name))
                return m_loadedTextures[name];

            var texture = GetSprite(name).GetTexture();
            m_loadedTextures.Add(name, texture);

            //Debug.Log($"[Tex={texture.name}, Width={texture.width}, Height={texture.height}]");

            return texture;
        }

        /// <summary>
        ///     Gets the sprite.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Sprite GetSprite(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogException(new ArgumentNullException(nameof(name)));
                return null;
            }

            if (m_dict.Count == 0)
            {
                Debug.LogError("Internal dictionary is null!");
                return null;
            }

            if (!ExistsOnInternalDictionary(name))
            {
                Debug.LogError($"Internal dictionary doesn't contains any value for '{name}'!");
                return null;
            }

            var rect = m_dict[name];

            var sprite = Sprite.Create(Atlas, rect, Vector2.one * .5f);
            sprite.name = name;

            return sprite;
        }

        /// <summary>
        ///     Exists on internal dictionary?
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool ExistsOnInternalDictionary(string name)
        {
            return m_dict.ContainsKey(name);
        }

        /// <summary>
        ///     Loads the icons.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Texture2D> LoadIcons()
        {
            return Atlas.GetTextures(m_atlas.Nodes.ToArray());
        }

        /// <summary>
        ///     Loads the icons.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public IEnumerable<Texture2D> LoadIcons(int count)
        {
            var nodes = m_atlas.Nodes.Take(count).ToArray();
            return Atlas.GetTextures(nodes);
        }

        /// <summary>
        ///     Loads the icons.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public IEnumerable<Texture2D> LoadIcons(int count, Func<Rect, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var nodes = new List<UnityMinifiedNode>();

            var i = 0;
            foreach (var kv in m_dict)
            {
                if (predicate(kv.Value))
                {
                    nodes.Add(m_atlas.Nodes.Find(node => node.Name == kv.Key));
                    ++i;
                }

                if (i > count - 1)
                    break;
            }

            return Atlas.GetTextures(nodes.ToArray());
        }

        /// <summary>
        ///     Loads the icons.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">predicate</exception>
        public IEnumerable<Texture2D> LoadIcons(int count, Func<KeyValuePair<string, Rect>, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var nodes = new List<UnityMinifiedNode>();

            var i = 0;
            foreach (var kv in m_dict)
            {
                if (predicate(kv))
                {
                    nodes.Add(m_atlas.Nodes.Find(node => node.Name == kv.Key));
                    ++i;
                }

                if (i > count - 1)
                    break;
            }

            return Atlas.GetTextures(nodes.ToArray());
        }
    }

#endif
}
