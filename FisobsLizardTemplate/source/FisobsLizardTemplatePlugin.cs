using BepInEx;
using System.Security.Permissions;
using System.Security;
using BepInEx.Logging;
using Fisobs.Core;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using Random = UnityEngine.Random;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace FisobsLizardTemplate;

[BepInPlugin(K_ID, "Fisobs Lizard Template", "1.0.0"), BepInDependency("io.github.dual.fisobs")]
public sealed class FisobsLizardTemplatePlugin : BaseUnityPlugin
{
    // your ID must be unique, replace it with your own thing, do not keep lb-fgf-m4r-ik please
    internal const string K_ID = "lb-fgf-m4r-ik.fisobs-lizard-template";
    // to send messages to logOutput.log
    [AllowNull] internal static ManualLogSource s_logger;

    // initialize your hooks and register your critob
    public void OnEnable()
    {
        s_logger = Logger;
        // now you can use s_logger
        On.RainWorld.OnModsDisabled += On_RainWorld_OnModsDisabled;
        IL.OverseerAbstractAI.HowInterestingIsCreature += IL_OverseerAbstractAI_HowInterestingIsCreature;
        On.LizardVoice.GetMyVoiceTrigger += On_LizardVoice_GetMyVoiceTrigger;
        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
        IL.AbstractCreature.InitiateAI += IL_AbstractCreature_InitiateAI;
        On.AbstractCreature.MSCInitiateAI += On_AbstractCreature_MSCInitiateAI;
        Content.Register(new MyLittleLizardCritob());
    }

    // don't delete me
    public void OnDisable() => s_logger = null!;

    // fixes a bug with fisobs
    internal static void IL_AbstractCreature_InitiateAI(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<CreatureTemplate.Type>("Slugcat"),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, AbstractCreature self) => flag || self.creatureTemplate.type == CreatureTemplateType.MyLittleLizard);
        }
        else
            s_logger.LogError("Couldn't ILHook AbstractCreature.InitiateAI!");
    }

    // fixes a bug with fisobs
    internal static void On_AbstractCreature_MSCInitiateAI(On.AbstractCreature.orig_MSCInitiateAI orig, AbstractCreature self)
    {
        if (self.creatureTemplate.type != CreatureTemplateType.MyLittleLizard)
            orig(self);
    }

    // template and breed parameters
    internal static CreatureTemplate On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
    {
        if (type == CreatureTemplateType.MyLittleLizard)
        {
            // you can change the base here
            var temp = orig(CreatureTemplate.Type.GreenLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            // don't delete what's below
            var breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            temp.name = nameof(CreatureTemplateType.MyLittleLizard);
            temp.type = type;
            temp.requireAImap = true;
            temp.doPreBakedPathing = false;
            temp.preBakedPathingAncestor = greenTemplate;
            return temp;
        }
        return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
    }


    // voice, check what voice names exist in SoundID
    internal static SoundID On_LizardVoice_GetMyVoiceTrigger(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
    {
        var res = orig(self);
        if (self.lizard is MyLittleLizard l)
        {
            var array = new[] { "A", "B", "C", "D", "E" };
            var list = new List<SoundID>();
            for (var i = 0; i < array.Length; i++)
            {
                var soundID = new SoundID("Lizard_Voice_Pink_" + array[i]);
                if (soundID.Index != -1 && l.abstractPhysicalObject.world.game.soundLoader.workingTriggers[soundID.Index])
                    list.Add(soundID);
            }
            if (list.Count == 0)
                res = SoundID.None;
            else
                res = list[Random.Range(0, list.Count)];
        }
        return res;
    }

    // unregistering enums (not really required)
    internal static void On_RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        for (var i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == K_ID)
            {
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.MyLittleLizard))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.MyLittleLizard);
                SandboxUnlockID.UnregisterValues();
                CreatureTemplateType.UnregisterValues();
                break;
            }
        }
    }

    // overseer reaction
    internal static void IL_OverseerAbstractAI_HowInterestingIsCreature(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("BlackLizard"),
            x => x.MatchCall(out _),
            x => x.MatchBrtrue(out label)))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((AbstractCreature testCrit) => testCrit.creatureTemplate.type == CreatureTemplateType.MyLittleLizard);
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            s_logger.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature!");
    }

    // texture loading
    /*
    internal static void On_RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            // for atlases
            if (!Futile.atlasManager.DoesContainAtlas("MyCustomAtlas"))
                Futile.atlasManager.LoadAtlas("atlases/MyCustomAtlas");
            // for a single picture
            if (!Futile.atlasManager.DoesContainAtlas("MyCustomImage"))
                Futile.atlasManager.ActuallyLoadAtlasOrImage("MyCustomImage", "atlases/MyCustomImage" + Futile.resourceSuffix, string.Empty);
        }
        catch (Exception e)
        {
            s_logger.LogError("Exception while loading atlases: " + e);
        }
    }*/

    // texture unloading
    /*
    internal static void On_RainWorld_UnloadResources(On.RainWorld.orig_UnloadResources orig, RainWorld self)
    {
        orig(self);
        // for atlases
        if (Futile.atlasManager.DoesContainAtlas("MyCustomAtlas"))
            Futile.atlasManager.UnloadAtlas("MyCustomAtlas");
        // for a single picture
        if (Futile.atlasManager.DoesContainAtlas("MyCustomImage"))
            Futile.atlasManager.UnloadAtlas("MyCustomImage");
    }*/
}