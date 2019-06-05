using System;
using System.Drawing;

namespace TexturePacker.Lib
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