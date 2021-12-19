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
        private const string MASTERDEX_URL = "https://raw.githubusercontent.com/project-pku/pkuData/main/masterdexes/";

        private static JObject GetMasterDex(string type)
            => DataUtil.DownloadJson($"{MASTERDEX_URL}master{type}Dex.json", $"{type}Dex");

        public static readonly JObject SPECIES_DEX = GetMasterDex("Species");
        public static readonly JObject ABILITY_DEX = GetMasterDex("Ability");
        public static readonly JObject MOVE_DEX = GetMasterDex("Move");
        public static readonly JObject ITEM_DEX = GetMasterDex("Item");
        public static readonly JObject GAME_DEX = GetMasterDex("Game");
        public static readonly JObject FORMAT_DEX = GetMasterDex("Format");
    }
}