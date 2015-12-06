// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;
    using WCHAR = System.Char;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
    internal struct VS_VERSIONINFO
    {
        /// <summary>
        /// The length, in bytes, of the VS_VERSIONINFO structure
        /// </summary>
        public WORD wLength;

        /// <summary>
        /// The length, in bytes, of the Value member. 
        /// </summary>
        public WORD wValueLength;

        /// <summary>
        /// The type of data in the version resource. 
        /// This member is 1 if the version resource contains text data and 0 if the version resource contains binary data.
        /// </summary>
        public WORD wType;

        /// <summary>
        /// The Unicode string L"VS_VERSION_INFO".
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string szKey;

        /// <summary>
        /// Contains as many zero words as necessary to align the Value member on a 32-bit boundary.
        /// </summary>
        public WORD Padding1;
        public WORD Padding1b;

        /// <summary>
        /// Arbitrary data associated with this VS_VERSIONINFO structure.
        /// </summary>
        public VS_FIXEDFILEINFO Value;

        /// <summary>
        /// As many zero words as necessary to align the Children member on a 32-bit boundary.
        /// </summary>
        //public WORD Padding2;

        /// <summary>
        /// An array of zero or one StringFileInfo structures,
        /// </summary>
        //public WORD Children;
    }
}

