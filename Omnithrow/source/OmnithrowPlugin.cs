using BepInEx;
using UnityEngine;
using System.Security.Permissions;
using System.Security;
using System.Runtime.CompilerServices;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Omnithrow;

[BepInPlugin("lb-fgf-m4r-ik.omnithrow-port", nameof(Omnithrow), "10.0.0")]
public sealed class OmnithrowPlugin : BaseUnityPlugin
{
    public static ConditionalWeakTable<Player, StrongBox<Vector2>> LastThrowDirection = new();

    public void OnEnable()
    {
        On.Player.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            if (!LastThrowDirection.TryGetValue(self, out _))
                LastThrowDirection.Add(self, new());
        };
        On.Player.Update += (orig, self, eu) =>
        {
            if (self.input is Player.InputPackage[] ip && ip.Length > 0)
            {
                ref readonly var inp = ref ip[0];
                var vec = new Vector2(inp.x, inp.y).normalized;
                if (LastThrowDirection.TryGetValue(self, out var dir) && vec.magnitude > 0f)
                    dir.Value = vec;
            }
            orig(self, eu);
        };
        On.Player.ThrowObject += (orig, self, grasp, eu) =>
        {
            var grabbed = self.grasps?[grasp]?.grabbed;
            orig(self, grasp, eu);
            if (grabbed is Weapon w && LastThrowDirection.TryGetValue(self, out var dir))
            {
                var val = dir.Value;
                var chs = w.bodyChunks;
                var mpos = self.mainBodyChunk.pos + val * 10f;
                for (var i = 0; i < chs.Length; i++)
                {
                    var ch = chs[i];
                    ch.pos = mpos;
                    ch.vel = val * 40f;
                }
                w.setRotation = val;
            }
        };
    }

    public void OnDisable() => LastThrowDirection = null!;
}