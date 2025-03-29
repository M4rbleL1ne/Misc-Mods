using BepInEx;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace GildedSanctuarySpecific;

[BepInPlugin("lb-fgf-m4r-ik.gilded-sanctuary-specific", "GildedSanctuarySpecific", "1.0.0")]
sealed class GildedSanctuarySpecificMod : BaseUnityPlugin
{
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += (orig, self) =>
        {
            orig(self);
            _ = RoomEffectType.GRJGoldenFlakes;
            _ = PlacedObjectType.GRJLeviathanPushBack;
        };
        On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
        {
            orig(self, newlyDisabledMods);
            for (var i = 0; i < newlyDisabledMods.Length; i++)
            {
                if (newlyDisabledMods[i].id == "lb-fgf-m4r-ik.gilded-sanctuary-specific")
                {
                    RoomEffectType.UnregisterValues();
                    PlacedObjectType.UnregisterValues();
                    break;
                }
            }
        };
    }
}
