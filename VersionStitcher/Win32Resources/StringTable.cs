
namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;

    public class StringTable : KeyedResource
    {
        public String[] Children;

        public override bool SerializeBody(ResourceSerializer serializer)
        {
            return serializer.Serialize(this, ref Children, ref wLength, typeof(String));
        }
    }
}
