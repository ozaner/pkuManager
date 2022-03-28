using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Species_O
{
    public OneOf<IField<BigInteger>, IField<string>> Species { get; }
}

public interface Species_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }
    public string FormatName { get; }

    public Species_O Species_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessSpecies() => ProcessSpeciesBase();

    public void ProcessSpeciesBase() => Species_Field.Species.Switch(
        x => x.SetAs(DexUtil.GetSpeciesIndexedValue<int?>(pku, FormatName, "Indices").Value), //int index
        x => x.Value = DexUtil.GetSpeciesIndexedValue<string>(pku, FormatName, "Indices") //string index
    );
}