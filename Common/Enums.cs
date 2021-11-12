namespace pkuManager.Common;

/// <summary>
/// A Ribbon or Mark a Pokémon can have.<br/>
/// Index numbers correspond to those used in the official games, except the
/// Contest and Battle Tower ribbons from Gens 3-4.<br/>
/// These are given negative index numbers since they don't exist in new formats.
/// </summary>
public enum Ribbon
{
    // Contest Gen 3
    Cool_G3 = -49, //Indexing starts at -49 because memory ribbons replaced old contest/battle ribbons
    Cool_Super_G3,
    Cool_Hyper_G3,
    Cool_Master_G3,
    Beauty_G3,
    Beauty_Super_G3,
    Beauty_Hyper_G3,
    Beauty_Master_G3,
    Cute_G3,
    Cute_Super_G3,
    Cute_Hyper_G3,
    Cute_Master_G3,
    Smart_G3,
    Smart_Super_G3,
    Smart_Hyper_G3,
    Smart_Master_G3,
    Tough_G3,
    Tough_Super_G3,
    Tough_Hyper_G3,
    Tough_Master_G3,

    // Contest Gen 4
    Cool_G4,
    Cool_Great_G4,
    Cool_Ultra_G4,
    Cool_Master_G4,
    Beauty_G4,
    Beauty_Great_G4,
    Beauty_Ultra_G4,
    Beauty_Master_G4,
    Cute_G4,
    Cute_Great_G4,
    Cute_Ultra_G4,
    Cute_Master_G4,
    Smart_G4,
    Smart_Great_G4,
    Smart_Ultra_G4,
    Smart_Master_G4,
    Tough_G4,
    Tough_Great_G4,
    Tough_Ultra_G4,
    Tough_Master_G4,

    // Battle Gen 3
    Winning,
    Victory,

    // Battle Gen 4
    Ability,
    Great_Ability,
    Double_Ability,
    Multi_Ability,
    Pair_Ability,
    World_Ability,

    // -1 reserved for no match

    // Everything else
    Kalos_Champion = 0,
    Champion,
    Sinnoh_Champion,
    Best_Friends,
    Training,
    Skillful_Battler,
    Expert_Battler,
    Effort,
    Alert,
    Shock,
    Downcast,
    Careless,
    Relax,
    Snooze,
    Smile,
    Gorgeous,
    Royal,
    Gorgeous_Royal,
    Artist,
    Footprint,
    Record,
    Legend,
    Country,
    National,
    Earth,
    World,
    Classic,
    Premier,
    Event, //History Ribbon in Gen 4
    Birthday, //Green Ribbon in Gen 4
    Special, //Blue Ribbon in Gen 4
    Souvenir, //Festival Ribbon in Gen 4
    Wishing, //Carnival Ribbon in Gen 4
    Battle_Champion, //Marine Ribbon in Gen 3/4
    Regional_Champion, //Land Ribbon in Gen 3/4
    National_Champion, //Sky Ribbon in Gen 3/4
    World_Champion, //Red Ribbon in Gen 4
    Contest_Memory,
    Battle_Memory,
    Hoenn_Champion,
    Contest_Star,
    Coolness_Master,
    Beauty_Master,
    Cuteness_Master,
    Cleverness_Master,
    Toughness_Master,
    Alola_Champion,
    Battle_Royale_Master,
    Battle_Tree_Great,
    Battle_Tree_Master,
    Galar_Champion,
    Tower_Master,
    Master_Rank,

    // Marks Gen 8
    Lunchtime_Mark,
    SleepyTime_Mark,
    Dusk_Mark,
    Dawn_Mark,
    Cloudy_Mark,
    Rainy_Mark,
    Stormy_Mark,
    Snowy_Mark,
    Blizzard_Mark,
    Dry_Mark,
    Sandstorm_Mark,
    Misty_Mark,
    Destiny_Mark,
    Fishing_Mark,
    Curry_Mark,
    Uncommon_Mark,
    Rare_Mark,
    Rowdy_Mark,
    AbsentMinded_Mark,
    Jittery_Mark,
    Excited_Mark,
    Charismatic_Mark,
    Calmness_Mark,
    Intense_Mark,
    ZonedOut_Mark,
    Joyful_Mark,
    Angry_Mark,
    Smiley_Mark,
    Teary_Mark,
    Upbeat_Mark,
    Peeved_Mark,
    Intellectual_Mark,
    Ferocious_Mark,
    Crafty_Mark,
    Scowling_Mark,
    Kindly_Mark,
    Flustered_Mark,
    PumpedUp_Mark,
    ZeroEnergy_Mark,
    Prideful_Mark,
    Unsure_Mark,
    Humble_Mark,
    Thorny_Mark,
    Vigor_Mark,
    Slump_Mark,
}

