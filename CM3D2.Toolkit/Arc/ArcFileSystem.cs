// --------------------------------------------------
// CM3D2.Toolkit - ArcFileSystem.cs
// --------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using CM3D2.Toolkit.Arc.Entry;
using CM3D2.Toolkit.Logging;

namespace CM3D2.Toolkit.Arc
{
    // Public Facing
    /// <summary>
    ///     Arc File System Class
    ///     <para></para>
    ///     Provides Methods and Functions to Handle Loading, Creating, Modifying and Saving of KISS's CM3D2 .arc Files
    /// </summary>
    public partial class ArcFileSystem
    {
        /// <summary>
        ///     Instance Logger (Defaults to <see cref="NullLogger.Instance"/>)
        /// </summary>
        public ILogger Logger { get; set; } = NullLogger.Instance;

        private readonly List<ArcDirectoryEntry> _directories;
        private readonly List<ArcFileEntry> _files;
        private string _name;
        private bool _nameNotSet = true;

        /// <summary>
        ///     List of Extensions that will be Compressed upon saving
        /// </summary>
        public List<string> CompressList { get; } = new List<string>
        {
            "*.ks",
            "*.menu",
            "*.tjs",
        };

        /// <summary>
        ///     Listing of all Directories
        /// </summary>
        public IEnumerable<ArcDirectoryEntry> Directories => _directories;

        /// <summary>
        ///     Listing of all Files
        /// </summary>
        public IEnumerable<ArcFileEntry> Files => _files;

        /// <summary>
        ///     Internal Name of this .arc path
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _nameNotSet = false;
                _name = value;
                Root.Name = @"CM3D2ToolKit:" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar + value;
            }
        }

        /// <summary>
        ///     File System Root
        /// </summary>
        public ArcDirectoryEntry Root { get; }

        private ArcFileSystem(bool dummy)
        {
            Root = new ArcDirectoryEntry(this);

            _directories = new List<ArcDirectoryEntry>();
            _files = new List<ArcFileEntry>();
        }

        /// <summary>
        ///     Creates a new Empty Instance of an Arc File System
        /// </summary>
        public ArcFileSystem() : this(true)
        {
            Name = "root";
            _nameNotSet = true;
        }

        /// <summary>
        ///     Creates a new Empty Instance of an Arc File System named <paramref name="name" />
        /// </summary>
        /// <param name="name">File System Name</param>
        public ArcFileSystem(string name) : this(true)
        {
            Name = name;
        }

        /// <summary>
        ///     Loads an Arc File in this Instance into <paramref name="parentDir" />
        /// </summary>
        /// <param name="arcFile">Arc File Path</param>
        /// <param name="parentDir">Parent Directory</param>
        /// <returns>True if Successful</returns>
        public bool LoadArc(string arcFile, ArcDirectoryEntry parentDir)
        {
            Logger.Info("Loading Arc '{0}' into '{1}'", Path.GetFileName(arcFile), parentDir);
            return LoadArcFile_Internal(arcFile, parentDir);
        }

        /// <summary>
        ///     Loads an Arc File into <see cref="Root" />
        /// </summary>
        /// <param name="arcFile">Arc File Path</param>
        /// <returns>True if Successful</returns>
        public bool LoadArc(string arcFile)
        {
            return LoadArc(arcFile, Root);
        }

        /// <summary>
        ///     Loads a Directory and all its files recursively into <paramref name="parentDir" />
        /// </summary>
        /// <param name="path">File Path</param>
        /// <param name="parentDir">Parent Directory</param>
        /// ///
        /// <returns>True if Successful</returns>
        public bool LoadDirectory(string path, ArcDirectoryEntry parentDir)
        {
            Logger.Info("Loading Directory '{0}' into '{1}'", Path.GetFileName(path), parentDir);
            return LoadDirectory_Internal(path, parentDir);
        }

        /// <summary>
        ///     Loads a Directory and its all files recursively into <see cref="Root" />
        /// </summary>
        /// <param name="path">File Path</param>
        /// <returns>True if Successful</returns>
        public bool LoadDirectory(string path)
        {
            return LoadDirectory(path, Root);
        }

        /// <summary>
        ///     Loads a Single File into <paramref name="parentDir" />
        /// </summary>
        /// <param name="path">File Path</param>
        /// <param name="parentDir">Parent Directory</param>
        /// <returns>True if Successful</returns>
        public bool LoadFile(string path, ArcDirectoryEntry parentDir)
        {
            Logger.Info("Loading File '{0}' into '{1}'", Path.GetFileName(path), parentDir);
            return LoadFile_Internal(path, parentDir);
        }

        /// <summary>
        ///     Loads a Single File into <see cref="Root" />
        /// </summary>
        /// <param name="path">File Path</param>
        /// <returns>True if Successful</returns>
        public bool LoadFile(string path)
        {
            return LoadFile(path, Root);
        }

        /// <summary>
        ///     Writes this File System instance to the <see cref="Stream" /> given by <paramref name="stream" />
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>True if Successful</returns>
        public bool Save(Stream stream)
        {
            Logger.Info("Saving into '{0}'", stream);
            return Save_Internal(stream);
        }

        /// <summary>
        ///     Removes invalid characters from the given File Name
        ///     <para />
        ///     See <see cref="Path.GetInvalidFileNameChars"/>
        /// </summary>
        /// <param name="input">File Name</param>
        /// <returns>File Name without invalid characters</returns>
        public static string CleanFileName(string input)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(input
                .Where(x => !invalidChars.Contains(x))
                .ToArray());
        }

        /// <summary>
        ///     Detects if the ARC Magic Identifier is present the specified file
        /// </summary>
        /// <param name="file">File to Check</param>
        /// <returns>True if ARC Magic ID detected</returns>
        public static bool DetectMagic(string file)
        {
            return DetectMagic_Internal(file);
        }
    }
}
