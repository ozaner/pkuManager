using Newtonsoft.Json.Linq;
using pkuManager.Formats.pkx.pk3;
using pkuManager.Formats.showdown;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;

namespace pkuManager
{
    public static class Registry
    {
        public readonly struct FormatInfo
        {
            public readonly Type importer, exporter;
            public readonly string name, ext;
            public readonly bool excludeCheckOut;
            public FormatInfo(string name, string ext, Type importer, Type exporter, bool excludeCheckOut = false)
            {
                this.name = name;
                this.ext = ext;
                this.importer = importer;
                this.exporter = exporter;
                this.excludeCheckOut = excludeCheckOut;
            }
        }

        public static readonly List<FormatInfo> FORMAT_LIST = new()
        {
            // Name, Extension, Exporter Class, Importer Class, (optional) Exclude Check-Out
            new FormatInfo("Gen 3", "pk3", null, typeof(pk3Exporter)), //typeof(pk3Importer) not ready yet
            new FormatInfo("Showdown!", "txt", null, typeof(ShowdownExporter), true)
        };

        private const string MASTER_DEX_BASE = "https://raw.githubusercontent.com/project-pku/pkuData/main/master-dexes/master";

        public static readonly JObject SPECIES_DEX = DexUtil.GetMasterDatadex($"{MASTER_DEX_BASE}SpeciesDex.json");
        public static readonly JObject ABILITY_DEX = DexUtil.GetMasterDatadex($"{MASTER_DEX_BASE}AbilityDex.json");
        public static readonly JObject MOVE_DEX = DexUtil.GetMasterDatadex($"{MASTER_DEX_BASE}MoveDex.json");
        public static readonly JObject ITEM_DEX = DexUtil.GetMasterDatadex($"{MASTER_DEX_BASE}ItemDex.json");
        public static readonly JObject FORMAT_DEX = DexUtil.GetMasterDatadex($"{MASTER_DEX_BASE}FormatDex.json");
    }
}
