// --------------------------------------------------
// CM3D2.Toolkit - FileHashTable.cs
// --------------------------------------------------

using System.Runtime.InteropServices;

namespace CM3D2.Toolkit.Arc.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FileHashTable
    {
        public long Header;
        public ulong ID;
        public int Dirs;
        public int Files;
        public int Depth;
        public int Junk;
        public FileEntry[] DirEntries;
        public FileEntry[] FileEntries;
        public ulong[] Parents;
        public FileHashTable[] SubdirEntries;
    }
}