/// <summary>
/// A marking a Pokémon can have displayed on their summary screen.
/// </summary>
public enum Marking
{
    // Blue markings (also refer to the black markings present in Gens 3-5)
    Blue_Circle,
    Blue_Triangle,
    Blue_Square,
    Blue_Heart,
    Blue_Star,
    Blue_Diamond,

    // Pink markings
    Pink_Circle,
    Pink_Triangle,
    Pink_Square,
    Pink_Heart,
    Pink_Star,
    Pink_Diamond,

    Favorite //Only in LGPE
}

/// <summary>
/// An EXP growth type a Pokémon species can have.
/// Index numbers correspond to those used in the official games.
/// </summary>
public enum GrowthRate
{
    Medium_Fast,
    Erratic,
    Fluctuating,
    Medium_Slow,
    Fast,
    Slow
}

/// <summary>
/// A gender a Pokémon, or trainer, can have.<br/>
/// Note that OT genders can only be male or female, not genderless.<br/>
/// Index numbers correspond to those used in the official games.
/// </summary>
public enum Gender
{
    Male,
    Female,
    Genderless
}

/// <summary>
/// An official language a Pokémon can have.
/// Index numbers correspond to those used in the official games.
/// </summary>
public enum Language
{
    /// <summary>
    /// Unset language ID.<br/>
    /// Note that Gen 5 Japanese in-game trades use this value. Great...
    /// </summary>
    //None = 0,

    /// <summary>
    /// Japanese (日本語)
    /// </summary>
    Japanese = 1,

    /// <summary>
    /// English (US/UK/AU)
    /// </summary>
    English = 2,

    /// <summary>
    /// French (Français)
    /// </summary>
    French = 3,

    /// <summary>
    /// Italian (Italiano)
    /// </summary>
    Italian = 4,

    /// <summary>
    /// German (Deutsch)
    /// </summary>
    German = 5,

    /// <summary>
    /// Unused language ID reserved for Korean in Gen 3 but never used.
    /// </summary>
    //Korean_Gen_3 = 6,

    /// <summary>
    /// Spanish (Español)
    /// </summary>
    Spanish = 7,

    /// <summary>
    /// Korean (한국어)
    /// </summary>
    Korean = 8,

    /// <summary>
    /// Chinese Simplified (简体中文)
    /// </summary>
    Chinese_Simplified = 9,

    /// <summary>
    /// Chinese Traditional (繁體中文)
    /// </summary>
    Chinese_Traditional = 10
}

/// <summary>
/// An official nature a Pokémon can have.<br/>
/// Index numbers correspond to those used in the official games.
/// </summary>
public enum Nature
{
    Hardy,
    Lonely,
    Brave,
    Adamant,
    Naughty,
    Bold,
    Docile,
    Relaxed,
    Impish,
    Lax,
    Timid,
    Hasty,
    Serious,
    Jolly,
    Naive,
    Modest,
    Mild,
    Quiet,
    Bashful,
    Rash,
    Calm,
    Gentle,
    Sassy,
    Careful,
    Quirky
}

/// <summary>
/// A gender ratio a Pokémon species can have.<br/>
/// Index numbers correspond to the gender threshold use to determine a Pokémon's gender.
/// </summary>
public enum GenderRatio
{
    All_Male = 0,
    Male_7_Female_1 = 31,
    Male_3_Female_1 = 63,
    Male_1_Female_1 = 127,
    Male_1_Female_3 = 191,
    Male_1_Female_7 = 225,
    All_Female = 254,
    All_Genderless = 255
}

/// <summary>
/// An official Pokéball a Pokémon can be caught with.<br/>
/// Index numbers correspond to those used in the official games.
/// </summary>
public enum Ball
{
    // None = 0, //Also Blue Master Ball in DPPt

    // Gen 3: Legacy
    Master_Ball = 1,
    Ultra_Ball = 2,
    Great_Ball = 3,
    Poké_Ball = 4,
    Safari_Ball = 5,

    // Gen 3
    Net_Ball = 6,
    Dive_Ball = 7,
    Nest_Ball = 8,
    Repeat_Ball = 9,
    Timer_Ball = 10,
    Luxury_Ball = 11,
    Premier_Ball = 12,

    // Gen 4
    Dusk_Ball = 13,
    Heal_Ball = 14,
    Quick_Ball = 15,
    Cherish_Ball = 16,

    // Gen 4: HGSS
    Fast_Ball = 17,
    Level_Ball = 18,
    Lure_Ball = 19,
    Heavy_Ball = 20,
    Love_Ball = 21,
    Friend_Ball = 22,
    Moon_Ball = 23,
    Sport_Ball = 24,

    Dream_Ball = 25, // Gen 5
    Beast_Ball = 26 // Gen 7
}