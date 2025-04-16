using BepInEx;
using UnityEngine;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace SKRegionCode;

[BepInPlugin("lb-fgf-m4r-ik.sk-wrayk-region-code", "SKRegionCode", "10.0.0")]
public sealed class SKRegionCodePlugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        On.Lightning.ctor += (orig, self, room, intensity, bkgOnly) =>
        {
            orig(self, room, intensity, bkgOnly);
            if (room?.world?.region?.name == "SK")
                self.bkgGradient[1] = self.bkgGradient[0] = Color.white;
        };
        On.Lightning.LightningSource.Update += (orig, self) =>
        {
            var flag = self.owner?.room?.world?.region?.name == "SK";
            if (flag)
                self.loopVol = 0f;
            orig(self);
            if (flag)
                self.loopVol = 0f;
        };
    }
}