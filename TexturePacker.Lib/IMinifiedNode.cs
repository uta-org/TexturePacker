using System.Drawing;

#if UNITY_TEAM_LICENSE
namespace UnityTexturePacker.Lib
#else
namespace TexturePacker.Lib
#endif
{
    public interface IMinifiedNode
    {
        Rectangle Bounds { get; set; }

        string Name { get; set; }
    }
}