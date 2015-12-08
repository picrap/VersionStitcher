// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Utility
{
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class StreamUtility
    {
        public static TMarshaledStruct Read<TMarshaledStruct>(this Stream stream)
        {
            var buffer = new byte[Marshal.SizeOf(typeof(TMarshaledStruct))];
            stream.Read(buffer, 0, buffer.Length);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return (TMarshaledStruct)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TMarshaledStruct));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
