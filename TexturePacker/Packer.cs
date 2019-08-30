using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TexturePacker.Lib;

namespace TexturePacker
{
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
        /// Toggle for debug mode, resulting in debug atlasses to check the packing algorithm
        /// </summary>
        public bool DebugMode;

        /// <summary>
        /// Which heuristic to use when doing the fit
        /// </summary>
        public BestFitHeuristic FitHeuristic;

        /// <summary>
        /// List of all the output atlases
        /// </summary>
        public List<Atlas> Atlasses;

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
            ScanForTextures(_SourceDir, _Pattern, _FullPath);

            List<TextureInfo> textures = new List<TextureInfo>();
            textures = SourceTextures.ToList();

            bool areIcons = !_SafeMode || textures.All(tex => tex.Width == tex.Height)
                            && (int)textures.Average(tex => tex.Width * tex.Height) == textures[0].Width * textures[1].Height;

            if (AtlasSize == 1024 && areIcons)
            {
                int atlasSize = (int)Math.Ceiling(Math.Sqrt(textures.Count)) * textures[0].Width;

                if (atlasSize <= 8192)
                {
                    Console.WriteLine($"Icon mode detected, resizing atlas to {AtlasSize} pixels.");
                    AtlasSize = atlasSize;
                }
                else
                    Console.WriteLine($"Atlas exceeded max size of 8192 pixels ({atlasSize})");
            }

            //2: generate as many atlasses as needed (with the latest one as small as possible)
            Atlasses = new List<Atlas>();
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

                Atlasses.Add(atlas);

                textures = leftovers;
            }

            if (_Save)
                SaveAtlasses(_Type, _DebugMode, _OutName);
        }

        private void SaveAtlasses(OutputType _Type, bool _Debug, string _Destination)
        {
            int atlasCount = 0;
            string prefix = _Destination.Replace(Path.GetExtension(_Destination), "");

            StreamWriter tw = new StreamWriter(_Destination);
            tw.WriteLine("source_tex, atlas_tex, u, v, scale_u, scale_v");

            foreach (Atlas atlas in Atlasses)
            {
                string atlasName = string.Format(prefix + "{0:000}" + ".png", atlasCount);

                //1: Save images
                Image img = CreateAtlasImage(atlas);
                img.Save(atlasName, System.Drawing.Imaging.ImageFormat.Png);

                switch (_Type)
                {
                    case OutputType.TXT:
                        OutputTXT(atlas, tw, atlasName);
                        break;

                    case OutputType.JSON:
                    case OutputType.JMin:
                    case OutputType.MinifiedJSON:
                        OutputJson(atlasName, _Type != OutputType.JSON);
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

                ++atlasCount;
            }
            tw.Close();

            if (!_Debug)
                return;

            tw = new StreamWriter(prefix + ".log");
            tw.WriteLine("--- LOG -------------------------------------------");
            tw.WriteLine(Log.ToString());
            tw.WriteLine("--- ERROR -----------------------------------------");
            tw.WriteLine(Error.ToString());
            tw.Close();
        }

        private void OutputJson(string _AtlasName, bool _Minified)
        {
            var atlasBlock = new AtlasBlock(Atlasses);

            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(folderPath ?? throw new InvalidOperationException(), $"{_AtlasName}{(_Minified ? ".min" : string.Empty)}.json");

            File.WriteAllText(filePath, JsonConvert.SerializeObject(atlasBlock, _Minified ? Formatting.None : Formatting.Indented));
        }

        private static void OutputTXT(Atlas _Atlas, StreamWriter _TextWriter, string _AtlasName)
        {
            // TODO: Implement Atlasses

            //2: save description in file
            foreach (Node n in _Atlas.Nodes)
            {
                if (n.Texture != null)
                {
                    _TextWriter.Write(n.Texture.Source + ", ");
                    _TextWriter.Write(_AtlasName + ", ");
                    _TextWriter.Write((float)n.Bounds.X / _Atlas.Width + ", ");
                    _TextWriter.Write((float)n.Bounds.Y / _Atlas.Height + ", ");
                    _TextWriter.Write((float)n.Bounds.Width / _Atlas.Width + ", ");
                    _TextWriter.WriteLine(((float)n.Bounds.Height / _Atlas.Height).ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void ScanForTextures(string _Path, string _Wildcard, bool _FullPath)
        {
            DirectoryInfo di = new DirectoryInfo(_Path);
            FileInfo[] files = di.GetFiles(_Wildcard, SearchOption.AllDirectories);

            foreach (FileInfo fi in files)
            {
                Image img = Image.FromFile(fi.FullName);
                if (img.Width <= AtlasSize && img.Height <= AtlasSize)
                {
                    TextureInfo ti = new TextureInfo();

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

        private Image CreateAtlasImage(Atlas _Atlas)
        {
            Image img = new Bitmap(_Atlas.Width, _Atlas.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(img);

            if (DebugMode)
            {
                g.FillRectangle(Brushes.Green, new Rectangle(0, 0, _Atlas.Width, _Atlas.Height));
            }

            foreach (Node n in _Atlas.Nodes)
            {
                if (n.Texture != null)
                {
                    Image sourceImg = Image.FromFile(n.Texture.Source);
                    g.DrawImage(sourceImg, n.Bounds);

                    if (DebugMode)
                    {
                        string label = Path.GetFileNameWithoutExtension(n.Texture.Source);
                        SizeF labelBox = g.MeasureString(label, SystemFonts.MenuFont, new SizeF(n.Bounds.Size));
                        RectangleF rectBounds = new Rectangle(n.Bounds.Location, new Size((int)labelBox.Width, (int)labelBox.Height));
                        g.FillRectangle(Brushes.Black, rectBounds);
                        g.DrawString(label, SystemFonts.MenuFont, Brushes.White, rectBounds);
                    }
                }
                else
                {
                    g.FillRectangle(Brushes.DarkMagenta, n.Bounds);

                    if (DebugMode)
                    {
                        string label = n.Bounds.Width + "x" + n.Bounds.Height;
                        SizeF labelBox = g.MeasureString(label, SystemFonts.MenuFont, new SizeF(n.Bounds.Size));
                        RectangleF rectBounds = new Rectangle(n.Bounds.Location, new Size((int)labelBox.Width, (int)labelBox.Height));
                        g.FillRectangle(Brushes.Black, rectBounds);
                        g.DrawString(label, SystemFonts.MenuFont, Brushes.White, rectBounds);
                    }
                }
            }

            return img;
        }
    }
}