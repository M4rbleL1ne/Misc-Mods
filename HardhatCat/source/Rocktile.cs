using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBHardhatCat;

public sealed class Rocktile : Rock
{
    public Rocktile(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        bounce = 0f;
        firstChunk.rad = 10f;
        firstChunk.mass = .1f;
        collisionLayer = 1;
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj is Creature cr)
            cr.Violence(firstChunk, firstChunk.vel * firstChunk.mass, result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, .2f, 0f);
        return base.HitSomething(result, eu);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (collisionLayer != 1)
            ChangeCollisionLayer(1);
        canBeHitByWeapons = true;
        var fc = firstChunk;
        if (grabbedBy.Count > 0)
        {
            fc.collideWithObjects = fc.collideWithSlopes = fc.collideWithTerrain = false;
            fc.goThroughFloors = true;
        }
        else
        {
            fc.collideWithObjects = fc.collideWithSlopes = fc.collideWithTerrain = true;
            fc.goThroughFloors = false;
        }
    }

    public override void HitWall()
    {
        var fc = firstChunk;
        if (room.BeingViewed)
        {
            for (var i = 0; i < 7; i++)
                room.AddObject(new Spark(fc.pos + throwDir.ToVector2() * (fc.rad - 1f), Custom.DegToVec(Random.value * 360f) * 10f * Random.value + -throwDir.ToVector2() * 10f, Color.white, null, 2, 4));
        }
        room.ScreenMovement(fc.pos, throwDir.ToVector2() * 1.5f, 0f);
        room.PlaySound(SoundID.Rock_Hit_Creature, fc, false, 1f, .5f);
        SetRandomSpin();
        ChangeMode(Mode.Free);
        forbiddenToPlayer = 10;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        sLeaser.sprites = [new($"LBRocktile{Random.Range(1, 16)}")
        {
            anchorX = .5f,
            anchorY = .5f,
            shader = Custom.rainWorld.Shaders["AlphaLevelColor"]
        }];
        Random.state = state;
        AddToContainer(sLeaser, rCam);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer = null)
    {
        var s0 = sLeaser.sprites[0];
        s0.RemoveFromContainer();
        rCam.ReturnFContainer("Midground").AddChild(s0);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var s0 = sLeaser.sprites[0];
        s0.SetPosition(Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos);
        s0.rotation = Custom.VecToDeg(Vector2.Lerp(lastRotation, rotation, timeStacker));
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }
}