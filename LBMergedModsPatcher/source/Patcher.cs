using System.Collections.Generic;
using Mono.Cecil;
using static Mono.Cecil.MethodAttributes;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using System.Linq;
using BepInEx.Logging;

namespace LBMergedMods;

public static class Patcher
{
    static readonly ManualLogSource s_logger = Logger.CreateLogSource("LBMergedMods.Patcher");

    public static IEnumerable<string> TargetDLLs => ["Assembly-CSharp.dll"];

    public static void Patch(AssemblyDefinition assembly)
    {
        s_logger.LogMessage("LBMergedMods.Patcher is active.");
        var pack = AssemblyDefinition.ReadAssembly(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("patchers", "plugins").Replace("LBMergedMods.Patcher", "LBMergedMods")).MainModule;
        var module = assembly.MainModule;
        var interf = module.ImportReference(pack.GetType("LBMergedMods.Items.IHaveAStalk"));
        var utils = pack.GetType("LBMergedMods.Items.StalkUtils");
        var boolReturn = module.TypeSystem.Boolean;
        var names = new[] { "BubbleGrass", "MoreSlugcats.DandelionPeach", "DangleFruit", "FirecrackerPlant", "FlareBomb", "FlyLure", "MoreSlugcats.GlowWeed", "MoreSlugcats.GooieDuck", "KarmaFlower", "Lantern", "MoreSlugcats.LillyPuck", "Mushroom", "NeedleEgg", "SlimeMold", "SporePlant", "WaterNut" };
        for (var i = 0; i < names.Length; i++)
        {
            var nm = names[i];
            s_logger.LogMessage("Patching: " + nm);
            var type = module.GetType(nm);
            type.Interfaces.Add(new(interf));
            MethodDefinition meth;
            type.Methods.Add(meth = new("get_StalkActive", Public | HideBySig | SpecialName | NewSlot | Virtual, boolReturn));
            var body = meth.Body;
            body.MaxStackSize = 8;
            body.Instructions.AddRange(
            [
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, module.ImportReference(utils.Methods.First(m =>
                {
                    var prms = m.Parameters;
                    return prms.Count == 1 && prms[0].ParameterType.Name.Contains(nm.Replace("MoreSlugcats.", string.Empty));
                }))),
                Instruction.Create(OpCodes.Ret)
            ]);
            type.Properties.Add(new("StalkActive", PropertyAttributes.None, boolReturn) { GetMethod = meth });
        }
        s_logger.LogMessage("LBMergedMods.Patcher is done.");
    }
}