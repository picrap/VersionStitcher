// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    internal abstract class VariableStruct
    {
        public abstract bool Read(Stream stream);
    }

    internal abstract class VariableStruct<TMarshaledStruct> : VariableStruct
        where TMarshaledStruct : struct
    {
        public TMarshaledStruct Struct;

        protected int ReadStruct(Stream stream)
        {
            var buffer = new byte[Marshal.SizeOf(typeof(TMarshaledStruct))];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead != buffer.Length)
                return -1;
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Struct = (TMarshaledStruct)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TMarshaledStruct));
                return bytesRead;
            }
            finally
            {
                handle.Free();
            }
        }
    }

    internal abstract class RawValueVariableStruct<TMarshaledStruct> : VariableStruct<TMarshaledStruct>
        where TMarshaledStruct : struct
    {
        protected abstract int ValueLength { get; set; }

        public byte[] Value { get; set; }

        public override bool Read(Stream stream)
        {
            var bytesRead = ReadStruct(stream);
            if (bytesRead == -1)
                return false;

            Value = new byte[ValueLength];
            return stream.Read(Value, 0, Value.Length) == Value.Length;
        }
    }

    internal abstract class ValueVariableStruct<TMarshaledStruct, TValueMarshaledStruct> : VariableStruct<TMarshaledStruct>
        where TMarshaledStruct : struct
        where TValueMarshaledStruct : VariableStruct, new()
    {
        protected abstract int TotalSize { get; set; }

        public TValueMarshaledStruct[] Value { get; set; }

        public override bool Read(Stream stream)
        {
            var bytesRead = ReadStruct(stream);
            if (bytesRead == -1)
                return false;
            var remainingSize = TotalSize - bytesRead;
            var buffer = new byte[remainingSize];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                return false;

            using (var memoryStream = new MemoryStream(buffer))
            {
                var children = new List<TValueMarshaledStruct>();
                for (;;)
                {
                    var child = new TValueMarshaledStruct();
                    if (!child.Read(memoryStream))
                        break;
                    children.Add(child);
                }
                Value = children.ToArray();
            }
            return true;
        }
    }
}
