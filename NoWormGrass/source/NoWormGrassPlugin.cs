using BepInEx;
using System.Security.Permissions;
using System.Security;
using System.Collections.Generic;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace NoWormGrass;

[BepInPlugin("lb-fgf-m4r-ik.no-worm-grass", "NoWormGrass", "10.0.0")]
sealed class NoWormGrassPlugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        On.WormGrass.Update += (orig, self, eu) =>
        {
            orig(self, eu);
            self.KillGrass();
        };
        On.Room.Loaded += (orig, self) =>
        {
            orig(self);
            if (self.Tiles is Room.Tile[,] tls)
            {
                for (var i = 0; i < tls.GetLength(0); i++)
                {
                    for (var j = 0; j < tls.GetLength(1); j++)
                        tls[i, j].wormGrass = false;
                }
            }
            if (StaticWorld.creatureTemplates is CreatureTemplate[] ar)
            {
                for (var i = 0; i < ar.Length; i++)
                {
                    if (ar[i] is CreatureTemplate t)
                    {
                        t.wormGrassImmune = true;
                        t.wormgrassTilesIgnored = true;
                    }
                }
            }
        };
        On.Room.Update += (orig, self) =>
        {
            orig(self);
            for (var i = 0; i < self.updateList?.Count; i++)
            {
                if (self.updateList[i] is WormGrass w)
                    w.KillGrass();
            }
        };
    }
}

static class Ext
{
    public static void Kill(this List<WormGrass.Worm> self)
    {
        for (var i = 0; i < self.Count; i++)
        {
            if (self[i] is WormGrass.Worm w)
            {
                w.RemoveFromRoom();
                w.Destroy();
            }
        }
        self.Clear();
    }

    public static void Kill(this List<WormGrass.WormGrassPatch> self)
    {
        for (var i = 0; i < self.Count; i++)
        {
            if (self[i] is WormGrass.WormGrassPatch w)
            {
                w.worms?.Kill();
                w.trackedCreatures?.Clear();
            }
        }
        self.Clear();
    }

    public static void KillGrass(this WormGrass self)
    {
        if (self is not null)
        {
            self.worms?.Kill();
            self.cosmeticWorms?.Kill();
            self.repulsiveObjects?.Clear();
            self.patches?.Kill();
            self.RemoveFromRoom();
            self.Destroy();
        }
    }
}