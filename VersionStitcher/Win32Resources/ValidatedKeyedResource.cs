// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Win32Resources
{
    public abstract class ValidatedKeyedResource : KeyedResource
    {
        public abstract bool Validate();
    }
}
