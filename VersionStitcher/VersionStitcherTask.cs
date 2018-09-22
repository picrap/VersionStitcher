// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

// ReSharper disable once CheckNamespace

using StitcherBoy;

public class VersionStitcherTask : StitcherTask<VersionStitcher.VersionStitcher>
{
    public static int Main(string[] args)
    {
        BlobberHelper.Setup();
        return Run(new VersionStitcherTask(), args);
    }
}
