using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BannerlordFracturedPeace.Actions;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerlordFracturedPeace.Patches
{
    [HarmonyPatch(typeof(ApplyHeirSelectionAction))]
    public static class ApplyHeirSelectionActionPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ApplyInternal))]
        public static void ApplyInternal(Hero heir, bool isRetirement = false)
        {
            try
            {
                if (Main.Settings != null && Main.Settings.EnableSplitKingdomOnHeirSelection)
                {
                    Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;

                    bool triggerSplit = Main.Settings.AlwaysSplitKingdomOnHeirSelection;
                    if (!triggerSplit)
                    {
                        var chance = isRetirement ? Main.Settings.SplitOnRetirementChance : Main.Settings.SplitOnDeathChance;
                        var random = MBRandom.RandomFloatRanged(0.01f, 1.00f);
                        triggerSplit = random <= chance;
                    }

                    if (triggerSplit && heir.Clan?.Kingdom != null && heir.IsKingdomLeader)
                    {
                        SplitKingdomAction.Apply(heir.Clan.Kingdom);
                    }

                    Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                }
            }
            catch (System.Exception e) { TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString()); Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); }
        }
    }
}
