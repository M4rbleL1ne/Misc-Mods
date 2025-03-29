using BepInEx;
using UnityEngine;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace PAStuff;

[BepInPlugin("lb-fgf-m4r-ik.pa-stuff", nameof(PAStuff), "1.1.1")]
public class PAStuffPlugin : BaseUnityPlugin
{
    const string _PA = "PA";

    public void OnEnable()
    {
        On.SSOracleSwarmer.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.room?.world?.region?.name is _PA && rCam.currentPalette.darkness > 0f)
            {
                var light = sLeaser.sprites[0];
                light.shader = rCam.game.rainWorld.Shaders["LightSource"];
                light.scale = 7.5f;
                light.alpha = rCam.currentPalette.darkness;
                light.isVisible = self.lastVisible;
            }
        };
        On.RoofTopView.Building.InitiateSprites += (orig, self, sLeaser, rCam) =>
        {
            orig(self, sLeaser, rCam);
            if (self.room?.world?.region?.name is _PA)
                sLeaser.sprites[0].scaleX = -sLeaser.sprites[0].scaleX;
        };
        On.RoofTopView.DistantBuilding.InitiateSprites += (orig, self, sLeaser, rCam) =>
        {
            orig(self, sLeaser, rCam);
            if (self.room?.world?.region?.name is _PA)
                sLeaser.sprites[0].scaleX = -sLeaser.sprites[0].scaleX;
        };
        On.RoofTopView.ctor += (orig, self, room, effect) =>
        {
            orig(self, room, effect);
            if (room.world?.region?.name is _PA)
            {
                var vec = room.abstractRoom.size.ToVector2();
                self.sceneOrigo = self.RoomToWorldPos(new(0f, vec.y * 10f));
                Shader.SetGlobalVector("_SceneOrigoPosition", self.sceneOrigo);
                foreach (var elem in self.elements)
                {
                    switch (elem) 
                    {
                        case RoofTopView.Building b:
                            b.pos.x = b.assetName switch
                            {
                                "city2" => self.PosFromDrawPosAtNeutralCamPos(new(-1780f, 0f), 11.5f).x,
                                "city1" => self.PosFromDrawPosAtNeutralCamPos(new(-880f, 0f), 10.5f).x,
                                _ => b.pos.x
                            };
                            break;
                        case RoofTopView.DistantBuilding db:
                            db.pos.x = db.assetName switch
                            {
                                "Rf_HoleFix" => 2676f,
                                "RF_CityA" => self.PosFromDrawPosAtNeutralCamPos(new(-300f, 0f), 8.5f).x,
                                "RF_CityB" => self.PosFromDrawPosAtNeutralCamPos(new(-515f, 0f), 6.5f).x,
                                "RF_CityC" => self.PosFromDrawPosAtNeutralCamPos(new(-400f, 0f), 5f).x,
                                _ => db.pos.x
                            };
                            break;
                    }
                }
            }
        };
    }
}