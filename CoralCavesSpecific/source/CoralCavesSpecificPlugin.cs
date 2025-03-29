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

[BepInPlugin("lb-fgf-m4r-ik.coral-caves-specific", "CoralCavesSpecific", "1.0.0")]
public class CoralCavesSpecificPlugin : BaseUnityPlugin
{
    [AllowNull] internal static ManualLogSource s_logger;

    public void OnEnable()
    {
        On.TentaclePlantGraphics.ApplyPalette += (orig, self, sLeaser, rCam, palette) =>
        {
            orig(self, sLeaser, rCam, palette);
            if (self.plant?.room is Room rm && (rm.abstractRoom.name is "reef" or "pump" or "cavity" || rm.world?.region?.name is "RF"))
            {
                for (var i = 0; i < self.danglers.Length; i++)
                    sLeaser.sprites[i + 1].color = Color.Lerp(new Color32(10, 84, 66, 255), palette.blackColor, rCam.room.Darkness(self.plant.rootPos));
            }
        };
    }

    public void OnDisable() => s_logger = null;
}