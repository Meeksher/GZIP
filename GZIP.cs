using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZIP
{
    public abstract class GZIP
    {
        protected FileInfo input, output;
        protected int blockSize = 80000; // <80k -> small object heap, >80 -> large object heap
        protected int countThreads = Environment.ProcessorCount;
        protected MemoryBlocksQueue producerQueue = new MemoryBlocksQueue();
        protected MemoryBlocksQueue consumerQueue = new MemoryBlocksQueue();
        protected Thread[] threads;
        protected bool errorFlag = false;

        public abstract void Start();
        protected abstract void Consume();
        protected abstract void PrepareData();
        protected abstract void DropData();

        public GZIP(string input, string output)
        {
            this.input = new FileInfo(input);
            this.output = new FileInfo(output);
            this.threads = new Thread[countThreads];
        }
    }
}
