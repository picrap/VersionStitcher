
namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;
    using DWORD = System.UInt32;

    public class Var : ValidatedKeyedResource
    {
        //public DWORD Value;
        public byte[] Value;

        public override bool Validate() => szKey == "Translation";

        public override bool SerializeBody(ResourceSerializer serializer)
        {
            return serializer.SerializeValue(ref Value, ref wValueLength);
        }
    }
}
