using System;

#if !UNITY_2020 && !UNITY_2019 && !UNITY_2018 && !UNITY_2017 && !UNITY_5

using System.Drawing;

#else

using _System.Drawing;

#endif

namespace _TexturePacker.Lib
{
    /// <summary>
    /// A node in the Atlas structure
    /// </summary>
    [Serializable]
    public sealed class Node
    {
        /// <summary>
        /// Bounds of this node in the atlas
        /// </summary>
        public Rectangle Bounds;

        /// <summary>
        /// Texture this node represents
        /// </summary>
        public TextureInfo Texture;

        /// <summary>
        /// If this is an empty node, indicates how to split it when it will  be used
        /// </summary>
        public SplitType SplitType;
    }
}