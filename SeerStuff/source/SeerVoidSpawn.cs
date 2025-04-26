namespace SeerStuff;

public class SeerVoidSpawn(AbstractPhysicalObject abstractPhysicalObject) : VoidSpawn(abstractPhysicalObject, 0f, true)
{
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is not Room rm || !rm.BeingViewed)
            Destroy();
    }
}