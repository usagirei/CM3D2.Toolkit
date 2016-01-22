using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using CM3D2.Toolkit;
using CM3D2.Toolkit.Arc;
using CM3D2.Utils.Common;
using CM3D2.Utils.Common.Logging;
using CM3D2.Utils.Common.Options;

namespace CM3D2.Arc.Merger
{
    class Program
    {
        public const string WILDCARD = "*";

        static void Main(string[] args)
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
                $"Output ARC Name. Use Wildcard for Glob 'merged' (i.e model_* -> model_merged).\n(Default: {argObj.OutputFile})",
                arg => argObj.OutputFile = arg);
            parser.Add("n|name=",
                $"ARC Metadata Name. Use Wildcard for Output Name.\n(Default: {argObj.Name})",
                arg => argObj.Name = arg);

            var ext = parser.Parse(args);
            // Set Unparsed as Inputs
            argObj.InputFiles.AddRange(unparsed);

            if (!argObj.InputFiles.Any())
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
            var logger = new ConsoleLogger("MERGER");
            var targets = args.InputFiles
                                .Select(s => new
                                {
                                    Glob = s,
                                    GlobbedFiles = Globbing.GlobFiles(s)
                                })
                                .SelectMany(t => t.GlobbedFiles.Select(s => new
                                {
                                    Glob = t.Glob,
                                    File = s
                                }))
                              //.Where(Directory.Exists)
                              .Select(t =>
                              {
                                  var dirName = Path.GetFileNameWithoutExtension(t.File);
                                  var outGlob = t.Glob.IndexOf(Program.WILDCARD) < 0
                                                    ? "merged"
                                                    : t.Glob.Replace(Program.WILDCARD, "merged");
                                  var outGlobName = Path.GetFileNameWithoutExtension(outGlob);
                                  var ouputArc = args.OutputFile.Replace(Program.WILDCARD, outGlobName);
                                  var outputName = Path.GetFileNameWithoutExtension(ouputArc);
                                  var metaName = args.Name.Replace(WILDCARD, outputName);
                                  return new
                                  {
                                      Input = t.File,
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
                var sourceFiles = group.Select(d => d.Input).ToList();

                var afs = new ArcFileSystem(group.First().Name)
                {
                    Logger = logger,
                };
                afs.CompressList.Clear();
                afs.CompressList.AddRange(args.Compress);

                foreach (var file in sourceFiles)
                {
                    var tempDir = afs.CreateDirectory($"<$TEMP-{Guid.NewGuid()}$>");
                    if (!afs.LoadArc(file, tempDir))
                    {
                        logger.Error("Error Loading Arc '{0}'", file);
                        afs.Delete(tempDir, true);
                    }
                    else
                    {
                        afs.MergeMove(tempDir, afs.Root);
                    }
                }

                Utilities.CreateBackup(targetArc);
                using (var fs = File.Open(targetArc, FileMode.Create))
                    afs.Save(fs);
            }
        }
    }


    public class Arguments
    {
        public IEnumerable<string> Compress => CompressRaw.Split('|');

        public string CompressRaw { get; set; } = "*.ks|*.tjs|*.menu";

        public List<string> GlobbedFiles => InputFiles.SelectMany(Globbing.GlobFiles).ToList();

        public List<string> InputFiles { get; set; } = new List<string>();

        public string Name { get; set; } = Program.WILDCARD;

        public string OutputFile { get; set; } = Program.WILDCARD;
    }
}
