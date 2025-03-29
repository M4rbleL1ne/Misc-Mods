using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;

namespace FisobsLizardTemplate;

public sealed class MyLittleLizardCritob : Critob
{
    internal MyLittleLizardCritob() : base(CreatureTemplateType.MyLittleLizard)
    {
        // icon, sprite & color: red 0-1, green 0-1, blue 0-1
        Icon = new SimpleIcon("Kill_Standard_Lizard", new(1f, 0f, 1f));
        // perf values, look in the RW code for references
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new(.5f, .5f);
        // unlock, score
        RegisterUnlock(KillScore.Configurable(7), SandboxUnlockID.MyLittleLizard);
    }

    public override int ExpeditionScore() => 7;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(1f, 0f, 1f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "myliz";

    // names used to spawn your lizard in the world file
    public override IEnumerable<string> WorldFileAliases() =>
    [
        "mylittlelizard",
        "my littlelizard",
        "mylittle lizard",
        "my little lizard"
    ];

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Lizards,
    ];

    // don't delete me
    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

    public override void EstablishRelationships()
    {
        var l = new Relationships(Type);
        // relationships, example:
        l.Rivals(Type, 1f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new MyLittleLizard(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    // don't delete me, otherwise fisobs will try to load a file it won't find
    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.GreenLizard;
}