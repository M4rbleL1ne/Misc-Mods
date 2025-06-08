using RWCustom;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MoonlitAcres;

public static class FestiveRot 
{
    public static Color EyeColor = new(0f, .6f, .3f), EffectColor = new(.8f, 0f, 0f);

    internal static void ApplyHooks() 
    {
        On.DaddyAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) =>
        {
            var res = orig(self, dRelation);
            if (self.daddy is DaddyLongLegs d && d.InHW() && d.IsValidDaddy() && dRelation.trackerRep?.representedCreature?.realizedCreature is DaddyLongLegs d2 && d2.InHW() && d2.IsValidDaddy())
                res = new(CreatureTemplate.Relationship.Type.Ignores, 0f);
            return res;
        };
        On.DaddyGraphics.Update += (orig, self) =>
        {
            if (self.daddy is DaddyLongLegs d && !d.slatedForDeletetion && d.IsValidDaddy())
            {
                if (d.room.IsHW() && !SantaHat.Hats.TryGetValue(self, out _))
                {
                    var hat = new SantaHat(self, self.BodySprite(0), Random.value * 360f, d.firstChunk.rad, false);
                    SantaHat.Hats.Add(self, hat);
                    d.room.AddObject(hat);
                }
                else if (!d.room.IsHW() && SantaHat.Hats.TryGetValue(self, out var hat))
                {
                    hat.Destroy();
                    SantaHat.Hats.Remove(self);
                }
            }
            orig(self);
        };
        On.RoomCamera.SpriteLeaser.CleanSpritesAndRemove += (orig, self) =>
        {
            orig(self);
            if (self.drawableObject is GraphicsModule gmod && SantaHat.Hats.TryGetValue(gmod, out var hat))
            {
                hat.Destroy();
                SantaHat.Hats.Remove(gmod);
            }
        };
        On.DaddyGraphics.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.daddy is DaddyLongLegs d && d.InHW() && d.IsValidDaddy() && SantaHat.Hats.TryGetValue(self, out var hat))
                hat.ParentDrawSprites(sLeaser);
        };
        On.DaddyLongLegs.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            if (abstractCreature.InHW() && self.IsValidDaddy())
            {
                self.eyeColor = EyeColor;
                self.effectColor = EffectColor;
            }
        };
        On.DaddyGraphics.DaddyTubeGraphic.ApplyPalette += (orig, self, sLeaser, rCam, palette) =>
        {
            orig(self, sLeaser, rCam, palette);
            if (self.owner?.owner is DaddyLongLegs d && d.InHW() && d.IsValidDaddy())
            {
                var sprites = sLeaser.sprites;
                var first = self.firstSprite;
                var verts = ((TriangleMesh)sprites[first]).verticeColors;
                for (var i = 0; i < verts.Length; i++)
                    verts[i] = palette.blackColor;
                var num = 0;
                var bumps = self.bumps;
                for (var j = 0; j < bumps.Length; j++)
                {
                    sprites[first + 1 + j].color = EffectColor;
                    if (bumps[j].eyeSize > 0f)
                    {
                        sprites[first + 1 + bumps.Length + num].color = EyeColor;
                        ++num;
                    }
                }
            }
        };
        On.DaddyGraphics.DaddyDangleTube.ApplyPalette += (orig, self, sLeaser, rCam, palette) =>
        {
            orig(self, sLeaser, rCam, palette);
            if (self.owner?.owner is DaddyLongLegs d && d.InHW() && d.IsValidDaddy())
            {
                var sprites = sLeaser.sprites;
                var first = self.firstSprite;
                var verts = ((TriangleMesh)sprites[first]).verticeColors;
                for (var i = 0; i < verts.Length; i++)
                    verts[i] = palette.blackColor;
                for (var j = 0; j < self.bumps.Length; j++)
                    sprites[first + 1 + j].color = EffectColor;
            }
        };
        On.ShortcutGraphics.ShortCutColor += (orig, self, crit, pos) =>
        {
            if (crit is DaddyLongLegs d && d.IsValidDaddy() && d.InHW())
                return EyeColor;
            return orig(self, crit, pos);
        };
        On.DaddyCorruption.ctor += (orig, self, room) =>
        {
            orig(self, room);
            if (room.IsHW() && !self.GWmode)
            {
                self.eyeColor = EyeColor;
                self.effectColor = EffectColor;
            }
        };
        On.DaddyCorruption.Update += (orig, self, eu) =>
        {
            orig(self, eu);
            if (self.room.IsHW() && !self.GWmode)
            {
                self.eyeColor = EyeColor;
                self.effectColor = EffectColor;
            }
        };
        On.DaddyCorruption.CorruptionTube.TubeGraphic.ApplyPalette += (On.DaddyCorruption.CorruptionTube.TubeGraphic.orig_ApplyPalette orig, DaddyCorruption.CorruptionTube.TubeGraphic self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) =>
        {
            orig(self, sLeaser, rCam, palette);
            if (self.owner is DaddyCorruption.CorruptionTube t && t.room.IsHW() && t.owner is DaddyCorruption d && !d.GWmode)
            {
                var sprites = sLeaser.sprites;
                var first = self.firstSprite;
                var verts = ((TriangleMesh)sprites[first]).verticeColors;
                for (var i = 0; i < verts.Length; i++)
                    verts[i] = palette.blackColor;
                var num = 0;
                var bumps = self.bumps;
                for (var j = 0; j < bumps.Length; j++)
                {
                    sprites[first + 1 + j].color = EffectColor;
                    if (bumps[j].eyeSize > 0f)
                    {
                        sprites[first + 1 + bumps.Length + num].color = EyeColor;
                        ++num;
                    }
                }
            }
        };
    }
}

