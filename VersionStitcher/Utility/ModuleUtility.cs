// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT

namespace VersionStitcher.Utility
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;

    internal static class ModuleUtility
    {
        /// <summary>
        /// Creates a module.
        /// </summary>
        /// <returns></returns>
        public static ModuleDef CreateModule()
        {
            ModuleDef module = new ModuleDefUser(Guid.NewGuid().ToString());
            module.Kind = ModuleKind.Dll;

            // Add the module to an assembly
            AssemblyDef assembly = new AssemblyDefUser(Guid.NewGuid().ToString(), new Version(1, 0), null, CultureInfo.InvariantCulture.DisplayName);
            assembly.Modules.Add(module);

            return module;
        }

        public static byte[] GetModuleBytes(this ModuleDef module)
        {
            using (var memoryStream = new MemoryStream())
            {
                module.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static void Copy(this TypeDef sourceTypeDef, ModuleDef moduleDef)
        {
            var targetType = new TypeDefUser(sourceTypeDef.Namespace, sourceTypeDef.Name, sourceTypeDef.BaseType);
            targetType.Attributes = sourceTypeDef.Attributes;
            moduleDef.Types.Add(targetType);
            Copy(sourceTypeDef, targetType);
        }

        public static void Copy(this TypeDef sourceTypeDef, TypeDef targetTypeDef)
        {
            foreach (var methodDef in sourceTypeDef.Methods)
                methodDef.CopyTo(targetTypeDef);
            foreach (var fieldDef in sourceTypeDef.Fields)
                targetTypeDef.Fields.Add(fieldDef);
            foreach (var property in sourceTypeDef.Properties)
                targetTypeDef.Properties.Add(property);
        }

        public static ParamDef Clone(this ParamDef paramDef)
        {
            var newParamDef = new ParamDefUser(paramDef.Name);
            newParamDef.Attributes = paramDef.Attributes;
            foreach (var attribute in paramDef.CustomAttributes)
                newParamDef.CustomAttributes.Add(attribute.Clone());
            return newParamDef;
        }

        public static CustomAttribute Clone(this CustomAttribute customAttribute)
        {
            var newAttribute = new CustomAttribute(customAttribute.Constructor, customAttribute.ConstructorArguments, customAttribute.NamedArguments);
            return newAttribute;
        }

        public static MethodDef CopyTo(this MethodDef methodDef, TypeDef targetTypeDef)
        {
            var targetMethod = new MethodDefUser(methodDef.Name, methodDef.MethodSig, methodDef.ImplAttributes, methodDef.Attributes);
            foreach (var parameter in methodDef.ParamDefs)
                targetMethod.ParamDefs.Add(parameter.Clone());
            if (methodDef.HasBody)
            {
                targetMethod.Body = new CilBody();
                targetMethod.Body.InitLocals = methodDef.Body.InitLocals;
                foreach (var instruction in methodDef.Body.Instructions)
                    targetMethod.Body.Instructions.Add(instruction);
                if (methodDef.Body.HasExceptionHandlers)
                    foreach (var exceptionHandler in methodDef.Body.ExceptionHandlers)
                        methodDef.Body.ExceptionHandlers.Add(exceptionHandler);
                if(methodDef.Body.HasVariables)
                foreach (var variable in methodDef.Body.Variables)
                    targetMethod.Body.Variables.Add(variable);
                targetMethod.Body.Scope = methodDef.Body.Scope;
            }
            targetTypeDef.Methods.Add(targetMethod);
            return targetMethod;
        }

        /// <summary>
        /// Loads the specified module definition.
        /// </summary>
        /// <param name="moduleDef">The module definition.</param>
        /// <returns></returns>
        public static Assembly Load(this ModuleDef moduleDef)
        {
            var assemblyBytes = moduleDef.GetModuleBytes();
            return Assembly.Load(assemblyBytes);
        }
    }
}
