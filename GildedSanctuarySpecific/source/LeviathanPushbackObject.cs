using RWCustom;
using UnityEngine;

namespace GildedSanctuarySpecific;

public class LeviathanPushbackObject : UpdatableAndDeletable
{
	public PlacedObject.ResizableObjectData Data;

	public Vector2 Pos => Data.owner.pos;

	public float Rad => Data.handlePos.magnitude;

	public Vector2 Dir => Data.handlePos.normalized;

	public LeviathanPushbackObject(Room room, PlacedObject.ResizableObjectData data)
	{
		base.room = room;
		Data = data;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room is null)
			return;
		var crits = room.abstractRoom.creatures;
        for (var i = 0; i < crits.Count; i++)
		{
			var cr = crits[i];
			if (cr.realizedCreature is not BigEel b || b.room != room || !b.Consious || b.grabbedBy.Count != 0)
				continue;
			var chs = cr.realizedCreature.bodyChunks;
            for (var j = 0; j < chs.Length; j++)
			{
				var chunk = chs[j];
				if (Custom.DistLess(Custom.RestrictInRect(chunk.pos, room.RoomRect), Pos, Rad))
					chunk.vel += Dir * 5f * Mathf.InverseLerp(Rad, Rad - 60f, Vector2.Distance(Custom.RestrictInRect(chunk.pos, room.RoomRect), Pos));
			}
		}
	}
}
