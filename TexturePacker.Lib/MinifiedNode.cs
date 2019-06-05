using System;
using System.Drawing;

namespace TexturePacker.Lib
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