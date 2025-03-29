using RWCustom;
using UnityEngine;

namespace GildedSanctuarySpecific;

public class LeviathanPushbackObject : UpdatableAndDeletable
{
	public PlacedObject.ResizableObjectData data;

	public Vector2 Pos => data.owner.pos;

	public float Rad => data.handlePos.magnitude;

	public Vector2 Dir => data.handlePos.normalized;

	public LeviathanPushbackObject(Room room, PlacedObject.ResizableObjectData data)
	{
		base.room = room;
		this.data = data;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room is null)
			return;
		for (var i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			var cr = room.abstractRoom.creatures[i];
			if (cr.realizedCreature is not BigEel b || b.room != room || !b.Consious || b.grabbedBy.Count != 0)
				continue;
			for (var j = 0; j < cr.realizedCreature.bodyChunks.Length; j++)
			{
				var chunk = cr.realizedCreature.bodyChunks[j];
				if (Custom.DistLess(Custom.RestrictInRect(chunk.pos, room.RoomRect), Pos, Rad))
					chunk.vel += Dir * 5f * Mathf.InverseLerp(Rad, Rad - 60f, Vector2.Distance(Custom.RestrictInRect(chunk.pos, room.RoomRect), Pos));
			}
		}
	}
}
