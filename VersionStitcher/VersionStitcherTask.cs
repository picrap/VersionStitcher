// ReSharper disable once CheckNamespace
public class VersionStitcherTask : StitcherTask<VersionStitcher.VersionStitcher>
{
    public static int Main(string[] args) => Run(new VersionStitcherTask(), args);
}
