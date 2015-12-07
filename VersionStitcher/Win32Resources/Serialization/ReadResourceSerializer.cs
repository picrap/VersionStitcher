// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32Resources.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class ReadResourceSerializer : ResourceSerializer, IDisposable
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer = new byte[4];
        private int _offset;
        public override bool IsWriting => false;

        public override int Offset => _offset;

        public ReadResourceSerializer(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }

        //public static implicit operator ReadResourceSerializer(Stream stream)
        //{
        //    return new ReadResourceSerializer(stream);
        //}

        private int Read(byte[] buffer, int length)
        {
            var bytesRead = _stream.Read(buffer, 0, length);
            _offset += bytesRead;
            return bytesRead;
        }

        public override bool SerializeWORD(ref ushort word)
        {
            if (Read(_buffer, 2) != 2)
                return false;

            word = BitConverter.ToUInt16(_buffer, 0);
            return true;
        }

        public override bool SerializeWORD(ref short word)
        {
            if (Read(_buffer, 2) != 2)
                return false;

            word = BitConverter.ToInt16(_buffer, 0);
            return true;
        }

        public override bool SerializeDWORD(ref uint dword)
        {
            if (Read(_buffer, 4) != 4)
                return false;

            dword = BitConverter.ToUInt32(_buffer, 0);
            return true;
        }

        public override bool SerializeWCHAR(ref string wchar)
        {
            ushort s = 0;
            var stringBuilder = new StringBuilder();
            for (;;)
            {
                if (!SerializeWORD(ref s))
                    return false;
                if (s == 0)
                {
                    wchar = stringBuilder.ToString();
                    return true;
                }
                var c = (char)s;
                stringBuilder.Append(c);
            }
        }

        public override bool SerializeWCHAR(ref string wchar, ref short valueLength) => SerializeWCHAR(ref wchar);

        public override bool SerializeValue(ref byte[] value, ref short valueLength)
        {
            value = new byte[valueLength];
            return Read(value, value.Length) == value.Length;
        }

        public override bool PadDWORD()
        {
            // this currently assumes we're only dealing with WORDs and DWORDs
            if (_offset % 4 == 0)
                return true;
            return Read(_buffer, 2) == 2;
        }

        public override bool Serialize<TSerializedResource>(ref TSerializedResource serializedResource, ref short length)
        {
            serializedResource = new TSerializedResource();
            return serializedResource.Serialize(this);
        }

        private KeyedResource SerializeKeyedResource(params Type[] expectedTypes)
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
                    return keyedResource.SerializeValue(this) && keyedResource.SerializeChildren(this) ? keyedResource : null;
            }
            return null;
        }

        public override bool Serialize<TSerializedResource>(KeyedResource owner, ref TSerializedResource[] serializedResources, ref short length, params Type[] expectedTypes)
        {
            var ownerLength = _offset - owner.Offset;
            var remainingLength = length - ownerLength;
            var buffer = new byte[remainingLength];
            if (Read(buffer, buffer.Length) != remainingLength)
                return false;

            using (var subSerializer = new ReadResourceSerializer(new MemoryStream(buffer)))
            {
                var children = new List<TSerializedResource>();
                for (;;)
                {
                    // :frowning:
                    var child = (TSerializedResource)(SerializedResource)subSerializer.SerializeKeyedResource(expectedTypes);
                    if (child == null)
                    {
                        serializedResources = children.ToArray();
                        return true;
                    }
                    children.Add(child);
                }
            }
        }
    }
}
