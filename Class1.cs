using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Linq;


namespace IncreaseHuntressRange
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.TeaBoneJones.IncreaseHuntressRange", "Increase Huntress Range", "1.0.0")]
    public class IncreaseHuntressRange : BaseUnityPlugin
    {
        private static ConfigWrapper<bool> godMode;
        private static ConfigWrapper<float> scaleFactor;
        private static ConfigWrapper<float> baseValue;

        public void Awake()
        {
            godMode = Config.Wrap<bool>("Huntress", "GodMode", "Set the range to be so massive that you can attack any enemy that is visible", false);
            scaleFactor = Config.Wrap<float>("Huntress", "ScaleFactor", "Set how much the range increases (in meters) each level. Recommended: 3 (but I'm not your dad)", 3f);
            baseValue = Config.Wrap<float>("Huntress", "Base Value", "Base range (Original is 60)", 100f);

            On.RoR2.HuntressTracker.SearchForTarget += (orig, self, aimRay) =>
            {
                float distance;
                if (godMode.Value)
                {
                    distance = 10000f;
                }
                else
                {
                    // get current level
                    uint currentLevel = TeamManager.instance.GetTeamLevel(TeamIndex.Player);
                    // change distance based on level.
                    // level 1 = 60 (default)
                    // level 40 = 177
                    distance = baseValue + (scaleFactor.Value * (currentLevel - 1));
                }

                self.GetFieldValue<BullseyeSearch>("search").teamMaskFilter = TeamMask.all;
                self.GetFieldValue<BullseyeSearch>("search").teamMaskFilter.RemoveTeam(self.GetFieldValue<TeamComponent>("teamComponent").teamIndex);
                self.GetFieldValue<BullseyeSearch>("search").filterByLoS = true;
                self.GetFieldValue<BullseyeSearch>("search").searchOrigin = aimRay.origin;
                self.GetFieldValue<BullseyeSearch>("search").searchDirection = aimRay.direction;
                self.GetFieldValue<BullseyeSearch>("search").sortMode = BullseyeSearch.SortMode.Distance;

                self.GetFieldValue<BullseyeSearch>("search").maxDistanceFilter = distance;

                self.GetFieldValue<BullseyeSearch>("search").maxAngleFilter = self.maxTrackingAngle;
                self.GetFieldValue<BullseyeSearch>("search").RefreshCandidates();
                self.SetFieldValue<HurtBox>("trackingTarget", self.GetFieldValue<BullseyeSearch>("search").GetResults().FirstOrDefault<HurtBox>());
            };
        }
    }
}