using System;
using System.Collections.Generic;

<<<<<<< HEAD
namespace _TexturePacker.Lib
=======
#if UNITY
namespace UnityTexturePacker.Lib
#else
namespace TexturePacker.Lib
#endif
>>>>>>> fe36a5433a51448163b2a3fc34d51a818e040e53
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