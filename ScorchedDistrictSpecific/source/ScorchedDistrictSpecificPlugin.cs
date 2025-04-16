using BepInEx;
using System.Security.Permissions;
using System.Security;
using Random = UnityEngine.Random;
using System.Diagnostics.CodeAnalysis;
using System;
using static System.Reflection.BindingFlags;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using RWCustom;
using System.Runtime.InteropServices;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ScorchedDistrictSpecific;

[BepInPlugin("lb-fgf-m4r-ik.scorched-district-specific", "ScorchedDistrictSpecific", "10.0.0"), BepInDependency("rwmodding.coreorg.rk", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class ScorchedDistrictSpecificPlugin : BaseUnityPlugin
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class SDRainCycle
    {
        public int BaseSunDownTime, NewSunDownTime;
    }

    [AllowNull] static ConditionalWeakTable<RainCycle, SDRainCycle> s_sdCycle = new();

    static void HookRegionKit(Assembly asm)
    {
        var snd = asm.GetType("RegionKit.Modules.AridBarrens.SandStorm");
        const BindingFlags FLAGS = Public | NonPublic | Instance;
        FieldInfo _soundLoop = snd.GetField("_soundLoop", FLAGS), _soundLoop2 = snd.GetField("_soundLoop2", FLAGS);
        var meth = snd.GetNestedType("Fog", FLAGS).GetMethod("InitiateSprites", FLAGS);
        new Hook(meth, (Action<BackgroundScene.FullScreenSingleColor, RoomCamera.SpriteLeaser, RoomCamera> origHK, BackgroundScene.FullScreenSingleColor selfHK, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) =>
        {
            origHK(selfHK, sLeaser, rCam);
            if (selfHK.room?.world?.region?.name == "SD")
                selfHK.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
        });
        var meth3 = snd.GetMethod("Update", FLAGS);
        new Hook(meth3, (Action<BackgroundScene, bool> origHK, BackgroundScene selfHK, bool eu) =>
        {
            origHK(selfHK, eu);
            if (selfHK.room?.world?.region?.name == "SD")
            {
                if (_soundLoop.GetValue(selfHK) is DisembodiedDynamicSoundLoop snd && snd.Volume > .4f)
                    snd.Volume = .4f;
                if (_soundLoop2.GetValue(selfHK) is DisembodiedDynamicSoundLoop snd2 && snd2.Volume > .4f)
                    snd2.Volume = .4f;
            }
        });
    }

    public void OnEnable()
    {
        On.RainCycle.ctor += (orig, self, world, minutes) =>
        {
            orig(self, world, minutes);
            if (!s_sdCycle.TryGetValue(self, out var rc))
                s_sdCycle.Add(self, new() { BaseSunDownTime = self.sunDownStartTime, NewSunDownTime = self.sunDownStartTime + self.sunDownStartTime / 6 });
            else
            {
                rc.BaseSunDownTime = self.sunDownStartTime;
                rc.NewSunDownTime = self.sunDownStartTime + self.sunDownStartTime / 6;
            }
        };
        On.RainCycle.Update += (orig, self) =>
        {
            if (s_sdCycle.TryGetValue(self, out var rc))
            {
                if (self.world?.region?.name == "SD" && self.sunDownStartTime != rc.NewSunDownTime)
                    self.sunDownStartTime = rc.NewSunDownTime;
                else if (self.sunDownStartTime != rc.BaseSunDownTime)
                    self.sunDownStartTime = rc.BaseSunDownTime;
            }
            orig(self);
            if (self.world?.region?.name == "SD")
                self.sunDownStartTime += self.sunDownStartTime / 3;
        };
        On.RainWorld.PostModsInit += (orig, self) =>
        {
            orig(self);
            var col = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < col.Length; i++)
            {
                if (col[i].FullName.ToLowerInvariant().Contains("regionkit"))
                {
                    HookRegionKit(col[i]);
                    break;
                }
            }
        };
        On.Centipede.Update += (orig, self, eu) =>
        {
            if (self.Small && !self.abstractCreature.superSizeMe && self.abstractCreature.world?.region?.name == "SD")
                self.abstractCreature.superSizeMe = true;
            orig(self, eu);
        };
        On.Centipede.ShortCutColor += (orig, self) =>
        {
            var res = orig(self);
            if (self.Small && self.abstractCreature.superSizeMe)
                res = Custom.HSL2RGB(Mathf.Lerp(.28f, .38f, .5f), .5f, .5f);
            return res;
        };
        On.CentipedeGraphics.Update += (orig, self) =>
        {
            if (self.centipede.Small && self.centipede.abstractCreature.superSizeMe && self.saturation != .85f)
            {
                self.hue = Mathf.Lerp(.28f, .38f, Random.value);
                self.saturation = .85f;
            }
            orig(self);
        };
        On.CollectToken.AddToContainer += (orig, self, sLeaser, rCam, newContainer) =>
        {
            orig(self, sLeaser, rCam, newContainer);
            if (self.room?.world?.region?.name == "SD")
            {
                var sprs = sLeaser.sprites;
                for (var i = 0; i < sLeaser.sprites.Length; i++)
                    sprs[i].RemoveFromContainer();
                var gs = self.GoldSprite;
                var flag = self.blueToken || self.greenToken || self.redToken || self.whiteToken || self.devToken;
                var cont = rCam.ReturnFContainer("GrabShaders");
                if (flag)
                    cont.AddChild(sprs[gs]);
                for (var j = 0; j < gs; j++)
                    cont.AddChild(sprs[j]);
                if (!flag)
                    cont.AddChild(sprs[gs]);
            }
        };
        On.KingTusks.Tusk.AddToContainer += (orig, self, vGraphics, spr, sLeaser, rCam, newContainer) =>
        {
            orig(self, vGraphics, spr, sLeaser, rCam, newContainer);
            if (vGraphics.vulture?.room?.world?.region?.name == "SD" && spr == self.LaserSprite(vGraphics))
            {
                var s = sLeaser.sprites[spr];
                s.RemoveFromContainer();
                rCam.ReturnFContainer("Midground").AddChild(s);
            }
        };
        On.VultureGrubGraphics.AddToContainer += (orig, self, sLeaser, rCam, newContainer) =>
        {
            orig(self, sLeaser, rCam, newContainer);
            if (self.worm?.room?.world?.region?.name == "SD")
            {
                var sprs = sLeaser.sprites;
                var cont = rCam.ReturnFContainer("Bloom");
                for (var i = 0; i < sprs.Length; i++)
                {
                    if (i == self.LaserSprite || i == self.FlashSprite)
                    {
                        var s = sprs[i];
                        s.RemoveFromContainer();
                        cont.AddChild(s);
                    }
                }
            }
        };
    }

    public void OnDisable() => s_sdCycle = null;
}