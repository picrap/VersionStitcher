// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

// ReSharper disable once CheckNamespace
public class VersionStitcherTask : StitcherTask<VersionStitcher.VersionStitcher>
{
    public static int Main(string[] args)
    {
        return Run(new VersionStitcherTask(), args);
    }
}
