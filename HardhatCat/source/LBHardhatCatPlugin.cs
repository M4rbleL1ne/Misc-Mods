using BepInEx;
using BepInEx.Logging;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using static System.Reflection.BindingFlags;
using UnityEngine;
using static Mono.Cecil.Cil.OpCodes;
using Random = UnityEngine.Random;
using Menu;
using HUD;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace LBHardhatCat;

[BepInPlugin("lb-fgf-m4r-ik.hardhat-cat", "LBHardhatCat", "10.0.1"), BepInDependency("slime-cubed.slugbase"), BepInDependency("rwmodding.coreorg.rk")]
public sealed class LBHardhatCatPlugin : BaseUnityPlugin
{
    internal static ManualLogSource? s_logger;
    internal static ConditionalWeakTable<PlayerGraphics, FSprite> s_hardHat = new();
    internal static ConditionalWeakTable<Room.Tile, FalseTile> s_falseTiles = new();
    internal static FalseTile s_falseTile = new();
    static bool s_lateInit;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class FalseTile { }

    public void OnEnable()
    {
        s_logger = Logger;
        On.MultiplayerUnlocks.ClassUnlocked += On_MultiplayerUnlocks_ClassUnlocked;
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += On_SLOracleBehaviorHasMark_TypeOfMiscItem;
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += On_MoonConversation_AddEvents;
        On.ItemSymbol.SpriteNameForItem += On_ItemSymbol_SpriteNameForItem;
        On.RainWorld.OnModsInit += On_RainWorld_OnModsInit;
        On.RainWorld.UnloadResources += On_RainWorld_UnloadResources; 
        On.RainWorld.OnModsDisabled += On_RainWorld_OnModsDisabled;
        IL.Menu.MenuScene.BuildScene += IL_MenuScene_BuildScene;
        IL.Menu.SlideShow.ctor += IL_SlideShow_ctor;
        On.Room.Loaded += On_Room_Loaded;
        On.ShortcutHelper.ctor += On_ShortcutHelper_ctor;
        IL.PlayerGraphics.Update += IL_PlayerGraphics_Update;
        On.Player.Grabability += On_Player_Grabability;
        IL.Player.GrabUpdate += IL_Player_GrabUpdate;
        IL.Spear.Update += IL_Spear_Update;
        On.TubeWorm.Tongue.Update += On_Tongue_Update;
        On.Player.Tongue.Update += On_Tongue_Update;
        IL.Player.TerrainImpact += IL_Player_TerrainImpact;
        On.Player.CanBeSwallowed += On_Player_CanBeSwallowed;
        On.Player.Regurgitate += On_Player_Regurgitate;
        On.Player.TossObject += On_Player_TossObject;
        new Hook(typeof(SaveState).GetMethod("get_CanSeeVoidSpawn", Public | NonPublic | Instance | Static), On_SaveState_get_CanSeeVoidSpawn);
        On.ElectricGate.Update += On_ElectricGate_Update;
        On.GateKarmaGlyph.DrawSprites += On_GateKarmaGlyph_DrawSprites;
        On.PlayerGraphics.InitiateSprites += On_PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.AddToContainer += On_PlayerGraphics_AddToContainer;
        On.PlayerGraphics.DrawSprites += On_PlayerGraphics_DrawSprites;
        On.Region.ctor_string_int_int_Timeline += On_Region_ctor_string_int_int_Timeline;
        On.AbstractPhysicalObject.Realize += On_AbstractPhysicalObject_Realize;
        On.PlayerGraphics.ApplyPalette += On_PlayerGraphics_ApplyPalette;
        On.RainWorld.PostModsInit += On_RainWorld_PostModsInit;
    }

