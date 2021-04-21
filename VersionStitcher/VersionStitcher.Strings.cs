﻿// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using Utility;
    using Win32Resources;
    using String = global::VersionStitcher.Win32Resources.String;

    partial class VersionStitcher
    {
        private static readonly Regex VersionFormatEx = new Regex(@"\{[Vv]ersion\.(?<id>\w+)(\:(?<format>\w))?\}");

        /// <summary>
        /// Processes the specified string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="information">The information.</param>
        /// <returns></returns>
        private string ProcessVersionString(string s, object information)
        {
            var r = VersionFormatEx.Replace(s, delegate (Match m)
            {
                var id = m.Groups["id"].Value;
                if (string.Equals(id, "help", StringComparison.OrdinalIgnoreCase))
                {
                    ShowValues(information);
                    throw new OperationCanceledException();
                }
             
                // main process: 1. try to get from information object (which contains build and git information)
                //               2. then try to get from environment with the same name
                //               3. finally defaults to an empty value
                var value = GetPropertyValue(information, id) ?? GetEnvironmentValue(id) ?? "";
             
                // then format
                var format = m.Groups["format"].Value;
                return GetFormattedValue(value, format);
            });
            if (r == s)
                return null;
            return r;
        }

        /// <summary>
        /// Gets the formatted value (if formattable).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        private static string GetFormattedValue(object value, string format)
        {
            var formattableValue = value as IFormattable;
            if (formattableValue != null)
                return formattableValue.ToString(format, CultureInfo.InvariantCulture);
            return value.ToString();
        }

        /// <summary>
        /// Gets the property value from the current information object.
        /// </summary>
        /// <param name="information">The information.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static object GetPropertyValue(object information, string propertyName)
        {
            var property = information.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            var propertyValue = property?.GetValue(information);
            return propertyValue;
        }

        /// <summary>
        /// Gets the environment value.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns></returns>
        private static object GetEnvironmentValue(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName);
        }

        private void ShowValues(object information)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public;
            foreach (var property in information.GetType().GetProperties(bindingFlags).OrderBy(p => p.Name))
                Logging.Write("{0}: {1}", property.Name, property.GetValue(information)?.ToString());
        }

        /// <summary>
        /// Processes the strings.
        /// </summary>
        /// <param name="moduleDef">The module definition.</param>
        /// <param name="versions">The versions.</param>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        private static bool ProcessStrings(ModuleDefMD moduleDef, IList<VS_VERSIONINFO> versions, Func<string, string> process)
        {
            return ProcessStringsInAssemblyTypes(moduleDef, process)
                .OrAny(ProcessStringsInAssemblyAttributes(moduleDef, process))
                .OrAny(ProcessStringsInVersions(versions, process));
        }

        private static bool ProcessStringsInVersions(IEnumerable<VS_VERSIONINFO> versions, Func<string, string> process)
        {
            return versions.Select(versioninfo => ProcessStringsInVersion(versioninfo, process)).AnyOfAll();
        }

        private static bool ProcessStringsInVersion(VS_VERSIONINFO version, Func<string, string> process)
        {
            return version.Children.OfType<StringFileInfo>().Select(info => ProcessStringsInStringFileInfo(info, process)).AnyOfAll();
        }

        private static bool ProcessStringsInStringFileInfo(StringFileInfo stringFileInfo, Func<string, string> process)
        {
            return stringFileInfo.Children.Select(child => ProcessStringsInStringTable(child, process)).AnyOfAll();
        }

        private static bool ProcessStringsInStringTable(StringTable stringTable, Func<string, string> process)
        {
            return stringTable.Children.Select(table => ProcessStringsInString(table, process)).AnyOfAll();
        }

        private static bool ProcessStringsInString(String @string, Func<string, string> process)
        {
            var s = process(@string.Value);
            if (s == null)
                return false;
            @string.Value = s;
            return true;
        }

        /// <summary>
        /// Processes the strings in assembly types.
        /// </summary>
        /// <param name="moduleDef">The module definition.</param>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        private static bool ProcessStringsInAssemblyTypes(ModuleDefMD moduleDef, Func<string, string> process)
        {
            return moduleDef.Types.Select(t => ProcessStringsInType(t, process)).AnyOfAll();
        }

        /// <summary>
        /// Processes the strings in type.
        /// </summary>
        /// <param name="typeDef">The type definition.</param>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        private static bool ProcessStringsInType(TypeDef typeDef, Func<string, string> process)
        {
            return typeDef.Methods.Select(m => ProcessStringsInMethod(m, process)).AnyOfAll()
                .OrAny(typeDef.NestedTypes.Select(t => ProcessStringsInType(t, process)).AnyOfAll());
        }

        /// <summary>
        /// Processes the strings in method.
        /// </summary>
        /// <param name="methodDef">The method definition.</param>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        private static bool ProcessStringsInMethod(MethodDef methodDef, Func<string, string> process)
        {
            if (!methodDef.HasBody)
                return false;

            var processed = false;
            foreach (var i in methodDef.Body.Instructions)
            {
                if (i.OpCode == OpCodes.Ldstr)
                {
                    var processedString = process((string)i.Operand);
                    if (processedString != null)
                    {
                        i.Operand = processedString;
                        processed = true;
                    }
                }
            }
            return processed;
        }

        /// <summary>
        /// Processes the strings in assembly attributes.
        /// </summary>
        /// <param name="moduleDef">The module definition.</param>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        private static bool ProcessStringsInAssemblyAttributes(ModuleDefMD moduleDef, Func<string, string> process)
        {
            var importer = new Importer(moduleDef);
            var stringType = importer.ImportAsTypeSig(typeof(string));

            bool processed = false;
            foreach (var assemblyAttribute in moduleDef.Assembly.CustomAttributes)
            {
                for (int constructorArgumentIndex = 0; constructorArgumentIndex < assemblyAttribute.ConstructorArguments.Count; constructorArgumentIndex++)
                {
                    var constructorArgument = assemblyAttribute.ConstructorArguments[constructorArgumentIndex];
                    if (constructorArgument.Type == stringType)
                    {
                        var utf8String = (UTF8String)constructorArgument.Value;
                        var processedString = process(utf8String.String);
                        if (processedString != null)
                        {
                            processed = true;
                            constructorArgument.Value = new UTF8String(processedString);
                        }
                    }
                }
            }
            return processed;
        }
    }
}
