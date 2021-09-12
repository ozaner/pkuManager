using pkuManager.Common;
using PokeApiNet;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace pkuManager.Utilities
{
    public static class PokeAPIUtil
    {
        public static readonly PokeApiClient paClient = new PokeApiClient(); // Shared PokeAPI Client

        // ----------
        // Wrapped Info Methods
        // ----------

        public static int? GetSpeciesIndex(int dex, int gen)
        {
            string game = gen switch
            {
                3 => "ruby",
                _ => throw new Exception("Method not implemented for this generation")
            };
            try
            {
                // Note: doesnt use form info, but no official game has seperate indicies for different pokemon variants.
                return Task.Run(() => getPokemonAsync(dex)).Result.GameIndicies.Find(x => x.Version.Name == game)?.GameIndex;
            }
            catch
            {
                return null; //dex # invalid
            }
        }

        public static int? GetItemIndex(string item, int gen)
        {
            if (item == null)
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
            if (ability == null)
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

        //maybe make an overload of this to accommodate custom valid move lists (only mainline example is gen 8).
        public static int? GetMoveIndex(string move)
        {
            if (move == null)
                return null;

            string searchMove = move.ToLower().Replace(' ', '-'); // lower case and replace spaces with dashes
            try
            {
                return Task.Run(() => getMoveIndexAsync(searchMove)).Result.Id;
            }
            catch
            {
                return null; //unofficial move
            }
        }

        public static int? GetMoveBasePP(int moveID)
        {
            try
            {
                return Task.Run(() => getMoveIndexAsync(moveID)).Result.Pp;
            }
            catch
            {
                return null; //move does not exist
            }
        }

        public static string GetMoveName(int moveID)
        {
            try
            {
                Move moveObj = Task.Run(() => getMoveIndexAsync(moveID)).Result;
                return moveObj.Names.FirstOrDefault(x => x.Language.Name == "en").Name;
            }
            catch
            {
                return null; //move does not exist
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
        /// Returns the minimum EXP the given Pokemon species must have at the given level.
        /// Makes use of PokeAPI.
        /// </summary>
        /// <param name="dex">The national dex # of the desired species. Must be official.</param>
        /// <param name="level">The level of the species. Must be between 1-100.</param>
        /// <returns></returns>
        public static int GetEXPFromLevel(int dex, int level)
        {
            if (level > 100 || level < 1)
                throw new ArgumentException("Level must be from 1-100 for GetExpFromLevel.");

            PokeApiNet.GrowthRate gr = Task.Run(() => getGrowthRateAsync(dex)).Result;

            return gr.Levels.Find(x => x.Level == level).Experience;
        }

        public static int GetLevelFromEXP(int dex, int exp)
        {
            int maxEXP = GetEXPFromLevel(dex, 100);
            if (exp > maxEXP || exp < 0)
                throw new ArgumentException("EXP must be from 0-maxEXP for GetLevelFromExp.");

            PokeApiNet.GrowthRate gr = Task.Run(() => PokeAPIUtil.getGrowthRateAsync(dex)).Result;

            if (exp == maxEXP)
                return 100;

            return gr.Levels.Find(x => exp < x.Experience).Level - 1;
        }

        /// <summary>
        /// Returns the GenderRatio of the species with the given national dex number.
        /// </summary>
        /// <param name="dex">The dex number of the species.</param>
        /// <returns></returns>
        public static GenderRatio GetGenderRatio(int dex)
        {
            //pokeapi uses percentage species is female in eighths...
            int gr = Task.Run(() => getPokemonSpeciesAsync(dex)).Result.GenderRate;
            if (gr == 0)
                return GenderRatio.All_Male;
            else if (gr == 8)
                return GenderRatio.All_Female;
            else if (gr == 4)
                return GenderRatio.Male_1_Female_1;
            else if (gr == 6)
                return GenderRatio.Male_1_Female_3;
            else if (gr == 7)
                return GenderRatio.Male_1_Female_7;
            else if (gr == 2)
                return GenderRatio.Male_3_Female_1;
            else if (gr == 1)
                return GenderRatio.Male_7_Female_1;
            else //if (gr == -1)
                return GenderRatio.All_Genderless;
        }

        // ----------
        // API Call Methods
        // ----------

        public static async Task<PokemonSpecies> getPokemonSpeciesAsync(int dex)
        {
            return await paClient.GetResourceAsync<PokemonSpecies>(dex);
        }

        public static async Task<Pokemon> getPokemonAsync(int dex)
        {
            return await paClient.GetResourceAsync<Pokemon>(dex);
        }

        public static async Task<Move> getMoveIndexAsync(int moveID)
        {
            return await paClient.GetResourceAsync<Move>(moveID);
        }

        public static async Task<Move> getMoveIndexAsync(string move)
        {
            return await paClient.GetResourceAsync<Move>(move);
        }

        public static async Task<Ability> getAbilityAsync(string ability)
        {
            return await paClient.GetResourceAsync<Ability>(ability);
        }

        public static async Task<Ability> getAbilityAsync(int abilityID)
        {
            return await paClient.GetResourceAsync<Ability>(abilityID);
        }

        public static async Task<PokeApiNet.GrowthRate> getGrowthRateAsync(int dex)
        {
            PokemonSpecies species = await paClient.GetResourceAsync<PokemonSpecies>(dex); // assume dex is valid
            PokeApiNet.GrowthRate gr = await paClient.GetResourceAsync(species.GrowthRate);
            return gr;
        }

        private static async Task<int> getItemIndexAsync(string item, int gen)
        {
            string genStr = gen switch
            {
                3 => "iii",
                _ => throw new Exception("Method not implemented for this generation")
            };
            Item itemResult = await PokeAPIUtil.paClient.GetResourceAsync<Item>(item);
            GenerationGameIndex ggi = itemResult.GameIndices.Find(x => x.Generation.Name == $"generation-{genStr}");
            return ggi.GameIndex;
        }
    }
}
