using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Mono.Cecil;
using BepInEx;
using static Mono.Cecil.Cil.OpCodes;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using static System.Reflection.BindingFlags;

namespace Warriors.DetourContextDisposeFix;

public static class Patcher
{
    static readonly ManualLogSource s_logger = Logger.CreateLogSource("Warriors.DetourContextDisposeFix");

    public static IEnumerable<string> TargetDLLs => new[] { Path.Combine(Paths.BepInExAssemblyDirectory, "MonoMod.RuntimeDetour.dll") };

    static Patcher()
    {
        s_logger.LogWarning("Warriors.DetourContextDisposeFix preloader patch is active.");
        new ILHook(typeof(DetourContext).GetMethod("Dispose", Public | NonPublic | Static | Instance), il =>
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<DetourContext>("IsDisposed"),
                x => x.MatchBrtrue(out _)))
            {
                c.Prev.OpCode = Brfalse_S;
                s_logger.LogInfo("Warriors.DetourContextDisposeFix has successfully patched MonoMod.RuntimeDetour.DetourContext.Dispose.");
            }
        });
        s_logger = null;
    }

    public static void Patch(AssemblyDefinition assembly) { }
}