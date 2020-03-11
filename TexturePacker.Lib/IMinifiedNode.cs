using System.Drawing;

namespace _TexturePacker.Lib
{
    public interface IMinifiedNode
    {
        Rectangle Bounds { get; set; }

        string Name { get; set; }
    }
}