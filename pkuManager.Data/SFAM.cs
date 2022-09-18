namespace pkuManager.Data;

/// <summary>
/// A data structure representing a particular type of Pokémon under the
/// pku indexing system (i.e. Species, Form, Appearance, and Modifers).
/// </summary>
public struct SFAM
{
    /// <summary>
    /// A pku species (e.g. "Pikachu")
    /// </summary>
    public string Species;

    /// <summary>
    /// A normalized pku form (e.g. "", "Origin", "Galarian|Zen Mode")
    /// </summary>
    public string Form;

    /// <summary>
    /// A normalized pku appearance (e.g. "Kabuki Trim", "Sinnoh Cap")<br/>
    /// Note that empty appearances are denoted by "".
    /// </summary>
    public string Appearance;

    /// <summary>
    /// Modifiers on the pku index (e.g. "Female", "Shadow", "Egg", etc.).
    /// </summary>
    public HashSet<string> Modifiers = new();

    /// <summary>
    /// Constructs an SFAM with the given parameters.
    /// </summary>
    /// <param name="species">The species.</param>
    /// <param name="form">The searchable form, delimited with '|' characters.</param>
    /// <param name="appearance">The searchable appearance, delimited with '|' characters.</param>
    /// <param name="modifiers">A list of SFAM modifiers in no particular order (e.g. "Female", "Shiny").</param>
    public SFAM(string species, string form, string appearance, HashSet<string> modifiers)
    {
        Species = species;
        Form = form;
        Appearance = appearance;
        Modifiers.UnionWith(modifiers);
    }

    public SFAM(string species, string form, string appearance, IModifiers modObj)
        : this(species, form, appearance, modObj.GetModifiers()) { }
}

public interface IModifiers
{
    bool Female => false;
    bool Shiny => false;
    bool Shadow => false;
    bool Egg => false;

    public HashSet<string> GetModifiers()
    {
        HashSet<string> hs = new();
        if (Female) hs.Add("Female");
        if (Shiny) hs.Add("Shiny");
        if (Shadow) hs.Add("Shadow");
        if (Egg) hs.Add("Egg");
        return hs;
    }
}