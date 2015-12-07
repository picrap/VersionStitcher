
namespace VersionStitcher.Win32
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
   internal struct StringFileInfo
    {
        public WORD wLength;
        public WORD wValueLength;
        public WORD wType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string szKey;
        public WORD Padding;
        //StringTable Children;
    }

    internal class VariableStringFileInfo: ValueVariableStruct<StringFileInfo>
}
