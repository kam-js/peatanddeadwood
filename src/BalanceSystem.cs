using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AssetEdit = System.Func<Newtonsoft.Json.Linq.JObject, bool>;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;

namespace PeatAndDeadwood
{
    /// <summary>
    /// Rebalances Immersive Firewood by scaling its own values instead of replacing them, and
    /// scales every seasoning/drying time to the world's days-per-month setting.
    ///
    /// Runs on the patched asset JSON, after the JSON patch loader (0.05) and before blocks and
    /// items are registered (0.2): blocks such as the pit kiln cache their attributes when they
    /// load, so editing the registered objects afterwards would be too late. Server only, since
    /// the client receives collectibles from the server.
    /// </summary>
    public class BalanceSystem : ModSystem
    {
        static readonly Regex FirewoodCode = new Regex(@"^([a-z]+)-firewood(-wet|-kilndried)?$");

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

        public override double ExecuteOrder() => 0.15;

        public override void AssetsLoaded(ICoreAPI api)
        {
            var balance = api.Assets.Get(new AssetLocation("peatanddeadwood", "config/balance.json")).ToObject<BalanceConfig>();

            int daysPerMonth = ReadDaysPerMonth(api);
            double calendarScale = (double)daysPerMonth / balance.BaseDaysPerMonth;
            api.Logger.Notification("[peatanddeadwood] {0} days per month, seasoning and drying times scaled x{1:0.###}", daysPerMonth, calendarScale);

            int firewood = 0;
            foreach (var asset in api.Assets.GetMany("itemtypes/", "vsfirewood"))
            {
                if (EditAsset(asset, root => ScaleFirewood(root, balance, calendarScale))) firewood++;
            }
            api.Logger.Notification("[peatanddeadwood] Scaled {0} Immersive Firewood items", firewood);

            EditOrWarn(api, "game:blocktypes/soil/peatbrick.json", root => ScaleDrying(root, calendarScale), "peat drying time");
            EditOrWarn(api, "game:blocktypes/wood/woodtyped/log.json", root => ScaleRawLogs(root, balance), "raw log burn temperature");
            EditOrWarn(api, "game:blocktypes/wood/pitkiln.json", root => ScalePeatPitKilnFuel(root, balance), "fresh peat pit kiln burn time");
        }

        static int ReadDaysPerMonth(ICoreAPI api)
        {
            // Stored as a string ("9") by the world creation screen
            string raw = api.World.Config?["daysPerMonth"]?.GetValue()?.ToString();
            return int.TryParse(raw, out int days) && days > 0 ? days : 9;
        }

        static bool ScaleFirewood(JObject root, BalanceConfig balance, double calendarScale)
        {
            var match = FirewoodCode.Match((string)root["code"] ?? "");
            if (!match.Success) return false;

            bool changed = false;
            balance.Firewood.TryGetValue(match.Groups[1].Value, out var species);

            if (species != null && root["combustibleProps"] is JObject props)
            {
                changed |= Multiply(props, "burnTemperature", species.Temperature);
                changed |= Multiply(props, "burnDuration", species.Duration);
            }

            if (match.Groups[2].Value == "-wet")
            {
                changed |= ScaleDrying(root, (species?.Seasoning ?? 1) * calendarScale);
            }

            return changed;
        }

        static bool ScaleDrying(JObject root, double factor)
        {
            bool changed = false;
            if (root["transitionableProps"] is not JArray transitions) return false;

            foreach (var transition in transitions)
            {
                if ((string)transition["type"] != "Dry") continue;
                if (transition["transitionHours"] is JObject hours) changed |= Multiply(hours, "avg", factor);
            }
            return changed;
        }

        static bool ScaleRawLogs(JObject root, BalanceConfig balance)
        {
            bool changed = false;
            if (root["combustiblePropsByType"] is not JObject byType) return false;

            foreach (var (key, factor) in balance.RawLogTemperature)
            {
                if (byType[key] is JObject props) changed |= Multiply(props, "burnTemperature", factor);
            }
            return changed;
        }

        static bool ScalePeatPitKilnFuel(JObject root, BalanceConfig balance)
        {
            if (root.SelectToken("attributes.buildMats.fuel") is not JArray fuels) return false;

            foreach (var fuel in fuels)
            {
                string code = (string)fuel["code"];
                if (code == "peatbrick" || code == "game:peatbrick")
                {
                    return Multiply((JObject)fuel, "burnTimeHours", balance.PeatPitKilnBurnTime);
                }
            }
            return false;
        }

        static bool Multiply(JObject obj, string field, double factor)
        {
            if (obj[field] == null || obj[field].Type is not (JTokenType.Integer or JTokenType.Float)) return false;

            obj[field] = (int)Math.Round((double)obj[field] * factor);
            return true;
        }

        static bool EditAsset(IAsset asset, AssetEdit edit)
        {
            if (JToken.Parse(asset.ToText()) is not JObject root || !edit(root)) return false;

            asset.Data = Encoding.UTF8.GetBytes(root.ToString());
            return true;
        }

        static void EditOrWarn(ICoreAPI api, string location, AssetEdit edit, string what)
        {
            var asset = api.Assets.TryGet(new AssetLocation(location));
            if (asset == null || !EditAsset(asset, edit))
            {
                api.Logger.Warning("[peatanddeadwood] Could not apply {0}: nothing to scale in {1}", what, location);
            }
        }
    }

    public class BalanceConfig
    {
        public int BaseDaysPerMonth = 9;
        public double PeatPitKilnBurnTime = 1;
        public Dictionary<string, double> RawLogTemperature = new();
        public Dictionary<string, SpeciesBalance> Firewood = new();
    }

    public class SpeciesBalance
    {
        public double Seasoning = 1;
        public double Temperature = 1;
        public double Duration = 1;
    }
}
