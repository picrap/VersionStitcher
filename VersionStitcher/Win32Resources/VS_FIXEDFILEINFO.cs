// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using Serialization;
    using DWORD = System.UInt32;

    public class VS_FIXEDFILEINFO: SerializedResource
    {
        public DWORD dwSignature;
        public DWORD dwStrucVersion;
        public DWORD dwFileVersionMS;
        public DWORD dwFileVersionLS;
        public DWORD dwProductVersionMS;
        public DWORD dwProductVersionLS;
        public DWORD dwFileFlagsMask;
        public DWORD dwFileFlags;
        public DWORD dwFileOS;
        public DWORD dwFileType;
        public DWORD dwFileSubtype;
        public DWORD dwFileDateMS;
        public DWORD dwFileDateLS;

        public override bool Serialize(ResourceSerializer serializer)
        {
            return serializer.SerializeDWORD(ref dwSignature) && serializer.SerializeDWORD(ref dwStrucVersion)
                   && serializer.SerializeDWORD(ref dwFileVersionMS) && serializer.SerializeDWORD(ref dwFileVersionLS)
                   && serializer.SerializeDWORD(ref dwProductVersionMS) && serializer.SerializeDWORD(ref dwProductVersionLS)
                   && serializer.SerializeDWORD(ref dwFileFlagsMask) && serializer.SerializeDWORD(ref dwFileFlags)
                   && serializer.SerializeDWORD(ref dwFileOS)
                   && serializer.SerializeDWORD(ref dwFileType) && serializer.SerializeDWORD(ref dwFileSubtype)
                   && serializer.SerializeDWORD(ref dwFileDateMS) && serializer.SerializeDWORD(ref dwFileDateLS);
        }
    };
}