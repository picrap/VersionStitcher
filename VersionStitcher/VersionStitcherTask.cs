// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT
public class VersionStitcherTask : StitcherTask<VersionStitcher.VersionStitcher>
{
    public static int Main(string[] args) => Run(new VersionStitcherTask(), args);
}
