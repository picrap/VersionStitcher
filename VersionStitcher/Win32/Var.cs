
namespace VersionStitcher.Win32
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;
    using DWORD = System.UInt32;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
    internal struct Var
    {
        WORD wLength;
        WORD wValueLength;
        WORD wType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string szKey;
        WORD Padding;
        DWORD Value;
    }
}
