#if !UNITY_2020 && !UNITY_2019 && !UNITY_2018 && !UNITY_2017 && !UNITY_5

using System.Drawing;

#else

using _System.Drawing;

#endif

namespace _TexturePacker.Lib.Interfaces
{
    public interface IMinifiedNode
    {
        Rectangle Bounds { get; set; }

        string Name { get; set; }
    }
}