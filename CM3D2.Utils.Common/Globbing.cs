using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CM3D2.Utils.Common
{
/// <summary>
/// Provides Globbing Methods
/// </summary>
    public static class Globbing
    {
        /// <summary>
        ///     Globbing Mode
        /// </summary>
        public enum GlobMode
        {
            Files = 0,
            Directories = 1,
            FilesAndDirectories = 2
        }

        /// <summary>
        ///     Converts a Wildcard Pattern into a Regular Expression
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";
        }

        /// <summary>
        ///     Returns a list of entries that matches a wildcard pattern
        /// </summary>
        /// <param name="list">Entries to Glob</param>
        /// <param name="glob">Pattern to Match</param>
        /// <returns></returns>
        public static IEnumerable<string> GlobList(IEnumerable<string> list, string glob)
        {
            var p = WildcardToRegex(glob);
            foreach (var li in list.Where(s => Regex.IsMatch(s, p)))
                yield return li;
        }

        /// <summary>
        ///     Return a list of entries that matches a wildcard pattern
        /// </summary>
        /// <param name="glob">Pattern to match</param>
        /// <param name="mode">Globbing Mode</param>
        /// <returns>All matching paths</returns>
        public static IEnumerable<string> Glob(string glob, GlobMode mode)
        {
            return Glob(PathHead(glob) + Path.DirectorySeparatorChar, PathTail(glob), mode);
        }

        /// <summary>
        ///     Returns a list of files that match a wildcard pattern
        /// </summary>
        /// <param name="glob">Pattern to match</param>
        /// <returns>All matching paths</returns>
        public static IEnumerable<string> GlobFiles(string glob)
        {
            return Glob(glob, GlobMode.Files);
        }

        /// <summary>
        ///     Returns a list of directories that match a wildcard pattern
        /// </summary>
        /// <param name="glob">Pattern to match</param>
        /// <returns>All matching paths</returns>
        public static IEnumerable<string> GlobDirs(string glob)
        {
            return Glob(glob, GlobMode.Directories);
        }

        /// <summary>
        ///     Returns a list of filess and directories that match a wildcard pattern
        /// </summary>
        /// <param name="glob">Pattern to match</param>
        /// <returns>All matching paths</returns>
        public static IEnumerable<string> GlobFilesAndDirs(string glob)
        {
            return Glob(glob, GlobMode.FilesAndDirectories);
        }

        /// <summary>
        ///     Combines Expanded Head with non-expanded Tail Enumeration
        /// </summary>
        /// <param name="head">Wildcard-expanded part</param>
        /// <param name="tail">Not wildcard-expanded part</param>
        /// <param name="mode">Glob Mode</param>
        /// <returns></returns>
        private static IEnumerable<string> Glob(string head, string tail, GlobMode mode)
        {
            Func<string, string, string[]> findFunc;
            switch (mode)
            {
                case GlobMode.Files:
                    findFunc = Directory.GetFiles;
                    break;
                case GlobMode.Directories:
                    findFunc = Directory.GetDirectories;
                    break;
                case GlobMode.FilesAndDirectories:
                    findFunc = Directory.GetFileSystemEntries;
                    break;
                default:
                    throw new ArgumentException("Invalid Mode", nameof(mode));
            }

            if (PathTail(tail) == tail)
                foreach (string path in findFunc(head, tail).OrderBy(s => s))
                    yield return path;
            else
                foreach (string dir in Directory.GetDirectories(head, PathHead(tail)).OrderBy(s => s))
                    foreach (string path in Glob(Path.Combine(head, dir), PathTail(tail), mode))
                        yield return path;
        }

        /// <summary>
        ///     Returns the first element of a file path
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>First logical unit</returns>
        private static string PathHead(string path)
        {
            var dsc = Path.DirectorySeparatorChar;
            if (!path.Contains(dsc))
            {
                return ".";
            }
            // handle case of \\share\vol\foo\bar -- return \\share\vol as 'head'
            // because the dir stuff won't let you interrogate a server for its share list
            else if (path.StartsWith("" + dsc + dsc))
            {
                var split = path.Substring(2).Split(dsc);
                return path.Substring(0, 2) + split[0] + dsc + split[1];
            }
            else
            {
                return path.Split(dsc)[0];
            }
        }

        /// <summary>
        ///     Return everything but the first element of a file path
        ///     e.g. PathTail("C:\TEMP\foo.txt") = "TEMP\foo.txt"
        /// </summary>
        /// <param name="path">file path</param>
        /// <returns>all but the first logical unit</returns>
        private static string PathTail(string path)
        {
            return !path.Contains(Path.DirectorySeparatorChar)
                       ? path
                       : path.Substring(1 + PathHead(path).Length);
        }
    }
}