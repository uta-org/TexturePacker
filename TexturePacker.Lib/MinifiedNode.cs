using System;
using _TexturePacker.Lib.Interfaces;

#if !UNITY_2020 && !UNITY_2019 && !UNITY_2018 && !UNITY_2017 && !UNITY_5

using System.Drawing;

#else

using _System.Drawing;

#endif

namespace _TexturePacker.Lib
{
    /// <summary>
    /// Minified Node (used to represent minimum quantity of information possible)
    /// </summary>
    [Serializable]
    public class MinifiedNode
        : IMinifiedNode
    {
        /// <summary>
        /// The bounds
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// The name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinifiedNode"/> class.
        /// </summary>
        public MinifiedNode() { }
    }
}