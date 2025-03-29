using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBHardhatCat;

public sealed class Runtile : PlayerCarryableItem, IDrawable
{
    public enum TileMode
    {
        Free,
        Grabbed,
        Attached,
        Destroyed,
        TileDestroyed
    }

    public Room.Tile? Tile;
    public Room.Tile.TerrainType OrigTerrain;
    Vector2 _lastRotation, _rotation;
    public float Scale;
    float _lastScale;
    int _life;
    public TileMode Mode;
    bool _spawned;

    public Runtile(AbstractPhysicalObject abstractPhysicalObject, Vector2 pos) : base(abstractPhysicalObject)
    {
        bodyChunks = [new(this, 0, pos, .1f, .1f) { loudness = 9f }];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = 0f;
        surfaceFriction = .4f;
        collisionLayer = 1;
        waterFriction = .98f;
        buoyancy = 0f;
    }

    public override float ThrowPowerFactor => .2f;

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        if (!_spawned)
        {
            _lastScale = Scale = 0f;
            _life = 500;
            placeRoom.PlaySound(SoundID.Gate_Clamp_Collision, firstChunk.pos, 1f, 1f + (Random.value - Random.value) * .1f);
            Mode = grabbedBy.Count > 0 ? TileMode.Grabbed : TileMode.Free;
            _spawned = true;
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (collisionLayer != 1)
            ChangeCollisionLayer(1);
        if (room is null || !_spawned)
            return;
        blink = 0;
        color = Color.white;
        _lastScale = Scale;
        _lastRotation = _rotation;
        firstChunk.rad = Math.Max(.1f, Scale * 10f);
        switch (Mode)
        {
            case TileMode.Free:
                if (Scale < 1f)
                    Scale = Mathf.Lerp(Scale, 1f, .05f);
                canBeHitByWeapons = firstChunk.collideWithObjects = firstChunk.collideWithSlopes = firstChunk.collideWithTerrain = true;
                firstChunk.goThroughFloors = false;
                if (_life > 0 && room.BeingViewed)
                {
                    if (grabbedBy.Count > 0)
                        Mode = TileMode.Grabbed;
                    else
                        --_life;
                }
                else
                {
                    room.PlaySound(SoundID.Gate_Clamp_Collision, firstChunk.pos, 1f, 1f + (Random.value - Random.value) * .1f);
                    Mode = TileMode.Destroyed;
                }
                if (firstChunk.ContactPoint.y < 0)
                {
                    _rotation = (_rotation - Custom.PerpendicularVector(_rotation) * .1f * firstChunk.vel.x).normalized;
                    firstChunk.vel.x *= .8f;
                }
                break;
            case TileMode.Grabbed:
                if (Scale < 1f)
                    Scale = Mathf.Lerp(Scale, 1f, .05f);
                firstChunk.collideWithObjects = firstChunk.collideWithSlopes = firstChunk.collideWithTerrain = false;
                firstChunk.goThroughFloors = canBeHitByWeapons = true;
                _life = 500;
                if (grabbedBy.Count > 0)
                {
                    _rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                    _rotation.y = Mathf.Abs(_rotation.y);
                }
                else
                    Mode = TileMode.Free;
                if (firstChunk.ContactPoint.y < 0)
                {
                    _rotation = (_rotation - Custom.PerpendicularVector(_rotation) * .1f * firstChunk.vel.x).normalized;
                    firstChunk.vel.x *= .8f;
                }
                break;
            case TileMode.Attached:
                if (Scale < 1f)
                    Scale = Mathf.Lerp(Scale, 1f, .05f);
                _rotation = default;
                canBeHitByWeapons = firstChunk.collideWithObjects = firstChunk.collideWithSlopes = firstChunk.collideWithTerrain = false;
                firstChunk.goThroughFloors = true;
                if (Tile is not null)
                {
                    firstChunk.HardSetPosition(room.MiddleOfTile(new IntVector2(Tile.X, Tile.Y)));
                    Tile.Terrain = Room.Tile.TerrainType.Solid;
                }
                if (_life > 0 && room.BeingViewed)
                    --_life;
                else
                {
                    if (Tile is not null)
                    {
                        if (LBHardhatCatPlugin.s_falseTiles.TryGetValue(Tile, out _))
                            LBHardhatCatPlugin.s_falseTiles.Remove(Tile);
                        Tile.Terrain = OrigTerrain;
                    }
                    room.PlaySound(SoundID.Gate_Clamp_Collision, firstChunk.pos, 1f, 1f + (Random.value - Random.value) * .1f);
                    Mode = TileMode.TileDestroyed;
                }
                break;
            case TileMode.Destroyed:
                canBeHitByWeapons = false;
                firstChunk.collideWithObjects = firstChunk.collideWithSlopes = firstChunk.collideWithTerrain = grabbedBy.Count == 0;
                firstChunk.goThroughFloors = grabbedBy.Count > 0;
                if (Scale >= .1f)
                    Scale = Mathf.Lerp(Scale, 0f, .05f);
                else
                    Destroy();
                break;
            case TileMode.TileDestroyed:
                _rotation = default;
                firstChunk.goThroughFloors = canBeHitByWeapons = firstChunk.collideWithObjects = firstChunk.collideWithSlopes = firstChunk.collideWithTerrain = false;
                if (Tile is not null)
                    firstChunk.pos = room.MiddleOfTile(new IntVector2(Tile.X, Tile.Y));
                if (Scale >= .1f)
                    Scale = Mathf.Lerp(Scale, 0f, .05f);
                else
                    Destroy();
                break;
        }
    }

    public override void Destroy()
    {
        if (Tile is not null)
            Tile.Terrain = OrigTerrain;
        base.Destroy();
    }

    public override void PickedUp(Creature upPicker) => room.PlaySound(SoundID.Slugcat_Pick_Up_Rock, firstChunk);

    void IDrawable.InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        sLeaser.sprites = [new($"LBRuntile{Random.Range(1, 45)}")
        {
            anchorX = .5f,
            anchorY = .5f,
            shader = Custom.rainWorld.Shaders["AlphaLevelColor"]
        }];
        Random.state = state;
        AddToContainer(sLeaser, rCam);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer = null)
    {
        var s0 = sLeaser.sprites[0];
        s0.RemoveFromContainer();
        rCam.ReturnFContainer("Midground").AddChild(s0);
    }

    void IDrawable.ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

    void IDrawable.DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var s0 = sLeaser.sprites[0];
        s0.rotation = Custom.VecToDeg(Vector3.Slerp(_lastRotation, _rotation, timeStacker));
        s0.SetPosition(Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos);
        s0.scale = Mathf.Lerp(_lastScale, Scale, timeStacker);
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }
}