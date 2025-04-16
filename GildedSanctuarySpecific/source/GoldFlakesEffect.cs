using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace GildedSanctuarySpecific;

sealed class GoldFlakesEffect : UpdatableAndDeletable
{
	readonly List<GoldFlake> _flakes = [];
	int _savedCamPos = -1;

	internal GoldFlakesEffect(Room room)
	{
		this.room = room;
		if (room?.roomSettings is RoomSettings rs)
		{
			var num = NumberOfFlakes(rs.GetEffectAmount(RoomEffectType.GRJGoldenFlakes));
			for (var j = 0; j < num; j++)
			{
				var lGoldFlake = new GoldFlake();
				_flakes.Add(lGoldFlake);
				room.AddObject(lGoldFlake);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room is Room rm && rm.roomSettings is RoomSettings rs)
		{
			var rCam = rm.game.cameras[0];
			if (rCam.currentCameraPosition != _savedCamPos)
			{
				_savedCamPos = rCam.currentCameraPosition;
				var num2 = NumberOfFlakes(rs.GetEffectAmount(RoomEffectType.GRJGoldenFlakes));
                var flks = _flakes;
                for (var i2 = 0; i2 < flks.Count; i2++)
				{
					var f = flks[i2];
                    if (i2 <= num2)
					{
						f._active = true;
						f.PlaceRandomlyInRoom();
						f._savedCamPos = _savedCamPos;
						f._reset = false;
					}
					else 
						f._active = false;
				}
			}
			if (!rm.BeingViewed)
			{
				var flks = _flakes;
                for (var j2 = 0; j2 < flks.Count; j2++) 
					flks[j2].Destroy();
				Destroy();
			}
		}
    }

	int NumberOfFlakes(float amount) => (int)(200f * Mathf.Pow(amount, 2f));

	internal sealed class GoldFlake : CosmeticSprite
	{
		float _scale, _rot, _lastRot, _yRot, _lastYRot, _rotSpeed, _yRotSpeed, _velRotAdd;
		internal int _savedCamPos;
		internal bool _reset, _active;

		internal GoldFlake()
		{
			_savedCamPos = -1;
			ResetMe();
		}

		public override void Update(bool eu)
		{
			if (!_active)
			{
				_savedCamPos = -1;
				return;
			}
			base.Update(eu);
			if (room is not Room rm)
				return;
			var rCam = rm.game.cameras[0];
			vel *= .82f;
			vel.y -= .25f;
			vel += Custom.DegToVec(180f + Mathf.Lerp(-45f, 45f, Random.value)) * .1f + Custom.DegToVec(_rot + _velRotAdd + _yRot) * Mathf.Lerp(.1f, .25f, Random.value);
			if (rm.GetTile(pos).Solid && rm.GetTile(lastPos).Solid) 
				_reset = true;
			if (_reset)
			{
				pos = rCam.pos + new Vector2(Mathf.Lerp(-20f, 1386f, Random.value), Mathf.Lerp(-200f, 968f, Random.value));
				lastPos = pos;
				ResetMe();
				_reset = false;
				vel *= 0f;
				return;
			}
			if (pos.x < rCam.pos.x - 20f) 
				_reset = true;
			if (pos.x > rCam.pos.x + 1366f + 20f)
				_reset = true;
			if (pos.y < rCam.pos.y - 200f) 
				_reset = true;
			if (pos.y > rCam.pos.y + 768f + 200f) 
				_reset = true;
			if (rCam.currentCameraPosition != _savedCamPos)
			{
				PlaceRandomlyInRoom();
				_savedCamPos = rCam.currentCameraPosition;
			}
			if (!rm.BeingViewed)
				Destroy();
			_lastRot = _rot;
			_rot += _rotSpeed;
			_rotSpeed = Mathf.Clamp(_rotSpeed + Mathf.Lerp(-1f, 1f, Random.value) / 30f, -10f, 10f);
			_lastYRot = _yRot;
			_yRot += _yRotSpeed;
			_yRotSpeed = Mathf.Clamp(_yRotSpeed + Mathf.Lerp(-1f, 1f, Random.value) / 320f, -.05f, .05f);
		}

		internal void PlaceRandomlyInRoom()
		{
			ResetMe();
			pos = (room?.game.cameras[0].pos ?? default) + new Vector2(Mathf.Lerp(-20f, 1386f, Random.value), Mathf.Lerp(-200f, 968f, Random.value));
			lastPos = pos;
		}

		void ResetMe()
		{
			_velRotAdd = Random.value * 360f;
			vel = Custom.RNV();
			_scale = Random.value;
			_rot = Random.value * 360f;
			_lastRot = _rot;
			_rotSpeed = Mathf.Lerp(2f, 10f, Random.value) * (Random.value >= .5f ? 1f : -1f);
			_yRot = Random.value * 3.14159274f;
			_lastYRot = _yRot;
			_yRotSpeed = Mathf.Lerp(.02f, .05f, Random.value) * (Random.value >= .5f ? 1f : -1f);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = [new("Pebble" + Random.Range(1, 15).ToString(), true) { shader = Custom.rainWorld.Shaders["RippleBasicBothSides"] }];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
		{
			newContainer ??= rCam.ReturnFContainer("Background");
			var sprs = sLeaser.sprites;
			for (var i = 0; i < sprs.Length; i++)
			{
				var s = sprs[i];
				s.RemoveFromContainer();
				newContainer.AddChild(s);
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			var sprite = sLeaser.sprites[0];
			sprite.isVisible = _active && !_reset;
			if (!_active) 
				return;
			var t = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(new(.70710677f, .70710677f), Custom.DegToVec(Mathf.Lerp(_lastYRot, _yRot, timeStacker) * 57.29578f + Mathf.Lerp(_lastRot, _rot, timeStacker))));
			var a = Custom.HSL2RGB(.08611111f, .65f, Mathf.Lerp(.53f, 0f, 1f));
			var b = Custom.HSL2RGB(.08611111f, Mathf.Lerp(1f, .65f, 1f), Mathf.Lerp(1f, .53f, 1f));
			sprite.color = Color.Lerp(a, b, t);
			sprite.SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
			sprite.scaleX = Mathf.Lerp(.25f, .45f, _scale) * Mathf.Sin(Mathf.Lerp(_lastYRot, _yRot, timeStacker) * Mathf.PI);
			sprite.scaleY = Mathf.Lerp(.35f, .65f, _scale);
			sprite.rotation = Mathf.Lerp(_lastRot, _rot, timeStacker);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}
}