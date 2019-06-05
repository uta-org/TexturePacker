#if UNITY_TEAM_LICENSE
namespace UnityTexturePacker.Lib
#else
namespace TexturePacker.Lib
#endif
{
    /// <summary>
    /// Indicates in which direction to split an unused area when it gets used
    /// </summary>
    public enum SplitType
    {
        /// <summary>
        /// Split Horizontally (textures are stacked up)
        /// </summary>
        Horizontal,

        /// <summary>
        /// Split verticaly (textures are side by side)
        /// </summary>
        Vertical,
    }
}