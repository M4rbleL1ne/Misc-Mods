using BepInEx;
using DevInterface;
using RWCustom;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using Random = UnityEngine.Random;
using MoreSlugcats;
using Watcher;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace BounceRects;

[BepInPlugin("lb-fgf-m4r-ik.bounce-rects", nameof(BounceRects), "10.0.0")]
sealed class BounceRectsPlugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += (orig, self, type) =>
        {
            var res = orig(self, type);
            if (type == PlacedObjectType.BounceRect)
                res = ObjectsPage.DevObjectCategories.Gameplay;
            return res;
        };
        On.DevInterface.ObjectsPage.CreateObjRep += (orig, self, tp, pObj) =>
        {
            if (tp == PlacedObjectType.BounceRect)
            {
                if (pObj is null)
                    self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                    {
                        pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                    });
                var pObjRep = new GridRectObjectRepresentation(self.owner, $"{nameof(PlacedObjectType.BounceRect)}_Rep", self, pObj, nameof(PlacedObjectType.BounceRect));
                self.tempNodes.Add(pObjRep);
                self.subNodes.Add(pObjRep);
            }
            else 
                orig(self, tp, pObj);
        };
        On.PlacedObject.GenerateEmptyData += (orig, self) =>
        {
            orig(self);
            if (self.type == PlacedObjectType.BounceRect) 
                self.data = new PlacedObject.GridRectObjectData(self);
        };
        On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
        {
            orig(self, newlyDisabledMods);
            for (var i = 0; i < newlyDisabledMods.Length; i++)
            {
                if (newlyDisabledMods[i].id == "lb-fgf-m4r-ik.bounce-rects")
                {
                    PlacedObjectType.UnregisterValues();
                    break;
                }
            }
        };
        On.Room.Loaded += (orig, self) =>
        {
            if (self.game is not RainWorldGame game || self.abstractRoom is not AbstractRoom rm)
                orig(self);
            else
            {
                orig(self);
                var objs = self.roomSettings.placedObjects;
                for (var rl = 1; rl <= 2; rl++)
                {
                    if (rl == 2 && self.warpPoints.Count > 0)
                        continue;
                    for (var i = 0; i < objs.Count; i++)
                    {
                        var pObj = objs[i];
                        if ((rl == 1 && pObj.deactivatedByWarpFilter) || (rl == 2 && !pObj.deactivatedByWarpFilter) || !pObj.active)
                            continue;
                        if (pObj.type == PlacedObjectType.BounceRect)
                            self.AddObject(new BounceRectObject(self, pObj));
                    }
                }
            }
        };
        On.PhysicalObject.Update += (orig, self, eu) =>
        {
            if (!BounceRectObject.DefaultBounce.TryGetValue(self, out _))
                BounceRectObject.DefaultBounce.Add(self, new(self.bounce));
            orig(self, eu);
        };
        On.Room.Update += (orig, self) =>
        {
            orig(self);
            if (!BounceRectObject.BounceRects.TryGetValue(self, out var rects))
                return;
            var phs = self.physicalObjects;
            for (var i = 0; i < phs.Length; i++)
            {
                var phsi = phs[i];
                for (var j = 0; j < phsi.Count; j++)
                {
                    var obj = phsi[j];
                    if (!(obj is Centipede c && !c.Small) && obj is not PoleMimic and not TentaclePlant and not GarbageWorm and not VoidSpawn and not Oracle and not SeedCob and not Vulture and not BigEel and not StowawayBug and not SkyWhale and not Deer and not SandGrub and not Loach and not BigMoth and not BoxWorm)
                    {
                        var ogBounce = BounceRectObject.DefaultBounce.TryGetValue(obj, out var box) ? box.Value : 0f;
                        var chs = obj.bodyChunks;
                        for (var k = 0; k < chs.Length; k++)
                        {
                            var chunk = chs[k];
                            Vector2 vector = chunk.ContactPoint.ToVector2(), pos = chunk.pos + vector * (chunk.rad + 30f);
                            var flag = false;
                            for (var l = 0; l < rects.Count; l++)
                            {
                                if (Custom.InsideRect(self.GetTilePosition(pos), rects[l].Rect))
                                {
                                    obj.bounce = box.Value + 1.5f;
                                    flag = true;
                                }
                            }
                            if (!flag)
                                obj.bounce = ogBounce;
                        }
                    }
                }
            }
        };
    }

    public void OnDisable()
    {
        BounceRectObject.BounceRects = null;
        BounceRectObject.DefaultBounce = null;
    }
}

public static class PlacedObjectType
{
    [AllowNull] public static PlacedObject.Type BounceRect = new(nameof(BounceRect), true);

    public static void UnregisterValues()
    {
        if (BounceRect is not null)
        {
            BounceRect.Unregister();
            BounceRect = null;
        }
    }
}