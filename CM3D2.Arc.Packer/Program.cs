// --------------------------------------------------
// CM3D2.Arc.Packer - Program.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using CM3D2.Toolkit.Arc;
using CM3D2.Utils.Common;
using CM3D2.Utils.Common.Logging;
using CM3D2.Utils.Common.Options;

namespace CM3D2.Arc.Packer
{
    internal class Program
    {
        private const string WILDCARD = "*";

        private static void Main(string[] args)
        {
            var argObj = new Arguments();
            var parser = new OptionSet();
            var unparsed = new List<string>();

            // Parse Arguments
            parser.Add("<>",
                arg =>
                {
                    var m = OptionSet.ValueOptionRegex.Match(arg);
                    if (m.Success)
                    {
                        PrintInvalid(m.Groups["name"].Value);
                        Environment.Exit(1);
                    }
                    unparsed.Add(arg);
                }
               );
            parser.Add("?|help",
                "Displays this help message",
                arg =>
                {
                    PrintHelp(parser);
                    Environment.Exit(0);
                });
            parser.Add("c|compress=",
                $"File Compression Filter, Pipe (|) Separated.\n(Default: {argObj.CompressRaw})",
                arg => argObj.CompressRaw = arg);
            parser.Add("o|output=",
                $"Output ARC Name. Use Wildcard for Directory Name.\n(Default: {argObj.OutputDir})",
                arg => argObj.OutputDir = arg);
            parser.Add("n|name=",
                $"ARC Metadata Name. Use Wildcard for Output Name.\n(Default: {argObj.Name})",
                arg => argObj.Name = arg);

            var ext = parser.Parse(args);
            // Set Unparsed as Inputs
            argObj.InputDirs.AddRange(unparsed);

            if (!argObj.InputDirs.Any())
            {
                PrintInvalid();
                Environment.Exit(1);
            }
            else
            {
                SubMain(argObj);
            }
        }

        private static void PrintHelp(OptionSet parser)
        {
            var exeName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Console.WriteLine($"Usage: {exeName} [options] <dir1> <dir2> .. <dirn>");
            Console.WriteLine("Options:");
            parser.WriteOptionDescriptions(Console.Out);
        }

        private static void PrintInvalid(string flag)
        {
            var exeName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Console.WriteLine($"Unrecognized Option '{flag}'.");
            Console.WriteLine($"The syntax of the command is incorrect.\nCall '{exeName} /?' for more information.");
        }

        private static void PrintInvalid()
        {
            var exeName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Console.WriteLine($"The syntax of the command is incorrect.\nCall '{exeName} /?' for more information.");
        }

        private static void SubMain(Arguments args)
        {
            var logger = new ConsoleLogger("PACKER");
            var targets = args.GlobbedDirs
                //.Where(Directory.Exists)
                              .Select(s =>
                              {
                                  var dirName = Path.GetFileNameWithoutExtension(s);
                                  var ouputArc = args.OutputDir.Replace(WILDCARD, dirName);
                                  var outputName = Path.GetFileNameWithoutExtension(ouputArc);
                                  var metaName = args.Name.Replace(WILDCARD, outputName);
                                  return new
                                  {
                                      Input = s,
                                      Output =
                                          ouputArc.EndsWith(".arc", StringComparison.OrdinalIgnoreCase)
                                              ? ouputArc
                                              : ouputArc + ".arc",
                                      Name = metaName
                                  };
                              });

            foreach (var group in targets.GroupBy(arg => arg.Output))
            {
                var targetArc = group.Key;
                var sourceDirs = group.Select(d => d.Input).ToList();

                var afs = new ArcFileSystem(group.First().Name)
                {
                    Logger = logger,
                };
                afs.CompressList.Clear();
                afs.CompressList.AddRange(args.Compress);

                foreach (var dir in sourceDirs)
                {
                    var tempDir = afs.CreateDirectory($"<$TEMP-{Guid.NewGuid()}$>");
                    if (!afs.LoadDirectory(dir, tempDir))
                    {
                        logger.Error("Error Loading Directory '{0}'", dir);
                        afs.Delete(tempDir, true);
                    }
                    else
                    {
                        afs.MergeMove(tempDir, afs.Root);
                    }
                }

                Utilities.CreateBackup(targetArc);
                var targetDir = Path.GetDirectoryName(targetArc);
                if (targetDir != null)
                    Directory.CreateDirectory(targetDir);
                using (var fs = File.Open(targetArc, FileMode.Create))
                    afs.Save(fs);
            }
        }

        public class Arguments
        {
            public IEnumerable<string> Compress => CompressRaw.Split('|');

            public string CompressRaw { get; set; } = "*.ks|*.tjs|*.menu";

            public List<string> GlobbedDirs => InputDirs.SelectMany(Globbing.GlobDirs).ToList();

            public List<string> InputDirs { get; set; } = new List<string>();

            public string Name { get; set; } = WILDCARD;

            public string OutputDir { get; set; } = WILDCARD;
        }
    }
}
