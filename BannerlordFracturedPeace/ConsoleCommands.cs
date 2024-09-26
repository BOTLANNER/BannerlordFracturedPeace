using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BannerlordFracturedPeace.Actions;

using Helpers;

using TaleWorlds.AchievementSystem;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.PlatformService.Steam;

using static TaleWorlds.Library.CommandLineFunctionality;

namespace BannerlordFracturedPeace
{
    class ConsoleCommands
    {

        [CommandLineArgumentFunction("help", "fractured_peace")]
        public static string Help(List<string> args)
        {
            string helpResponse = @"
Commands:

fractured_peace.help                               - Shows console command usage
fractured_peace.split_all                          - Splits each kingdom
fractured_peace.list_kingdoms                      - List all supported kingdom names for use with fractured_peace.split
fractured_peace.split             kingdomName      - Split the specified kingdom name (Call fractured_peace.list_kingdoms for all supported kingdom names)


";

            return helpResponse;
        }

        [CommandLineArgumentFunction("split_all", "fractured_peace")]
        public static string SplitAll(List<string> args)
        {
            try
            {
                if (Campaign.Current == null || Campaign.Current.Kingdoms == null || Campaign.Current.Kingdoms.Count <= 0)
                {
                    return "Must be in a campaign or sandbox session!";
                }


                string result = "";
                var all = Campaign.Current.Kingdoms.ToList();
                foreach (var kingdom in all)
                {
                    if (kingdom.IsEliminated || kingdom.Leader == null)
                    {
                        continue;
                    }

                    result += "\r\n";
                    bool success = SplitKingdomInternal(kingdom, out string output);
                    result += kingdom.InformalName.ToString() + " / " + kingdom.Name.ToString() + " / " + kingdom.GetName().ToString() + " - " + output;
                }
                return result;
            }
            catch (System.Exception e)
            {
                return e.ToString();
            }
        }

        [CommandLineArgumentFunction("list_kingdoms", "fractured_peace")]
        public static string ListKingdoms(List<string> args)
        {
            try
            {
                if (Campaign.Current == null || Campaign.Current.Kingdoms == null || Campaign.Current.Kingdoms.Count <= 0)
                {
                    return "Must be in a campaign or sandbox session!";
                }


                string result = "";
                foreach (var kingdom in Campaign.Current.Kingdoms)
                {
                    if (kingdom.IsEliminated || kingdom.Leader == null)
                    {
                        continue;
                    }
                    result += "\r\n";
                    result += kingdom.InformalName.ToString() + " or " + kingdom.Name.ToString() + " or " + kingdom.GetName().ToString();
                }
                return result;
            }
            catch (System.Exception e)
            {
                return e.ToString();
            }


        }

        [CommandLineArgumentFunction("split", "fractured_peace")]
        public static string Split(List<string> args)
        {
            try
            {
                if (args.Count > 0)
                {
                    string kingdom = ArgsToString(args).Replace("\"", "").ToLower();

                    if (Campaign.Current == null || Campaign.Current.Kingdoms == null || Campaign.Current.Kingdoms.Count <= 0)
                    {
                        return "Must be in a campaign or sandbox session!";
                    }


                    foreach (var k in Campaign.Current.Kingdoms)
                    {
                        var name = k.GetName().ToString().ToLower();
                        var name2 = k.Name.ToString().ToLower();
                        var name3 = k.InformalName.ToString().ToLower();
                        if (name == kingdom || name2 == kingdom || name3 == kingdom)
                        {
                            if (k.IsEliminated || k.Leader == null)
                            {
                                continue;
                            }
                            bool result = SplitKingdomInternal(k, out string output);
                            return output;
                        }
                    }
                    return "Kingdom not found!";
                }
                else
                {
                    return Help(args);
                }
            }
            catch (System.Exception e)
            {
                return e.ToString();
            }
        }

        internal static bool SplitKingdomInternal(Kingdom kingdom, out string output)
        {
            try
            {
                if (kingdom.IsEliminated || kingdom.Leader == null)
                {
                    output = "Kingdom is not valid!";
                    return false;
                }
                var name = kingdom.Name.ToString();

                var results = SplitKingdomAction.Apply(kingdom);

                output = $"Split {name} into \r\n\t{string.Join("\r\n\t", results.Select(r => r.Name.ToString()))}";
                return true;
            }
            catch (System.Exception e)
            {
                output = e.ToString();
                return false;
            }
        }

        private static string ArgsToString(List<string> args)
        {
            return string.Join(" ", args).Trim();
        }
    }
}
