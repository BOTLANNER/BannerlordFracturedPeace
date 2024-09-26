using System.Collections.Generic;

using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
#if MCM_v5
using MCM.Abstractions.Base.Global;
#else
using MCM.Abstractions.Settings.Base.Global;
#endif

namespace BannerlordFracturedPeace
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => $"{Main.Name}_v1";
        public override string DisplayName => Main.DisplayName;
        public override string FolderName => Main.Name;
        public override string FormatType => "json";

        private const string AddSplitKingdom_Hint = "Adds a button on Kingdom Encyclopedia to trigger a split. [ Default: ON ]";

        [SettingPropertyBool("Add 'Split Kingdom' Button", HintText = AddSplitKingdom_Hint, RequireRestart = false, Order = 0, IsToggle = false)]
        [SettingPropertyGroup("Encyclopedia Settings", GroupOrder = 0)]
        public bool AddSplitKingdom { get; set; } = true;

        private const string EnableSplitKingdomOnHeirSelection_Hint = "Enables splitting player kingdom after selecting an heir. [ Default: ON ]";

        [SettingPropertyBool("Split player kingdom after selecting an heir", HintText = EnableSplitKingdomOnHeirSelection_Hint, RequireRestart = false, Order = 0, IsToggle = true)]
        [SettingPropertyGroup("Heir Selection Settings", GroupOrder = 1)]
        public bool EnableSplitKingdomOnHeirSelection { get; set; } = true;

        private const string AlwaysSplitKingdomOnHeirSelection_Hint = "Always triggers the splitting of player kingdom after selecting an heir. Otherwise will apply based on percentage chances. [ Default: OFF ]";

        [SettingPropertyBool("Always split player kingdom after selecting an heir", HintText = AlwaysSplitKingdomOnHeirSelection_Hint, RequireRestart = false, Order = 1, IsToggle = false)]
        [SettingPropertyGroup("Heir Selection Settings")]
        public bool AlwaysSplitKingdomOnHeirSelection { get; set; } = false;

        private const string SplitOnDeathChance_Hint = "Percentage chance to trigger the splitting of player kingdom after selecting an heir on player death. [ Default: 50% ]";

        [SettingPropertyFloatingInteger("Split on player death chance", 0.00f, 1.00f, "#0%", HintText = SplitOnDeathChance_Hint, RequireRestart = false, Order = 2)]
        [SettingPropertyGroup("Heir Selection Settings")]
        public float SplitOnDeathChance { get; set; } = 0.5f;

        private const string SplitOnRetirementChance_Hint = "Percentage chance to trigger the splitting of player kingdom after selecting an heir on player retirement. [ Default: 25% ]";

        [SettingPropertyFloatingInteger("Split on player retirement chance", 0.00f, 1.00f, "#0%", HintText = SplitOnRetirementChance_Hint, RequireRestart = false, Order = 3)]
        [SettingPropertyGroup("Heir Selection Settings")]
        public float SplitOnRetirementChance { get; set; } = 0.25f;
    }
}
