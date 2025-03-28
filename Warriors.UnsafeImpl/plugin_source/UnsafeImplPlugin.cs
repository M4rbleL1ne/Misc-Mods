using BepInEx;
/*using MonoMod.RuntimeDetour;
using System;
using MonoMod.Cil;
using Mono.Cecil;
using System.Reflection;
using System.Runtime.CompilerServices;*/

namespace Warriors.UnsafeImpl.Plugin;

[BepInPlugin("lb-fgf-m4r-ik.warriors.unsafeimpl", "Warriors.UnsafeImpl.Plugin", "1.0.0")]
sealed class UnsafeImplPlugin : BaseUnityPlugin
{
	public /*unsafe*/ void OnEnable()
	{
        /*new ILHook(typeof(UnsafeImplPlugin).GetMethod("OnEnable"), il =>
        {
            var methods = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location).MainModule.GetType("System.Runtime.CompilerServices.Unsafe").Methods;
            for (var i = 0; i < methods.Count; i++)
                Logger.LogDebug(new ILContext(methods[i]));
        });
        try
        {
            var x = 1;
            var y = 2;
            ref var r = ref x;
            ref var g = ref y;
            Logger.LogDebug(Unsafe.SizeOf<BaseUnityPlugin>());
            Logger.LogDebug(Unsafe.AreSame(ref r, ref r));
            Logger.LogDebug(Unsafe.AreSame(ref r, ref g));
            Unsafe.SkipInit(out long t);
            Logger.LogDebug(t);
            Logger.LogDebug(Unsafe.Unbox<int>(x));
            Logger.LogDebug((nint)Unsafe.AsPointer(ref Unsafe.Subtract(ref r, 2)));
            Logger.LogDebug((nint)Unsafe.AsPointer(ref Unsafe.Add(ref r, 2)));
            Logger.LogDebug((nint)Unsafe.AsPointer(ref Unsafe.Subtract(ref r, (nuint)2)));
            Logger.LogDebug((nint)Unsafe.AsPointer(ref Unsafe.Add(ref r, (nuint)2)));
            Logger.LogDebug((nint)Unsafe.AsPointer(ref Unsafe.SubtractByteOffset(ref r, (nuint)2)));
            Logger.LogDebug((nint)Unsafe.AsPointer(ref Unsafe.SubtractByteOffset(ref r, (nuint)2)));
            Logger.LogDebug(Unsafe.ByteOffset(ref r, ref g));
            Unsafe.CopyBlock(ref Unsafe.As<int, byte>(ref g), ref Unsafe.As<int, byte>(ref y), sizeof(int));
            Logger.LogDebug(x);
            Logger.LogDebug(y);
            Unsafe.InitBlock(ref Unsafe.As<long, byte>(ref t), 0, sizeof(long));
            Logger.LogDebug(t);
            Logger.LogDebug((nint)Unsafe.AsPointer(ref Unsafe.NullRef<bool>()));
            Logger.LogDebug(Unsafe.IsNullRef(ref Unsafe.NullRef<bool>()));
            Logger.LogDebug(Unsafe.IsAddressGreaterThan(ref r, ref r));
            scoped ref readonly var il = ref y;
            Logger.LogDebug((nint)Unsafe.AsPointer(ref Unsafe.AsRef(il)));
            Unsafe.AsRef(il) = 78;
            Logger.LogDebug(y);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }*/
    }
}