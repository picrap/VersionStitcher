namespace VersionStitcher.Win32Resources.Serialization
{
    using System;
    using System.IO;

    public class WriteResourceSerializer : ResourceSerializer, IDisposable
    {
        private readonly Stream _stream;
        private int _totalWritten;

        public override bool IsWriting => true;

        public override int Offset => _totalWritten;

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
            return Write(new byte[2]);
        }

        public override bool Serialize<TSerializedResource>(ref TSerializedResource serializedResource, ref short length)
        {
            // the trick is: the resource is serialized twice:
            // - first time to adjust headers
            // - second time to final resource
            using (var m = new MemoryStream())
            {
                serializedResource.Serialize(new WriteResourceSerializer(m));
                length = (short)m.Length;
            }
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
                    short l = 0;
                    // TODO: something with this unused length
                    writer.Serialize(ref localSerializedResource, ref l);
                }
                var bytes = memoryStream.ToArray();
                length = (short)(ownerLength + bytes.Length);
                return Write(bytes);
            }
        }
    }
}