// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;
    using Utility;
    using Win32Resources;
    using String = global::VersionStitcher.Win32Resources.String;

    partial class VersionStitcher
    {
        private bool ProcessCustomVersion(ModuleDefMD moduleDef, IList<VS_VERSIONINFO> versions)
        {
            const string assemblyTypeName = "Assembly";
            var assemblyTypeDef = moduleDef.Types.SingleOrDefault(t => t.FullName == assemblyTypeName);
            if (assemblyTypeDef == null)
                return false;

            bool updated = false;
            using (var customModule = ModuleUtility.CreateModule())
            {
                assemblyTypeDef.Copy(customModule);
                var customAssembly = customModule.Load();
                var assemblyType = customAssembly.GetType(assemblyTypeName);
                // first of all, try to get at least a version
                var version = GetVersion(assemblyType, "GetVersion");
                if (version == null)
                    return false;
                var literalVersion = version.ToString();

                // now we can try to get file and product versions
                // ...files...
                var literalFileVersion = GetLiteralVersion(assemblyType, "GetFileVersion") ?? literalVersion;
                Version fileVersion;
                Version.TryParse(literalFileVersion, out fileVersion);
                // ...product...
                var literalProductVersion = GetLiteralVersion(assemblyType, "GetProductVersion") ?? literalFileVersion;
                Version productVersion;
                Version.TryParse(literalProductVersion, out productVersion);

                // Assembly version
                if (moduleDef.Assembly.Version != version)
                {
                    moduleDef.Assembly.Version = version;
                    updated = true;
                }
                updated = SetVersionString(versions, "Assembly Version", literalVersion) || updated;

                // File version
                updated = SetAssemblyAttribute(moduleDef, typeof(AssemblyFileVersionAttribute), literalFileVersion) || updated;
                updated = SetVersionString(versions, "FileVersion", literalFileVersion) || updated;
                updated = versions.Select(v => SetFileVersionDWORDs(v, fileVersion)).AnyOfAll() || updated;

                // Product version
                updated = SetAssemblyAttribute(moduleDef, typeof(AssemblyInformationalVersionAttribute), literalProductVersion) || updated;
                updated = SetVersionString(versions, "ProductVersion", literalProductVersion) || updated;
                updated = versions.Select(v => SetProductVersionDWORDs(v, productVersion)).AnyOfAll() || updated;
            }
            return updated;
        }

        private static bool SetFileVersionDWORDs(VS_VERSIONINFO versionInfo, Version version)
        {
            var versionMS = version.GetVersionMS();
            var versionLS = version.GetVersionLS();
            if (versionInfo.Value.dwFileVersionMS == versionMS && versionInfo.Value.dwFileVersionLS == versionLS)
                return false;
            versionInfo.Value.dwFileVersionMS = versionMS;
            versionInfo.Value.dwFileVersionLS = versionLS;
            return true;
        }

        private static bool SetProductVersionDWORDs(VS_VERSIONINFO versionInfo, Version version)
        {
            var versionMS = version.GetVersionMS();
            var versionLS = version.GetVersionLS();
            if (versionInfo.Value.dwProductVersionMS == versionMS && versionInfo.Value.dwProductVersionLS == versionLS)
                return false;
            versionInfo.Value.dwProductVersionMS = versionMS;
            versionInfo.Value.dwProductVersionLS = versionLS;
            return true;
        }

        private static bool SetVersionString(IEnumerable<VS_VERSIONINFO> versionInfos, string versionName, string value)
        {
            return versionInfos.Select(v => SetVersionString(v, versionName, value)).AnyOfAll();
        }

        private static bool SetVersionString(VS_VERSIONINFO versionInfo, string versionName, string value)
        {
            return versionInfo.Children.OfType<StringFileInfo>().Select(f => SetVersionString(f, versionName, value)).AnyOfAll();
        }

        private static bool SetVersionString(StringFileInfo stringFileInfo, string versionName, string value)
        {
            return stringFileInfo.Children.Select(t => SetVersionString(t, versionName, value)).AnyOfAll();
        }

        private static bool SetVersionString(StringTable stringTable, string versionName, string value)
        {
            var matchingString = stringTable.Children.SingleOrDefault(s => s.szKey == versionName);
            if (matchingString != null)
            {
                if (matchingString.Value == value)
                    return false;
                matchingString.wType = 1;
                matchingString.Value = value;
                return true;
            }

            var children = stringTable.Children.ToList();
            var newString = new String { wType = 1, szKey = versionName, Value = value };
            children.Add(newString);
            stringTable.Children = children.ToArray();
            return true;
        }

        private static bool SetAssemblyAttribute(ModuleDef moduleDef, Type attributeType, string value)
        {
            var attributeAssemblyFileName = attributeType.Module.FullyQualifiedName;
            using (var attributeModule = ModuleDefMD.Load(attributeAssemblyFileName))
            {
                var attributeTypeRef = moduleDef.Import(attributeType);
                var attributeTypeDef = attributeModule.GetTypes().Single(t => t.FullName == attributeTypeRef.FullName);
                var existingAttribute = moduleDef.Assembly.CustomAttributes.SingleOrDefault(t => t.TypeFullName == attributeTypeDef.FullName);
                if (existingAttribute != null)
                {
                    // if it exists and is already initialized with the same value, then no need to change it
                    if (((UTF8String)existingAttribute.ConstructorArguments[0].Value).String == value)
                        return false;
                    moduleDef.Assembly.CustomAttributes.Remove(existingAttribute);
                }
                var ctor = moduleDef.Import(attributeTypeDef.FindConstructors().Single());
                var stringTypeSig = moduleDef.Import(typeof(string)).ToTypeSig();
                moduleDef.Assembly.CustomAttributes.Add(new CustomAttribute(ctor, new[] { new CAArgument(stringTypeSig, new UTF8String(value)) }));
            }
            return true;
        }

        private Version GetVersion(Type type, string methodName)
        {
            var getVersionMethod = type.GetMethod(methodName);
            if (getVersionMethod == null)
                return null;
            return InvokeGetVersion(getVersionMethod);
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

            Logging.WriteError($"Method Assembly.{methodInfo.Name}() must return a System.Version or compatible System.String");
            throw new OperationCanceledException();
        }

        private string GetLiteralVersion(Type type, string methodName)
        {
            var getVersionMethod = type.GetMethod(methodName);
            if (getVersionMethod == null)
                return null;
            return InvokeGetLiteralVersion(getVersionMethod);
        }

        private string InvokeGetLiteralVersion(MethodInfo methodInfo)
        {
            if (!methodInfo.IsStatic)
            {
                Logging.WriteError($"Method Assembly.{methodInfo.Name}() must be static");
                throw new OperationCanceledException();
            }

            var parameters = new object[0];
            if (methodInfo.ReturnType == typeof(string))
                return (string)methodInfo.Invoke(null, parameters);
            if (methodInfo.ReturnType == typeof(Version))
                return ((Version)methodInfo.Invoke(null, parameters)).ToString();

            Logging.WriteError($"Method Assembly.{methodInfo.Name}() must return a System.Version or System.String");
            throw new OperationCanceledException();
        }
    }
}
