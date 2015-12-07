// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32Resources.Serialization
{
    using System;
    using System.IO;

    public abstract class ResourceSerializer
    {
        public abstract bool IsWriting { get; }

        public abstract int Offset { get; }

        public abstract bool SerializeWORD(ref ushort word);
        public abstract bool SerializeWORD(ref short word);
        public abstract bool SerializeDWORD(ref uint dword);
        public abstract bool SerializeWCHAR(ref string wchar);
        public abstract bool SerializeWCHAR(ref string wchar, ref short valueLength);
        public abstract bool SerializeValue(ref byte[] value, ref short valueLength);

        public abstract bool PadDWORD();

        public abstract bool Serialize<TSerializedResource>(ref TSerializedResource serializedResource, ref short length)
            where TSerializedResource : SerializedResource, new();

        public abstract bool Serialize<TSerializedResource>(KeyedResource owner, ref TSerializedResource[] serializedResources, ref short length, params Type[] expectedTypes)
            where TSerializedResource : SerializedResource, new();

        public static TResource Deserialize<TResource>(Stream stream)
            where TResource : SerializedResource, new()
        {
            var deserializer = new ReadResourceSerializer(stream);
            var resource = new TResource();
            short l = 0;
            deserializer.Serialize(ref resource, ref l);
            return resource;
        }
        public static void Serialize<TResource>(TResource resource, Stream stream)
            where TResource : SerializedResource, new()
        {
            var serializer = new WriteResourceSerializer(stream);
            short l = 0;
            serializer.Serialize(ref resource, ref l);
        }
    }
}
