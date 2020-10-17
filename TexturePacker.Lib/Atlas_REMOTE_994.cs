using System;
using System.Collections.Generic;

#if UNITY
namespace UnityTexturePacker.Lib
#else
namespace TexturePacker.Lib
#endif
{
    /// <summary>
    /// The texture atlas
    /// </summary>
    [Serializable]
    public sealed class Atlas
    {
        /// <summary>
        /// Width in pixels
        /// </summary>
        public int Width;

        /// <summary>
        /// Height in Pixel
        /// </summary>
        public int Height;

        /// <summary>
        /// List of the nodes in the Atlas. This will represent all the textures that are packed into it and all the remaining free space
        /// </summary>
        public List<Node> Nodes;
    }
}