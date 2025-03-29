using Random = UnityEngine.Random;
using UnityEngine;
using RWCustom;
using DevInterface;

namespace GildedSanctuarySpecific;

static class Hooks
{
    public static void Apply()
    {
        On.Room.Loaded += (orig, self) =>
        {
            if (self.game is not RainWorldGame game)
                return;
            orig(self);
            var pObjs = self.roomSettings.placedObjects;
            for (var i = 0; i < pObjs.Count; i++)
            {
                var pObj = pObjs[i];
                if (pObj.type == PlacedObjectType.GRJLeviathanPushBack && pObj.active)
                    self.AddObject(new LeviathanPushbackObject(self, (pObj.data as PlacedObject.ResizableObjectData)!));
            }
        };
        On.PlacedObject.GenerateEmptyData += (orig, self) =>
        {
            orig(self);
            if (self.type == PlacedObjectType.GRJLeviathanPushBack)
                self.data = new PlacedObject.ResizableObjectData(self);
        };
        On.DevInterface.ObjectsPage.CreateObjRep += (orig, self, tp, pObj) =>
        {
            if (tp == PlacedObjectType.GRJLeviathanPushBack)
            {
                if (pObj is null)
                    self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                    {
                        pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                    });
                var pObjRep = new ResizeableObjectRepresentation(self.owner, $"{tp}_Rep", self, pObj, tp.ToString(), true);
                self.tempNodes.Add(pObjRep);
                self.subNodes.Add(pObjRep);
            }
            else
                orig(self, tp, pObj);
        };
        On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += (orig, self, type) => type == PlacedObjectType.GRJLeviathanPushBack ? ObjectsPage.DevObjectCategories.Gameplay : orig(self, type);
        On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += (orig, self, type) => type == RoomEffectType.GRJGoldenFlakes ? RoomSettingsPage.DevEffectsCategories.Decorations : orig(self, type);
        On.Room.NowViewed += (orig, self) =>
        {
            orig(self);
            var effects = self.roomSettings.effects;
            for (var j = 0; j < effects.Count; j++)
            {
                var effect = effects[j];
                if (effect.type == RoomEffectType.GRJGoldenFlakes)
                {
                    for (var l = 0; l < self.cameraPositions.Length; l++)
                    {
                        if (effect.amount > 0f) 
                            self.AddObject(new GoldFlakesEffect(self));
                    }
                }
            }
        };
        On.BigEel.NewRoom += (orig, self, newRoom) =>
        {
            orig(self, newRoom);
            var pObjs = newRoom.roomSettings.placedObjects;
            for (var i = 0; i < pObjs.Count; i++)
            {
                if (pObjs[i].type == PlacedObjectType.GRJLeviathanPushBack)
                    self.antiStrandingZones.Add(pObjs[i]);
            }
        };
    }
}