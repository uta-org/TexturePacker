using System.Collections.Generic;
using _TexturePacker.Lib;

namespace _TexturePacker
{
    public class AtlasBlock
    {
        public List<Atlas> Atlases { get; }

        private AtlasBlock()
        {
        }

        public AtlasBlock(List<Atlas> atlases)
        {
            Atlases = atlases;
        }
    }
}