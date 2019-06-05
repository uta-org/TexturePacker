using System.Drawing;

namespace TexturePacker.Lib
{
    public interface IMinifiedNode
    {
        Rectangle Bounds { get; set; }

        string Name { get; set; }
    }
}