// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher
{
    using System;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;
    using dnlib.W32Resources;
    using Utility;
    using Win32;
    
    partial class VersionStitcher
    {
        private bool ProcessCustomVersion(ModuleDefMD moduleDef)
        {
            const string assemblyTypeName = "Assembly";
            var assemblyTypeDef = moduleDef.Types.SingleOrDefault(t => t.FullName == assemblyTypeName);
            if (assemblyTypeDef == null)
                return false;

            var resourceDirectory = moduleDef.MetaData.PEImage.Win32Resources.Root.FindDirectory(new ResourceName(16));
            try
            {
                X(moduleDef, resourceDirectory);
            }
            catch (Exception e)
            {
            }
            using (var customModule = ModuleUtility.CreateModule())
            {
                assemblyTypeDef.Copy(customModule);
                var customAssembly = customModule.Load();
                var assemblyType = customAssembly.GetType(assemblyTypeName);
                var getVersionMethod = assemblyType.GetMethod("GetVersion");
                if (getVersionMethod != null)
                {
                    var version = InvokeGetVersion(getVersionMethod);
                    moduleDef.Assembly.Version = version;
                }
                var getFileVersionMethod = assemblyType.GetMethod("GetFileVersion");
                if (getFileVersionMethod != null)
                {
                    var version = InvokeGetVersion(getFileVersionMethod);
                    SetAssemblyAttribute(moduleDef, typeof(AssemblyFileVersionAttribute), version.ToString());
                }
            }
            return true;
        }

        private void X(ModuleDef moduleDef, ResourceDirectory directory)
        {
            if (directory == null)
                return;

            foreach (var resourceDirectory in directory.Directories)
                X(moduleDef, resourceDirectory);

            foreach (var x in directory.Data)
            {
                try
                {
                    using (var m = x.ToDataStream())
                    {
                        var vi = m.Read<VS_VERSIONINFO>();
                        var vfi = m.Read<VarFileInfo>();
                        var v1 = m.Read<Var>();
                    }
                    //using (var r1 = new System.Resources.ResourceReader(m))
                    //{
                    //    foreach (var r1e in r1)
                    //    {

                    //    }
                    //}
                    //                    using (var m = x.ToDataStream())
                    //                    {
                    //var h1=                        m.ReadByte();
                    //var h2=                        m.ReadByte();
                    //var h3=                        m.ReadByte();
                    //var h4=                        m.ReadByte();
                    //                    }

                }
                catch (Exception e)
                {

                }
            }
        }

        private static void SetAssemblyAttribute(ModuleDef moduleDef, Type attributeType, string value)
        {
            var attributeAssemblyFileName = attributeType.Module.FullyQualifiedName;
            using (var attributeModule = ModuleDefMD.Load(attributeAssemblyFileName))
            {
                var attributeTypeRef = moduleDef.Import(attributeType);
                var attributeTypeDef = attributeModule.GetTypes().Single(t => t.FullName == attributeTypeRef.FullName);
                var existingAttribute = moduleDef.Assembly.CustomAttributes.SingleOrDefault(t => t.TypeFullName == attributeTypeDef.FullName);
                if (existingAttribute != null)
                    moduleDef.Assembly.CustomAttributes.Remove(existingAttribute);
                var ctor = moduleDef.Import(attributeTypeDef.FindConstructors().Single());
                var stringTypeSig = moduleDef.Import(typeof(string)).ToTypeSig();
                moduleDef.Assembly.CustomAttributes.Add(new CustomAttribute(ctor, new[] { new CAArgument(stringTypeSig, new UTF8String(value)) }));
            }
        }

        private Version InvokeGetVersion(MethodInfo methodInfo)
        {
            if (!methodInfo.IsStatic)
            {
                Logging.WriteError($"Method Assembly.{methodInfo.Name}() must be static");
                throw new OperationCanceledException();
            }

            var parameters = new object[0];
            if (methodInfo.ReturnType == typeof(string))
                return new Version((string)methodInfo.Invoke(null, parameters));
            if (methodInfo.ReturnType == typeof(Version))
                return (Version)methodInfo.Invoke(null, parameters);

            Logging.WriteError($"Method Assembly.{methodInfo.Name}() must return a System.Version");
            throw new OperationCanceledException();
        }
    }
}
