// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using dnlib.DotNet;
    using dnlib.IO;
    using dnlib.W32Resources;
    using Information;
    using StitcherBoy.Weaving;
    using Utility;
    using Win32Resources;
    using Win32Resources.Serialization;

    public partial class VersionStitcher : SingleStitcher
    {
        /// <summary>
        /// Gets the information provider.
        /// </summary>
        /// <value>
        /// The information provider.
        /// </value>
        protected InformationProvider InformationProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionStitcher"/> class.
        /// </summary>
        public VersionStitcher()
        {
            InformationProvider = new InformationProvider();
        }

        /// <summary>
        /// Processes the specified module.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected override bool Process(StitcherContext context)
        {
            try
            {
                var information = InformationProvider.GetInformation(context.ProjectPath, context.SolutionPath);
                var versions = LoadVersions(context.Module).ToArray();
                var update = ProcessStrings(context.Module, versions, s => ProcessVersionString(s, information))
                    .OrAny(ProcessCustomVersion(context.Module, versions, context.BuildTime));
                if (update)
                    SaveVersions(context.Module, versions);
                return update;
            }
            catch (OperationCanceledException) { }
            return false;
        }

        private static IEnumerable<VS_VERSIONINFO> LoadVersions(ModuleDefMD moduleDef)
        {
            var resourceDirectory = moduleDef.MetaData.PEImage.Win32Resources.Root.FindDirectory(new ResourceName(16));
            if (resourceDirectory == null)
                yield break;
            foreach (var resourceEntry in resourceDirectory.Directories)
            {
                foreach (var versionEntry in resourceEntry.Data)
                {
                    var vi = ResourceSerializer.Deserialize<VS_VERSIONINFO>(versionEntry.ToDataStream());
                    vi.DirectoryName = resourceEntry.Name;
                    vi.DataName = versionEntry.Name;
                    yield return vi;
                }
            }
        }

        private static void SaveVersions(ModuleDefMD moduleDef, IEnumerable<VS_VERSIONINFO> versions)
        {
            var versionResourceName = new ResourceName(16);
            var resourceDirectory = moduleDef.MetaData.PEImage.Win32Resources.Root.FindDirectory(versionResourceName);
            if (resourceDirectory == null)
            {
                resourceDirectory = new ResourceDirectoryUser(versionResourceName);
                moduleDef.MetaData.PEImage.Win32Resources.Root.Directories.Add(resourceDirectory);
            }
            resourceDirectory.Directories.Clear();
            foreach (var version in versions)
            {
                var resourceEntry = (ResourceDirectoryUser)resourceDirectory.FindDirectory(version.DirectoryName);
                if (resourceEntry == null)
                {
                    resourceEntry = new ResourceDirectoryUser(version.DirectoryName);
                    resourceDirectory.Directories.Add(resourceEntry);
                }

                using (var memoryStream = new MemoryStream())
                {
                    ResourceSerializer.Serialize(version, memoryStream);
                    var versionEntry = new ResourceData(version.DataName, MemoryImageStream.Create(memoryStream.ToArray()));
                    resourceEntry.Data.Add(versionEntry);
                }
            }
        }
    }
}
