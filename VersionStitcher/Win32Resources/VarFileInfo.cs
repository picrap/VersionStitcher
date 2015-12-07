
namespace VersionStitcher.Win32Resources
{
    using System.Runtime.InteropServices;
    using WORD = System.Int16;

    public class VarFileInfo : ValidatedKeyedResource
    {
        //Var Children;
        public Var[] Children;

        public override bool Validate() => szKey == "VarFileInfo";

        public override bool SerializeBody(ResourceSerializer serializer)
        {
            return serializer.Serialize(this, ref Children, ref wLength, typeof(Var));
        }
    }
}
