
namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using Serialization;
    using WORD = System.Int16;

    public class StringFileInfo : ValidatedKeyedResource
    {
        public StringTable[] Children;

        public override bool Validate() => szKey == "StringFileInfo";

        public override bool SerializeChildren(ResourceSerializer serializer)
        {
            return serializer.Serialize(this, ref Children, ref wLength, typeof(StringTable));
        }
    }
}
