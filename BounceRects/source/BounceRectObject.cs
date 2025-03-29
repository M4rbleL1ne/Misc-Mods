using RWCustom;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BounceRects;

public sealed class BounceRectObject : UpdatableAndDeletable
{
    [AllowNull] public static ConditionalWeakTable<Room, List<BounceRectObject>> BounceRects = new();
    [AllowNull] public static ConditionalWeakTable<PhysicalObject, StrongBox<float>> DefaultBounce = new();

	public IntRect Rect;
    readonly PlacedObject _pObj;

	public BounceRectObject(Room room, PlacedObject pObj)
	{
		this.room = room;
        _pObj = pObj;
		Rect = ((PlacedObject.GridRectObjectData)pObj.data).Rect;
		if (BounceRects.TryGetValue(room, out var rects))
			rects.Add(this);
        else
            BounceRects.Add(room, [this]);
	}

    public override void Update(bool eu)
    {
        if (_pObj?.data is PlacedObject.GridRectObjectData d)
            Rect = d.Rect;
        base.Update(eu);
    }

    public override void Destroy()
    {
        if (room is not null && BounceRects.TryGetValue(room, out var rects))
            rects.Remove(this);
        base.Destroy();
    }
}