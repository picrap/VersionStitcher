// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32Resources
{
    using System;

    public abstract class ResourceSerializer
    {
        public abstract bool IsWriting { get; }

        public abstract int Offset { get; }

        public abstract bool SerializeWORD(ref ushort word);
        public abstract bool SerializeWORD(ref short word);
        public abstract bool SerializeDWORD(ref uint dword);
        public abstract bool SerializeWCHAR(ref string wchar);
        public abstract bool SerializeValue(ref byte[] value, ref short valueLength);

        public abstract bool PadDWORD();

        public abstract bool Serialize<TSerializedResource>(ref TSerializedResource serializedResource)
            where TSerializedResource : SerializedResource, new();

        public abstract bool Serialize<TSerializedResource>(KeyedResource owner, ref TSerializedResource[] serializedResource, ref short length, params Type[] expectedTypes)
            where TSerializedResource : SerializedResource, new();

        public KeyedResource SerializeKeyedResource(params Type[] expectedTypes)
        {
            var keyedResourceHeader = new KeyedResource();
            if (!keyedResourceHeader.Serialize(this))
                return null;
            foreach (var expectedType in expectedTypes)
            {
                var keyedResource = (KeyedResource)Activator.CreateInstance(expectedType);
                keyedResource.Offset = keyedResourceHeader.Offset;
                keyedResource.wValueLength = keyedResourceHeader.wValueLength;
                keyedResource.wLength = keyedResourceHeader.wLength;
                keyedResource.wType = keyedResourceHeader.wType;
                keyedResource.szKey = keyedResourceHeader.szKey;
                var validatedKeyedResource = keyedResource as ValidatedKeyedResource;
                if (validatedKeyedResource == null || validatedKeyedResource.Validate())
                    return keyedResource.SerializeBody(this) ? keyedResource : null;
            }
            return null;
        }
    }
}
