using BepInEx;
using BepInEx.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Permissions;
using RWCustom;
using System;
using HUD;
using Menu;
using SlugBase;
using SlugBase.Features;
using SlugBase.Assets;
using UnityEngine;
using Random = UnityEngine.Random;
using MoreSlugcats;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using OverseerHolograms;
using Expedition;
using System.Runtime.CompilerServices;
using MonoMod.RuntimeDetour;
using static System.Reflection.BindingFlags;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace SeerStuff;

[BepInPlugin(K_ID, nameof(SeerStuff), "1.0.0"), BepInDependency("slime-cubed.slugbase")]
public sealed class SeerStuffPlugin : BaseUnityPlugin
{
    [AllowNull] internal static ManualLogSource s_logger;
    const string K_ID = "lb-fgf-m4r-ik.seerstuff";
    static bool s_lateInit;
    [AllowNull] internal static SlugcatStats.Name s_seer;
    [AllowNull] public static DreamsState.DreamID SeerIntroDream = new(nameof(SeerIntroDream), true);
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType SeerSpawn = new(nameof(SeerSpawn), true);
    [AllowNull] public static ConditionalWeakTable<Region, EchoDirectionFinder> RegionEchoDirFinder = new();

    public void OnEnable()
    {
        s_logger = Logger;
        On.RainWorld.PostModsInit += On_RainWorld_PostModsInit;
        On.HUD.Map.ResetNotRevealedMarkers += On_Map_ResetNotRevealedMarkers;
        On.Room.Loaded += On_Room_Loaded;
        On.RainWorld.OnModsDisabled += On_RainWorld_OnModsDisabled;
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += On_MoonConversation_AddEvents;
        On.Player.Update += On_Player_Update;
        On.SaveState.ctor += On_SaveState_ctor;
        On.SLOrcacleState.ForceResetState += On_SLOrcacleState_ForceResetState;
        On.SLOracleBehaviorHasMark.PlayerHoldingSSNeuronsGreeting += On_SLOracleBehaviorHasMark_PlayerHoldingSSNeuronsGreeting;
        IL.AboveCloudsView.ctor += IL_AboveCloudsView_ctor;
        IL.AboveCloudsView.Update += IL_AboveCloudsView_Update;
        IL.RoofTopView.ctor += IL_RoofTopView_ctor;
        IL.RoofTopView.Update += IL_RoofTopView_Update;
        IL.OverseerCommunicationModule.ReevaluateConcern += IL_OverseerCommunicationModule_ReevaluateConcern;
        IL.OverseerAbstractAI.ctor += IL_OverseerAbstractAI_ctor;
        On.MiscWorldSaveData.FromString += On_MiscWorldSaveData_FromString;
        On.Overseer.TryAddHologram += On_Overseer_TryAddHologram;
        IL.OverseerTutorialBehavior.Update += IL_OverseerTutorialBehavior_Update;
        On.Region.GetProperRegionAcronym += On_Region_GetProperRegionAcronym;
        IL.ScavengerAbstractAI.InitGearUp += IL_ScavengerAbstractAI_InitGearUp;
        IL.WorldLoader.CreatingWorld += IL_WorldLoader_CreatingWorld;
        On.MoreSlugcats.FiltrationPowerController.ctor += On_FiltrationPowerController_ctor;
        On.Expedition.ChallengeTools.AppendAdditionalCreatureSpawns += On_ChallengeTools_AppendAdditionalCreatureSpawns;
        On.Expedition.VistaChallenge.ModifyVistaCandidates += On_VistaChallenge_ModifyVistaCandidates;
        IL.GlobalRain.DeathRain.NextDeathRainMode += IL_DeathRain_NextDeathRainMode;
        On.Player.SpitOutOfShortCut += On_Player_SpitOutOfShortCut;
        new Hook(typeof(RegionGate).GetMethod("get_MeetRequirement", Public | NonPublic | Instance | Static), On_RegionGate_get_MeetRequirement);
        new Hook(typeof(SaveState).GetMethod("get_CanSeeVoidSpawn", Public | NonPublic | Instance | Static), On_SaveState_get_CanSeeVoidSpawn);
        On.ElectricGate.Update += On_ElectricGate_Update;
        On.GateKarmaGlyph.DrawSprites += On_GateKarmaGlyph_DrawSprites;
        On.AbstractPhysicalObject.Realize += On_AbstractPhysicalObject_Realize;
    }

