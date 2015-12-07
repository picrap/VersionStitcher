
namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;

    public class String : KeyedResource
    {
        public string Value;

        public override bool SerializeBody(ResourceSerializer serializer)
        {
            return serializer.SerializeWCHAR(ref Value) && serializer.PadDWORD();
        }
    }
}
