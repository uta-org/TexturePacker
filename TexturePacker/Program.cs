using System;
using System.Collections.Generic;
using System.Linq;

namespace TexturePacker
{
    internal sealed class Program
    {
        private static void DisplayInfo()
        {
            Console.WriteLine("  usage: TexturePacker -sp xxx -ft xxx -o xxx [-s xxx] [-b x] [-d]");
            Console.WriteLine("            -sp | --sourcepath : folder to recursively scan for textures to pack");
            Console.WriteLine("            -ft | --filetype   : types of textures to pack (*.png only for now)");
            Console.WriteLine("            -f  | --filename   : name of the atlas file to generate");
            Console.WriteLine("            -o  | --output     : specify different type of output (TXT, JSON, XML, YAML, CSV, HTML...) [non case sensitive]");
            Console.WriteLine("            -s  | --size       : size of 1 side of the atlas file in pixels. Default = 1024");
            Console.WriteLine("            -b  | --border     : nb of pixels between textures in the atlas. Default = 0");
            Console.WriteLine("            -sm | --safemode   : ensure that textures have the same dimensions.");
            Console.WriteLine("            -fp | --fullpath   : output full paths in json files.");
            Console.WriteLine("            -d  | --debug      : output debug info in the atlas");
            Console.WriteLine("  ex: TexturePacker -sp C:\\Temp\\Textures -ft *.png -f C:\\Temp\atlas.txt -o jmin -s 512 -b 2 --safemode -fullpath --debug");
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("TexturePacker - Package rect/non pow 2 textures into square power of 2 atlas");

            if (args.Length == 0)
            {
                DisplayInfo();
                return;
            }

            List<string> prms = args.ToList();

            string sourcePath = "";
            string searchPattern = "";
            string outName = "";
            OutputType type = default;
            int textureSize = 1024;
            int border = 0;
            bool safeMode = false;
            bool fullPath = false;
            bool debug = false;

            for (int ip = 0; ip < prms.Count; ++ip)
            {
                prms[ip] = prms[ip].ToLowerInvariant();

                switch (prms[ip])
                {
                    case "-sp":
                    case "--sourcepath":
                        if (!prms[ip + 1].StartsWith("-"))
                        {
                            sourcePath = prms[ip + 1];
                            ++ip;
                        }
                        break;

                    case "-ft":
                    case "--filetype":
                        if (!prms[ip + 1].StartsWith("-"))
                        {
                            searchPattern = prms[ip + 1];
                            ++ip;
                        }
                        break;

                    case "-f":
                    case "--filename":
                        if (!prms[ip + 1].StartsWith("-"))
                        {
                            outName = prms[ip + 1];
                            ++ip;
                        }
                        break;

                    case "-o":
                    case "--output":
                        if (!prms[ip + 1].StartsWith("-"))
                        {
                            string rawType = prms[ip + 1];
                            type = (OutputType)Enum.Parse(typeof(OutputType), rawType, true);
                            ++ip;
                        }
                        break;

                    case "-s":
                    case "--size":
                        if (!prms[ip + 1].StartsWith("-"))
                        {
                            textureSize = int.Parse(prms[ip + 1]);
                            ++ip;
                        }
                        break;

                    case "-b":
                    case "--border":
                        if (!prms[ip + 1].StartsWith("-"))
                        {
                            border = int.Parse(prms[ip + 1]);
                            ++ip;
                        }
                        break;

                    case "-sm":
                    case "--safemode":
                        safeMode = true;
                        break;

                    case "-fp":
                    case "--fullpath":
                        fullPath = true;
                        break;

                    case "-d":
                    case "--debug":
                        debug = true;
                        break;
                }
            }

            if (sourcePath == "" || searchPattern == "" || outName == "")
            {
                DisplayInfo();
                return;
            }
            else
            {
                Console.WriteLine("Processing, please wait");
            }

            Packer packer = new Packer();

            packer.Process(type, outName, sourcePath, searchPattern, textureSize, border, debug, safeMode, fullPath);
        }
    }
}