// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT
namespace VersionStitcher.Information
{
    using System;

    public class BuildInformation
    {
        public DateTime BuildTime { get; set; }
        public DateTime BuildTimeUTC { get; set; }
    }
}
