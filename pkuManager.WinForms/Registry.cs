global using static pkuManager.WinForms.Registry.DataDexes; //DataDexes are global constants
global using pkuManager.Data;

using Newtonsoft.Json.Linq;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Formats.pkx.pk3;
using pkuManager.WinForms.Formats.showdown;
using pkuManager.WinForms.Utilities;
using System;
using System.Collections.Generic;

namespace pkuManager.WinForms;

public static class Registry
{
    public readonly struct FormatInfo
    {
        public readonly Type Importer, Exporter, Collection;
        public readonly string Ext, SaveExt;
        public readonly bool ExcludeCheckOut;
        public FormatInfo(string ext, string saveExt, Type importer, Type exporter, Type collection, bool excludeCheckOut = false)
        {
            Ext = ext;
            SaveExt = saveExt;
            Importer = importer;
            Exporter = exporter;
            Collection = collection;
            ExcludeCheckOut = excludeCheckOut;
        }
    }

    public static readonly Dictionary<string, FormatInfo> FORMATS = new()
    {
        { "pku", new FormatInfo("pku", null, null, null, typeof(pkuCollection)) },
        { "pk3", new FormatInfo("pk3", "sav", typeof(pk3Importer), typeof(pk3Exporter), typeof(pk3Collection)) },
        { "Showdown", new FormatInfo("txt", null, null, typeof(ShowdownExporter), null, true) }
    };

    public static class DataDexes
    {
        public static readonly DataDexManager DDM = new();

        private const string MASTERDEX_URL = "https://raw.githubusercontent.com/project-pku/pkuData/build/";

        private static JObject GetMasterDex(string type)
            => DataUtil.DownloadJson($"{MASTERDEX_URL}{type}Dex.json", $"{type}Dex");

        public static readonly JObject SPECIES_DEX = GetMasterDex("Species");
        public static readonly JObject ABILITY_DEX = GetMasterDex("Ability");
        public static readonly JObject MOVE_DEX = GetMasterDex("Move");
        public static readonly JObject GAME_DEX = GetMasterDex("Game");
        public static readonly JObject LANGUAGE_DEX = GetMasterDex("Language");
    }
}