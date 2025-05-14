using BepInEx;
using UnityEngine;
using System.Security.Permissions;
using System.Security;
using System;
using MoreSlugcats;
using System.Runtime.CompilerServices;
using BepInEx.Logging;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace CandyCaneSpears;

[BepInPlugin("lb-fgf-m4r-ik.candy-cane-spears", "Candy Cane Spears", "10.0.0")]
public sealed class CandyCanesPlugin : BaseUnityPlugin
{
    public static ConditionalWeakTable<Spear, StrongBox<int>> CandyIndex = new();
    static ManualLogSource? s_logger;

    public void OnEnable()
    {
        s_logger = Logger;
        On.RainWorld.OnModsInit += (orig, self) =>
        {
            orig(self);
            try
            {
                if (!Futile.atlasManager.DoesContainAtlas("candycanespear"))
                    Futile.atlasManager.LoadAtlas("atlases/candycanespear");
            }
            catch (Exception e)
            {
                s_logger.LogError("Exception while loading atlases: " + e);
            }
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
            if (self is not ExplosiveSpear and not ElectricSpear && !self.bugSpear && !self.IsNeedle && self.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
                sLeaser.sprites[self.hasPoisonGraphicsActive ? 1 : 0].element = Futile.atlasManager.GetElementWithName("candycanespearW1");
                var ps = sLeaser.sprites.Length - 1;
                sLeaser.sprites[ps] = new("candycanespearW2");
                if (CandyIndex.TryGetValue(self, out var index))
                    index.Value = ps;
                else
                    CandyIndex.Add(self, new(ps));
                self.AddToContainer(sLeaser, rCam, null);
            }
        };
        On.Spear.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self is not ExplosiveSpear and not ElectricSpear && !self.bugSpear && !self.IsNeedle && self.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                var spr = sLeaser.sprites[self.hasPoisonGraphicsActive ? 1 : 0];
                spr.element = Futile.atlasManager.GetElementWithName("candycanespearW1");
                var dark = rCam.PaletteDarkness() * .2f;
                spr.color = Color.Lerp(Color.white, rCam.currentPalette.blackColor, dark);
                if (CandyIndex.TryGetValue(self, out var index) && index.Value < sLeaser.sprites.Length)
                {
                    var spr2 = sLeaser.sprites[index.Value];
                    spr2.element = Futile.atlasManager.GetElementWithName("candycanespearW2");
                    spr2.color = Color.Lerp(new(1f, 17f / 255f, 0f), rCam.currentPalette.blackColor, dark);
                    spr2.x = spr.x;
                    spr2.y = spr.y;
                    spr2.rotation = spr.rotation;
                    spr2.anchorX = spr.anchorX;
                    spr2.anchorY = spr.anchorY;
                    spr2.MoveInFrontOfOtherNode(spr);
                }
            }
        };
        On.Spear.ApplyPalette += (orig, self, sLeaser, rCam, palette) =>
        {
            orig(self, sLeaser, rCam, palette);
            if (self is not ExplosiveSpear and not ElectricSpear && !self.bugSpear && !self.IsNeedle && self.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                var dark = rCam.PaletteDarkness() * .2f;
                sLeaser.sprites[self.hasPoisonGraphicsActive ? 1 : 0].color = Color.Lerp(Color.white, rCam.currentPalette.blackColor, dark);
                if (CandyIndex.TryGetValue(self, out var index) && index.Value < sLeaser.sprites.Length)
                    sLeaser.sprites[index.Value].color = Color.Lerp(new(1f, 17f / 255f, 0f), rCam.currentPalette.blackColor, dark);
            }
        };
    }

    public void OnDisable()
    {
        s_logger = null;
        CandyIndex = null!;
    }
}