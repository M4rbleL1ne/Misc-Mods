using BepInEx;
using BepInEx.Logging;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using UnityEngine;
using static Mono.Cecil.Cil.OpCodes;
using Random = UnityEngine.Random;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace LBHardhatCat;

[BepInPlugin("lb-fgf-m4r-ik.hardhat-cat", "LBHardhatCat", "1.0.1"), BepInDependency("slime-cubed.slugbase"), BepInDependency("rwmodding.coreorg.rk")]
public sealed class LBHardhatCatPlugin : BaseUnityPlugin
{
    internal static ManualLogSource? s_logger;
    static RegionGate.GateRequirement? s_forbidden;
    internal static ConditionalWeakTable<PlayerGraphics, FSprite> s_hardHat = new();
    internal static ConditionalWeakTable<Room.Tile, FalseTile> s_falseTiles = new();
    internal static FalseTile s_falseTile = new();

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class FalseTile { }

    public void OnEnable()
    {
        s_logger = Logger;
        On.MultiplayerUnlocks.ClassUnlocked += (orig, self, classID) =>
        {
            if (classID?.value == "LBHardhatCat")
                return self.progression.miscProgressionData.GetTokenCollected(SlugcatUnlockID.LBHardhatCat);
            return orig(self, classID);
        };
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += (orig, self, testItem) =>
        {
            if (testItem is Rocktile)
                return MiscItemType.LBHardhatCatRocktile;
            if (testItem is Runtile)
                return MiscItemType.LBHardhatCatRuntile;
            return orig(self, testItem);
        };
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += (orig, self) =>
        {
            orig(self);
            if (self.id == Conversation.ID.Moon_Misc_Item)
            {
                if (self.describeItem == MiscItemType.LBHardhatCatRocktile)
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a strange object.<LINE>It seems large enough to hurt a creature."), 0));
                else if (self.describeItem == MiscItemType.LBHardhatCatRuntile)
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a piece of wall.<LINE>How did you manage to take it?"), 0));
            }
        };
        On.ItemSymbol.SpriteNameForItem += (orig, itemType, intData) =>
        {
            if (itemType == AbstractPhysicalObjectType.LBHardhatCatRocktile)
                return "Symbol_LBHardhatCatRocktile";
            if (itemType == AbstractPhysicalObjectType.LBHardhatCatRuntile)
                return "Symbol_LBHardhatCatRuntile";
            return orig(itemType, intData);
        };
        On.RainWorld.OnModsInit += (orig, self) =>
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
        };
        On.RainWorld.UnloadResources += (orig, self) =>
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("lbhardhatcatspr"))
                Futile.atlasManager.UnloadAtlas("lbhardhatcatspr");
        };
        On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
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
        };
        IL.Menu.MenuScene.BuildScene += il =>
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
                     .EmitDelegate((string fileName, Menu.MenuScene self) => (self.menu.manager.currentMainLoop is not RainWorldGame game ? self.menu.manager.rainWorld.progression.PlayingAsSlugcat : game.StoryCharacter)?.value is "LBHardhatCat" ? fileName + " LBHardhatCat" : fileName);
                }
                else
                    s_logger.LogError($"Couldn't ILHook Menu.MenuScene.BuildScene (part {i + 1})!");
            }
        };
        IL.Menu.SlideShow.ctor += il =>
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<Menu.SlideShow.SlideShowID>("WhiteOutro"),
                x => x.MatchCall(out _))
             && c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<Menu.SlideShow.SlideShowID>("WhiteOutro"),
                x => x.MatchCall(out _)))
            {
                c.Emit(Ldarg_0)
                 .EmitDelegate((bool flag, Menu.SlideShow self) => flag && (self.manager.currentMainLoop is not RainWorldGame game ? self.manager.rainWorld.progression.PlayingAsSlugcat : game.StoryCharacter)?.value != "LBHardhatCat");
            }
            else
                s_logger.LogError("Couldn't ILHook Menu.SlideShow.ctor!");
        };
        On.Room.Loaded += (orig, self) =>
        {
            var firstTimeR = self.abstractRoom.firstTimeRealized;
            orig(self);
            if (self.game?.session is StoryGameSession sess && sess.saveState.cycleNumber < 2 && sess.saveState.saveStateNumber?.value is "LBHardhatCat" && self.abstractRoom.name == "HI_exvulturehole" && firstTimeR)
                self.AddObject(new HardhatTutorial(self));
        };
        On.ShortcutHelper.ctor += (orig, self, room) =>
        {
            orig(self, room);
            if (room.abstractRoom.name.Equals("su_a22", StringComparison.InvariantCultureIgnoreCase))
            {
                var tl = room.shortcuts[0].StartTile;
                self.pushers.Add(new(room, true, tl, room.ShorcutEntranceHoleDirection(tl)));
            }
        };
        IL.PlayerGraphics.Update += il =>
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
                s_logger.LogError("Couldn't ILHook PlayerGraphics.Update (part 1)!");
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
                s_logger.LogError("Couldn't ILHook PlayerGraphics.Update (part 2)!");
        };
        On.Player.Grabability += (orig, self, obj) =>
        {
            if (obj is Runtile t)
            {
                if (t.Mode == Runtile.TileMode.TileDestroyed || t.Mode == Runtile.TileMode.Destroyed || t.Mode == Runtile.TileMode.Attached)
                    return Player.ObjectGrabability.CantGrab;
                return Player.ObjectGrabability.OneHand;
            }
            return orig(self, obj);
        };
        IL.Player.GrabUpdate += il =>
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
                    s_logger.LogError($"Couldn't ILHook Player.GrabUpdate! (part {i})");
            }
        };
        IL.Spear.Update += il =>
        {
            var c = new ILCursor(il);
            var ind = 0;
            var vars = il.Body.Variables;
            for (; ind < vars.Count && !vars[ind].VariableType.Name.Contains("Boolean"); ind++) { }
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
                s_logger.LogError("Couldn't ILHook Spear.Update!");
        };
        On.TubeWorm.Tongue.Update += (orig, self) =>
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
        };
        On.Player.Tongue.Update += (orig, self) =>
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
        };
        IL.Player.TerrainImpact += il =>
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(60f)))
            {
                c.Emit(Ldarg_0)
                 .EmitDelegate((float val, Player self) => self.SlugCatClass?.value == "LBHardhatCat" ? float.MaxValue : val);
            }
            else
                s_logger.LogError("Couldn't ILHook Player.TerrainImpact!");
        };
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
        On.Player.CanBeSwallowed += (orig, self, testObj) => testObj is not Rocktile and not Runtile && orig(self, testObj);
        On.Player.Regurgitate += (orig, self) =>
        {
            if (self.SlugCatClass?.value == "LBHardhatCat" && self.objectInStomach is null && self.room is Room rm && ((ModManager.CoopAvailable && rm.game.IsStorySession && rm.game.Players[0] is AbstractCreature p && p != self.abstractCreature && p.state is PlayerState st && !self.isNPC) ? (st.quarterFoodPoints >= 2 || st.foodInStomach >= 1) : (self.playerState.quarterFoodPoints >= 2 || self.FoodInStomach >= 1)))
            {
                Subtract2QuarterFoodPoints(self);
                if (rm.game.cameras[0]?.hud?.foodMeter?.quarterPipShower is HUD.FoodMeter.QuarterPipShower mt)
                    mt.Reset();
                self.objectInStomach = new(rm.world, AbstractPhysicalObjectType.LBHardhatCatRuntile, null, rm.GetWorldCoordinate(self.firstChunk.pos), rm.game.GetNewID());
            }
            orig(self);
        };
        On.Player.TossObject += (orig, self, grasp, eu) =>
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
        };
        On.RegionGate.ctor += (orig, self, room) =>
        {
            orig(self, room);
            if (room.game?.GetStorySession?.saveStateNumber?.value == "LBHardhatCat" && room.abstractRoom.name.ToUpperInvariant().Contains("_OE"))
            {
                s_forbidden ??= new("Forbidden");
                self.unlocked = false;
                if (self.karmaGlyphs is GateKarmaGlyph[] ar)
                {
                    for (var i = 0; i < ar.Length; i++)
                        ar[i].requirement = s_forbidden;
                }
                if (self.karmaRequirements is RegionGate.GateRequirement[] reqs)
                {
                    for (var i = 0; i < reqs.Length; i++)
                        reqs[i] = s_forbidden;
                }
            }
        };
        On.PlayerGraphics.InitiateSprites += (orig, self, sLeaser, rCam) =>
        {
            orig(self, sLeaser, rCam);
            if (!self.owner.room.game.DEBUGMODE && self.player is Player p && p.SlugCatClass?.value == "LBHardhatCat")
            {
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
                if (!s_hardHat.TryGetValue(self, out var spr))
                    s_hardHat.Add(self, spr = sLeaser.sprites[sLeaser.sprites.Length - 1] = new("LBHardhatHeadA0"));
                else
                    sLeaser.sprites[sLeaser.sprites.Length - 1] = spr;
                spr.RemoveFromContainer();
                rCam.ReturnFContainer("Midground").AddChild(spr);
            }
        };
        On.PlayerGraphics.AddToContainer += (orig, self, sLeaser, rCam, newContainer) =>
        {
            orig(self, sLeaser, rCam, newContainer);
            if (s_hardHat.TryGetValue(self, out var spr))
            {
                spr.RemoveFromContainer();
                rCam.ReturnFContainer("Midground").AddChild(spr);
            }
        };
        On.PlayerGraphics.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
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
                var ar = Regex.Split(nm, cut);
                if (ar.Length > 1)
                {
                    var elem = "LBHardhatHeadA" + ar[1];
                    if (Futile.atlasManager.DoesContainElementWithName(elem))
                        spr.element = Futile.atlasManager.GetElementWithName(elem);
                }
            }
        };
        On.Region.ctor += (orig, self, name, firstRoomIndex, regionNumber, storyIndex) =>
        {
            orig(self, name, firstRoomIndex, regionNumber, storyIndex);
            if (storyIndex?.value == "LBHardhatCat")
            {
                self.regionParams.slugPupSpawnChance = 0f;
                self.regionParams.playerGuideOverseerSpawnChance = 0f;
                if (self.regionParams.batDepleteCyclesMin > 0)
                    self.regionParams.batDepleteCyclesMin += 2;
                if (self.regionParams.batDepleteCyclesMax > 0)
                    self.regionParams.batDepleteCyclesMax += 2;
                if (self.regionParams.batDepleteCyclesMaxIfLessThanFiveLeft > 0)
                    self.regionParams.batDepleteCyclesMaxIfLessThanFiveLeft += 2;
                if (self.regionParams.batDepleteCyclesMaxIfLessThanTwoLeft > 0)
                    self.regionParams.batDepleteCyclesMaxIfLessThanTwoLeft += 2;
                if (self.regionParams.batsPerActiveSwarmRoom > 0)
                    self.regionParams.batsPerActiveSwarmRoom += 2;
                if (self.regionParams.batsPerInactiveSwarmRoom > 0)
                    self.regionParams.batsPerInactiveSwarmRoom += 2;
                self.regionParams.earlyCycleChance = 0f;
                self.regionParams.earlyCycleFloodChance = 0f;
                if (self.regionParams.scavsMax > 0)
                    self.regionParams.scavsMax += 2;
                if (self.regionParams.scavsMin > 0)
                    self.regionParams.scavsMin += 2;
                if (self.regionParams.scavsSpawnChance > 0f)
                    self.regionParams.scavsSpawnChance += .2f;
            }
        };
        On.AbstractPhysicalObject.Realize += (orig, self) =>
        {
            var f = self.realizedObject is null;
            if (f && self.type == AbstractPhysicalObjectType.LBHardhatCatRuntile)
            {
                var tl = self.pos.Tile;
                self.realizedObject = new Runtile(self, new(tl.x * 20f, tl.y * 20f));
                for (var i = 0; i < self.stuckObjects.Count; i++)
                {
                    var stuckObj = self.stuckObjects[i];
                    if (stuckObj.A.realizedObject is null && stuckObj.A != self)
                        stuckObj.A.Realize();
                    if (stuckObj.B.realizedObject is null && stuckObj.B != self)
                        stuckObj.B.Realize();
                }
            }
            else if (f && self.type == AbstractPhysicalObjectType.LBHardhatCatRocktile)
            {
                self.realizedObject = new Rocktile(self, self.world);
                for (var i = 0; i < self.stuckObjects.Count; i++)
                {
                    var stuckObj = self.stuckObjects[i];
                    if (stuckObj.A.realizedObject is null && stuckObj.A != self)
                        stuckObj.A.Realize();
                    if (stuckObj.B.realizedObject is null && stuckObj.B != self)
                        stuckObj.B.Realize();
                }
            }
            else if (f && self.type == AbstractPhysicalObject.AbstractObjectType.Rock && self.world?.game?.GetStorySession?.saveStateNumber?.value == "LBHardhatCat")
            {
                var state = Random.state;
                Random.InitState(self.ID.RandomSeed);
                var b = Random.value <= .25f;
                Random.state = state;
                if (b)
                {
                    self.realizedObject = new Rocktile(self, self.world);
                    for (var i = 0; i < self.stuckObjects.Count; i++)
                    {
                        var stuckObj = self.stuckObjects[i];
                        if (stuckObj.A.realizedObject is null && stuckObj.A != self)
                            stuckObj.A.Realize();
                        if (stuckObj.B.realizedObject is null && stuckObj.B != self)
                            stuckObj.B.Realize();
                    }
                }
                else
                    orig(self);
            }
            else
                orig(self);
        };
    }

    public void OnDisable()
    {
        s_forbidden = null;
        s_logger = null;
        s_falseTile = null!;
        s_falseTiles = null!;
    }
}