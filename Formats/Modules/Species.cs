using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Species_O
{
    public OneOf<IntegralField, Field<string>> Species { get; }
}

public interface Species_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }
    public string FormatName { get; }

    public Species_O Data { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessSpecies() => Data.Species.Switch(
        x => x.SetAs(DexUtil.GetSpeciesIndexedValue<int?>(pku, FormatName, "Indices").Value), //int index
        x => x.Set(DexUtil.GetSpeciesIndexedValue<string>(pku, FormatName, "Indices")) //string index
    );
}