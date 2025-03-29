using RWCustom;
using UnityEngine;

namespace FisobsLizardTemplate;

public class MyLittleLizard : Lizard
{
    public MyLittleLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        // effect color, (hue, saturation, lightness)
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.87f, .1f, .6f), 1f, Custom.ClampedRandomVariation(.5f, .15f, .1f));
        Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new MyLittleLizardGraphics(this);

    // not needed but just skips some useless code
    public override void LoseAllGrasps() => ReleaseGrasp(0);
}