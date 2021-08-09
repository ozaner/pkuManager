using Newtonsoft.Json.Linq;
using pkuManager.Common;
using pkuManager.pk3;
using pkuManager.showdown;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;

namespace pkuManager
{
    public static class Registry
    {
        // A dictionary of all the Importers, in the order they appear in the Import tab in the ManagerWindow
        // The key is the name used for the import button (e.g. "Import from .pk4") and the value is the type of the implemented importer class


        // A dictionary of all the Exporters, in the order they appear in the Export tab in the ManagerWindow
        // The key is the name used for the check-out/export button (e.g. "Export to pk4") and the value is the type of the implemented exporter class
        public static readonly Dictionary<string, Type> EXPORTER_DICT = new Dictionary<string, Type>()
        {
            {"Showdown!", typeof(ShowdownExporter)},
            {"pk3", typeof(pk3Exporter)}
        };

        // A JObject of all the species data for different formats.
        // Currently needed to declare the default form for a species.
        public static readonly JObject MASTER_DEX = DataUtil.getCombinedJson(new JObject[]
        {
            pkxUtil.NATIONALDEX_DATA,
            pkxUtil.POKESTAR_DATA,
            ShowdownUtil.SHOWDOWN_SPECIES_DATA //includes Showdown CAP species
        });
    }
}
