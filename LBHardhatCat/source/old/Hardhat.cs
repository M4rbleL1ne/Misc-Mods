using UnityEngine;
using RWCustom;

namespace LBHardhatCat;

public sealed class Hardhat(Player player)
{
    readonly Player _player = player;
    Vector2 _rotation, _lastRotation, _rotVel;
    float _viewFromSide, _lastViewFromSide;
    bool _slatedForDeletion;
    FSprite? _sprite;

    public void Destroy() => _slatedForDeletion = true;

    public void Update()
    {
        if (_player is not Player p || _slatedForDeletion)
            return;
        _lastRotation = _rotation;
        _lastViewFromSide = _viewFromSide;
        _rotation = Custom.DegToVec(Custom.VecToDeg(_rotation) + _rotVel.x);
        _rotVel = Vector2.ClampMagnitude(_rotVel, 50f);
        _rotVel *= Custom.LerpMap(_rotVel.magnitude, 5f, 50f, 1f, .8f);
        if (p.graphicsModule is PlayerGraphics pgr)
        {
            var to = 0f;
            var pos = pgr.head.pos;
            var vector = Custom.PerpendicularVector(pos, p.mainBodyChunk.pos);
            vector *= Mathf.Sign(Custom.DistanceToLine(pos, p.firstChunk.pos, p.bodyChunks[1].pos));
            if (p.input[0].x != 0 && Mathf.Abs(p.bodyChunks[1].lastPos.x - p.bodyChunks[1].pos.x) > 2f)
                to = p.input[0].x;
            _rotation = Vector3.Slerp(_rotation, vector, .5f);
            _viewFromSide = Custom.LerpAndTick(_viewFromSide, to, .11f, 1f / 30f);
        }
    }

    public void InitiateSprites(FSprite[] sprites, int index, RoomCamera rCam)
    {
        _sprite = sprites[index] = new("lbhardhat1");
        rCam.ReturnFContainer("Items").AddChild(_sprite);
    }

    public void DrawSprites(RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (_player is not Player p || _slatedForDeletion || p.room != rCam.room || _sprite is not FSprite spr)
            return;
        Vector2 a = default,
            vector2 = Vector3.Slerp(_lastRotation, _rotation, timeStacker),
            vector3 = Vector2.up;
        float num2;
        if (p.graphicsModule is PlayerGraphics pgr)
        {
            num2 = Mathf.Lerp(_lastViewFromSide, _viewFromSide, timeStacker);
            var vector4 = Custom.DirVec(Vector2.Lerp(pgr.drawPositions[1, 1], pgr.drawPositions[1, 0], timeStacker), Vector2.Lerp(pgr.drawPositions[0, 1], pgr.drawPositions[0, 0], timeStacker));
            a = Vector2.Lerp(Vector2.Lerp(pgr.drawPositions[0, 1], pgr.drawPositions[0, 0], timeStacker) + vector4 * 3f, Vector2.Lerp(pgr.head.lastPos, pgr.head.pos, timeStacker) + vector4 * 3f, .5f) + Vector2.Lerp(pgr.lastLookDir, pgr.lookDirection, timeStacker) * 1.5f;
            vector2 = vector4;
            if (num2 != 0f)
            {
                vector2 = Custom.DegToVec(Custom.VecToDeg(vector4) - 20f * num2);
                vector3 = Vector3.Slerp(vector3, Custom.DegToVec(-50f * num2), Mathf.Abs(num2));
                a += vector4 * 2f * Mathf.Abs(num2);
                a -= Custom.PerpendicularVector(vector4) * 4f * num2;
            }
        }
        num2 = Custom.VecToDeg(vector2);
        spr.element = Futile.atlasManager.GetElementWithName("KrakenMask" + Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(num2 / 180f) * 8f), 0, 8));
        spr.scaleX = Mathf.Sign(num2);
        spr.anchorY = Custom.LerpMap(Mathf.Abs(num2), 0f, 100f, .5f, .675f, 2.1f);
        spr.anchorX = .5f - vector3.x * .1f * Mathf.Sign(num2);
        spr.rotation = num2;
        spr.SetPosition(a - camPos);
        spr.color = Color.Lerp(Color.white, rCam.currentPalette.blackColor, Mathf.Lerp(.2f, 1f, Mathf.Pow(rCam.room.Darkness(a) * (1f - rCam.room.LightSourceExposure(a)) * .8f, 2f)));
    }
}