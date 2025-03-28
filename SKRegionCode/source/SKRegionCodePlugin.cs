using BepInEx;
using UnityEngine;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace SKRegionCode;

[BepInPlugin("lb-fgf-m4r-ik.sk-wrayk-region-code", "SKRegionCode", "1.0.1")]
public sealed class SKRegionCodePlugin : BaseUnityPlugin
{
    //public static RoomSettings.RoomEffect.Type SKBackgroundRain = new(nameof(SKBackgroundRain), true);

    public void OnEnable()
    {
        On.Lightning.ctor += (orig, self, room, intensity, bkgOnly) =>
        {
            orig(self, room, intensity, bkgOnly);
            if (room?.world?.region?.name is "SK")
            {
                self.bkgGradient[0] = Color.white;
                self.bkgGradient[1] = Color.white;
            }
        };
        On.Lightning.LightningSource.Update += (orig, self) =>
        {
            var flag = self.owner?.room?.world?.region?.name is "SK";
            if (flag)
                self.loopVol = 0f;
            orig(self);
            if (flag)
                self.loopVol = 0f;
        };
        /*On.RoomRain.AddToContainer += (orig, self, sLeaser, rCam, newContainer) =>
        {
            orig(self, sLeaser, rCam, newContainer);
            if (self.room?.roomSettings?.GetEffectAmount(SKBackgroundRain) is float f && f > 0f)
            {
                newContainer = rCam.ReturnFContainer("Background");
                for (var i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].RemoveFromContainer();
                    newContainer.AddChild(sLeaser.sprites[i]);
                }
            }
        };
        On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
        {
            orig(self, newlyDisabledMods);
            for (var i = 0; i < newlyDisabledMods.Length; i++)
            {
                if (newlyDisabledMods[i].id == "lb-fgf-m4r-ik.sk-wrayk-region-code")
                {
                    SKBackgroundRain?.Unregister();
                    SKBackgroundRain = null;
                    break;
                }
            }
        };*/
    }
}