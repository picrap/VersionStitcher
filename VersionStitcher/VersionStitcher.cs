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
    using StitcherBoy.Weaving.Build;
    using Utility;
    using Win32Resources;
    using Win32Resources.Serialization;

    public partial class VersionStitcher : AssemblyStitcher
    {
        public override string Name => "VersionStitcher";

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
        protected override bool Process(AssemblyStitcherContext context)
        {
            try
            {
                var information = InformationProvider.GetInformation(context.Module.Location);
                var versions = LoadVersions(context.Module).ToArray();
                var buildTime = DateTime.UtcNow; //context.BuildTime;
                var update = ProcessStrings(context.Module, versions, s => ProcessVersionString(s, information))
                    .OrAny(ProcessCustomVersion(context.Module, versions, buildTime));
                if (update)
                    SaveVersions(context.Module, versions);
                return update;
            }
            catch (OperationCanceledException) { }
            return false;
        }

        private static IEnumerable<VS_VERSIONINFO> LoadVersions(ModuleDefMD moduleDef)
        {
            var resourceDirectory = moduleDef.Metadata.PEImage.Win32Resources.Root.FindDirectory(new ResourceName(16));
            if (resourceDirectory is null)
                yield break;
            foreach (var resourceEntry in resourceDirectory.Directories)
            {
                foreach (var versionEntry in resourceEntry.Data)
                {
                    var vi = ResourceSerializer.Deserialize<VS_VERSIONINFO>(versionEntry.CreateReader().AsStream());
                    vi.DirectoryName = resourceEntry.Name;
                    vi.DataName = versionEntry.Name;
                    yield return vi;
                }
            }
        }

        private static void SaveVersions(ModuleDefMD moduleDef, IEnumerable<VS_VERSIONINFO> versions)
        {
            var versionResourceName = new ResourceName(16);
            var resourceDirectory = moduleDef.Metadata.PEImage.Win32Resources.Root.FindDirectory(versionResourceName);
            if (resourceDirectory is null)
            {
                resourceDirectory = new ResourceDirectoryUser(versionResourceName);
                moduleDef.Metadata.PEImage.Win32Resources.Root.Directories.Add(resourceDirectory);
            }
            resourceDirectory.Directories.Clear();
            foreach (var version in versions)
            {
                var resourceEntry = (ResourceDirectoryUser)resourceDirectory.FindDirectory(version.DirectoryName);
                if (resourceEntry is null)
                {
                    resourceEntry = new ResourceDirectoryUser(version.DirectoryName);
                    resourceDirectory.Directories.Add(resourceEntry);
                }

                using var memoryStream = new MemoryStream();
                ResourceSerializer.Serialize(version, memoryStream);
                var versionBytes = memoryStream.ToArray();
                var versionEntry = new ResourceData(version.DataName, ByteArrayDataReaderFactory.Create(versionBytes, version.szKey), 0, (uint)versionBytes.Length);
                resourceEntry.Data.Add(versionEntry);
            }
        }
    }
}