    // late hook for compat
    static void On_RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        if (!s_lateInit)
        {
            s_lateInit = true;
            new Hook(typeof(RegionGate).GetMethod("get_MeetRequirement", Public | NonPublic | Instance | Static), On_RegionGate_get_MeetRequirement);
        }
    }

    static SLOracleBehaviorHasMark.MiscItemType On_SLOracleBehaviorHasMark_TypeOfMiscItem(On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark self, PhysicalObject testItem)
    {
        if (testItem is Rocktile)
            return MiscItemType.LBHardhatCatRocktile;
        if (testItem is Runtile)
            return MiscItemType.LBHardhatCatRuntile;
        return orig(self, testItem);
    }

    static void On_MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        orig(self);
        if (self.id == Conversation.ID.Moon_Misc_Item)
        {
            if (self.describeItem == MiscItemType.LBHardhatCatRocktile)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a strange object.<LINE>It seems large enough to hurt a creature."), 0));
            else if (self.describeItem == MiscItemType.LBHardhatCatRuntile)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a piece of wall.<LINE>How did you manage to take it?"), 0));
        }
    }

    static string On_ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        if (itemType == AbstractPhysicalObjectType.LBHardhatCatRocktile)
            return "Symbol_LBHardhatCatRocktile";
        if (itemType == AbstractPhysicalObjectType.LBHardhatCatRuntile)
            return "Symbol_LBHardhatCatRuntile";
        return orig(itemType, intData);
    }

    static bool On_MultiplayerUnlocks_ClassUnlocked(On.MultiplayerUnlocks.orig_ClassUnlocked orig, MultiplayerUnlocks self, SlugcatStats.Name classID)
    {
        if (classID?.value == "LBHardhatCat")
            return self.progression.miscProgressionData.GetTokenCollected(SlugcatUnlockID.LBHardhatCat);
        return orig(self, classID);
    }

    static void On_RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        _ = SlugcatUnlockID.LBHardhatCat;
        _ = AbstractPhysicalObjectType.LBHardhatCatRuntile;
        _ = MiscItemType.LBHardhatCatRuntile;
        if (!Futile.atlasManager.DoesContainAtlas("lbhardhatcatspr"))
            Futile.atlasManager.LoadAtlas("atlases/lbhardhatcatspr");
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LBHardhatCatRocktile))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.LBHardhatCatRocktile);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LBHardhatCatRuntile))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.LBHardhatCatRuntile);
    }

    static void On_RainWorld_UnloadResources(On.RainWorld.orig_UnloadResources orig, RainWorld self)
    {
        orig(self);
        if (Futile.atlasManager.DoesContainAtlas("lbhardhatcatspr"))
            Futile.atlasManager.UnloadAtlas("lbhardhatcatspr");
    }

    static void On_RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        for (var i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == "lb-fgf-m4r-ik.hardhat-cat")
            {
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LBHardhatCatRocktile))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.LBHardhatCatRocktile);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LBHardhatCatRuntile))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.LBHardhatCatRuntile);
                SandboxUnlockID.UnregisterValues();
                MiscItemType.UnregisterValues();
                SlugcatUnlockID.UnregisterValues();
                AbstractPhysicalObjectType.UnregisterValues();
                break;
            }
        }
    }

    static void IL_MenuScene_BuildScene(ILContext il)
    {
        var c = new ILCursor(il);
        string[] strs = ["Outro 1 - Left Swim - Flat", "6 - SlugcatsA", "5 - MainSlugcat", "Outro 2 - Up Swim - Flat", "5 - MainSwimmer", "1 - ForegroundSlugcats", "Outro 3 - Face - Flat", "2 - FaceCloseUp", "New Death - Flat", "New Death Flower - Flat", "New Death - 2", "New Death - 2", "Dead Red - Flat", "Red Death - 2"];
        for (var i = 0; i < strs.Length; i++)
        {
            var s = strs[i];
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr(s)))
            {
                c.Emit(Ldarg_0)
                 .EmitDelegate((string fileName, MenuScene self) => (self.menu.manager.currentMainLoop is not RainWorldGame game ? self.menu.manager.rainWorld.progression.PlayingAsSlugcat : game.StoryCharacter)?.value is "LBHardhatCat" ? fileName + " LBHardhatCat" : fileName);
            }
            else
                s_logger!.LogError($"Couldn't ILHook Menu.MenuScene.BuildScene (part {i + 1})!");
        }
    }

    static void IL_SlideShow_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<SlideShow.SlideShowID>("WhiteOutro"),
            x => x.MatchCall(out _))
         && c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<SlideShow.SlideShowID>("WhiteOutro"),
            x => x.MatchCall(out _)))
        {
            c.Emit(Ldarg_0)
             .EmitDelegate((bool flag, SlideShow self) => flag && (self.manager.currentMainLoop is not RainWorldGame game ? self.manager.rainWorld.progression.PlayingAsSlugcat : game.StoryCharacter)?.value != "LBHardhatCat");
        }
        else
            s_logger!.LogError("Couldn't ILHook Menu.SlideShow.ctor!");
    }

    static void On_Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        var firstTimeR = self.abstractRoom.firstTimeRealized;
        orig(self);
        if (self.game?.session is StoryGameSession sess && sess.saveState.cycleNumber == 0 && sess.saveState.saveStateNumber?.value == "LBHardhatCat" && self.abstractRoom.name == "HI_exvulturehole" && firstTimeR)
            self.AddObject(new HardhatTutorial(self));
    }

    static void On_ShortcutHelper_ctor(On.ShortcutHelper.orig_ctor orig, ShortcutHelper self, Room room)
    {
        orig(self, room);
        if (room.abstractRoom.name.Equals("su_a22", StringComparison.InvariantCultureIgnoreCase))
        {
            var tl = room.shortcuts[0].StartTile;
            self.pushers.Add(new(room, true, tl, room.ShorcutEntranceHoleDirection(tl)));
        }
    }

    static void IL_PlayerGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchLdfld<Player>("objectInStomach"),
            x => x.MatchBrtrue(out label)))
        {
            ++c.Index;
            c.EmitDelegate((PlayerGraphics self) => self.player.SlugCatClass?.value is "LBHardhatCat" && self.player.room is Room rm && ((ModManager.CoopAvailable && rm.game.IsStorySession && rm.game.Players[0] is AbstractCreature p && p != self.player.abstractCreature && p.state is PlayerState st && !self.player.isNPC) ? (st.quarterFoodPoints >= 2 || st.foodInStomach >= 1) : (self.player.playerState.quarterFoodPoints >= 2 || self.player.FoodInStomach >= 1)));
            c.Emit(Brtrue, label)
             .Emit(Ldarg_0);
        }
        else
            s_logger!.LogError("Couldn't ILHook PlayerGraphics.Update (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchLdfld<Player>("standing"))
         && c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchLdfld<Player>("standing")))
        {
            c.Emit(Ldarg_0)
             .EmitDelegate((bool flag, PlayerGraphics self) =>
             {
                 if (self.player.SlugCatClass?.value is "LBHardhatCat")
                     self.lookDirection *= .55f;
                 return flag;
             });
        }
        else
            s_logger!.LogError("Couldn't ILHook PlayerGraphics.Update (part 2)!");
    }

    static Player.ObjectGrabability On_Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is Runtile t)
        {
            if (t.Mode == Runtile.TileMode.TileDestroyed || t.Mode == Runtile.TileMode.Destroyed || t.Mode == Runtile.TileMode.Attached)
                return Player.ObjectGrabability.CantGrab;
            return Player.ObjectGrabability.OneHand;
        }
        return orig(self, obj);
    }

    static void IL_Player_GrabUpdate(ILContext il)
    {
        var c = new ILCursor(il);
        for (var i = 1; i <= 2; i++)
        {
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Player>("get_isGourmand")))
            {
                c.Emit(Ldarg_0)
                 .EmitDelegate((bool flag, Player self) => flag || self.SlugCatClass?.value == "LBHardhatCat");
            }
            else
                s_logger!.LogError($"Couldn't ILHook Player.GrabUpdate! (part {i})");
        }
    }

    static void IL_Spear_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var ind = 0;
        var vars = il.Body.Variables;
        while (ind < vars.Count && !vars[ind].VariableType.Name.Contains("Boolean"))
            ++ind;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(ind))
         && c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(ind))
         && c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(ind)))
        {
            c.Emit(Ldarg_0)
             .EmitDelegate((bool flag, Spear self) =>
             {
                 if (flag && self.room is Room rm)
                 {
                     var ps = rm.GetTilePosition(self.firstChunk.pos);
                     for (var i = -1; i <= 1; i++)
                     {
                         for (var j = -1; j <= 1; j++)
                         {
                             var ps2 = new IntVector2(ps.x + i, ps.y + j);
                             if (rm.IsPositionInsideBoundries(ps2) && s_falseTiles.TryGetValue(rm.GetTile(ps2), out _))
                             {
                                 flag = false;
                                 break;
                             }
                         }
                     }
                 }
                 return flag;
             });
        }
        else
            s_logger!.LogError("Couldn't ILHook Spear.Update!");
    }

    static void On_Tongue_Update(On.TubeWorm.Tongue.orig_Update orig, TubeWorm.Tongue self)
    {
        orig(self);
        if (self.mode == TubeWorm.Tongue.Mode.AttachedToTerrain && self.worm?.room is Room rm)
        {
            var ps = rm.GetTilePosition(self.terrainStuckPos);
            for (var i = -1; i <= 1; i++)
            {
                for (var j = -1; j <= 1; j++)
                {
                    var ps2 = new IntVector2(ps.x + i, ps.y + j);
                    if (rm.IsPositionInsideBoundries(ps2) && s_falseTiles.TryGetValue(rm.GetTile(ps2), out _))
                    {
                        self.Release();
                        break;
                    }
                }
            }
        }
    }

    static void On_Tongue_Update(On.Player.Tongue.orig_Update orig, Player.Tongue self)
    {
        orig(self);
        if (self.mode == Player.Tongue.Mode.AttachedToTerrain && self.player?.room is Room rm)
        {
            var ps = rm.GetTilePosition(self.terrainStuckPos);
            for (var i = -1; i <= 1; i++)
            {
                for (var j = -1; j <= 1; j++)
                {
                    var ps2 = new IntVector2(ps.x + i, ps.y + j);
                    if (rm.IsPositionInsideBoundries(ps2) && s_falseTiles.TryGetValue(rm.GetTile(ps2), out _))
                    {
                        self.Release();
                        break;
                    }
                }
            }
        }
    }

    static bool On_Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj) => testObj is not Rocktile and not Runtile && orig(self, testObj);

    static void IL_Player_TerrainImpact(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdcR4(60f)))
        {
            c.Emit(Ldarg_0)
             .EmitDelegate((float val, Player self) => self.SlugCatClass?.value == "LBHardhatCat" ? float.MaxValue : val);
        }
        else
            s_logger!.LogError("Couldn't ILHook Player.TerrainImpact!");
    }

    static void Subtract2QuarterFoodPoints(Player self)
    {
        if (ModManager.CoopAvailable && self.abstractCreature.world.game.IsStorySession && self.abstractCreature.world.game.Players[0] is AbstractCreature p && p != self.abstractCreature && !self.isNPC)
        {
            var st = (p.state as PlayerState)!;
            if (st.quarterFoodPoints >= 2)
                st.quarterFoodPoints -= 2;
            else if (st.foodInStomach >= 1)
            {
                --st.foodInStomach;
                if (p.world?.game?.GetStorySession?.saveState is SaveState save)
                    --save.totFood;
                st.quarterFoodPoints += 2;
            }
            self.playerState.quarterFoodPoints = st.quarterFoodPoints;
            self.playerState.foodInStomach = st.foodInStomach;
        }
        else if (self.playerState.quarterFoodPoints >= 2)
            self.playerState.quarterFoodPoints -= 2;
        else if (self.FoodInStomach > 0)
        {
            --self.playerState.foodInStomach;
            if (self.abstractCreature.world?.game?.GetStorySession?.saveState is SaveState save)
                --save.totFood;
            self.playerState.quarterFoodPoints += 2;
        }
    }

    static void On_Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self)
    {
        if (self.SlugCatClass?.value == "LBHardhatCat" && self.objectInStomach is null && self.room is Room rm && ((ModManager.CoopAvailable && rm.game.IsStorySession && rm.game.Players[0] is AbstractCreature p && p != self.abstractCreature && p.state is PlayerState st && !self.isNPC) ? (st.quarterFoodPoints >= 2 || st.foodInStomach >= 1) : (self.playerState.quarterFoodPoints >= 2 || self.FoodInStomach >= 1)))
        {
            Subtract2QuarterFoodPoints(self);
            if (rm.game.cameras[0]?.hud?.foodMeter?.quarterPipShower is FoodMeter.QuarterPipShower mt)
                mt.Reset();
            self.objectInStomach = new(rm.world, AbstractPhysicalObjectType.LBHardhatCatRuntile, null, rm.GetWorldCoordinate(self.firstChunk.pos), rm.game.GetNewID());
        }
        orig(self);
    }

    static bool IsOEGate(string nm) => nm.EndsWith("_OE") || nm.StartsWith("GATE_OE_");

    static void On_GateKarmaGlyph_DrawSprites(On.GateKarmaGlyph.orig_DrawSprites orig, GateKarmaGlyph self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.slatedForDeletetion && self.room is Room room && room == rCam.room && room.game?.StoryCharacter?.value == "LBHardhatCat" && IsOEGate(room.abstractRoom.name.ToUpperInvariant()))
        {
            var sprs = sLeaser.sprites;
            for (var i = 0; i < sprs.Length; i++)
                sprs[i].isVisible = false;
        }
    }

    static void On_ElectricGate_Update(On.ElectricGate.orig_Update orig, ElectricGate self, bool eu)
    {
        orig(self, eu);
        if (self.room is Room room && room.game?.StoryCharacter?.value == "LBHardhatCat" && IsOEGate(room.abstractRoom.name.ToUpperInvariant()))
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

    static bool On_SaveState_get_CanSeeVoidSpawn(Func<SaveState, bool> orig, SaveState self) => orig(self) || self.saveStateNumber?.value == "LBHardhatCat";

    static bool On_RegionGate_get_MeetRequirement(Func<RegionGate, bool> orig, RegionGate self) => (self.room is not Room room || room.game?.StoryCharacter?.value != "LBHardhatCat" || !IsOEGate(room.abstractRoom.name.ToUpperInvariant())) && orig(self);

    static void On_Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
    {
        orig(self, grasp, eu);
        if (self.grasps[grasp].grabbed is Runtile t && self.room is Room rm)
        {
            var pos = rm.GetTilePosition(self.mainBodyChunk.pos);
            pos.x += self.ThrowDirection;
            if (rm.IsPositionInsideBoundries(pos))
            {
                var tl = rm.GetTile(pos);
                if (tl.shortCut == 0)
                {
                    t.Tile = tl;
                    s_falseTiles.Add(tl, s_falseTile);
                    t.OrigTerrain = tl.Terrain;
                    t.Mode = Runtile.TileMode.Attached;
                    rm.PlaySound(SoundID.Gate_Clamp_Lock, t.firstChunk.pos, .8f, 1f + (Random.value - Random.value) * .1f);
                }
            }
        }
    }

    static void On_PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (!self.owner.room.game.DEBUGMODE && self.player is Player p && p.SlugCatClass?.value == "LBHardhatCat")
        {
            var lgt = sLeaser.sprites.Length;
            Array.Resize(ref sLeaser.sprites, lgt + 1);
            if (s_hardHat.TryGetValue(self, out var spr0))
            {
                spr0.isVisible = false;
                spr0.RemoveFromContainer();
                s_hardHat.Remove(self);
            }
            var spr = sLeaser.sprites[lgt] = new("LBHardhatHeadA0");
            s_hardHat.Add(self, spr);
            spr.RemoveFromContainer();
            rCam.ReturnFContainer("Midground").AddChild(spr);
        }
    }

    static void On_PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        if (s_hardHat.TryGetValue(self, out var spr))
        {
            spr.RemoveFromContainer();
            rCam.ReturnFContainer("Midground").AddChild(spr);
        }
    }

    static void On_PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!rCam.room.game.DEBUGMODE && s_hardHat.TryGetValue(self, out var spr))
        {
            var spr3 = sLeaser.sprites[3];
            var ps = spr3.GetPosition();
            spr.SetPosition(ps);
            spr.rotation = spr3.rotation;
            spr.alpha = spr3.alpha;
            spr.isVisible = spr3.isVisible;
            spr.anchorX = spr3.anchorX;
            spr.anchorY = spr3.anchorY;
            spr.scaleX = spr3.scaleX;
            spr.scaleY = spr3.scaleY;
            spr.color = Color.Lerp(Color.white, rCam.currentPalette.blackColor, rCam.room.DarknessOfPoint(rCam, ps) * .4f);
            var nm = spr3.element.name;
            var cut = nm.Contains("HeadB") ? "HeadB" : (nm.Contains("HeadC") ? "HeadC" : "HeadA");
            var ar = nm.Split([cut], StringSplitOptions.None);
            if (ar.Length > 1)
            {
                var elem = "LBHardhatHeadA" + ar[1];
                if (Futile.atlasManager.DoesContainElementWithName(elem))
                    spr.element = Futile.atlasManager.GetElementWithName(elem);
            }
        }
    }

    static void On_PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (s_hardHat.TryGetValue(self, out var spr))
            spr.color = Color.Lerp(Color.white, palette.blackColor, rCam.room.DarknessOfPoint(rCam, sLeaser.sprites[3].GetPosition()) * .4f);
    }

    static void On_Region_ctor_string_int_int_Timeline(On.Region.orig_ctor_string_int_int_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Timeline timelineIndex)
    {
        orig(self, name, firstRoomIndex, regionNumber, timelineIndex);
        if (timelineIndex?.value == "LBHardhatCat")
        {
            var regionParams = self.regionParams;
            regionParams.slugPupSpawnChance = 0f;
            regionParams.playerGuideOverseerSpawnChance = 0f;
            if (regionParams.batDepleteCyclesMin > 0)
                regionParams.batDepleteCyclesMin += 2;
            if (regionParams.batDepleteCyclesMax > 0)
                regionParams.batDepleteCyclesMax += 2;
            if (regionParams.batDepleteCyclesMaxIfLessThanFiveLeft > 0)
                regionParams.batDepleteCyclesMaxIfLessThanFiveLeft += 2;
            if (regionParams.batDepleteCyclesMaxIfLessThanTwoLeft > 0)
                regionParams.batDepleteCyclesMaxIfLessThanTwoLeft += 2;
            if (regionParams.batsPerActiveSwarmRoom > 0)
                regionParams.batsPerActiveSwarmRoom += 2;
            if (regionParams.batsPerInactiveSwarmRoom > 0)
                regionParams.batsPerInactiveSwarmRoom += 2;
            regionParams.earlyCycleChance = 0f;
            regionParams.earlyCycleFloodChance = 0f;
            if (regionParams.scavsMax > 0)
                regionParams.scavsMax += 2;
            if (regionParams.scavsMin > 0)
                regionParams.scavsMin += 2;
            if (regionParams.scavsSpawnChance > 0f)
                regionParams.scavsSpawnChance += .2f;
        }
    }

    static void On_AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        var f = self.realizedObject is null;
        AbstractPhysicalObject obj;
        if (f && self.type == AbstractPhysicalObjectType.LBHardhatCatRuntile)
        {
            var tl = self.pos.Tile;
            self.realizedObject = new Runtile(self, new(tl.x * 20f, tl.y * 20f));
            var sts = self.stuckObjects;
            for (var i = 0; i < sts.Count; i++)
            {
                var stuckObj = sts[i];
                obj = stuckObj.A;
                if (obj.realizedObject is null && obj != self)
                    obj.Realize();
                obj = stuckObj.B;
                if (obj.realizedObject is null && obj != self)
                    obj.Realize();
            }
        }
        else if (f && self.type == AbstractPhysicalObjectType.LBHardhatCatRocktile)
        {
            self.realizedObject = new Rocktile(self, self.world);
            var sts = self.stuckObjects;
            for (var i = 0; i < sts.Count; i++)
            {
                var stuckObj = sts[i];
                obj = stuckObj.A;
                if (obj.realizedObject is null && obj != self)
                    obj.Realize();
                obj = stuckObj.B;
                if (obj.realizedObject is null && obj != self)
                    obj.Realize();
            }
        }
        else if (f && self.type == AbstractPhysicalObject.AbstractObjectType.Rock && self.world?.game?.StoryCharacter?.value == "LBHardhatCat")
        {
            var state = Random.state;
            Random.InitState(self.ID.RandomSeed);
            var b = Random.value <= .25f;
            Random.state = state;
            if (b)
            {
                self.realizedObject = new Rocktile(self, self.world);
                var sts = self.stuckObjects;
                for (var i = 0; i < sts.Count; i++)
                {
                    var stuckObj = sts[i];
                    obj = stuckObj.A;
                    if (obj.realizedObject is null && obj != self)
                        obj.Realize();
                    obj = stuckObj.B;
                    if (obj.realizedObject is null && obj != self)
                        obj.Realize();
                }
            }
            else
                orig(self);
        }
        else
            orig(self);
    }

    public void OnDisable()
    {
        s_logger = null;
        s_falseTile = null!;
        s_falseTiles = null!;
    }
}