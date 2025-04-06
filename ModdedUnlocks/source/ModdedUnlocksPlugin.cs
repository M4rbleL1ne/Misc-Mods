using BepInEx;
using System.Reflection;
using System.Security.Permissions;
using System.Security;
using System.Collections.Generic;
using MoreSlugcats;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ModdedUnlocks;

[BepInPlugin("lb-fgf-m4r-ik.modded-unlocks", nameof(ModdedUnlocks), "10.0.0")]
public sealed class ModdedUnlocksPlugin : BaseUnityPlugin
{
    static HashSet<MultiplayerUnlocks.SandboxUnlockID> s_sandbox = [];
    static HashSet<MultiplayerUnlocks.LevelUnlockID> s_levels = [];

    public void OnEnable()
    {
        On.RainWorld.PostModsInit += (orig, self) =>
        {
            orig(self);
            var sbox = s_sandbox;
            var lbox = s_levels;
            var sb = typeof(MultiplayerUnlocks.SandboxUnlockID).GetFields(BindingFlags.Public | BindingFlags.Static);
            for (var i = 0; i < sb.Length; i++)
                sbox.Add((MultiplayerUnlocks.SandboxUnlockID)sb[i].GetValue(null));
            var l = typeof(MultiplayerUnlocks.LevelUnlockID).GetFields(BindingFlags.Public | BindingFlags.Static);
            for (var i = 0; i < l.Length; i++)
                lbox.Add((MultiplayerUnlocks.LevelUnlockID)l[i].GetValue(null));
            if (ModManager.MSC)
            {
                sb = typeof(MoreSlugcatsEnums.SandboxUnlockID).GetFields(BindingFlags.Public | BindingFlags.Static);
                for (var i = 0; i < sb.Length; i++)
                    sbox.Add((MultiplayerUnlocks.SandboxUnlockID)sb[i].GetValue(null));
                l = typeof(MoreSlugcatsEnums.LevelUnlockID).GetFields(BindingFlags.Public | BindingFlags.Static);
                for (var i = 0; i < l.Length; i++)
                    lbox.Add((MultiplayerUnlocks.LevelUnlockID)l[i].GetValue(null));
            }
        };
        On.MultiplayerUnlocks.SandboxItemUnlocked += (orig, self, unlockID) => orig(self, unlockID) || (unlockID is not null && !s_sandbox.Contains(unlockID));
        On.MultiplayerUnlocks.IsLevelUnlocked += (orig, self, levelName) => orig(self, levelName) || (levelName is not null && !s_levels.Contains(MultiplayerUnlocks.LevelLockID(levelName)));
    }

    public void OnDisable()
    {
        s_sandbox?.Clear();
        s_sandbox = null;
        s_levels?.Clear();
        s_levels = null;
    }
}