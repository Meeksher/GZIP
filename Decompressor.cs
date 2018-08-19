using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZIP
{
    class Decompressor : GZIP
    {
        public Decompressor(string input, string output) : base(input, output)
        {

        }

        protected override void Consume()
        {
            try
            {
                byte[] decompressedMemoryRaw = new byte[blockSize];
                while (!errorFlag)
                {
                    var memBlock = producerQueue.Dequeue();
                    if (memBlock == null) return;

                    using (MemoryStream compressedMemoryStream = new MemoryStream(memBlock.MemoryBuffer))
                    {
                        using (GZipStream gzip = new GZipStream(compressedMemoryStream, CompressionMode.Decompress))
                        {
                            int read = gzip.Read(decompressedMemoryRaw, 0, decompressedMemoryRaw.Length);
                            byte[] decompressedMemory = new byte[read];
                            Array.Copy(decompressedMemoryRaw, 0, decompressedMemory, 0, read);

                            MemoryBlock decompressed = new MemoryBlock(memBlock.Index, decompressedMemory);

                            consumerQueue.Enqueue(decompressed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorFlag = true;
                throw;
            }
        }

        protected override void PrepareData()
        {
            try
            {
                int nextIndex = 0;

                using (FileStream fsIn = input.OpenRead())
                {
                    while (fsIn.Position != fsIn.Length && !errorFlag)
                    {
                        while (producerQueue.Count < countThreads)
                        {
                            byte[] sizeBytes = new byte[sizeof(int)];
                            fsIn.Read(sizeBytes, 0, sizeBytes.Length);
                            int sizeMemoryBlock = BitConverter.ToInt32(sizeBytes, 0);

                            byte[] memoryRead = new byte[sizeMemoryBlock];

                            fsIn.Read(memoryRead, 0, memoryRead.Length);
                            
                            MemoryBlock memoryProduced = new MemoryBlock(nextIndex++, memoryRead);
                            producerQueue.Enqueue(memoryProduced);
                        }
                    }
                }
                producerQueue.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorFlag = true;
                throw;
            }
        }

        public override void Start()
        {
            try
            {
                Thread preparerDataThread = new Thread(PrepareData);
                preparerDataThread.Start();

                Thread dropperDataThread = new Thread(DropData);
                dropperDataThread.Start();

                for (int i = 0; i < countThreads; i++)
                {
                    threads[i] = new Thread(Consume);
                    threads[i].Start();
                }

                foreach (var thread in threads.AsEnumerable())
                {
                    thread.Join();
                }

                consumerQueue.Stop();

                preparerDataThread.Join();
                dropperDataThread.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorFlag = true;
            }

            if (errorFlag) Console.WriteLine("Error occured, the programm have not finished correctly.");
            else Console.WriteLine("Decompressing finished.");
        }



        protected override void DropData()
        {
            try
            {
                using (FileStream fsOut = new FileStream(output.FullName, FileMode.Create))
                {
                    while (!errorFlag)
                    {
                        MemoryBlock memBlock = consumerQueue.Dequeue();
                        if (memBlock == null && consumerQueue.IsDead)
                            return;
                        
                        fsOut.Write(memBlock.MemoryBuffer, 0, memBlock.MemoryBuffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorFlag = true;
            }
        }
    }
}
