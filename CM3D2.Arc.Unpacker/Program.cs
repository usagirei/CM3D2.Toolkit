// --------------------------------------------------
// CM3D2.Arc.Unpacker - Program.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CM3D2.Toolkit;
using CM3D2.Toolkit.Arc;
using CM3D2.Toolkit.Logging;
using CM3D2.Utils.Common;
using CM3D2.Utils.Common.Logging;

namespace CM3D2.Arc.Unpacker
{
    class Program
    {
        private const string WILDCARD = "*";

        static void Main(string[] args)
        {
            var opts = new Options();

            var parser = new Utils.Common.Options.OptionSet();
            parser.Add("o|output=", arg => opts.InputFiles.Add(arg));
            parser.Add("<>", arg => opts.InputFiles.Add(arg));
            parser.Parse(args);
            SubMain(opts);
        }

        private static void SubMain(Options opts)
        {
            var logger = new ConsoleLogger("UNPACK");
            foreach (var file in opts.InputFiles)
            {
                if (!File.Exists(file))
                {
                    logger.Error("File not Found: '{0}'", file);
                    continue;
                }
                if (!ArcFileSystem.DetectMagic(file))
                {
                    logger.Error("Not a ARC File: '{0}'", file);
                    continue;
                }

                var afs = new ArcFileSystem
                {
                    Logger = logger
                };
                if (!afs.LoadArc(file))
                {
                    logger.Error("Error Loading ARC File: '{0}'", file);
                    continue;
                }

                var arcName = Path.GetFileNameWithoutExtension(file);
                var outDir = opts.OutputDir.Replace("*", arcName);

                var cnt = 0;
                var tot = afs.Files.Count();
                foreach (var entry in afs.Files)
                {
                    cnt++;
                    var shortName = entry.Name;
                    var fullName = entry.FullName;
                    var ptr = entry.Pointer;
                    var rootName = afs.Root.Name;
                    var relativeName = fullName.Substring(rootName.Length + 1);
                    var targetPath = Path.Combine(outDir, relativeName);
                    var targetDir = Path.GetDirectoryName(targetPath);
                    logger.Info("[{0}/{1}] Extracting '{2}'", cnt, tot, shortName);
                    Directory.CreateDirectory(targetDir);
                    File.WriteAllBytes(targetPath, ptr.Compressed ? ptr.Decompress().Data : ptr.Data);
                }
            }
        }

        public class Options
        {
            public List<string> InputFiles { get; set; } = new List<string>();

            public List<string> GlobbedFiles => InputFiles.SelectMany(Globbing.GlobFiles).ToList();

            public string OutputDir { get; set; } = "*";
        }
    }
}