public class SantaHat(GraphicsModule parent, int anchorSprite, float rotation, float headRadius, bool flipFlips) : UpdatableAndDeletable, IDrawable 
{
    public static ConditionalWeakTable<GraphicsModule, SantaHat> Hats = new();
    public Vector2 BaseSpritePos, TuftPos, LastTuftPos, TuftVel;
    public float BaseRotation;
    public bool FlipX, FlipY, First = true;

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new TriangleMesh("Futile_White",
            [
                new(0, 1, 2),
                new(1, 2, 3),
                new(2, 3, 4),
                new(3, 4, 5),
                new(4, 5, 6),
                new(5, 6, 7),
                new(6, 7, 8)
            ], false, false),
            new("JetFishEyeA"),
            new("LizardScaleA6")
        ];
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void ParentDrawSprites(RoomCamera.SpriteLeaser sLeaser) 
    {
        if (sLeaser.sprites.Length > anchorSprite) 
        {
            var spr = sLeaser.sprites[anchorSprite];
            BaseSpritePos = spr.GetPosition();
            BaseRotation = spr.rotation;
            if (flipFlips) 
            {
                FlipX = spr.scaleY > 0;
                FlipY = spr.scaleX < 0;
            }
            else 
            {
                FlipX = spr.scaleX > 0;
                FlipY = spr.scaleY < 0;
            }
        }
        if (First) 
        {
            First = false;
            TuftPos = BaseSpritePos;
            LastTuftPos = BaseSpritePos;
        }
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) 
    {
        var sprites = sLeaser.sprites;
        Vector2 drawPos = BaseSpritePos,
            upDir = new(Mathf.Cos((rotation + BaseRotation) * -Mathf.Deg2Rad), Mathf.Sin((rotation + BaseRotation) * -Mathf.Deg2Rad)),
            rightDir = -Custom.PerpendicularVector(upDir);
        if (FlipY)
            upDir *= -1;
        if (FlipX) 
            rightDir *= -1;
        drawPos += upDir * headRadius;
        var targetTuftPos = drawPos + upDir * 20f;
        // Rim
        var rim = sprites[2];
        rim.SetPosition(drawPos);
        rim.rotation = rotation + BaseRotation;
        rim.scaleY = FlipX ? -1f : 1f;
        // Tuft
        if (!Custom.DistLess(TuftPos, targetTuftPos, 20f)) {
            TuftPos = targetTuftPos + (TuftPos - targetTuftPos).normalized * 20f;
            if (!Custom.DistLess(LastTuftPos, TuftPos, 20f))
                LastTuftPos = TuftPos + (LastTuftPos - TuftPos).normalized * 20f;
        }
        // Cone
        sprites[1].SetPosition(Vector2.Lerp(LastTuftPos, TuftPos, timeStacker));
        var cone = (TriangleMesh)sprites[0];
        var coneTip = Vector2.Lerp(LastTuftPos, TuftPos, timeStacker);
        for (int i = 0, len = cone.vertices.Length; i < len; i++)
        {
            var r = i % 2 == 1;
            var h = i / 2 / (float)(len - 1) * 2f;
            Vector2 coneBase;
            if (r)
                coneBase = drawPos - rightDir * 7f;
            else
                coneBase = drawPos + rightDir * 7f;
            Vector2 coneMid = Vector2.Lerp(coneBase, targetTuftPos, .5f),
                pos = Vector2.Lerp(Vector2.Lerp(coneBase, coneMid, h), Vector2.Lerp(coneMid, coneTip, h), h);
            cone.MoveVertice(i, pos);
        }
        if (parent is not null && parent.culled && !parent.lastCulled)
        {
            for (var i = 0; i < sprites.Length; i++)
                sprites[i].isVisible = !parent.culled;
        }
        if (slatedForDeletetion || rCam.room != room || (parent?.owner is DaddyLongLegs d && room != d.room))
            sLeaser.CleanSpritesAndRemove();
    }

    public override void Update(bool eu) 
    {
        base.Update(eu);
        LastTuftPos = TuftPos;
        if (parent?.owner is not DaddyLongLegs d || d.slatedForDeletetion)
            Destroy();
        else if (d.room is not null) 
        {
            Vector2 tipPos = BaseSpritePos,
                upDir = new(Mathf.Cos((rotation + BaseRotation) * -Mathf.Deg2Rad), Mathf.Sin((rotation + BaseRotation) * -Mathf.Deg2Rad)),
                rightDir = -Custom.PerpendicularVector(upDir);
            if (FlipY)
                upDir *= -1;
            if (FlipX)
                rightDir *= -1;
            tipPos += upDir * 20f;
            TuftVel.y -= d.gravity;
            TuftVel += rightDir * ((Vector2.Dot(rightDir, TuftPos - tipPos) > 0) ? 1.5f : -1.5f);
            TuftVel += (tipPos - TuftPos) * .2f;
            TuftVel *= .6f;
            TuftPos += TuftVel;
            if (!Custom.DistLess(TuftPos, tipPos, 13f))
                TuftPos = tipPos + (TuftPos - tipPos).normalized * 13f;
        }
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
        {
            var s = sprites[i];
            s.RemoveFromContainer();
            newContainer.AddChild(s);
        }
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) 
    {
        var sprites = sLeaser.sprites;
        sprites[0].color = FestiveRot.EffectColor;
        sprites[2].color = sprites[1].color = Color.white;
    }
}