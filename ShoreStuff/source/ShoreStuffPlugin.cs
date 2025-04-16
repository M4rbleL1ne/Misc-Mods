using BepInEx;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System;
using UnityEngine;
using RWCustom;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using BepInEx.Logging;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ShoreStuff;

[BepInPlugin("lb-fgf-m4r-ik.shore-stuff", nameof(ShoreStuff), "10.0.0")]
sealed class ShoreStuffPlugin : BaseUnityPlugin
{
    static ManualLogSource? s_logger;

    public void OnEnable()
    {
        s_logger = Logger;
        new Hook(typeof(RoomRain).GetMethod("get_FloodLevel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), On_RoomRain_get_FloodLevel);
        On.Water.DrawSprites += On_Water_DrawSprites;
        IL.Water.Update += IL_Water_Update;
    }

    static void IL_Water_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<RoomSettings.RoomEffect.Type>(nameof(RoomSettings.RoomEffect.Type.WaterViscosity)),
            x => x.MatchCallOrCallvirt<RoomSettings>(nameof(RoomSettings.GetEffectAmount))))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float amount, Water self) =>
             {
                 var rm = self.room;
                 if (rm.game?.cameras[0] is RoomCamera cam && cam.room == rm && rm.abstractRoom.name is string roomName && string.Equals(roomName, "FR_C20", StringComparison.OrdinalIgnoreCase))
                 {
                     if (cam.currentCameraPosition == 2)
                     {
                         Shader.EnableKeyword("HR");
                         Shader.EnableKeyword("Gutter");
                         return Math.Max(amount, .8f);
                     }
                     else
                     {
                         Shader.DisableKeyword("HR");
                         Shader.DisableKeyword("Gutter");
                     }
                 }
                 return amount;
             });
        }
        else
            s_logger!.LogError("Couldn't ILHook Water.Update!");
    }

    static float On_RoomRain_get_FloodLevel(Func<RoomRain, float> orig, RoomRain self) => self.room is Room rm && rm.world?.region?.name == "FR" && rm.waterFlux is Room.WaterFluxController c ? c.fluxWaterLevel : orig(self);

    static void On_Water_DrawSprites(On.Water.orig_DrawSprites orig, Water self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!rCam.voidSeaMode && !self.slatedForDeletetion && rCam.room is Room room && room == self.room && room.abstractRoom?.name is string s && string.Equals(s, "FR_C20", StringComparison.OrdinalIgnoreCase))
        {
            var sprs = sLeaser.sprites;
            var sh = sprs[0].shader = rCam.currentCameraPosition == 2 || (ModManager.DLCShared && room.roomSettings?.DangerType == DLCSharedEnums.RoomRainDangerType.Blizzard) ? Custom.rainWorld.Shaders["WaterSlush"] : Custom.rainWorld.Shaders["WaterSurface"];
            var surfs = self.surfaces;
            for (var k = 1; k < surfs.Length; k++)
                sprs[k * 2].shader = sh;
        }
    }

    public void OnDisable() => s_logger = null;
}