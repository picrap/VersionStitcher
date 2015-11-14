namespace VersionStitcher.Information
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LibGit;
    using LibGit2Sharp;

    public class InformationProvider
    {
        public object GetInformation(string projectPath, string solutionPath)
        {
            return GetGitInformation(projectPath, solutionPath) ?? GetBuildInformation();
        }

        private static void FillBuildInformation(BuildInformation buildInformation)
        {
            buildInformation.BuildTime = DateTime.Now;
            buildInformation.BuildTimeUTC = buildInformation.BuildTime.ToUniversalTime();
        }

        private static BuildInformation GetBuildInformation()
        {
            var buildInformation = new BuildInformation();
            FillBuildInformation(buildInformation);
            return buildInformation;
        }

        private GitInformation GetGitInformation(string projectPath, string solutionPath)
        {
            CheckNativeBinaries();
            using (var repository = FindGitRepository(projectPath, solutionPath))
            {
                if (repository == null)
                    return null;

                var gitInformation = new GitInformation();
                var currentBranch = repository.Head;
                gitInformation.BranchName = currentBranch.Name;
                gitInformation.BranchRemoteName = currentBranch.Remote?.Name;
                var latestCommit = currentBranch.Commits.First();
                gitInformation.CommitID = latestCommit.Id.Sha;
                gitInformation.CommitShortID = latestCommit.Id.Sha.Substring(0, 8);
                gitInformation.CommitMessage = latestCommit.Message.Trim();
                gitInformation.CommitAuthor = latestCommit.Author.ToString();
                var repositoryStatus = repository.RetrieveStatus();
                gitInformation.IsDirty = repositoryStatus.IsDirty;
                gitInformation.IsDirtyLiteral = repositoryStatus.IsDirty ? "dirty" : "";
                FillBuildInformation(gitInformation);
                return gitInformation;
            }
        }

        private void CheckNativeBinaries()
        {
            var thisAssembly = GetType().Assembly;
            var thisDirectory = Path.GetDirectoryName(thisAssembly.Location);
            var nativeBinariesDirectory = Path.Combine(thisDirectory, "NativeBinaries");
            if (!Directory.Exists(nativeBinariesDirectory))
            {
                Directory.CreateDirectory(nativeBinariesDirectory);
                var ns = typeof(NativeBinariesRoot).Namespace;
                foreach (var n in thisAssembly.GetManifestResourceNames())
                {
                    if (n.StartsWith(ns))
                    {
                        var name = n.Substring(ns.Length + 1);
                        var parts = name.Split(new[] { '.' }, name.Count(c => c == '.'));
                        var subDirectory = nativeBinariesDirectory;
                        foreach (var subDirectoryPart in parts.Take(parts.Length - 1))
                        {
                            subDirectory = Path.Combine(subDirectory, subDirectoryPart);
                            if (!Directory.Exists(subDirectory))
                                Directory.CreateDirectory(subDirectory);
                        }
                        var filePath = Path.Combine(subDirectory, parts.Last());
                        using (var fs = File.Create(filePath))
                            thisAssembly.GetManifestResourceStream(n).CopyTo(fs);
                    }
                }
            }
        }

        private static Repository FindGitRepository(string projectPath, string solutionPath)
        {
            foreach (var path in GetPaths(projectPath, solutionPath))
            {
                try
                {
                    return new Repository(path);
                }
                catch (RepositoryNotFoundException) { }
            }
            return null;
        }

        private static IEnumerable<string> GetPaths(string projectPath, string solutionPath)
        {
            var projectDir = GetFullPath(Path.GetDirectoryName(projectPath));
            yield return projectDir;
            if (solutionPath != null)
            {
                var solutionDir = GetFullPath(Path.GetDirectoryName(solutionPath));
                yield return solutionDir;
            }
        }

        private static string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                path = ".";
            return path;
        }
    }
}
