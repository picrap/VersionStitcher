
namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using Serialization;
    using WORD = System.Int16;
    using DWORD = System.UInt32;

    public class Var : ValidatedKeyedResource
    {
        public byte[] Value;

        public override bool Validate() => szKey == "Translation";

        public override bool SerializeValue(ResourceSerializer serializer)
        {
            return serializer.SerializeValue(ref Value, ref wValueLength);
        }
    }
}
