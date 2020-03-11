using System.Drawing;

namespace _TexturePacker.Lib.Interfaces
{
    public interface IMinifiedNode
    {
        Rectangle Bounds { get; set; }

        string Name { get; set; }
    }
}