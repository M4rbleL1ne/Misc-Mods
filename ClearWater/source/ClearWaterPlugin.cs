using BepInEx;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ClearWaterPlugin;

[BepInPlugin("lb-fgf-m4r-ik.clear-water-plugin", "ClearWaterPlugin", "10.0.0")]
sealed class ClearWaterPlugin : BaseUnityPlugin
{
	public void OnEnable() => On.Water.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        var spr = sLeaser.sprites[1];
        spr.scale = 0f;
        spr.isVisible = false;
    };
}