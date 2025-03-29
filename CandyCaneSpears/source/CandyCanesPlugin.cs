using BepInEx;
using UnityEngine;
using System.Security.Permissions;
using System.Security;
using System;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace CandyCaneSpears;

[BepInPlugin("lb-fgf-m4r-ik.candy-cane-spears", "Candy Cane Spears", "1.1.1")]
sealed class CandyCanesPlugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += (orig, self) =>
        {
            orig(self);
            if (!Futile.atlasManager.DoesContainAtlas("candycanespear"))
                Futile.atlasManager.LoadAtlas("atlases/candycanespear");
        };
        On.RainWorld.UnloadResources += (orig, self) =>
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("candycanespear"))
                Futile.atlasManager.UnloadAtlas("candycanespear");
        };
        On.Spear.InitiateSprites += (orig, self, sLeaser, rCam) =>
        {
            orig(self, sLeaser, rCam);
            if (self is not ExplosiveSpear && !self.bugSpear && !self.IsNeedle)
                sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("candycanespear");

        };
        On.Spear.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self is not ExplosiveSpear && !self.bugSpear && !self.IsNeedle)
                sLeaser.sprites[0].color = Color.white;
        };
    }
}