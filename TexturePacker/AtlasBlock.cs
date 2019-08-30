using System.Collections.Generic;
using TexturePacker.Lib;

namespace TexturePacker
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