    static void On_AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);
        if (self.realizedObject is null && self.type == SeerSpawn)
            self.realizedObject = new SeerVoidSpawn(self);
    }

    static void On_GateKarmaGlyph_DrawSprites(On.GateKarmaGlyph.orig_DrawSprites orig, GateKarmaGlyph self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.slatedForDeletetion && self.room is Room room && room == rCam.room && room.game?.GetStorySession?.saveStateNumber == s_seer)
        {
            var nm = room.abstractRoom.name.ToUpperInvariant();
            if (nm.Contains("_SS") || nm.Contains("_RM"))
            {
                var sprs = sLeaser.sprites;
                for (var i = 0; i < sprs.Length; i++)
                    sprs[i].isVisible = false;
            }
        }
    }

    static void On_ElectricGate_Update(On.ElectricGate.orig_Update orig, ElectricGate self, bool eu)
    {
        orig(self, eu);
        if (self.room is Room room && room.game?.GetStorySession?.saveStateNumber == s_seer)
        {
            var nm = room.abstractRoom.name.ToUpperInvariant();
            if (nm.Contains("_SS") || nm.Contains("_RM"))
            {
                var lmps = self.lampsOn;
                if (lmps is not null)
                {
                    for (var i = 0; i < lmps.Length; i++)
                        lmps[i] = false;
                }
                self.batteryLeft = 0f;
            }
        }
    }

    static bool On_SaveState_get_CanSeeVoidSpawn(Func<SaveState, bool> orig, SaveState self) => orig(self) || self.saveStateNumber == s_seer;

    static bool On_RegionGate_get_MeetRequirement(Func<RegionGate, bool> orig, RegionGate self)
    {
        if (self.room is Room room && room.game?.GetStorySession?.saveStateNumber == s_seer)
        {
            var nm = room.abstractRoom.name.ToUpperInvariant();
            if (nm.Contains("_SS") || nm.Contains("_RM"))
                return false;
        }
        return orig(self);
    }
    
    static void On_Player_SpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        var changedRoom = self.room != newRoom;
        orig(self, pos, newRoom, spitOutAllSticks);
        if (changedRoom && newRoom is not null && newRoom.shortcutData(pos).shortCutType == ShortcutData.Type.RoomExit && self.SlugCatClass == s_seer && newRoom.world is World w && w.name is string s && w.game is RainWorldGame game && game.session is StoryGameSession session)
        {
            var index = s == "NP" ? w.GetAbstractRoom("NP_dustgarden")?.index : w.worldGhost?.ghostRoom?.index;
            if (index.HasValue && index.Value != newRoom.abstractRoom.index)
            {
                var ghostID = GhostWorldPresence.GetGhostID(s);
                if (!session.saveState.deathPersistentSaveData.ghostsTalkedTo.TryGetValue(ghostID, out var val) || val < 2)
                {
                    AbstractPhysicalObject abstractSpawn;
                    newRoom.abstractRoom.AddEntity(abstractSpawn = new(w, SeerSpawn, null, newRoom.GetWorldCoordinate(pos), game.GetNewID()));
                    abstractSpawn.RealizeInRoom();
                    var spawn = (abstractSpawn.realizedObject as SeerVoidSpawn)!;
                    spawn.voidMeltInRoom = newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt);
                    spawn.firstChunk.HardSetPosition(newRoom.MiddleOfTile(pos));
                    spawn.behavior = new FindEchoBehavior(spawn, newRoom, w, index.Value);
                }
            }
        }
    }

    static void IL_DeathRain_NextDeathRainMode(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, GlobalRain.DeathRain self) => flag || self.globalRain.game.GetStorySession.saveState.saveStateNumber == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook DeathRain.NextDeathRainMode!");
    }

    static void On_VistaChallenge_ModifyVistaCandidates(On.Expedition.VistaChallenge.orig_ModifyVistaCandidates orig, VistaChallenge self, VistaChallenge input)
    {
        orig(self, input);
        if (ModManager.MSC && input.room == "UW_C02" && ExpeditionData.slugcatPlayer == s_seer)
        {
            input.room = "UW_C02RIV";
            input.location = new(450f, 1170f);
            ExpLog.Log("Switch room to future version");
        }
    }

    static void On_ChallengeTools_AppendAdditionalCreatureSpawns(On.Expedition.ChallengeTools.orig_AppendAdditionalCreatureSpawns orig)
    {
        orig();
        if (ModManager.MSC && ChallengeTools.creatureSpawns.TryGetValue("Seer", out var dict))
            dict.Add(new()
            {
                creature = MoreSlugcatsEnums.CreatureTemplateType.BigJelly,
                points = ChallengeTools.creatureScores.TryGetValue(MoreSlugcatsEnums.CreatureTemplateType.BigJelly.value, out var value) ? value : 0,
                spawns = 2
            });
    }

    static void On_FiltrationPowerController_ctor(On.MoreSlugcats.FiltrationPowerController.orig_ctor orig, FiltrationPowerController self, World world)
    {
        orig(self, world);
        if (self.saveStateNumber == s_seer)
        {
            self.fullTurnOff = true;
            self.deadFilter = true;
            self.outtageIntensity = 1f;
        }
    }

    static void IL_WorldLoader_CreatingWorld(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, WorldLoader self) => flag || (self.game.session as StoryGameSession)!.saveStateNumber == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook WorldLoader.CreatingWorld!");
    }

    static void IL_ScavengerAbstractAI_InitGearUp(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, ScavengerAbstractAI self) => flag || (self.world.game.session as StoryGameSession)!.saveStateNumber == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook ScavengerAbstractAI.InitGearUp!");
    }

    static string On_Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
    {
        if (ModManager.MSC && character == s_seer && baseAcronym == "SS")
            return orig(character, "RM");
        return orig(character, baseAcronym);
    }

    static void IL_OverseerTutorialBehavior_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, OverseerTutorialBehavior self) => flag || self.overseer.abstractCreature.world.game.StoryCharacter == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook OverseerTutorialBehavior.Update!");
    }

    static void On_Overseer_TryAddHologram(On.Overseer.orig_TryAddHologram orig, Overseer self, OverseerHologram.Message message, Creature communicateWith, float importance)
    {
        if (ModManager.MSC && self.room is Room rm && rm.game.StoryCharacter == s_seer && message != OverseerHologram.Message.DangerousCreature && message != OverseerHologram.Message.Shelter && message != OverseerHologram.Message.ForcedDirection && message != OverseerHologram.Message.FoodObject && message != OverseerHologram.Message.ProgressionDirection && message != MoreSlugcatsEnums.OverseerHologramMessage.Advertisement)
            return;
        orig(self, message, communicateWith, importance);
    }

    static void On_MiscWorldSaveData_FromString(On.MiscWorldSaveData.orig_FromString orig, MiscWorldSaveData self, string s)
    {
        if (ModManager.MSC && self.saveStateNumber == s_seer)
        {
            self.moonHeartRestored = true;
            self.moonGivenRobe = true;
        }
        orig(self, s);
    }

    static void IL_OverseerAbstractAI_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, World world) => flag && world.game.StoryCharacter != s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook OverseerAbstractAI.ctor!");
    }

    static void IL_OverseerCommunicationModule_ReevaluateConcern(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, OverseerCommunicationModule self) => flag || (self.overseerAI.creature.world.game.session as StoryGameSession)!.saveState.saveStateNumber == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook OverseerCommunicationModule.ReevaluateConcern!");
    }

    static void IL_RoofTopView_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, RoofTopView self) => flag || self.room.game.GetStorySession?.saveStateNumber == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook RoofTopView.Update!");
    }

    static void IL_RoofTopView_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, Room room) => flag || room.game.GetStorySession?.saveStateNumber == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook RoofTopView.ctor!");
    }

    static void IL_AboveCloudsView_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, AboveCloudsView self) => flag || self.room.game.GetStorySession?.saveStateNumber == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook AboveCloudsView.Update!");
    }

    static void IL_AboveCloudsView_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Rivulet)),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, Room room) => flag || room.game.GetStorySession?.saveStateNumber == s_seer);
        }
        else
            s_logger.LogError("Couldn't ILHook AboveCloudsView.ctor!");
    }

    static void On_SLOracleBehaviorHasMark_PlayerHoldingSSNeuronsGreeting(On.SLOracleBehaviorHasMark.orig_PlayerHoldingSSNeuronsGreeting orig, SLOracleBehaviorHasMark self)
    {
        if (self.oracle.room.game.StoryCharacter == s_seer)
        {
            switch (self.State.neuronsLeft)
            {
                case 0:
                    return;
                case 1:
                    self.dialogBox.Interrupt("...", 40);
                    return;
                case 2:
                    self.dialogBox.Interrupt(self.Translate("...oh... to... save me?"), 20);
                    return;
                case 3:
                    self.dialogBox.Interrupt(self.Translate("You... brought that... for me?"), 20);
                    return;
            }
            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
            {
                self.dialogBox.Interrupt(self.Translate(Random.value < .5f ? "You are bringing a neuron. Is it to taunt me?" : "A neuron."), 30);
                return;
            }
            switch (Random.Range(0, 3))
            {
                case 0:
                    self.dialogBox.Interrupt(self.Translate("That... That is for me?"), 10);
                    break;
                case 1:
                    self.dialogBox.Interrupt(self.Translate("Hello" + (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes ? "!" : ".")), 10);
                    self.dialogBox.NewMessage(self.Translate("That... Oh, thank you."), 10);
                    break;
                default:
                    self.dialogBox.Interrupt(self.Translate("Ah... <PlayerName>, a neuron from Chasing Wind?"), 30);
                    break;
            }
        }
        else
            orig(self);
    }

    static void On_SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
    {
        orig(self, saveStateNumber, progression);
        if (saveStateNumber == s_seer && ModManager.MSC)
        {
            var data = self.miscWorldSaveData;
            data.moonGivenRobe = true;
            data.moonHeartRestored = true;
            data.pebblesEnergyTaken = true;
            data.pebblesRivuletPostgame = true;
        }
    }

    static void On_SLOrcacleState_ForceResetState(On.SLOrcacleState.orig_ForceResetState orig, SLOrcacleState self, SlugcatStats.Name saveStateNumber)
    {
        orig(self, saveStateNumber);
        if (saveStateNumber == s_seer)
            self.neuronsLeft = 7;
    }

    static void On_Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        if (self.SlugCatClass == s_seer && self.room is Room rm && rm.world is World w && w.game?.session is StoryGameSession session && w.name is string s)
        {
            if (string.IsNullOrEmpty(self.lastPingRegion))
                self.lastPingRegion = s;
            if (self.lastPingRegion != s && !rm.abstractRoom.gate)
            {
                self.lastPingRegion = s;
                if (World.CheckForRegionGhost(s_seer, s))
                {
                    var ghostID = GhostWorldPresence.GetGhostID(s);
                    if (!session.saveState.deathPersistentSaveData.ghostsTalkedTo.TryGetValue(ghostID, out var val) || val < 2)
                        rm.AddObject(new GhostPing(rm));
                }
            }
        }
        orig(self, eu);
    }

    static void On_MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        if (self.myBehavior?.oracle.room.game.StoryCharacter == s_seer)
        {
            if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
            {
                switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                {
                    case 0:
                        break;
                    case 1:
                        self.events.Add(new Conversation.TextEvent(self, 40, "...", 10));
                        break;
                    case 2:
                        self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Get... get away... glowy...yellow.... thing."), 10));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Please... thiss all I have left."), 10));
                        break;
                    case 3:
                        self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("You!"), 10));
                        self.events.Add(new Conversation.TextEvent(self, 60, self.Translate("...you ate... me. Please go away. I won't speak... to you.<LINE>I... CAN'T speak to you... because... you ate...me..."), 0));
                        break;
                    case 4:
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Oh so you've returned."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Come to take more from me? My memories, my thoughts... just something to fill your stomach?"), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Your predecessor was definitely more polite!"), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("No... never mind."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("It's useless to be angry at an animal following its instincts. Once, a single neuron meant nothing to me..."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I do wonder where you got that mark. Definitely not from around here."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("My neighbor, Five Pebbles, have become completely unresponsive."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("His can is showing increasing signs of decay with each passing cycle."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("At least the frequent outbursts of water have stabilized, but I'm not sure if this is a great indicator of his health."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("..."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I'm still angry at you, but it is good to have someone to talk to after all this time."), 0));
                        break;
                    case 5:
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Hello, little creature!"), 0));
                        if (self.State.playerEncounters > 0 && self.State.playerEncountersWithMark == 0)
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You came back!"), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("What brings you here?"), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I haven't received any visitors of your kind in a long time!"), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("The last one was very brave, but you probably are too to have come this far."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I do wonder where you got that mark. Definitely not from around here."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("My neighbor, Five Pebbles, has become completely unresponsive."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("His can is showing increasing signs of decay with each passing cycle."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("At least the frequent outbursts of water have stabilized, but I'm not sure if this is a great indicator of his health."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("This lull seems to have brought new life to this land. I would stay careful if I was you."), 0));
                        break;
                }
                return;
            }
            else if (self.id == Conversation.ID.MoonSecondPostMarkConversation)
            {
                switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                {
                    case 0:
                        break;
                    case 1:
                        self.events.Add(new Conversation.TextEvent(self, 40, "...", 10));
                        break;
                    case 2:
                        self.events.Add(new Conversation.TextEvent(self, 80, self.Translate("...leave..."), 10));
                        break;
                    case 3:
                        self.events.Add(new Conversation.TextEvent(self, 20, self.Translate("You..."), 10));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Please don't... take... more from me... Go."), 0));
                        break;
                    case 4:
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                        {
                            self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Oh. You."), 0));
                            break;
                        }
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                            self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Oh, hello! It's you again!"), 0));
                        else
                            self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Oh, hello! It's you again."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I appreciate the company, but I'm afraid I can't really help you in any way."), 0));
                        if (self.State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes)
                        {
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Eating one of my neuron definitely didn't help!<LINE>But I forgive you."), 0));
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I will continue following you around with my overseer, but stay cautious."), 0));
                        }
                        break;
                    case 5:
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                        {
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You again."), 10));
                            break;
                        }
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Oh, hello! It's you again!"), 10));
                        else
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Oh, hello! It's you again."), 10));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I appreciate the company, but I'm afraid I can't really help you in any way."), 0));
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                            break;
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I will continue following you around with my overseer, but stay cautious."), 0));
                        self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("You look like you're used to the world outside though.<LINE>You must have seen a lot."), 0));
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                        {
                            if (self.myBehavior.CheckSlugpupsInRoom())
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Good luck. You and your family are always welcome here."), 5));
                            else if (ModManager.MMF && self.myBehavior.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Good luck to you and your friend, <PlayerName>."), 5));
                            else
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Good luck. You can stay here a while if you want, but don't get caught in the rain!"), 5));
                        }
                        break;
                }
                return;
            }
            else if (self.id == Conversation.ID.MoonRecieveSwarmer || self.id?.value == "SL_CWNeuron")
            {
                if (self.myBehavior is not SLOracleBehaviorHasMark bhv)
                    return;
                if (self.State.neuronsLeft - 1 > 2 && bhv.respondToNeuronFromNoSpeakMode)
                {
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("You... Strange thing. Now this?"), 10));
                    self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I will accept your gift..."), 10));
                }
                switch (self.State.neuronsLeft)
                {
                    case 0 or 1:
                        break;
                    case 2:
                        self.events.Add(new Conversation.TextEvent(self, 40, "...", 10));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You!"), 10));
                        self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("...you...killed..."), 10));
                        self.events.Add(new Conversation.TextEvent(self, 0, "...", 10));
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("...me"), 10));
                        break;
                    case 3:
                        self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("...thank you... better..."), 10));
                        self.events.Add(new Conversation.TextEvent(self, 20, self.Translate("still, very... bad."), 10));
                        break;
                    case 4:
                        self.events.Add(new Conversation.TextEvent(self, 20, self.Translate("Thank you... That is a little better. Thank you, creature."), 10));
                        if (!bhv.respondToNeuronFromNoSpeakMode)
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Where does this neuron come from? My nearest functional neighbor is so... far."), 0));
                        break;
                    default:
                        if (bhv.respondToNeuronFromNoSpeakMode)
                        {
                            self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Thank you. I do wonder what you want."), 10));
                            break;
                        }
                        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I am grateful - the relief is indescribable!"), 10));
                        self.events.Add(new Conversation.TextEvent(self, 0, "...", 10));
                        if (self.State.neuronGiveConversationCounter == 0)
                        {
                            if (self.State.neuronsLeft == 5)
                            {
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I could read a bit of Chasing Wind in this neuron before formatting it.<LINE>What a weird bundle of ideas this neuron was carrying. I don't think I understood any of it."), 10));
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("We were supposed to help everyone, you know. Everything.<LINE>That was our purpose: a great gift to the lesser beings of the world.<LINE>When facing our inability to do so, we all reacted differently.<LINE>Many with madness."), 10));
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Some, out there, might still be trying.<LINE>Communications have been bad for a long time, and by now I suspect most of us are isolated like me, or connected only in small groups."), 10));
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("But even back when we were all more or less connected there were those who reacted to their task with anger.<LINE>I can only imagine they are angrier now, alone in their cans, left only with their insatiable drive."), 10));
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("But to be honest, I don't know how many of us are still alive at this point.<LINE>Knowing Grey Wind is still out here is reassuring though."), 10));
                            }
                            else
                            {
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("I could read a bit of Chasing Wind in this neuron before formatting it.<LINE>What a weird bundle of ideas this neuron was carrying. I don't think I understood any of it."), 10));
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("We were supposed to help everyone, you know. Everything.<LINE>That was our purpose: a great gift to the lesser beings of the world.<LINE>When facing our inability to do so, we all reacted differently.<LINE>Many with madness."), 10));
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Some, out there, might still be trying.<LINE>Communications have been bad for a long time, and by now I suspect most of us are isolated like me, or connected only in small groups."), 10));
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("But even back when we were all more or less connected there were those who reacted to their task with anger.<LINE>I can only imagine they are angrier now, alone in their cans, left only with their insatiable drive."), 10));
                                self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("But to be honest, I don't know how many of us are still alive at this point.<LINE>Knowing Grey Wind is still out here is reassuring though."), 10));
                            }
                        }
                        else if (self.State.neuronGiveConversationCounter == 1)
                        {
                            self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("You brought these all the way from Chasing Wind?<LINE>Thank you so much."), 10));
                            self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("I deeply appreciate your efforts."), 0));
                            self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("I hope they don't mind.<LINE>I have no way of knowing their current state.<LINE>...<LINE>Thank you, little creature."), 0));
                        }
                        else
                        {
                            switch (Random.Range(0, 4))
                            {
                                case 0:
                                    self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Thank you, again. I feel wonderful."), 10));
                                    break;
                                case 1:
                                    self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Thank you so very much!"), 10));
                                    break;
                                case 2:
                                    self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("It is strange... I'm remembering myself, but also... them."), 10));
                                    break;
                                default:
                                    self.events.Add(new Conversation.TextEvent(self, 30, self.Translate("Thank you... Sincerely."), 10));
                                    break;
                            }
                        }
                        ++self.State.neuronGiveConversationCounter;
                        break;
                }
                bhv.respondToNeuronFromNoSpeakMode = false;
                return;
            }
        }
        orig(self);
    }

    static void On_RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        for (var i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == K_ID)
            {
                SeerIntroDream?.Unregister();
                SeerIntroDream = null;
                SeerSpawn?.Unregister();
                SeerSpawn = null;
                break;
            }
        }
    }

    static void On_Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        var firstTimeR = self.abstractRoom.firstTimeRealized;
        orig(self);
        if (firstTimeR && self.game is RainWorldGame game && game.session is StoryGameSession sess && sess.saveState.cycleNumber == 0 && sess.saveStateNumber == s_seer && SlugBaseCharacter.TryGet(s_seer, out var slug) && GameFeatures.StartRoom.TryGet(slug, out var strs) && Array.IndexOf(strs, self.abstractRoom.name) >= 0)
        {
            var pls = game.Players;
            for (var i = 0; i < pls.Count; i++)
            {
                var player = pls[i];
                var lantern = new AbstractConsumable(self.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, player.pos, game.GetNewID(), self.abstractRoom.index, -1, null) { isConsumed = false };
                self.abstractRoom.AddEntity(lantern);
                new AbstractPhysicalObject.CreatureGripStick(player, lantern, 0, true);
            }
            CustomDreams.QueueDream(sess, SeerIntroDream);
        }
    }

    static void On_Map_ResetNotRevealedMarkers(On.HUD.Map.orig_ResetNotRevealedMarkers orig, Map self)
    {
        orig(self);
        if (self.hud is not HUD.HUD hud || hud.owner is not IOwnAHUD hudOwner)
            return;
        SaveState? saveState = null;
        var ownerType = hudOwner.GetOwnerType();
        if (ownerType == HUD.HUD.OwnerType.Player || ownerType == HUD.HUD.OwnerType.FastTravelScreen)
            saveState = hud.rainWorld.progression.currentSaveState;
        else if (ownerType == HUD.HUD.OwnerType.DeathScreen || ownerType == HUD.HUD.OwnerType.SleepScreen)
            saveState = (hudOwner as SleepAndDeathScreen)!.saveState;
        if (saveState?.saveStateNumber == s_seer)
        {
            var unRevs = self.notRevealedFadeMarkers;
            for (var num = unRevs.Count - 1; num >= 0; num--)
            {
                var mapObj = unRevs[num];
                if (mapObj is Map.ShelterMarker sh)
                {
                    sh.FadeIn(30 + unRevs.Count);
                    unRevs.RemoveAt(num);
                }
            }
        }
    }

    static void On_RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        if (!s_lateInit)
        {
            CustomDreams.SetDreamScene(SeerIntroDream, new("SeerIntroDream"));
            s_seer = new("Seer");
            On.GhostWorldPresence.SpawnGhost += On_GhostWorldPresence_SpawnGhost;
            s_lateInit = true;
        }
    }

    static bool On_GhostWorldPresence_SpawnGhost(On.GhostWorldPresence.orig_SpawnGhost orig, GhostWorldPresence.GhostID ghostID, int karma, int karmaCap, int ghostPreviouslyEncountered, bool playingAsRed)
    {
        var res = orig(ghostID, karma, karmaCap, ghostPreviouslyEncountered, playingAsRed);
        if (!res && !Custom.rainWorld.safariMode && (!ModManager.Expedition || !Custom.rainWorld.ExpeditionMode || Custom.rainWorld.progression.currentSaveState.cycleNumber != 0) && Custom.rainWorld.progression.currentSaveState.saveStateNumber == s_seer)
            return (ghostID?.value is "NP" ? ghostPreviouslyEncountered == 1 : ghostPreviouslyEncountered < 2) && karma >= karmaCap;
        return res;
    }

    public void OnDisable()
    {
        s_logger = null;
        s_seer = null;
        RegionEchoDirFinder = null;
    }
}