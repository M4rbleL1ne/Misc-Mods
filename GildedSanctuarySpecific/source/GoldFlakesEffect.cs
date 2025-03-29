using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace GildedSanctuarySpecific;

internal sealed class GoldFlakesEffect : UpdatableAndDeletable
{
	readonly List<GoldFlake> flakes = [];
	int savedCamPos = -1;

	internal GoldFlakesEffect(Room room)
	{
		this.room = room;
		var num = 0f;
		if (room?.roomSettings is RoomSettings rs)
		{
			for (var i = 0; i < room.cameraPositions.Length; i++) 
				num = rs.GetEffectAmount(RoomEffectType.GRJGoldenFlakes);
			for (var j = 0; j < NumberOfFlakes(num); j++)
			{
				var lGoldFlake = new GoldFlake();
				flakes.Add(lGoldFlake);
				room.AddObject(lGoldFlake);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room?.roomSettings is RoomSettings rs)
		{
			var rCam = room.game.cameras[0];
			if (rCam.currentCameraPosition != savedCamPos)
			{
				savedCamPos = rCam.currentCameraPosition;
				var num2 = NumberOfFlakes(rs.GetEffectAmount(RoomEffectType.GRJGoldenFlakes));
				for (var i2 = 0; i2 < flakes.Count; i2++)
				{
					var f = flakes[i2];
                    if (i2 <= num2)
					{
						f.active = true;
						f.PlaceRandomlyInRoom();
						f.savedCamPos = savedCamPos;
						f.reset = false;
					}
					else 
						f.active = false;
				}
			}
			if (!room.BeingViewed)
			{
				for (var j2 = 0; j2 < flakes.Count; j2++) 
					flakes[j2].Destroy();
				Destroy();
			}
		}
	}

	int NumberOfFlakes(float amount) => (int)(200f * Mathf.Pow(amount, 2f));

	internal sealed class GoldFlake : CosmeticSprite
	{
		float scale;
		float rot;
		float lastRot;
		float yRot;
		float lastYRot;
		float rotSpeed;
		float yRotSpeed;
		float velRotAdd;
		internal int savedCamPos;
		internal bool reset;
		internal bool active;

		internal GoldFlake()
		{
			savedCamPos = -1;
			ResetMe();
		}

		public override void Update(bool eu)
		{
			if (!active)
			{
				savedCamPos = -1;
				return;
			}
			base.Update(eu);
			if (room is null)
				return;
			var rCam = room.game.cameras[0];
			vel *= .82f;
			vel.y -= .25f;
			vel += Custom.DegToVec(180f + Mathf.Lerp(-45f, 45f, Random.value)) * .1f + Custom.DegToVec(rot + velRotAdd + yRot) * Mathf.Lerp(.1f, .25f, Random.value);
			if (room.GetTile(pos).Solid && room.GetTile(lastPos).Solid) 
				reset = true;
			if (reset)
			{
				pos = rCam.pos + new Vector2(Mathf.Lerp(-20f, 1386f, Random.value), Mathf.Lerp(-200f, 968f, Random.value));
				lastPos = pos;
				ResetMe();
				reset = false;
				vel *= 0f;
				return;
			}
			if (pos.x < rCam.pos.x - 20f) 
				reset = true;
			if (pos.x > rCam.pos.x + 1366f + 20f)
				reset = true;
			if (pos.y < rCam.pos.y - 200f) 
				reset = true;
			if (pos.y > rCam.pos.y + 768f + 200f) 
				reset = true;
			if (rCam.currentCameraPosition != savedCamPos)
			{
				PlaceRandomlyInRoom();
				savedCamPos = rCam.currentCameraPosition;
			}
			if (!room.BeingViewed)
				Destroy();
			lastRot = rot;
			rot += rotSpeed;
			rotSpeed = Mathf.Clamp(rotSpeed + Mathf.Lerp(-1f, 1f, Random.value) / 30f, -10f, 10f);
			lastYRot = yRot;
			yRot += yRotSpeed;
			yRotSpeed = Mathf.Clamp(yRotSpeed + Mathf.Lerp(-1f, 1f, Random.value) / 320f, -.05f, .05f);
		}

		internal void PlaceRandomlyInRoom()
		{
			ResetMe();
			pos = (room?.game.cameras[0].pos ?? Vector2.zero) + new Vector2(Mathf.Lerp(-20f, 1386f, Random.value), Mathf.Lerp(-200f, 968f, Random.value));
			lastPos = pos;
		}

		void ResetMe()
		{
			velRotAdd = Random.value * 360f;
			vel = Custom.RNV();
			scale = Random.value;
			rot = Random.value * 360f;
			lastRot = rot;
			rotSpeed = Mathf.Lerp(2f, 10f, Random.value) * (Random.value >= .5f ? 1f : -1f);
			yRot = Random.value * 3.14159274f;
			lastYRot = yRot;
			yRotSpeed = Mathf.Lerp(.02f, .05f, Random.value) * (Random.value >= .5f ? 1f : -1f);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = [new("Pebble" + Random.Range(1, 15).ToString(), true)];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
		{
			newContatiner ??= rCam.ReturnFContainer("Background");
			foreach (var s in sLeaser.sprites)
			{
				s.RemoveFromContainer();
				newContatiner.AddChild(s);
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			var sprite = sLeaser.sprites[0];
			sprite.isVisible = active && !reset;
			if (!active) 
				return;
			var t = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DegToVec(45f), Custom.DegToVec(Mathf.Lerp(lastYRot, yRot, timeStacker) * 57.29578f + Mathf.Lerp(lastRot, rot, timeStacker))));
			var a = Custom.HSL2RGB(.08611111f, .65f, Mathf.Lerp(.53f, 0f, 1f));
			var b = Custom.HSL2RGB(.08611111f, Mathf.Lerp(1f, .65f, 1f), Mathf.Lerp(1f, .53f, 1f));
			sprite.color = Color.Lerp(a, b, t);
			sprite.x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sprite.y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sprite.scaleX = Mathf.Lerp(.25f, .45f, scale) * Mathf.Sin(Mathf.Lerp(lastYRot, yRot, timeStacker) * 3.14159274f);
			sprite.scaleY = Mathf.Lerp(.35f, .65f, scale);
			sprite.rotation = Mathf.Lerp(lastRot, rot, timeStacker);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}
}