using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Pokerus_O
{
    public IField<BigInteger> Pokerus_Strain { get; }
    public IField<BigInteger> Pokerus_Days { get; }
}

public interface Pokerus_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportPokerus()
    {
        Pokerus_O pokerusObj = Data as Pokerus_O;
        var defVals = new BigInteger[] { 0, 0 };
        var dataFields = new[] { pokerusObj.Pokerus_Strain, pokerusObj.Pokerus_Days };

        AlertType[] ats = NumericTagUtil.ExportMultiNumericTag(new[] { pku.Pokerus.Strain, pku.Pokerus.Days }, dataFields, defVals);
        Alert a = NumericTagUtil.GetMultiNumericAlert("Pokérus", new[] { "Pokérus strain", "Pokérus days" }, ats, defVals,
            Array.ConvertAll(dataFields, x => x as IBoundable), true);
        Warnings.Add(a);
    }
}