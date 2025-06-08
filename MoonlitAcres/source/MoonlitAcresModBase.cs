using BepInEx;
using System.Reflection;
using MonoMod.RuntimeDetour;
using DevInterface;
using UnityEngine;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;
using System;
using Random = UnityEngine.Random;
using BepInEx.Logging;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MoonlitAcres; 

[BepInPlugin("com.rainworldgame.halloweegionjam.plugin", "Halloweegion Jam (Moonlit Acres)", "10.0.0"), BepInDependency("com.rainworldgame.reaperlizard.plugin", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class MoonlitAcreModBase : BaseUnityPlugin 
{
    [AllowNull] public static PlacedObject.Type PumpkinPlacedObject = new(nameof(PumpkinPlacedObject), true);
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType Pumpkin = new(nameof(Pumpkin), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID PumpkinUnlock = new(nameof(PumpkinUnlock), true);
    [AllowNull] public static SLOracleBehaviorHasMark.MiscItemType PumpkinMiscItem = new(nameof(PumpkinMiscItem), true);
    [AllowNull] static ManualLogSource s_logger;

    public void OnEnable()
    {
        s_logger = Logger;
        FestiveRot.ApplyHooks();
        On.RainWorld.OnModsInit += (orig, self) =>
        {
            orig(self);
            if (!MultiplayerUnlocks.ItemUnlockList.Contains(PumpkinUnlock))
                MultiplayerUnlocks.ItemUnlockList.Add(PumpkinUnlock);
            if (!Futile.atlasManager.DoesContainAtlas("moonlitacres_spr2"))
                Futile.atlasManager.LoadAtlas("atlases/moonlitacres_spr2");
        };
        On.RainWorld.UnloadResources += (orig, self) =>
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("moonlitacres_spr2"))
                Futile.atlasManager.UnloadAtlas("moonlitacres_spr2");
        };
        On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
        {
            orig(self, newlyDisabledMods);
            for (var i = 0; i < newlyDisabledMods.Length; i++)
            {
                if (newlyDisabledMods[i].id == "com.rainworldgame.halloweegionjam.plugin")
                {
                    if (MultiplayerUnlocks.ItemUnlockList.Contains(PumpkinUnlock))
                        MultiplayerUnlocks.ItemUnlockList.Remove(PumpkinUnlock);
                    Pumpkin?.Unregister();
                    Pumpkin = null;
                    PumpkinPlacedObject?.Unregister();
                    PumpkinPlacedObject = null;
                    PumpkinUnlock?.Unregister();
                    PumpkinUnlock = null;
                    PumpkinMiscItem?.Unregister();
                    PumpkinMiscItem = null;
                    break;
                }
            }
        };
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += (orig, self, testItem) => testItem is Pumpkin ? PumpkinMiscItem : orig(self, testItem);
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += (orig, self) =>
        {
            orig(self);
            if (self.id == Conversation.ID.Moon_Misc_Item && self.describeItem == PumpkinMiscItem)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a pumpkin! Happy Halloween!"), 0));
        };
        On.AbstractPhysicalObject.Realize += (orig, self) =>
        {
            orig(self);
            if (self.realizedObject is null && self.type == Pumpkin)
                self.realizedObject = new Pumpkin(self);
        };
        On.AbstractConsumable.IsTypeConsumable += (orig, type) => orig(type) || type == Pumpkin;
        On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += (orig, self, type) =>
        {
            var res = orig(self, type);
            if (type == PumpkinPlacedObject)
                res = ObjectsPage.DevObjectCategories.Consumable;
            return res;
        };
        On.DevInterface.ObjectsPage.CreateObjRep += (orig, self, tp, pObj) =>
        {
            if (tp == PumpkinPlacedObject)
            {
                if (pObj is null)
                    self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                    {
                        pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                    });
                var pObjRep = new ConsumableRepresentation(self.owner, $"{nameof(PumpkinPlacedObject)}_Rep", self, pObj, nameof(PumpkinPlacedObject));
                self.tempNodes.Add(pObjRep);
                self.subNodes.Add(pObjRep);
            }
            else
                orig(self, tp, pObj);
        };
        On.PlacedObject.GenerateEmptyData += (orig, self) =>
        {
            orig(self);
            if (self.type == PumpkinPlacedObject)
                self.data = new PlacedObject.ConsumableObjectData(self);
        };
        On.Room.Loaded += (orig, self) =>
        {
            var firstTimeRealized = self.abstractRoom.firstTimeRealized;
            orig(self);
            if (firstTimeRealized)
            {
                var objs = self.roomSettings.placedObjects;
                for (var rl = 1; rl <= 2; rl++)
                {
                    if (rl == 2 && self.warpPoints.Count > 0)
                        continue;
                    for (var i = 0; i < objs.Count; i++)
                    {
                        var pObj = objs[i];
                        if ((rl == 1 && pObj.deactivatedByWarpFilter) || (rl == 2 && !pObj.deactivatedByWarpFilter) || !pObj.active || self.CheckForWarpedObjects(i))
                            continue;
                        if (pObj.type == PumpkinPlacedObject && (self.game.session is not StoryGameSession session || !session.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i)))
                            self.abstractRoom.AddEntity(new AbstractConsumable(self.world, Pumpkin, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID(), self.abstractRoom.index, i, (PlacedObject.ConsumableObjectData)pObj.data)
                            {
                                isConsumed = false,
                                placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(self.abstractRoom.name, i)
                            });
                    }
                }
            }
        };
        //prevents an exception
        IL.MultiplayerUnlocks.ctor += il =>
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(1)))
            {
                c.Emit(OpCodes.Ldsfld, il.Import(typeof(MoonlitAcreModBase).GetField(nameof(PumpkinUnlock))))
                 .Emit(OpCodes.Pop)
                 .Emit(OpCodes.Ldsfld, il.Import(typeof(MoonlitAcreModBase).GetField(nameof(Pumpkin))))
                 .Emit(OpCodes.Pop);
            }
            else
                s_logger.LogError("Couldn't ILHook MultiplayerUnlocks.ctor!");
        };
        On.Player.Grabability += (orig, self, obj) => obj is Pumpkin ? Player.ObjectGrabability.TwoHands : orig(self, obj);
        On.ItemSymbol.SpriteNameForItem += (orig, itemType, intData) => itemType == Pumpkin ? "Symbol_Pumpkin" : orig(itemType, intData);
        On.ItemSymbol.ColorForItem += (orig, itemType, intData) => itemType == Pumpkin ? new(.608f, .22f, 0f) : orig(itemType, intData);
        On.MultiplayerUnlocks.SandboxUnlockForSymbolData += (orig, data) => data.itemType == Pumpkin ? PumpkinUnlock : orig(data);
        On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += (orig, unlockID) => unlockID == PumpkinUnlock ? new(CreatureTemplate.Type.StandardGroundCreature, Pumpkin, 0) : orig(unlockID);
        On.Lightning.ctor += (orig, self, room, intensity, bkgOnly) =>
        {
            orig(self, room, intensity, bkgOnly);
            if (room.IsHW())
            {
                self.bkgGradient[0] = new(.984313727f, .564705881f, .160784325f);
                self.bkgGradient[1] = new(1f, .5f, 0f);
            }
        };
        //Reaper lizards shouldn't be always unlocked anymore
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (var i = 0; i < assemblies.Length; i++)
        {
            var asm = assemblies[i];
            if (asm.FullName.Contains("ReaperLizard"))
            {
                new Hook(asm.GetType("ReaperLizard.NLESandbox").GetMethod("Unlock", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance),
                    (On.MultiplayerUnlocks.orig_SandboxItemUnlocked orig, MultiplayerUnlocks self, MultiplayerUnlocks.SandboxUnlockID unlock) => orig(self, unlock));
                break;
            }
        }
    }

    public void OnDisable() => s_logger = null;
}

public static class RoomExtension
{
    public static bool IsHW(this Room room) => room?.abstractRoom?.name?.ToLower() is string s && (s.StartsWith("hw_") || s == "holiday tower" || s == "bridge");

    public static bool InHW(this Creature creature) => creature.abstractCreature?.Room?.name?.ToLower() is string s && (s.StartsWith("hw_") || s == "holiday tower" || s == "bridge");

    public static bool InHW(this AbstractCreature creature) => creature.Room?.name?.ToLower() is string s && (s.StartsWith("hw_") || s == "holiday tower" || s == "bridge");

    public static bool IsValidDaddy(this DaddyLongLegs daddy) => daddy.colorClass && (daddy.Template.type == CreatureTemplate.Type.DaddyLongLegs || (ModManager.DLCShared && daddy.Template.type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs));
}