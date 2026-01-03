// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT
namespace VersionStitcher.LibGit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LibGit2Sharp;

    /// <summary>
    /// Wrapper for GIT <see cref="Repository"/>
    /// </summary>
    public class GitRepository : IDisposable
    {
        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>
        /// The repository.
        /// </value>
        public Repository Repository { get; private set; }

        private readonly IList<Action> _dispose = new List<Action>();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Repository?.Dispose();
            foreach (var d in _dispose.Reverse())
                d();
        }

        public static GitRepository TryLoad(params string[] paths)
        {
            var gitRepository = new GitRepository();
            foreach (var path in paths)
            {
                try
                {
                    gitRepository.Repository = new Repository(path);
                    return gitRepository;
                }
                catch (RepositoryNotFoundException)
                {
                }
                catch
                {
                    gitRepository.Dispose();
                    throw;
                }
            }
            gitRepository.Dispose();
            return null;
        }

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        private void CreateDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                return;
            Directory.CreateDirectory(directoryPath);
            _dispose.Add(delegate
            {
                try
                {
                    Directory.Delete(directoryPath);
                }
                catch (IOException) { }
            });
        }
    }
}
