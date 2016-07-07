// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

using System;

public class VersionStitcherTask : StitcherTask<VersionStitcher.VersionStitcher>
{
    public static int Main(string[] args)
    {
        try
        {
            return Run(new VersionStitcherTask(), args);
        }
        catch
        {
        }
        return -1;
    }
}
