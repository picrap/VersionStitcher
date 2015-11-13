namespace VersionStitcher
{
    using dnlib.DotNet;
    using StitcherBoy.Weaving;

    internal class VersionStitcher: SingleStitcher
    {
        protected override bool Process(ModuleDefMD moduleDef)
        {
            return true;
        }
    }
}