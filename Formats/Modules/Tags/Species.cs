using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Species_O
{
    public OneOf<IField<BigInteger>, IField<string>> Species { get; }
}

public interface Species_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportSpecies() => ExportSpeciesBase();

    public void ExportSpeciesBase() => (Data as Species_O).Species.Switch(
        x => x.SetAs(DexUtil.GetSpeciesIndexedValue<int?>(pku, FormatName, "Indices").Value), //int index
        x => x.Value = DexUtil.GetSpeciesIndexedValue<string>(pku, FormatName, "Indices") //string index
    );
}