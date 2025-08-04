namespace SeerStuff;

public class SeerVoidSpawn : VoidSpawn
{
    public SeerVoidSpawn(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject, 0f, true)
    {
        firstChunk.collideWithSlopes = false;
        baseWindAffectiveness = 0f;
    }

    public override float windAffectiveness { get => 0f; set { } }

    public override float VisibilityBonus => -1f;

    public override bool SandstormImmune => true;

    public override float EffectiveRoomGravity => 0f;

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is not Room rm || !rm.BeingViewed)
            Destroy();
    }

    public override void Grabbed(Creature.Grasp grasp) => grasp?.Release();
}