namespace VersionStitcher
{
    internal class VersionStitcherTask : StitcherTask<VersionStitcher>
    {
        public static int Main(string[] args) => Run(new VersionStitcherTask(), args);
    }
}
