using BepInEx;
using System.Security;
using System.Security.Permissions;
using MoreSlugcats;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace UnknownTokenFix;

[BepInPlugin("lb-fgf-m4r-ik.unknown-token-fix", "UnknownTokenFix", "10.0.0")]
sealed class UnknownTokenFixMod : BaseUnityPlugin
{
    public void OnEnable() => On.CollectToken.AvailableToPlayer += On_CollectToken_AvailableToPlayer;

    static bool On_CollectToken_AvailableToPlayer(On.CollectToken.orig_AvailableToPlayer orig, CollectToken self)
    {
        if (self.placedObj?.data is CollectToken.CollectTokenData d)
        {
            if ((d.SandboxUnlock is MultiplayerUnlocks.SandboxUnlockID id && id.Index < 0) ||
                (d.SlugcatUnlock is MultiplayerUnlocks.SlugcatUnlockID id2 && id2.Index < 0) ||
                (d.ChatlogCollect is ChatlogData.ChatlogID id3 && id3.Index < 0) ||
                (d.LevelUnlock is MultiplayerUnlocks.LevelUnlockID id4 && id4.Index < 0) ||
                (d.SafariUnlock is MultiplayerUnlocks.SafariUnlockID id5 && id5.Index < 0))
                return false;
        }
        return orig(self);
    }
}