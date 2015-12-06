
namespace VersionStitcher.Win32
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    internal struct VarFileInfo
    {
        public WORD wLength;
        public WORD wValueLength;
        public WORD wType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string szKey;
        public WORD Padding;
        public WORD Padding2;
        //Var Children;
    }
}
