using BepInEx;
using UnityEngine;
using RWCustom;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;
using System.Security;
using ArenaBehaviors;
using MonoMod.Cil;
using Mono.Cecil.Cil;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace BatBombs;

[BepInPlugin("lb-fgf-m4r-ik.bat-bombs", nameof(BatBombs), "1.1.1")]
sealed class BatBombsPlugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        //prevents an exception
        IL.MultiplayerUnlocks.ctor += il =>
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchLdarg(0), x => x.MatchLdarg(1)))
            {
                c.Emit(OpCodes.Ldsfld, il.Import(typeof(SandboxUnlockID).GetField(nameof(SandboxUnlockID.BatBomb))));
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldsfld, il.Import(typeof(AbstractObjectType).GetField(nameof(AbstractObjectType.BatBomb))));
                c.Emit(OpCodes.Pop);
            }
            else
                Logger.LogError("Couldn't ILHook MultiplayerUnlocks.ctor!");
        };
        On.RainWorld.OnModsInit += (orig, self) =>
        {
            orig(self);
            if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.BatBomb))
                MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.BatBomb);
        };
        On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += (orig, unlockID) =>
        {
            var res = orig(unlockID);
            if (unlockID == SandboxUnlockID.BatBomb)
                res = new(CreatureTemplate.Type.StandardGroundCreature, AbstractObjectType.BatBomb, 0);
            return res;
        };
        On.MultiplayerUnlocks.SandboxUnlockForSymbolData += (orig, data) =>
        {
            var res = orig(data);
            if (data.itemType == AbstractObjectType.BatBomb)
                res = SandboxUnlockID.BatBomb;
            return res;
        };
        On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
        {
            orig(self, newlyDisabledMods);
            for (var i = 0; i < newlyDisabledMods.Length; i++)
            {
                if (newlyDisabledMods[i].id == "lb-fgf-m4r-ik.bat-bombs")
                {
                    if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.BatBomb))
                        MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.BatBomb);
                    SandboxUnlockID.UnregisterValues();
                    AbstractObjectType.UnregisterValues();
                    break;
                }
            }
        };
        On.ScavengerBomb.Explode += (orig, self, hitChunk) =>
        {
            if (self is BatBomb && !self.slatedForDeletetion && self.room is Room rm && rm.game is RainWorldGame g)
            {
                if (hitChunk?.owner is PhysicalObject p && p is not Fly && p.bodyChunks is BodyChunk[] b)
                {
                    for (var i = 0; i < b.Length; i++)
                    {
                        var crit = new AbstractCreature(rm.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), null, rm.GetWorldCoordinate(b[i].pos), g.GetNewID());
                        rm.abstractRoom.AddEntity(crit);
                        crit.RealizeInRoom();
                    }
                    if (p is Creature cr)
                        cr.Die();
                    p.Destroy();
                }
                var fs = self.firstChunk;
                var vector = Vector2.Lerp(fs.pos, fs.lastPos, .35f);
                rm.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, self.explodeColor));
                rm.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, Color.white));
                rm.AddObject(new ExplosionSpikes(rm, vector, 14, 30f, 9f, 7f, 170f, self.explodeColor));
                rm.AddObject(new ShockWave(vector, 330f, .045f, 5));
                for (var l = 0; l < 6; l++)
                    rm.AddObject(new ScavengerBomb.BombFragment(vector, Custom.DegToVec((l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
                rm.ScreenMovement(vector, Vector2.zero, 1.3f);
                for (var m = 0; m < self.abstractPhysicalObject.stuckObjects.Count; m++)
                    self.abstractPhysicalObject.stuckObjects[m].Deactivate();
                rm.PlaySound(SoundID.Token_Collect, vector, 1.2f, 1.5f);
                self.smoke?.Destroy();
                self.Destroy();
                return;
            }
            orig(self, hitChunk);
        };
        On.AbstractPhysicalObject.Realize += (orig, self) =>
        {
            orig(self);
            if (self.realizedObject is null)
            {
                if (self.type == AbstractObjectType.BatBomb)
                    self.realizedObject = new BatBomb(self, self.world);
            }
        };
        On.MultiplayerUnlocks.SandboxItemUnlocked += (orig, self, unlockID) => orig(self, unlockID) || unlockID == SandboxUnlockID.BatBomb;
        On.ItemSymbol.SpriteNameForItem += (orig, itemType, intData) =>
        {
            var res = orig(itemType, intData);
            if (itemType == AbstractObjectType.BatBomb)
                res = "Symbol_StunBomb";
            return res;
        };
        On.ItemSymbol.ColorForItem += (orig, itemType, intData) =>
        {
            var res = orig(itemType, intData);
            if (itemType == AbstractObjectType.BatBomb)
                res = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            return res;
        };
        On.ArenaBehaviors.SandboxEditor.GetPerformanceEstimate += delegate (On.ArenaBehaviors.SandboxEditor.orig_GetPerformanceEstimate orig, SandboxEditor.PlacedIcon placedIcon, ref float exponentialPart, ref float linearPart)
        {
            if (placedIcon is SandboxEditor.CreatureOrItemIcon i && i.iconData.itemType == AbstractObjectType.BatBomb)
                linearPart += 1.2f;
            else
                orig(placedIcon, ref exponentialPart, ref linearPart);
        };
    }
}

public static class AbstractObjectType
{
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType BatBomb = new(nameof(BatBomb), true);

    public static void UnregisterValues()
    {
        if (BatBomb is not null)
        {
            BatBomb.Unregister();
            BatBomb = null;
        }
    }
}

public static class SandboxUnlockID
{
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID BatBomb = new(nameof(BatBomb), true);

    public static void UnregisterValues()
    {
        if (BatBomb is not null)
        {
            BatBomb.Unregister();
            BatBomb = null;
        }
    }
}

public sealed class BatBomb : ScavengerBomb
{
    public BatBomb(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world) => explodeColor = new(.3f, .3f, .3f);

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (slatedForDeletetion || room != rCam.room)
            return;
        var color = Color.Lerp(explodeColor, Color.grey, .5f + .2f * Mathf.Pow(Random.value, .2f));
        color = Color.Lerp(color, Color.white, Mathf.Pow(Random.value, !ignited ? 30f : 3f));
        for (var j = 0; j < 2; j++)
            sLeaser.sprites[j].color = color;
    }
}