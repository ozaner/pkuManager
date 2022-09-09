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
    /// A normalized pku form (e.g. "", or "Origin", or "Galarian|Zen Mode")
    /// </summary>
    public string Form;

    /// <summary>
    /// A normalized pku appearance (e.g. null or "Sinnoh Cap")
    /// </summary>
    public string Appearance;

    /// <summary>
    /// Modifiers on the pku index (e.g. male-female, shiny-fusion-partner).
    /// </summary>
    public Dictionary<string, int> Modifiers = new();

    /// <summary>
    /// A short-hand for telling if <see cref="Modifiers"/> contains the M/F split with index 1 (female).
    /// </summary>
    public bool IsFemale => Modifiers.TryGetValue("$Male-Female", out int val) && val is 1;

    /// <summary>
    /// Constructs an SFAM with the given parameters.
    /// </summary>
    /// <param name="species">The species.</param>
    /// <param name="form">The searchable form, delimited with '|' characters.</param>
    /// <param name="appearance">The searchable appearance, delimited with '|' characters.</param>
    /// <param name="isFemale">Whether this SFAM has the M/F split with index = 1 (female).</param>
    public SFAM(string species, string form, string appearance, bool isFemale)
    {
        Species = species;
        Form = form;
        Appearance = appearance;

        if (isFemale)
            Modifiers.Add("$Male-Female", 1);
    }
}