// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcherTest
{
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VersionStitcher.Win32Resources;
    using VersionStitcher.Win32Resources.Serialization;

    [TestClass]
    public class Win32ResourcesTest
    {
        [TestMethod]
        public void DeserializeTest()
        {
            using (var s = GetType().Assembly.GetManifestResourceStream(GetType(), "version"))
            {
                var i = ResourceSerializer.Deserialize<VS_VERSIONINFO>(s);
                var sfi = i.Children.OfType<StringFileInfo>().SingleOrDefault();
                Assert.IsNotNull(sfi);
                var st = sfi.Children[0];
                Assert.IsTrue(st.Children.Length > 0);
            }
        }

        [TestMethod]
        public void SerializeTest()
        {
            using (var s = GetType().Assembly.GetManifestResourceStream(GetType(), "version"))
            {
                var m = new MemoryStream();
                s.CopyTo(m);
                m.Seek(0, SeekOrigin.Begin);

                var i = ResourceSerializer.Deserialize<VS_VERSIONINFO>(m);

                var m2 = new MemoryStream();
                ResourceSerializer.Serialize(i, m2);

                var b = m.ToArray();
                var b2 = m2.ToArray();
                Assert.IsTrue(b.SequenceEqual(b2));
            }
        }
    }
}