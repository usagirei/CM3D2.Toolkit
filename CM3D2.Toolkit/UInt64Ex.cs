// --------------------------------------------------
// CM3D2.Toolkit - UInt64Ex.cs
// --------------------------------------------------

#region Usings

using System;
using System.Runtime.InteropServices;

#endregion

namespace CM3D2.Toolkit
{
    /// <summary>
    ///     Packed UInt64, with Individually Accessible Words
    ///     <para />
    ///     _Byte =  8 Bits;
    ///     _Word = 16 Bits;
    ///     DWord = 32 Bits;
    ///     QWord = 64 Bits;
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    internal partial struct UInt64Ex
    {
        [FieldOffset(0)] public UInt64 QWORD;
    }

    internal partial struct UInt64Ex
    {
        [FieldOffset(0)] public UInt32 DWORD_0;

        [FieldOffset(4)] public UInt32 DWORD_1;
    }

#if DEF_WORDS
    internal partial struct UInt64Ex
    {
        [FieldOffset(0)] public UInt16 WORD_0;

        [FieldOffset(2)] public UInt16 WORD_1;

        [FieldOffset(4)] public UInt16 WORD_2;

        [FieldOffset(6)] public UInt16 WORD_3;
    }
#endif

#if DEF_BYTES
    internal partial struct UInt64Ex
    {
        [FieldOffset(0)] public Byte BYTE_0;

        [FieldOffset(1)] public Byte BYTE_1;

        [FieldOffset(2)] public Byte BYTE_2;

        [FieldOffset(3)] public Byte BYTE_3;

        [FieldOffset(5)] public Byte BYTE_5;

        [FieldOffset(6)] public Byte BYTE_6;

        [FieldOffset(7)] public Byte BYTE_7;

        [FieldOffset(8)] public Byte BYTE_8;
    }
#endif
}
