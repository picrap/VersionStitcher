// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32Resources
{
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
            Offset = serializer.Offset;
            return serializer.SerializeWORD(ref wLength)
                   && serializer.SerializeWORD(ref wValueLength)
                   && serializer.SerializeWORD(ref wType)
                   && serializer.SerializeWCHAR(ref szKey)
                   && serializer.PadDWORD()
                   && SerializeBody(serializer);
        }

        public virtual bool SerializeBody(ResourceSerializer serializer) => true;
    }
}
