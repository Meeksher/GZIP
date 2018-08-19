using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GZIP
{
    public class MemoryBlock
    {
        public int Index { get; private set; }
        public byte[] MemoryBuffer { get; private set; }

        public MemoryBlock(int index, byte[] memory)
        {
            this.Index = index;
            this.MemoryBuffer = memory;
        }
    }

}