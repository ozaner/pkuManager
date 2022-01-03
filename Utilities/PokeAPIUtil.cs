using PokeApiNet;
using System;
using System.Threading.Tasks;

namespace pkuManager.Utilities;

public static class PokeAPIUtil
{
    private static readonly PokeApiClient paClient = new(); // PokeAPI Client

    /* ------------------------------------
     * Wrapped Info Methods
     * ------------------------------------
    */
    public static int? GetAbilityIndex(string ability)
    {
        if (ability is null)
            return null;

        string searchAbility = ability.ToLowerInvariant().Replace(' ', '-'); // lower case and replace spaces with dashes
        try
        {
            Ability ab = Task.Run(() => getAbilityAsync(searchAbility)).Result;
            if (!ab.IsMainSeries)
                return null; //only main series abilities
            return ab.Id;
        }
        catch
        {
            return null; //ability DNE
        }
    }

    public static string GetAbility(int abilityID)
    {
        try
        {
            Ability ab = Task.Run(() => getAbilityAsync(abilityID)).Result;
            if (!ab.IsMainSeries)
                return null; //only main series abilities
            return ab.Names.Find(x => x.Language.Name == "en").Name;
        }
        catch
        {
            return null; //ability DNE
        }
    }

    public static string GetSpeciesNameTranslated(int dex, Formats.Modules.Language lang)
    {
        string langID = lang switch
        {
            Formats.Modules.Language.Japanese => "ja-Hrkt",
            Formats.Modules.Language.English => "en",
            Formats.Modules.Language.French => "fr",
            Formats.Modules.Language.Italian => "it",
            Formats.Modules.Language.German => "de",
            Formats.Modules.Language.Spanish => "es",
            Formats.Modules.Language.Korean => "ko",
            Formats.Modules.Language.Chinese_Simplified => "zh-Hans",
            Formats.Modules.Language.Chinese_Traditional => "zh-Hant",
            _ => throw new ArgumentException("GetSpeciesNameTranslated is missing a language...")
        };

        try
        {
            return Task.Run(() => getPokemonSpeciesAsync(dex)).Result.Names.Find(x => x.Language.Name == langID).Name;
        }
        catch
        {
            return null; //dex # invalid/name doesn't exist in this language...
        }
    }

    /// <summary>
    /// Gets the minimum EXP the given Pokemon species must have at the given level.
    /// </summary>
    /// <param name="dex">The national dex # of the desired species. Must be official.</param>
    /// <param name="level">The level of the species. Must be between 1-100.</param>
    /// <returns>The minimum EXP a species must have at <paramref name="level"/>.</returns>
    public static int GetEXPFromLevel(int dex, int level)
    {
        if (level is > 100 or < 1)
            throw new ArgumentException("Level must be from 1-100 for GetExpFromLevel.");

        PokeApiNet.GrowthRate gr = Task.Run(() => getGrowthRateAsync(dex)).Result;

        return gr.Levels.Find(x => x.Level == level).Experience;
    }

    public static int GetLevelFromEXP(int dex, int exp)
    {
        int maxEXP = GetEXPFromLevel(dex, 100);
        if (exp > maxEXP || exp < 0)
            throw new ArgumentException("EXP must be from 0-maxEXP for GetLevelFromExp.");

        PokeApiNet.GrowthRate gr = Task.Run(() => getGrowthRateAsync(dex)).Result;

        if (exp == maxEXP)
            return 100;

        return gr.Levels.Find(x => exp < x.Experience).Level - 1;
    }


    /* ------------------------------------
     * API Call Methods
     * ------------------------------------
    */
    private static async Task<PokemonSpecies> getPokemonSpeciesAsync(int dex)
        => await paClient.GetResourceAsync<PokemonSpecies>(dex);

    private static async Task<Ability> getAbilityAsync(string ability)
        => await paClient.GetResourceAsync<Ability>(ability);

    private static async Task<Ability> getAbilityAsync(int abilityID)
        => await paClient.GetResourceAsync<Ability>(abilityID);

    private static async Task<GrowthRate> getGrowthRateAsync(int dex)
    {
        PokemonSpecies species = await paClient.GetResourceAsync<PokemonSpecies>(dex); // assume dex is valid
        return await paClient.GetResourceAsync(species.GrowthRate);
    }
}