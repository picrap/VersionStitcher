
namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using Serialization;
    using WORD = System.Int16;

    public class StringTable : KeyedResource
    {
        public String[] Children;

        public override bool SerializeChildren(ResourceSerializer serializer)
        {
            return serializer.Serialize(this, ref Children, ref wLength, typeof(String));
        }
    }
}
