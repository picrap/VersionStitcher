// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using Utility;

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
                var format = m.Groups["format"].Value;
                var property = information.GetType().GetProperty(id, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                var propertyValue = property?.GetValue(information);
                if (propertyValue == null)
                    return "";
                var formattableValue = propertyValue as IFormattable;
                if (formattableValue != null)
                    return formattableValue.ToString(format, CultureInfo.InvariantCulture);
                return propertyValue.ToString();
            });
            if (r == s)
                return null;
            return r;
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
        /// <param name="process">The process.</param>
        /// <returns></returns>
        private static bool ProcessStrings(ModuleDefMD moduleDef, Func<string, string> process)
        {
            return ProcessStringsInAssemblyTypes(moduleDef, process).OrAny(ProcessStringsInAssemblyAttributes(moduleDef, process));
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
