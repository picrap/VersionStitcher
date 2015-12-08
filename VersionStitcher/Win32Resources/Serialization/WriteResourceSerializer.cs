namespace VersionStitcher.Win32Resources.Serialization
{
    using System;
    using System.IO;

    public class WriteResourceSerializer : ResourceSerializer, IDisposable
    {
        private readonly Stream _stream;
        private int _totalWritten;
        private int _unpaddedOffset;

        public override bool IsWriting => true;

        public override int UnpaddedOffset => _unpaddedOffset;

        public WriteResourceSerializer(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        private bool Write(byte[] bytes)
        {
            _totalWritten += bytes.Length;
            _unpaddedOffset = _totalWritten;
            _stream.Write(bytes, 0, bytes.Length);
            return true;
        }

        public override bool SerializeWORD(ref ushort word) => Write(BitConverter.GetBytes(word));

        public override bool SerializeWORD(ref short word) => Write(BitConverter.GetBytes(word));

        public override bool SerializeDWORD(ref uint dword) => Write(BitConverter.GetBytes(dword));

        public override bool SerializeWCHAR(ref string wchar)
        {
            foreach (var c in wchar)
            {
                var s = (short)c;
                SerializeWORD(ref s);
            }
            short z = 0;
            return SerializeWORD(ref z);
        }

        public override bool SerializeWCHAR(ref string wchar, ref short valueLength)
        {
            valueLength = (short)(wchar.Length + 1);
            return SerializeWCHAR(ref wchar);
        }

        public override bool SerializeValue(ref byte[] value, ref short valueLength)
        {
            valueLength = (short)value.Length;
            return Write(value);
        }

        public override bool PadDWORD()
        {
            // this currently assumes we're only dealing with WORDs and DWORDs
            if (_totalWritten % 4 == 0)
                return true;
            _totalWritten += 2;
            _stream.Write(new byte[2], 0, 2);
            return true;
        }

        public override bool Serialize<TSerializedResource>(ref TSerializedResource serializedResource)
        {
            // the trick is: the resource is serialized twice:
            // - first time to adjust headers
            // - second time to final resource
            using (var m = new MemoryStream())
                serializedResource.Serialize(new WriteResourceSerializer(m));
            return serializedResource.Serialize(this);
        }

        public override bool Serialize<TSerializedResource>(KeyedResource owner, ref TSerializedResource[] serializedResources, ref short length, params Type[] expectedTypes)
        {
            var ownerLength = _totalWritten - owner.Offset;
            using (var memoryStream = new MemoryStream())
            using (var writer = new WriteResourceSerializer(memoryStream))
            {
                foreach (var serializedResource in serializedResources)
                {
                    var localSerializedResource = serializedResource;
                    writer.Serialize(ref localSerializedResource);
                }
                var bytes = memoryStream.ToArray();
                length = (short)(ownerLength + bytes.Length);
                return Write(bytes);
            }
        }

        public override bool SerializeLength(Func<ResourceSerializer, bool> subSerializer, ref short length)
        {
            var offset = _totalWritten;
            var ok = subSerializer(this);
            if (ok)
                length = (short)(_unpaddedOffset - offset);
            return ok;
        }
    }
}