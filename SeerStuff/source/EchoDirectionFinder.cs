using System.Collections.Generic;
using System.Runtime.InteropServices;
using RWCustom;

namespace SeerStuff;

[StructLayout(LayoutKind.Sequential)]
public class EchoDirectionFinder
{
    public World World;
    public Region Region;
    public float[][] Matrix;
    public List<IntVector2> CheckNext;
    public CreatureTemplate FlyTemplate;
    public int GhostRoomIndex = -1;
    public bool Done;

    public EchoDirectionFinder(World world, Region region, int ghostRoomIndex)
    {
        FlyTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly);
        Region = region;
        World = world;
        var chk = CheckNext = [];
        var mat = Matrix = new float[region.numberOfRooms][];
        float[] matP;
        for (var i = 0; i < mat.Length; i++)
        {
            matP = mat[i] = world.GetAbstractRoom(i + region.firstRoomIndex) is AbstractRoom rm ? new float[rm.connections.Length] : [];
            for (var j = 0; j < matP.Length; j++)
                matP[j] = -1f;
        }
        GhostRoomIndex = ghostRoomIndex;
        if (region.IsRoomInRegion(ghostRoomIndex))
        {
            var ind = ghostRoomIndex - region.firstRoomIndex;
            matP = mat[ind];
            for (var m = 0; m < matP.Length; m++)
            {
                chk.Add(new(ind, m));
                matP[m] = 0f;
            }
        }
    }

    public virtual void Work()
    {
        while (!Done)
            Update();
    }

    public virtual void Update()
    {
        if (Done)
            return;
        var chk = CheckNext;
        if (chk.Count == 0)
        {
            Done = true;
            return;
        }
        var num = float.MaxValue;
        var num2 = -1;
        for (var i = 0; i < chk.Count; i++)
        {
            var num3 = ResistanceOfCell(chk[i]);
            if (num3 > -1f && num3 < num)
            {
                num = num3;
                num2 = i;
            }
        }
        if (num2 < 0 || num2 >= chk.Count)
        {
            Done = true;
            return;
        }
        var testCell = chk[num2];
        chk.RemoveAt(num2);
        var abstractRoom = World.GetAbstractRoom(testCell.x + Region.firstRoomIndex);
        if (abstractRoom is null)
            return;
        var num4 = ResistanceOfCell(testCell);
        var cons = abstractRoom.connections;
        var nodes = abstractRoom.nodes;
        for (var j = 0; j < cons.Length && j < nodes.Length; j++)
        {
            float num5;
            WorldCoordinate worldCoordinate;
            var con = cons[j];
            if (j == testCell.y && con > -1)
            {
                worldCoordinate = new(con, -1, -1, World.GetAbstractRoom(con)?.ExitIndex(abstractRoom.index) ?? -1);
                num5 = World.TotalShortCutLengthBetweenTwoConnectedRooms(abstractRoom.index, con);
            }
            else
            {
                var nd = nodes[j];
                worldCoordinate = new(abstractRoom.index, -1, -1, j);
                num5 = nd.ConnectionLength(testCell.y, FlyTemplate);
                if (num5 == -1f)
                    num5 = 10000f;
            }
            var ind = worldCoordinate.room - Region.firstRoomIndex;
            if (num5 > -1f && ind >= 0 && ind < Matrix.Length)
            {
                var matP = Matrix[ind];
                var nd = worldCoordinate.abstractNode;
                if (nd >= 0 && nd < matP.Length && matP[nd] == -1f)
                {
                    matP[nd] = num4 + num5;
                    chk.Add(new(ind, nd));
                }
            }
        }
    }

    public virtual float ResistanceOfCell(IntVector2 testCell)
    {
        if (testCell.x < 0 || testCell.x >= Region.numberOfRooms)
            return -1f;
        if (testCell.y < 0)
            return -1f;
        var matP = Matrix[testCell.x];
        if (testCell.y >= matP.Length)
            return -1f;
        return matP[testCell.y];
    }

    public virtual float DistanceToDestination(WorldCoordinate testPos)
    {
        var ind = testPos.room - Region.firstRoomIndex;
        if (ind < 0 || ind >= Region.numberOfRooms)
            return -1f;
        var matP = Matrix[ind];
        ind = testPos.abstractNode;
        if (ind < 0 || ind >= matP.Length)
            return -1f;
        return matP[ind];
    }
}