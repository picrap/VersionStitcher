// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32
{
    using System.Runtime.InteropServices;
    using DWORD = System.UInt32;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VS_FIXEDFILEINFO
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
    };
}