using BepInEx;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace CourageMousePlugin;

[BepInPlugin("lb-fgf-m4r-ik.courage-mouse-port", "CourageMousePlugin", "2.0.0")]
sealed class CourageMousePlugin : BaseUnityPlugin
{
    public void OnEnable() => On.MouseAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => dRelation?.trackerRep?.representedCreature?.creatureTemplate.type == CreatureTemplate.Type.Slugcat ? new(CreatureTemplate.Relationship.Type.Ignores, 0f) : orig(self, dRelation);
}