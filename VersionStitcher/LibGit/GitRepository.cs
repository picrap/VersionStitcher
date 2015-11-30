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
        /// Initializes a new instance of the <see cref="GitRepository" /> class.
        /// </summary>
        public GitRepository()
        {
            DeployNativeBinaries();
        }

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
            _dispose.Add(() => Directory.Delete(directoryPath));
        }

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="baseDirectory">The base directory.</param>
        /// <param name="subParts">The sub parts.</param>
        /// <returns></returns>
        private string CreateDirectory(string baseDirectory, IEnumerable<string> subParts)
        {
            var subDirectory = baseDirectory;
            foreach (var subPart in subParts)
            {
                subDirectory = Path.Combine(subDirectory, subPart);
                CreateDirectory(subDirectory);
            }
            return subDirectory;
        }

        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="resourceStream">The resource stream.</param>
        private void CreateFile(string filePath, Stream resourceStream)
        {
            if (File.Exists(filePath))
                return;
            using (var fileStream = File.Create(filePath))
                resourceStream.CopyTo(fileStream);
            _dispose.Add(() => File.Delete(filePath));
        }

        /// <summary>
        /// Deploys the native binaries... If necessary
        /// </summary>
        private void DeployNativeBinaries()
        {
            // create temp folder
            var nativeBinariesDirectory = Path.Combine(Path.GetTempPath(), $"GitNativeBinaries-{Guid.NewGuid()}");
            CreateDirectory(nativeBinariesDirectory);

            // find resource
            var architecture = Environment.Is64BitProcess ? "amd64" : "x86";
            const string dllName = "git2-e0902fb.dll";
            var thisAssembly = GetType().Assembly;
            var resourceStream = thisAssembly.GetManifestResourceStream(GetType(), architecture + "." + dllName);

            // open resource and create file
            var filePath = Path.Combine(nativeBinariesDirectory, dllName);
            CreateFile(filePath, resourceStream);

            // update env path
            const string pathVariable = "path";
            var environmentPath = Environment.GetEnvironmentVariable(pathVariable);
            Environment.SetEnvironmentVariable(pathVariable, nativeBinariesDirectory + ";" + environmentPath);
            _dispose.Add(() => Environment.SetEnvironmentVariable(pathVariable, environmentPath));
        }
    }
}
