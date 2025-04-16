using BepInEx;
using System.Security.Permissions;
using System.Security;
using BepInEx.Logging;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace CoralCavesSpecific;

[BepInPlugin("lb-fgf-m4r-ik.coral-caves-specific", "CoralCavesSpecific", "10.0.0")]
public sealed class CoralCavesSpecificPlugin : BaseUnityPlugin
{
    [AllowNull] internal static ManualLogSource s_logger;

    public void OnEnable()
    {
        On.TentaclePlantGraphics.ApplyPalette += (orig, self, sLeaser, rCam, palette) =>
        {
            orig(self, sLeaser, rCam, palette);
            if (rCam.room is Room rm && (rm.abstractRoom.name is "reef" or "pump" or "cavity" || rm.world?.region?.name == "RF"))
            {
                var l = self.danglers.Length;
                for (var i = 0; i < l; i++)
                    sLeaser.sprites[i + 1].color = Color.Lerp(new(10f / 255f, 84 / 255f, 66 / 255f), palette.blackColor, rm.Darkness(self.plant.rootPos));
            }
        };
    }

    public void OnDisable() => s_logger = null;
}