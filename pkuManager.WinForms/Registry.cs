global using static pkuManager.WinForms.Registry; //to give global scope to DDM

using pkuManager.Data;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Formats.pkx.pk3;
using pkuManager.WinForms.Formats.showdown;
using System;
using System.Collections.Generic;

namespace pkuManager.WinForms;

public static class Registry
{
    public static readonly DataDexManager DDM = new();
    public static readonly SpriteDexManager SDM = new();

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
}