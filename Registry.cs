global using static pkuManager.Registry.DataDexes; //DataDexes are global constants

using Newtonsoft.Json.Linq;
using pkuManager.Formats.pkx.pk3;
using pkuManager.Formats.showdown;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;

namespace pkuManager;

public static class Registry
{
    public readonly struct FormatInfo
    {
        public readonly Type Importer, Exporter;
        public readonly string Name, Ext;
        public readonly bool ExcludeCheckOut;
        public FormatInfo(string name, string ext, Type importer, Type exporter, bool excludeCheckOut = false)
        {
            Name = name;
            Ext = ext;
            Importer = importer;
            Exporter = exporter;
            ExcludeCheckOut = excludeCheckOut;
        }
    }

    public static readonly List<FormatInfo> FORMAT_LIST = new()
    {
        new FormatInfo("Gen 3", "pk3", null, typeof(pk3Exporter)), //typeof(pk3Importer) not ready yet
        new FormatInfo("Showdown!", "txt", null, typeof(ShowdownExporter), true)
    };

    public static class DataDexes
    {
        private static string GetMasterDexURL(string type)
            => $"https://raw.githubusercontent.com/project-pku/pkuData/main/master-dexes/master{type}Dex.json";

        public static readonly JObject SPECIES_DEX = DexUtil.GetMasterDatadex(GetMasterDexURL("Species"), "Species");
        public static readonly JObject ABILITY_DEX = DexUtil.GetMasterDatadex(GetMasterDexURL("Ability"), "Ability");
        public static readonly JObject MOVE_DEX = DexUtil.GetMasterDatadex(GetMasterDexURL("Move"), "Moves");
        public static readonly JObject ITEM_DEX = DexUtil.GetMasterDatadex(GetMasterDexURL("Item"), "Items");
        public static readonly JObject GAME_DEX = DexUtil.GetMasterDatadex(GetMasterDexURL("Game"), "Games");
        public static readonly JObject FORMAT_DEX = DexUtil.GetMasterDatadex(GetMasterDexURL("Format"), "Format");
    }
}