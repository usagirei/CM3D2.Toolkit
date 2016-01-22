// --------------------------------------------------
// CM3D2.Toolkit - FileTableEntryHolder.cs
// --------------------------------------------------

using CM3D2.Toolkit.Arc.FilePointer;

namespace CM3D2.Toolkit.Arc.LambdaHolders
{
    internal class FileTableEntryHolder
    {
        public bool Compress;
        public FilePointerBase Pointer;
        public ulong UUID;
        public string Name;
    }
}
