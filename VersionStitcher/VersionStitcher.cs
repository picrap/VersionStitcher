// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher
{
    using System;
    using dnlib.DotNet;
    using Information;
    using StitcherBoy.Project;
    using StitcherBoy.Weaving;
    using Utility;

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
        /// <param name="moduleDef">The module definition.</param>
        /// <param name="assemblyPath"></param>
        /// <param name="project"></param>
        /// <param name="projectPath"></param>
        /// <param name="solutionPath"></param>
        /// <returns></returns>
        protected override bool Process(ModuleDefMD moduleDef, string assemblyPath, ProjectDefinition project, string projectPath, string solutionPath)
        {
            try
            {
                var information = InformationProvider.GetInformation(projectPath, solutionPath);
                return ProcessStrings(moduleDef, s => ProcessVersionString(s, information))
                    .OrAny(ProcessCustomVersion(moduleDef));
            }
            catch (OperationCanceledException) { }
            return false;
        }
    }
}
