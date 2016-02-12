// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT
namespace VersionStitcher.Information
{
    using System;

    public class GitInformation : BuildInformation
    {
        public string BranchName { get; set; }
        public string BranchRemoteName { get; set; }
        public string CommitID { get; set; }
        public string CommitShortID { get; set; }
        public string CommitMessage { get; set; }
        public string CommitAuthor { get; set; }
        public DateTime CommitTime { get; set; }
        public string CommitTimeIso { get; set; }
        public bool IsDirty { get; set; }
        public string IsDirtyLiteral { get; set; }
    }
}