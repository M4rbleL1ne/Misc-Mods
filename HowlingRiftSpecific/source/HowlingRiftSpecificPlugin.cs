using BepInEx;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace HowlingRiftSpecific;

[BepInPlugin("lb-fgf-m4r-ik.howling-rift-specific", "HowlingRiftSpecific", "10.0.0")]
public sealed class HowlingRiftSpecificPlugin : BaseUnityPlugin
{
	public void OnEnable() => On.Lightning.ctor += (orig, self, room, intensity, bkgOnly) =>
    {
        orig(self, room, intensity, bkgOnly);
        if (room?.world?.region?.name == "HC")
        {
            self.bkgGradient[0] = new(.984313727f, .564705881f, .160784325f);
            self.bkgGradient[1] = new(1f, .5f, 0f);
        }
    };
}