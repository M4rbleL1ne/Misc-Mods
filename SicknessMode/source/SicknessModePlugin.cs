using BepInEx;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace SicknessModePlugin;

[BepInPlugin("lb-fgf-m4r-ik.sickness-mode-plugin", "SicknessModePlugin", "2.0.0")]
sealed class SicknessModePlugin : BaseUnityPlugin
{
	public void OnEnable()
    {
		On.SlugcatStats.ctor += (orig, self, slugcatNumber, malnourished) => orig(self, slugcatNumber, true);
    }
}
