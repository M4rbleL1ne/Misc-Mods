using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SeerStuff;

public class FindEchoBehavior : VoidSpawn.Behavior
{
    public Vector2 Point = new(float.PositiveInfinity, 0f), FinalDest;
    public bool Phase;

    public override Vector2 SwimTowards
    {
        get
        {
            if (!Phase)
            {
                if (Custom.DistLess(owner.mainBody[0].pos, Point, 70f))
                    Phase = true;
                return Point;
            }
            return FinalDest;
        }
    }

    public FindEchoBehavior(VoidSpawn owner, Room room, World world, int ghostRoomIndex) : base(owner)
    {
        var toExit = -1;
        var dst = float.MaxValue;
        var abRm = room.abstractRoom;
        var cons = abRm.connections;
        if (world.region is not Region reg)
        {
            owner.Destroy();
            return;
        }
        if (!SeerStuffPlugin.RegionEchoDirFinder.TryGetValue(reg, out var dirFinder))
            SeerStuffPlugin.RegionEchoDirFinder.Add(reg, dirFinder = new(world, reg, ghostRoomIndex));
        dirFinder.Work();
        for (var i = 0; i < cons.Length; i++)
        {
            var con = cons[i];
            if (con > -1)
            {
                var tempDst = dirFinder.DistanceToDestination(new(abRm.index, -1, -1, i));
                if (tempDst > -1f && tempDst < dst)
                {
                    toExit = i;
                    dst = tempDst;
                }
            }
        }
        var shs = room.shortcuts;
        for (var i = 0; i < shs.Length; i++)
        {
            var sh = shs[i];
            if (sh.destNode == toExit)
            {
                Point = room.MiddleOfTile(sh.StartTile);
                break;
            }
        }
        if (float.IsInfinity(Point.x))
            owner.Destroy();
        else
        {
            var num = toExit >= 0 && toExit < cons.Length ? cons[toExit] : -1;
            if (num >= 0)
            {
                var sz = world.GetAbstractRoom(num).size;
                var vector = world.RoomToWorldPos(new(sz.x * Random.value * 20f, sz.y * Random.value * 20f), num);
                FinalDest = vector - world.RoomToWorldPos(default, abRm.index) + Custom.DirVec(world.RoomToWorldPos(owner.mainBody[0].pos, abRm.index), vector) * 2000f;
            }
            else
                owner.Destroy();
        }
    }
}