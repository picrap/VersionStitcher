// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32Resources
{
    using Serialization;
    using WORD = System.Int16;
    using WCHAR = System.String;

    public class KeyedResource : SerializedResource
    {
        internal int Offset { get; set; }

        public WORD wLength;
        public WORD wValueLength;
        public WORD wType;
        public WCHAR szKey;

        public override bool Serialize(ResourceSerializer serializer)
        {
            return serializer.SerializeLength(s => SerializeHeader(s) && SerializeValue(s) && SerializeChildren(s), ref wLength);
        }

        public virtual bool SerializeHeader(ResourceSerializer serializer)
        {
            return serializer.SerializeWORD(ref wLength)
                   && serializer.SerializeWORD(ref wValueLength)
                   && serializer.SerializeWORD(ref wType)
                   && serializer.SerializeWCHAR(ref szKey)
                   && serializer.PadDWORD();
        }

        public virtual bool SerializeValue(ResourceSerializer serializer) => true;

        public virtual bool SerializeChildren(ResourceSerializer serializer) => true;
    }
}
