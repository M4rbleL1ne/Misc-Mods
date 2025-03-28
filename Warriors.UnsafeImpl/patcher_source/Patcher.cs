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

    public static IEnumerable<string> TargetDLLs => new[] { Assembly.GetExecutingAssembly().Location.Replace("patchers/Warriors.UnsafeImpl.dll", "plugins/System.Runtime.CompilerServices.Unsafe.dll").Replace("patchers\\Warriors.UnsafeImpl.dll", "plugins\\System.Runtime.CompilerServices.Unsafe.dll") };

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
            body.MaxStackSize = nm switch
            {
                "SkipInit" => 0,
                "AsPointer" or "SizeOf" or "As" or "ReadUnaligned" or "Read" or "AsRef" or "NullRef" or "Unbox" => 1,
                "AddByteOffset" or "AreSame" or "Copy" or "IsAddressGreaterThan" or "IsAddressLessThan" or "WriteUnaligned" or "Write" or "ByteOffset" or "IsNullRef" or "SubtractByteOffset" => 2,
                "Add" or "CopyBlock" or "CopyBlockUnaligned" or "InitBlock" or "InitBlockUnaligned" or "Subtract" => 3,
                _ => 0
            };
            var instrs = body.Instructions;
            instrs.Clear();
            instrs.AddRange(nm switch
            {
                "AsPointer" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Conv_U),
                    Instruction.Create(OpCodes.Ret)
                },
                "SizeOf" => new[]
                {
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                },
                "As" or "AsRef" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ret)
                },
                "Add" => method.Parameters[1].Name.Contains("System.Int32") ? new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Conv_I),
                    Instruction.Create(OpCodes.Mul),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Ret)
                } : new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Mul),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Ret)
                },
                "AddByteOffset" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Add),
                    Instruction.Create(OpCodes.Ret)
                },
                "AreSame" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ceq),
                    Instruction.Create(OpCodes.Ret)
                },
                "Copy" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Stobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                },
                "CopyBlock" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Cpblk),
                    Instruction.Create(OpCodes.Ret)
                },
                "CopyBlockUnaligned" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Unaligned, (byte)0x1),
                    Instruction.Create(OpCodes.Cpblk),
                    Instruction.Create(OpCodes.Ret)
                },
                "IsAddressGreaterThan" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Cgt_Un),
                    Instruction.Create(OpCodes.Ret)
                },
                "IsAddressLessThan" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Clt_Un),
                    Instruction.Create(OpCodes.Ret)
                },
                "InitBlock" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Initblk),
                    Instruction.Create(OpCodes.Ret)
                },
                "InitBlockUnaligned" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_2),
                    Instruction.Create(OpCodes.Unaligned, (byte)0x1),
                    Instruction.Create(OpCodes.Initblk),
                    Instruction.Create(OpCodes.Ret)
                },
                "ReadUnaligned" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Unaligned, (byte)0x1),
                    Instruction.Create(OpCodes.Ldobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                },
                "WriteUnaligned" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Unaligned, (byte)0x1),
                    Instruction.Create(OpCodes.Stobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                },
                "Read" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                },
                "Write" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Stobj, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                },
                "ByteOffset" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Ret)
                },
                "NullRef" => new[]
                {
                    Instruction.Create(OpCodes.Ldc_I4_0),
                    Instruction.Create(OpCodes.Conv_U),
                    Instruction.Create(OpCodes.Ret)
                },
                "IsNullRef" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldc_I4_0),
                    Instruction.Create(OpCodes.Conv_U),
                    Instruction.Create(OpCodes.Ceq),
                    Instruction.Create(OpCodes.Ret)
                },
                "SkipInit" => new[]
                {
                    Instruction.Create(OpCodes.Ret)
                },
                "Subtract" => method.Parameters[1].Name.Contains("System.Int32") ? new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Conv_I),
                    Instruction.Create(OpCodes.Mul),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Ret)
                } : new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sizeof, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Mul),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Ret)
                },
                "SubtractByteOffset" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Ldarg_1),
                    Instruction.Create(OpCodes.Sub),
                    Instruction.Create(OpCodes.Ret)
                },
                "Unbox" => new[]
                {
                    Instruction.Create(OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Unbox, method.GenericParameters[0]),
                    Instruction.Create(OpCodes.Ret)
                },
                _ => new Instruction[0]
            });
        }
        assembly.Write();
        s_logger.LogWarning("Warriors.UnsafeImpl is done.");
        s_logger = null;
    }

    public static void Patch(AssemblyDefinition assembly) { }
}