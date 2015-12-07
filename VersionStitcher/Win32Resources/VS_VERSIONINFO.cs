// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using Serialization;
    using WORD = System.Int16;
    using WCHAR = System.Char;

    public class VS_VERSIONINFO : ValidatedKeyedResource
    {
        /// <summary>
        /// Arbitrary data associated with this VS_VERSIONINFO structure.
        /// </summary>
        public VS_FIXEDFILEINFO Value;

        /// <summary>
        /// An array of zero or one StringFileInfo structures,
        /// </summary>
        //public WORD Children;
        public KeyedResource[] Children;

        public override bool Validate() => szKey == "VS_VERSION_INFO";

        public override bool SerializeValue(ResourceSerializer serializer) => serializer.Serialize(ref Value, ref wValueLength) && serializer.PadDWORD();
        public override bool SerializeChildren(ResourceSerializer serializer) => serializer.Serialize(this, ref Children, ref wLength, typeof(VarFileInfo), typeof(StringFileInfo));
    }
}
