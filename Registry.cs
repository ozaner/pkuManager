using System;
using System.Collections.Generic;

namespace pkuManager.Exporters
{
    public static class Registry
    {
        // A dictionary of all the Importers, in the order they appear in the Export tab in the ManagerWindow
        // The key is the name used for the export button (e.g. "Import from .pk4") and the value is the type of the implemented exporter class


        // A dictionary of all the Exporters, in the order they appear in the Export tab in the ManagerWindow
        // The key is the name used for the export button (e.g. "Export to .pk4") and the value is the type of the implemented exporter class
        public static readonly Dictionary<string, Type> EXPORTER_DICT = new Dictionary<string, Type>()
        {
            {"Showdown!", typeof(ShowdownExporter)},
            //{".pk4", typeof(pk4Exporter)}
        };
    }
}
