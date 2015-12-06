// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;
    using WCHAR = System.Char;

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    internal struct VS_VERSIONINFO
    {
        /// <summary>
        /// The length, in bytes, of the VS_VERSIONINFO structure
        /// </summary>
        [FieldOffset(0)]
        public WORD wLength;

        /// <summary>
        /// The length, in bytes, of the Value member. 
        /// </summary>
        [FieldOffset(2)]
        public WORD wValueLength;

        /// <summary>
        /// The type of data in the version resource. 
        /// This member is 1 if the version resource contains text data and 0 if the version resource contains binary data.
        /// </summary>
        [FieldOffset(4)]
        public WORD wType;

        /// <summary>
        /// The Unicode string L"VS_VERSION_INFO".
        /// </summary>
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        // this is the expression of my miserable failure
        [FieldOffset(6)]
        public WCHAR szKey0;
        [FieldOffset(8)]
        public WCHAR szKey1;
        [FieldOffset(10)]
        public WCHAR szKey2;
        [FieldOffset(12)]
        public WCHAR szKey3;
        [FieldOffset(14)]
        public WCHAR szKey4;
        [FieldOffset(16)]
        public WCHAR szKey5;
        [FieldOffset(18)]
        public WCHAR szKey6;
        [FieldOffset(20)]
        public WCHAR szKey7;
        [FieldOffset(22)]
        public WCHAR szKey8;
        [FieldOffset(24)]
        public WCHAR szKey9;
        [FieldOffset(26)]
        public WCHAR szKey10;
        [FieldOffset(28)]
        public WCHAR szKey11;
        [FieldOffset(30)]
        public WCHAR szKey12;
        [FieldOffset(32)]
        public WCHAR szKey13;
        [FieldOffset(34)]
        public WCHAR szKey14;

        /// <summary>
        /// Contains as many zero words as necessary to align the Value member on a 32-bit boundary.
        /// </summary>
        [FieldOffset(36)]
        public WORD Padding1;

        /// <summary>
        /// Arbitrary data associated with this VS_VERSIONINFO structure.
        /// </summary>
        [FieldOffset(40)]
        public VS_FIXEDFILEINFO Value;

        /// <summary>
        /// As many zero words as necessary to align the Children member on a 32-bit boundary.
        /// </summary>
        [FieldOffset(92)]
        public WORD Padding2;

        /// <summary>
        /// An array of zero or one StringFileInfo structures,
        /// </summary>
        [FieldOffset(94)]
        public WORD Children;
    }
}

