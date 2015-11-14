namespace VersionStitcher.Information
{
    using LibGit2Sharp;

    public class GitInformation : BuildInformation
    {
        public string BranchName { get; set; }
        public string BranchRemoteName { get; set; }
        public string CommitID { get; set; }
        public string CommitShortID { get; set; }
        public string CommitMessage { get; set; }
        public string CommitAuthor { get; set; }
        public bool IsDirty { get; set; }
        public string IsDirtyLiteral { get; set; }
    }
}