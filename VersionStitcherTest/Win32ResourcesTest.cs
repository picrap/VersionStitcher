// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcherTest
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VersionStitcher.Win32Resources;

    [TestClass]
    public class Win32ResourcesTest
    {
        [TestMethod]
        public void DeserializeTest()
        {
            using (var s = new ReadResourceSerializer(GetType().Assembly.GetManifestResourceStream(GetType(), "version")))
            {
                var i = new VS_VERSIONINFO();
                i.Serialize(s);
                var sfi = i.Children.OfType<StringFileInfo>().SingleOrDefault();
                Assert.IsNotNull(sfi);
                var st = sfi.Children[0];
                Assert.IsTrue(st.Children.Length > 0);
            }
        }
    }
}