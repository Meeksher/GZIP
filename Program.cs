using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Threading;

namespace GZIP
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 3)
            {
                ShowInfo();
                return;
            }
            
            string command = args[0], input = args[1], output = args[2];

            GZIP zipper;

            switch (command.ToLower())
            {
                case "compress":
                    zipper = new Compressor(input, output);
                    break;
                case "decompress":
                    zipper = new Decompressor(input, output);
                    break;
                default:
                    Console.WriteLine($"Error: Command '{command}' not found.");
                    ShowInfo();
                    return;
            }
            zipper.Start();
            
        }

        static void ShowInfo()
        {
            Console.WriteLine("USAGE:\n" +
                "   compress <source file> <dest file> - compressing source file and saves into dest.\n" +
                "   decompress <source file> <dest file> - decompressing source file and saves into dest.");
        }
    }
}
