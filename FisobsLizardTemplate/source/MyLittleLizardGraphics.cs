using UnityEngine;
using LizardCosmetics;

namespace FisobsLizardTemplate;

public class MyLittleLizardGraphics : LizardGraphics
{
    public MyLittleLizardGraphics(MyLittleLizard ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        // add your cosmetics (some cosemtics might be already added), example:
        spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        Random.state = state;
    }
}