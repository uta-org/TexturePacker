#if UNITY_2020 || UNITY_2019 || UNITY_2018 || UNITY_2017 || UNITY_5

using _System.Drawing;

#else

using System.Drawing;

#endif

namespace _TexturePacker.Lib.Interfaces
{
    public interface IMinifiedNode
    {
        Rectangle Bounds { get; set; }

        string Name { get; set; }
    }
}