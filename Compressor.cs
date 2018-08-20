using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZIP
{
    class Compressor : GZIP
    {
        public Compressor(string input, string output) : base(input, output)
        {

        }

        protected override void Consume()
        {
            try
            {
                while (!errorFlag)
                {
                    var memBlock = producerQueue.Dequeue();
                    if (memBlock == null) return;

                    using (MemoryStream compressedMemoryStream = new MemoryStream())
                    {
                        using (GZipStream gzip = new GZipStream(compressedMemoryStream, CompressionMode.Compress))
                        {
                            gzip.Write(memBlock.MemoryBuffer, 0, memBlock.MemoryBuffer.Length);
                        }

                        MemoryBlock compressed = new MemoryBlock(memBlock.Index, compressedMemoryStream.ToArray());
                        consumerQueue.Enqueue(compressed);
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
                            byte[] memoryRead;

                            if(fsIn.Length - fsIn.Position < blockSize)
                                memoryRead = new byte[fsIn.Length - fsIn.Position];
                            else 
                                memoryRead = new byte[blockSize];

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
        
        protected override void DropData()
        {
            try
            {
                using (FileStream fsOut = new FileStream(output.FullName + ".gz", FileMode.OpenOrCreate))
                {
                    while (!errorFlag)
                    {
                        /* write memory blocks as struct:
                         *   int sizeOfBlock;
                         *   byte[] memory;
                        */

                        MemoryBlock memBlock = consumerQueue.Dequeue();
                        if (memBlock == null && consumerQueue.IsDead)
                            return;

                        var bytesLength = BitConverter.GetBytes(memBlock.MemoryBuffer.Length);
                        fsOut.Write(bytesLength, 0, bytesLength.Length);

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

            if (errorFlag) Console.WriteLine("Error occured, the program has not finished correctly.");
            else Console.WriteLine("Compressing finished.");
        }

    }
}
