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
    public static string GetSpeciesNameTranslated(int dex, string lang)
    {
        string langID = lang switch
        {
            "Japanese" => "ja-Hrkt",
            "English" => "en",
            "French" => "fr",
            "Italian" => "it",
            "German" => "de",
            "Spanish" => "es",
            "Korean" => "ko",
            "Chinese Simplified" => "zh-Hans",
            "Chinese Traditional" => "zh-Hant",
            _ => null //invalid lang
        };

        if (langID is null)
            return null;
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

    private static async Task<PokeApiNet.GrowthRate> getGrowthRateAsync(int dex)
    {
        PokemonSpecies species = await paClient.GetResourceAsync<PokemonSpecies>(dex); // assume dex is valid
        return await paClient.GetResourceAsync(species.GrowthRate);
    }
}