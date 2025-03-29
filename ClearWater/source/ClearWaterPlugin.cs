using BepInEx;

namespace ClearWaterPlugin;

[BepInPlugin("lb-fgf-m4r-ik.clear-water-plugin", "ClearWaterPlugin", "1.1.1")]
sealed class ClearWaterPlugin : BaseUnityPlugin
{
	public void OnEnable()
	{
		On.Water.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) =>
		{
			orig(self, sLeaser, rCam, timeStacker, camPos);
			sLeaser.sprites[1].alpha = 0f;
			sLeaser.sprites[1].scale = 0f;
		};
	}
}