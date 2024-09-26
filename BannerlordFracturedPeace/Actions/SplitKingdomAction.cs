using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Helpers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerlordFracturedPeace.Actions
{
    public static class SplitKingdomAction
    {
        public static MBList<Kingdom> Apply(Kingdom kingdom)
        {
            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
            var oldLeader = kingdom.Leader;
            var newKingdoms = new MBList<Kingdom>
            {
                kingdom
            };

            var primaryTown = kingdom.Settlements.FirstOrDefault(s => s.Owner == oldLeader && s.IsTown);
            if (primaryTown == null)
            {
                primaryTown = kingdom.Settlements.FirstOrDefault(s => s.Owner == oldLeader)?.Town?.Settlement;
                if (primaryTown == null)
                {
                    primaryTown = kingdom.Settlements.FirstOrDefault(s => s.IsTown);
                    if (primaryTown == null)
                    {
                        primaryTown = kingdom.Settlements.FirstOrDefault(s => s.IsCastle);
                        if (primaryTown == null)
                        {
                            primaryTown = kingdom.Settlements.FirstOrDefault();
                        }
                    }
                }
            }

            if (primaryTown == null)
            {
                // Cannot split, kingdom doesn't even have a single town
                return newKingdoms;
            }

            var newFactionTowns = kingdom.Settlements.Where(s => s.IsTown && s != primaryTown).ToList();
            var rebellionsCampaignBehavior = SandBoxManager.Instance?.GameStarter?.CampaignBehaviors?.FirstOrDefault(b => b is RebellionsCampaignBehavior) as RebellionsCampaignBehavior;

            foreach (var town in newFactionTowns)
            {
                var rebelClan = GetRulingClan(kingdom, oldLeader, newKingdoms, primaryTown, town);

                if (rebelClan == null)
                {
                    if (rebellionsCampaignBehavior != null)
                    {
                        if (!town.InRebelliousState)
                        {
                            rebellionsCampaignBehavior.StartRebellionEvent(town);
                        }
                    }
                    else
                    {
                        // No more clans to split off, rebellion behaviour is missing
                        break;
                    }
                }
                else
                {
                    var encyclopediaTitle = new TextObject("{=ZOEamqUd}Kingdom of {NAME}", null);
                    encyclopediaTitle.SetTextVariable("NAME", town.Name);

                    var rebelKingdom = CreateSplitKingdom(town, town.Name, town.Name, town.Culture, rebelClan, kingdom.ActivePolicies.ToMBList(), town.EncyclopediaText, encyclopediaTitle);

                    if (town.Owner != rebelKingdom.Leader)
                    {
                        TaleWorlds.CampaignSystem.Actions.ChangeOwnerOfSettlementAction.ApplyByRebellion(rebelKingdom.Leader, town);
                    }

                    newKingdoms.Add(rebelKingdom);
                }

            }

            var clansLeft = kingdom.Clans.Where(c => !newKingdoms.Any(k => k.RulingClan == c)).OrderByDescending(c => c.Influence).ToList();

            while (clansLeft.Count > 0)
            {
                foreach (var k in newKingdoms)
                {
                    if (clansLeft.Count == 0)
                    {
                        break;
                    }

                    var clan = clansLeft.Last();
                    var banner = clan.Banner.Serialize();
                    if (clan.Kingdom != k)
                    {
                        TaleWorlds.CampaignSystem.Actions.
                        ChangeKingdomAction.ApplyByJoinToKingdomByDefection(clan, k);
                    }

                    clan.Banner.Deserialize(banner);
                    clansLeft.Remove(clan);
                }
            }


            foreach (var k in newKingdoms)
            {
                foreach (IFaction faction in newKingdoms)
                {
                    if (faction != k && !faction.IsAtWarWith(k) && !k.IsAtWarWith(faction))
                    {
                        DeclareWarAction.ApplyByKingdomCreation(k, faction);
                    }
                }
            }

            return newKingdoms;
        }

        private static Clan? GetRulingClan(Kingdom kingdom, Hero oldLeader, List<Kingdom> kingdoms, TaleWorlds.CampaignSystem.Settlements.Settlement primaryTown, TaleWorlds.CampaignSystem.Settlements.Settlement town)
        {
            var rebelClan = town.OwnerClan;
            var newLeader = town.Owner;

            var isPartOfOldRegime = newLeader == oldLeader || newLeader == primaryTown.Owner || newLeader.Clan == oldLeader.Clan || newLeader.Clan == primaryTown.OwnerClan;
            var rulesAnyNewRegime = kingdoms.Any(k => newLeader == k.Leader || newLeader.Clan == k.RulingClan);
            if (isPartOfOldRegime || rulesAnyNewRegime)
            {
                rebelClan = kingdom.Clans.Where(c => c != oldLeader.Clan && c != primaryTown.OwnerClan && !kingdoms.Any(k => k.Leader == c.Leader || k.RulingClan == c)).OrderByDescending(c => c.Influence).FirstOrDefault();
                newLeader = rebelClan?.Leader;
            }

            return rebelClan;
        }

        private static Kingdom CreateSplitKingdom(Settlement forTown, TextObject kingdomName, TextObject informalName, CultureObject culture, Clan founderClan, MBReadOnlyList<PolicyObject>? initialPolicies = null, TextObject? encyclopediaText = null, TextObject? encyclopediaTitle = null, TextObject? encyclopediaRulerTitle = null)
        {
            //var banner = new Banner(); // (founderClan.Banner.Serialize(), BannerManager.ColorPalette.GetRandomElementInefficiently().Value.Color, BannerManager.ColorPalette.GetRandomElementInefficiently().Value.Color); // Banner.CreateRandomClanBanner(founderClan.StringId.GetDeterministicHashCode()) ?? 

            //MBFastRandom mBFastRandom = new MBFastRandom((uint) (forTown.StringId.GetDeterministicHashCode() + founderClan.StringId.GetDeterministicHashCode()));
            //BannerData iconData = new BannerData(BannerManager.Instance.GetRandomBackgroundId(mBFastRandom), mBFastRandom.Next(BannerManager.ColorPalette.Count), mBFastRandom.Next(BannerManager.ColorPalette.Count), new Vec2(1528f, 1528f), new Vec2(764f, 764f), drawStroke: false, mirror: false, 0f);
            //banner.AddIconData(iconData);
            //var iconMeshId = BannerManager.Instance.GetRandomBannerIconId(new MBFastRandom((uint) (forTown.StringId.GetDeterministicHashCode() + kingdomName.GetHashCode())));
            //banner.AddIconData(new BannerData(iconMeshId, BannerManager.ColorPalette.GetRandomElementInefficiently().Key, BannerManager.ColorPalette.GetRandomElementInefficiently().Key, new Vec2(512f, 512f), new Vec2(764f, 764f), drawStroke: false, mirror: false, 0f));

            //banner.ChangePrimaryColor(BannerManager.ColorPalette.GetRandomElementInefficiently().Value.Color);
            //banner.ChangeIconColors(BannerManager.ColorPalette.GetRandomElementInefficiently().Value.Color);
            Banner banner = Banner.CreateRandomClanBanner(forTown.StringId.GetDeterministicHashCode() + founderClan.StringId.GetDeterministicHashCode());

            founderClan.InitializeClan(founderClan.Name, founderClan.InformalName, founderClan.Culture, banner, initialPosition: founderClan.InitialPosition, isDeserialize: true);

            Kingdom kingdom = Kingdom.CreateKingdom("new_kingdom");
            if (encyclopediaTitle == null)
            {
                encyclopediaTitle = new TextObject("{=ZOEamqUd}Kingdom of {NAME}", null);
                encyclopediaTitle.SetTextVariable("NAME", founderClan.Name);
            }
            if (encyclopediaText == null)
            {
                encyclopediaText = new TextObject("{=drZC1Frp}The {KINGDOM_NAME} was created in {CREATION_YEAR} by {RULER.NAME}, leader of a group of {CULTURE_ADJECTIVE} rebels.", null);
                encyclopediaText.SetTextVariable("KINGDOM_NAME", encyclopediaTitle);
                encyclopediaText.SetTextVariable("CREATION_YEAR", CampaignTime.Now.GetYear);
                encyclopediaText.SetTextVariable("CULTURE_ADJECTIVE", FactionHelper.GetAdjectiveForFactionCulture(culture));
                StringHelpers.SetCharacterProperties("RULER", founderClan.Leader.CharacterObject, encyclopediaText, false);
            }
            if (encyclopediaRulerTitle == null)
            {
                Kingdom kingdom1 = Kingdom.All.FirstOrDefault<Kingdom>((Kingdom x) => x.Culture == culture);
                encyclopediaRulerTitle = (kingdom1 != null ? kingdom1.EncyclopediaRulerTitle : TextObject.Empty);
            }
            kingdom.InitializeKingdom(kingdomName, informalName, culture, banner, banner.GetPrimaryColor(), banner.GetSecondaryColor(), forTown ?? founderClan.HomeSettlement, encyclopediaText, encyclopediaTitle, encyclopediaRulerTitle);
            List<IFaction> factions = new List<IFaction>(FactionManager.GetEnemyFactions(founderClan));
            ChangeKingdomAction.ApplyByCreateKingdom(founderClan, kingdom, false);
            foreach (IFaction faction in factions)
            {
                DeclareWarAction.ApplyByKingdomCreation(kingdom, faction);
            }
            if (initialPolicies != null)
            {
                foreach (PolicyObject initialPolicy in initialPolicies)
                {
                    kingdom.AddPolicy(initialPolicy);
                }
            }
            CampaignEventDispatcher.Instance.OnKingdomCreated(kingdom);

            //founderClan.Banner.Deserialize(banner.Serialize());
            //kingdom.Banner.Deserialize(banner.Serialize());

            founderClan.UpdateBannerColor(banner.GetPrimaryColor(), banner.GetSecondaryColor());
            founderClan.Kingdom = kingdom;

            return kingdom;
        }
    }
}
