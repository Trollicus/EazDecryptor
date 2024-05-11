using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using HarmonyLib;


void PatchAntiInvoke()
{
    Harmony harmony = new("Trollicus.Eazfuscator");
    harmony.PatchAll(Assembly.GetExecutingAssembly());
}

string file = args[0];

if (!File.Exists(file))
{
    throw new FileNotFoundException($"Couldn't find file: {file}");
}

var module = ModuleDefinition.FromFile(file);
var assembly = Assembly.LoadFile(file);

PatchAntiInvoke();

Console.WriteLine(assembly.FullName);

foreach (var type in module.TopLevelTypes)
{
    foreach (MethodDefinition methods in type.Methods)
    {
        if (!methods.HasMethodBody || methods.CilMethodBody == null) continue;

        var instructions = methods.CilMethodBody.Instructions;


        for (var i = 0; i < instructions.Count; i++)
        {
            if (instructions[i].OpCode == CilOpCodes.Ldc_I4 &&
                instructions[i + 1].OpCode == CilOpCodes.Call &&
                instructions[i + 1].Operand is IMethodDescriptor
                {
                    Signature:
                    {
                        ReturnType.FullName: "System.String", ParameterTypes: [{ FullName: "System.Int32" }]
                    }
                } methodDescriptor)
            {
                Console.WriteLine(
                    $"Found a call with Int32 parameter at {methods.FullName} in type {type.FullName}");

                var operands = instructions[i].GetLdcI4Constant();

                var methodInfo =
                    assembly.ManifestModule.ResolveMethod(methodDescriptor.MetadataToken.ToInt32());

                object? result = methodInfo?.Invoke(null, [operands]);
                Console.WriteLine("Result: " + result);

                instructions[i].OpCode = CilOpCodes.Ldstr;
                instructions[i].Operand = result;
                instructions[i + 1].ReplaceWithNop();
                
                Console.WriteLine("Replaced call with string: " + result);
            }
        }
    }
}

module.Write($"{file}-decrypted.exe");

Console.ReadLine();