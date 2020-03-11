using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _TexturePacker.Lib
{
    /// <summary>
    /// Minified Atlas class
    /// </summary>
    [Serializable]
    public sealed class MinifiedAtlas<T>
        where T : IMinifiedNode, new()
    {
        /// <summary>
        /// The nodes
        /// </summary>
        public List<T> Nodes { get; set; }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Atlas"/> to <see cref="MinifiedAtlas"/>.
        /// </summary>
        /// <param name="atlas">The atlas.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator MinifiedAtlas<T>(Atlas atlas)
        {
            var minAtlas = new MinifiedAtlas<T>();

            minAtlas.Nodes = atlas.Nodes.Select(n => new T()
            {
                Bounds = n.Bounds,
                Name = Path.GetFileNameWithoutExtension(n.Texture.Source)
            }).ToList();

            return minAtlas;
        }
    }
}