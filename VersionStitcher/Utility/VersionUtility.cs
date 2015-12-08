// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Utility
{
    using System;

    public static class VersionUtility
    {
        public static uint GetVersionMS(this Version version)
        {
            return (uint)(version.Minor | (version.Major << 16));
        }
        public static uint GetVersionLS(this Version version)
        {
            var build = version.Build;
            if (build == -1)
                build = 0;
            var revision = version.Revision;
            if (revision == -1)
                revision = 0;
            return (uint)(revision | (build << 16));
        }
    }
}