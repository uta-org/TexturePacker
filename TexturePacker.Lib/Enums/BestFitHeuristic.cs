#if UNITY
namespace UnityTexturePacker.Lib
#else
namespace TexturePacker.Lib
#endif
{
    /// <summary>
    /// Different types of heuristics in how to use the available space
    /// </summary>
    public enum BestFitHeuristic
    {
        /// <summary>
        ///
        /// </summary>
        Area,

        /// <summary>
        ///
        /// </summary>
        MaxOneAxis,
    }
}