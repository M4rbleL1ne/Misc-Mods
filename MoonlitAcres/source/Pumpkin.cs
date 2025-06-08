using RWCustom;
using UnityEngine;

namespace MoonlitAcres;

public class Pumpkin : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public Vector2 Rotation, LastRotation;
    public Color Color2;
    public Vector2? SetRotation;

    public virtual int BitesLeft { get; set; } = 3;

    public virtual int FoodPoints => 2;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public virtual AbstractConsumable AbsCons => (abstractPhysicalObject as AbstractConsumable)!;

    public Pumpkin(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        //Stripped from DangleFruit
        bodyChunks = [new(this, 0, default, 8f, .9f)];
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = .2f;
        surfaceFriction = .7f;
        collisionLayer = 1;
        waterFriction = .95f;
        buoyancy = 1.1f;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        Rotation = Custom.RNV();
        LastRotation = Rotation;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var fc = firstChunk;
        if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            fc.vel += Custom.DirVec(fc.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
        LastRotation = Rotation;
        if (grabbedBy.Count > 0)
        {
            if (!AbsCons.isConsumed)
                AbsCons.Consume();
            Rotation = Custom.PerpendicularVector(Custom.DirVec(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            Rotation.y = Mathf.Abs(Rotation.y);
        }
        if (SetRotation is Vector2 rot)
        {
            Rotation = rot;
            SetRotation = null;
        }
        if (fc.ContactPoint.y < 0)
        {
            Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fc.vel.x).normalized;
            fc.vel.x *= .8f;
        }
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [new("pumpkin2"), new("pumpkin2Grad1"), new("pumpkin2Grad2")];
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos;
        var rot = Custom.VecToDeg(Vector3.Slerp(LastRotation, Rotation, timeStacker));
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
        {
            var s = sprites[i];
            s.SetPosition(pos);
            s.rotation = rot;
            if (i < 2 && BitesLeft is not 0 and not 3)
                s.element = Futile.atlasManager.GetElementWithName((i == 0 ? "pumpkin2bit" : "pumpkin2bitGrad1") + (3 - BitesLeft));
            if (i > 0)
            {
                if (blink > 0 && Random.value < .5f)
                    s.color = blinkColor;
                else
                    s.color = i == 2 ? Color2 : color;
            }
        }
        if (!slatedForDeletetion && room == rCam.room)
            return;
        sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var sprites = sLeaser.sprites;
        sprites[0].color = palette.blackColor;
        color = Color.Lerp(new(155f / 255f, 56f / 255f, 0f), palette.blackColor, palette.darkness * .2f);
        Color2 = Color.Lerp(new(0f, 55f / 255f, 15f / 255f), palette.blackColor, palette.darkness * .2f);
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer) 
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        var sprites = sLeaser.sprites;
        for (var i = 0; i < sprites.Length; i++)
        {
            var sprite = sprites[i];
            sprite.RemoveFromContainer();
            newContainer.AddChild(sprite);
        }
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        --BitesLeft;
        room.PlaySound(BitesLeft != 0 ? SoundID.Slugcat_Bite_Dangle_Fruit : SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk.pos);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (BitesLeft >= 1)
            return;
        if (grasp.grabber is Player p)
            p.ObjectEaten(this);
        grasp.Release();
        Destroy();
    }

    public virtual void ThrowByPlayer() { }
}