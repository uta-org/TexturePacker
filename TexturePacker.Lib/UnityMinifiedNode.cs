//#if !(!UNITY_2020 && !UNITY_2019 && !UNITY_2018 && !UNITY_2017 && !UNITY_5)

using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using _TexturePacker.Lib;
using _TexturePacker.Lib.Interfaces;
using UnityEngine.Extensions;
using uzLib.Lite.ExternalCode.Extensions;

#if !UNITY_2020 && !UNITY_2019 && !UNITY_2018 && !UNITY_2017 && !UNITY_5

using System.Drawing;

#else

using _System.Drawing;

#endif

//using System.Drawing; // TODO

namespace UnityEngine.Utils.TexturePackerTool
{
    /// <summary>
    /// The Minified Node class
    /// </summary>
    public class UnityMinifiedNode
        : IMinifiedNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinifiedNode"/> class.
        /// </summary>
        public UnityMinifiedNode() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinifiedNode"/> class.
        /// </summary>
        /// <param name="rect">The rect.</param>
        public UnityMinifiedNode(Rect rect)
        {
            this.rect = rect;
        }

        /// <summary>
        /// The bounds
        /// </summary>
        [JsonIgnore]
        public Rectangle Bounds { get; set; }

        [JsonProperty("Bounds")]
        public string BoundsProxy
        {
            get => Bounds.ToString();
            set => Bounds = DeserializeBoundsProperty(value);
        }

        private Rectangle DeserializeBoundsProperty(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new NotImplementedException();

            var components = value.Split(',');
            return new Rectangle(
                GetInt(components[0]),
                GetInt(components[1]),
                GetInt(components[2]),
                GetInt(components[3]));
        }

        private int GetInt(string value)
        {
            return int.Parse(Regex.Match(value, @"\d+").Value);
        }

        /// <summary>
        /// The name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The rect
        /// </summary>
        private Rect rect;

        /// <summary>
        /// Gets the rectangle.
        /// </summary>
        /// <value>
        /// The rectangle.
        /// </value>
        public Rect Rectangle => rect == default ? rect = GetStringRect() : rect;

        /// <summary>
        /// Gets the string rect.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Rect GetStringRect()
        {
            return Bounds.ToRect();
        }
    }
}

//#endif