using Newtonsoft.Json.Linq;
using pkuManager.Formats.pkx;
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

        // A JObject of all the species data for different formats.
        // Currently needed to declare the default form for a species.
        public static readonly JObject MASTER_DEX = DataUtil.GetCombinedJson(true, new JObject[]
        {
            pkxUtil.NATIONALDEX_DATA,
            pkxUtil.POKESTAR_DATA,
            ShowdownObject.SHOWDOWN_DATA //includes Showdown CAP species
        });
    }
}
