using BepInEx;
using UnityEngine;
using System.Security.Permissions;
using System.Security;
using System.Runtime.CompilerServices;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Omnithrow;

[BepInPlugin("lb-fgf-m4r-ik.omnithrow-port", nameof(Omnithrow), "2.0.1")]
sealed class OmnithrowPlugin : BaseUnityPlugin
{
    public static ConditionalWeakTable<Player, OmnithrowDir>? Omni = new();

    internal sealed class OmnithrowDir
    {
        internal Vector2 lastThrowDirection;
    }

    public void OnEnable()
    {
        On.Player.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            if (!Omni!.TryGetValue(self, out _))
                Omni.Add(self, new());
        };
        On.Player.Update += (orig, self, eu) =>
        {
            if (self.input is Player.InputPackage[] ip && ip.Length > 0)
            {
                var vec = new Vector2(ip[0].x, ip[0].y).normalized;
                if (Omni!.TryGetValue(self, out var o) && vec.magnitude > 0f)
                    o.lastThrowDirection = vec;
            }
            orig(self, eu);
        };
        On.Player.ThrowObject += (orig, self, grasp, eu) =>
        {
            var grabbed = self.grasps?[grasp]?.grabbed;
            orig(self, grasp, eu);
            if (grabbed is Weapon w && Omni!.TryGetValue(self, out var o))
            {
                for (var i = 0; i < w.bodyChunks.Length; i++)
                {
                    var obj = w.bodyChunks[i];
                    obj.pos = self.mainBodyChunk.pos + o.lastThrowDirection * 10f;
                    obj.vel = o.lastThrowDirection * 40f;
                }
                w.setRotation = o.lastThrowDirection;
            }
        };
    }

    public void OnDisable() => Omni = null;
}