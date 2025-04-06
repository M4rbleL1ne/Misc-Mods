using System.Collections.Generic;
using BepInEx.Logging;
using Mono.Cecil;
using System.Reflection;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using System.Linq;

namespace Warriors.UnsafeImpl;

public static class Patcher
{
    static readonly ManualLogSource s_logger = Logger.CreateLogSource("Warriors.UnsafeImpl");

    public static IEnumerable<string> TargetDLLs => [Assembly.GetExecutingAssembly().Location.Replace("patchers/Warriors.UnsafeImpl.dll", "plugins/System.Runtime.CompilerServices.Unsafe.dll").Replace("patchers\\Warriors.UnsafeImpl.dll", "plugins\\System.Runtime.CompilerServices.Unsafe.dll")];

    static Patcher()
    {
        using var assembly = AssemblyDefinition.ReadAssembly(TargetDLLs.First(), new() { ReadWrite = true });
        s_logger.LogWarning("Warriors.UnsafeImpl preloader patch is active!");
        var methods = assembly.MainModule.GetType("System.Runtime.CompilerServices.Unsafe").Methods;
        for (var i = 0; i < methods.Count; i++)
        {
            var method = methods[i];
            var nm = method.Name;
            var body = method.Body;
            var instrs = body.Instructions;
            if (nm == "BitCast")
            {
                instrs[0] = Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]);
                instrs[1] = Instruction.Create(OpCodes.Sizeof, method.GenericParameters[1]);
                continue;
            }
            body.MaxStackSize = nm switch
            {
                "SkipInit" => 0,
                "AsPointer" or "SizeOf" or "As" or "ReadUnaligned" or "Read" or "AsRef" or "NullRef" or "Unbox" => 1,
                "AddByteOffset" or "AreSame" or "Copy" or "IsAddressGreaterThan" or "IsAddressLessThan" or "WriteUnaligned" or "Write" or "ByteOffset" or "IsNullRef" or "SubtractByteOffset" => 2,
                "Add" or "CopyBlock" or "CopyBlockUnaligned" or "InitBlock" or "InitBlockUnaligned" or "Subtract" => 3,
                _ => 0
            };
            instrs.Clear();
            instrs.AddRange(nm switch
            {
                "AsPointer" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Conv_U),
                    Instruction.Create(OpCodes.Ret)
                ],
                "SizeOf" =>
                [
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                ],
                "As" or "AsRef" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ret)
                ],
                "Add" => method.Parameters[1].Name.Contains("System.Int32") ?
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Conv_I),
                    Instruction.Create(OpCodes.Mul),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Ret)
                ] :
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Mul),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Ret)
                ],
                "AddByteOffset" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Ret)
                ],
                "AreSame" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ceq),
                    Instruction.Create(OpCodes.Ret)
                ],
                "Copy" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Stobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                ],
                "CopyBlock" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Cpblk),
                    Instruction.Create(OpCodes.Ret)
                ],
                "CopyBlockUnaligned" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Unaligned, (byte)1),
                    Instruction.Create(OpCodes.Cpblk),
                    Instruction.Create(OpCodes.Ret)
                ],
                "IsAddressGreaterThan" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Cgt_Un),
                    Instruction.Create(OpCodes.Ret)
                ],
                "IsAddressLessThan" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Clt_Un),
                    Instruction.Create(OpCodes.Ret)
                ],
                "InitBlock" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Initblk),
                    Instruction.Create(OpCodes.Ret)
                ],
                "InitBlockUnaligned" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Unaligned, (byte)1),
                    Instruction.Create(OpCodes.Initblk),
                    Instruction.Create(OpCodes.Ret)
                ],
                "ReadUnaligned" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Unaligned, (byte)1),
                    Instruction.Create(OpCodes.Ldobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                ],
                "WriteUnaligned" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Unaligned, (byte)1),
                    Instruction.Create(OpCodes.Stobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                ],
                "Read" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                ],
                "Write" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Stobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                ],
                "ByteOffset" =>
                [
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Ret)
                ],
                "NullRef" =>
                [
                    Instruction.Create(OpCodes.Ldc_I4_0),
                    Instruction.Create(OpCodes.Conv_U),
                    Instruction.Create(OpCodes.Ret)
                ],
                "IsNullRef" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldc_I4_0),
                    Instruction.Create(OpCodes.Conv_U),
                    Instruction.Create(OpCodes.Ceq),
                    Instruction.Create(OpCodes.Ret)
                ],
                "SkipInit" =>
                [
                    Instruction.Create(OpCodes.Ret)
                ],
                "Subtract" => method.Parameters[1].Name.Contains("System.Int32") ?
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Conv_I),
                    Instruction.Create(OpCodes.Mul),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Ret)
                ] :
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Mul),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Ret)
                ],
                "SubtractByteOffset" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Ret)
                ],
                "Unbox" =>
                [
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Unbox, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                ],
                _ => []
            });
        }
        assembly.Write();
        s_logger.LogWarning("Warriors.UnsafeImpl is done.");
        s_logger = null;
    }

    public static void Patch(AssemblyDefinition _) { }
}