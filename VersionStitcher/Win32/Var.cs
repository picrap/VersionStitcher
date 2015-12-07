
namespace VersionStitcher.Win32
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;
    using DWORD = System.UInt32;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    internal struct Var
    {
        public WORD wLength;
        public WORD wValueLength;
        public WORD wType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string szKey;
        public WORD Padding;
        public WORD Padding2;
        //public DWORD Value;
    }

    internal class VariableVar : RawValueVariableStruct<Var>
    {
        protected override int ValueLength { get { return Struct.wValueLength; } set { Struct.wValueLength = (WORD)value; } }
    }
}
