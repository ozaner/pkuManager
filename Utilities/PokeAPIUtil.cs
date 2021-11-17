using pkuManager.Common;
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
    public static int? GetItemIndex(string item, int gen)
    {
        if (item is null)
            return null;

        string searchItem = item.ToLowerInvariant().Replace(' ', '-'); // lower case and replace spaces with dashes
        try
        {
            return Task.Run(() => getItemIndexAsync(searchItem, gen)).Result;
        }
        catch
        {
            return null; //item does not exist in gen (or maybe its formatting doesn't match pokeapi)
        }
    }

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

    public static string GetSpeciesNameTranslated(int dex, Common.Language lang)
    {
        string langID = lang switch
        {
            Common.Language.Japanese => "ja-Hrkt",
            Common.Language.English => "en",
            Common.Language.French => "fr",
            Common.Language.Italian => "it",
            Common.Language.German => "de",
            Common.Language.Spanish => "es",
            Common.Language.Korean => "ko",
            Common.Language.Chinese_Simplified => "zh-Hans",
            Common.Language.Chinese_Traditional => "zh-Hant",
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

    /// <summary>
    /// Gets the GenderRatio of the species with the given national <paramref name="dex"/>.
    /// </summary>
    /// <param name="dex">The dex number of the species.</param>
    /// <returns>The GenderRatio of the species with <paramref name="dex"/>.</returns>
    public static GenderRatio GetGenderRatio(int dex) => Task.Run(() => getPokemonSpeciesAsync(dex)).Result.GenderRate switch
    {
        //pokeapi uses percentage species is female in eighths...
        0 => GenderRatio.All_Male,
        8 => GenderRatio.All_Female,
        4 => GenderRatio.Male_1_Female_1,
        6 => GenderRatio.Male_1_Female_3,
        7 => GenderRatio.Male_1_Female_7,
        2 => GenderRatio.Male_3_Female_1,
        1 => GenderRatio.Male_7_Female_1,
        _ => GenderRatio.All_Genderless //if (gr == -1)
    };


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

    private static async Task<PokeApiNet.GrowthRate> getGrowthRateAsync(int dex)
    {
        PokemonSpecies species = await paClient.GetResourceAsync<PokemonSpecies>(dex); // assume dex is valid
        return await paClient.GetResourceAsync(species.GrowthRate);
    }

    private static async Task<int> getItemIndexAsync(string item, int gen)
    {
        string genStr = gen switch
        {
            3 => "iii",
            _ => throw new Exception("Method not implemented for this generation")
        };
        Item itemResult = await paClient.GetResourceAsync<Item>(item);
        GenerationGameIndex ggi = itemResult.GameIndices.Find(x => x.Generation.Name == $"generation-{genStr}");
        return ggi.GameIndex;
    }
}