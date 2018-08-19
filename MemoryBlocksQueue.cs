using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZIP
{
    public class MemoryBlocksQueue
    {
        object mutex = new object();
        Queue<MemoryBlock> queue = new Queue<MemoryBlock>();
        int nextIndex = 0;

        public int Count { get { return queue.Count; } }
        public bool IsDead { get; private set; } = false;

        public void Enqueue(MemoryBlock block)
        {
            try
            {
                if (block == null)
                    throw new ArgumentNullException("block");
                lock (mutex)
                {
                    if (IsDead)
                        throw new InvalidOperationException("Queue already stopped");

                    while (nextIndex != block.Index)
                    {
                        Monitor.Wait(mutex);
                    }

                    nextIndex++;
                    queue.Enqueue(block);
                    Monitor.PulseAll(mutex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IsDead = true;
            }
        }

        public MemoryBlock Dequeue()
        {
            try
            {
                lock (mutex)
                {
                    while (queue.Count == 0 && !IsDead)
                        Monitor.Wait(mutex);

                    if (queue.Count == 0)
                        return null;

                    return queue.Dequeue();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IsDead = true;
                return null;
            }
        }

        public void Stop()
        {
            lock (mutex)
            {
                IsDead = true;
                Monitor.PulseAll(mutex);
            }
        }

    }
}