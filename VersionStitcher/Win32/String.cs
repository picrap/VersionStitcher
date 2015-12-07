
namespace VersionStitcher.Win32
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    internal struct String{
 public WORD wLength;
    public WORD wValueLength;
    public WORD wType;
    //public WCHAR szKey;
    public WORD Padding;
    public WORD Value;
}


}
