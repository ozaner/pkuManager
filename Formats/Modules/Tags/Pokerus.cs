using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Pokerus_O
{
    public IField<BigInteger> Pokerus_Strain { get; }
    public IField<BigInteger> Pokerus_Days { get; }
}

public interface Pokerus_E : RelatedNumericTag_E
{

    // Pokérus
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportPokerus()
    {
        Pokerus_O pokerusObj = Data as Pokerus_O;
        ExportRelatedNumericTag("Pokérus", new[] { "Pokérus strain", "Pokérus days" },
            new[] { pku.Pokerus.Strain, pku.Pokerus.Days },
            new[] { pokerusObj.Pokerus_Strain, pokerusObj.Pokerus_Days }, new BigInteger[] { 0, 0 }, false);
    }
}