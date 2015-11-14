namespace VersionStitcher
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using StitcherBoy.Weaving;

    internal class VersionStitcher : SingleStitcher
    {
        private Regex SCEx = new Regex(@"\{SC\:(?<id>\w+)\}");

        protected override bool Process(ModuleDefMD moduleDef)
        {
            return ProcessStrings(moduleDef, Process);
        }

        private string Process(string s)
        {

            var r = SCEx.Replace(s, delegate (Match m)
            {
                var id = m.Groups["id"].Value;
                return id;
            });
            if (r == s)
                return null;
            return r;
        }

        private bool ProcessStrings(ModuleDefMD moduleDef, Func<string, string> process)
        {
            var processed = ProcessStringsInAssemblyAttributes(moduleDef, process);
            processed = ProcessStringsInAssemblyTypes(moduleDef, process);
            return processed;
        }

        private bool ProcessStringsInAssemblyTypes(ModuleDefMD moduleDef, Func<string, string> process)
        {
            return moduleDef.Types.Aggregate(false, (p, t) => ProcessStringsInType(t, process) || p);
        }

        private bool ProcessStringsInType(TypeDef typeDef, Func<string, string> process)
        {
            var processed = false;
            foreach (var methodDef in typeDef.Methods)
                processed = ProcessStringsInMethod(methodDef, process) || processed;
            foreach (var nestedTypeDef in typeDef.NestedTypes)
                processed = ProcessStringsInType(nestedTypeDef, process);
            return processed;
        }

        private bool ProcessStringsInMethod(MethodDef methodDef, Func<string, string> process)
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

        private bool ProcessStringsInAssemblyAttributes(ModuleDefMD moduleDef, Func<string, string> process)
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
