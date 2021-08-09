namespace pkuManager.Common
{
    /// <summary>
    /// List of all Ribbons and Marks from Gens 3-8 by index number.
    /// Contest and Battle tower ribbons from gen 3 & 4 are given negative index numbers
    /// since they don't exist on new formats.
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
    /// A list of the markings Pokemon can be given in the PC.
    /// </summary>
    public enum MarkingIndex
    {
        // Blue Markings used to just be markigns before pink were introduced
        BlueCircle,
        BlueTriangle,
        BlueSquare,
        BlueHeart,
        BlueStar,
        BlueDiamond,

        // Pink markings introduced in gen 6
        PinkCircle,
        PinkTriangle,
        PinkSquare,
        PinkHeart,
        PinkStar,
        PinkDiamond,

        Favorite //Only in LGPE
    }

    /// <summary>
    /// The different EXP growth types of a pokemon species.
    /// </summary>
    public enum EXPGrowthIndex
    {
        MediumFast,
        Erratic,
        Fluctuating,
        MediumSlow,
        Fast,
        Slow
    }

    /// <summary>
    /// The index numbers of the 3 genders a pokemon can have.
    /// OT genders can only be male or female.
    /// </summary>
    public enum Gender
    {
        Male,
        Female,
        Genderless
    }

    /// <summary>
    /// A list of the different languages a pku's origin game can have.
    /// </summary>
    public enum Language
    {
        Japanese, //Added in gen 1
        English, //Added in gen 1
        French, //Added in gen 1
        Italian, //Added in gen 1
        German, //Added in gen 1
        Spanish, //Added in gen 1
        Korean, //Added in gen 2
        Chinese_Simplified, //Added in gen 7
        Chinese_Traditional //Added in gen 7
    }

    /// <summary>
    /// The different natures a pokemon can have in the official games. Canonical ordering.
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
    /// The different gender ratios pokemon species can have.
    /// </summary>
    public enum GenderRatio
    {
        ALL_MALE = 0,
        MALE_FEMALE_7_1 = 31,
        MALE_FEMALE_3_1 = 63,
        MALE_FEMALE_1_1 = 127,
        MALE_FEMALE_1_3 = 191,
        MALE_FEMALE_1_7 = 225,
        ALL_FEMALE = 254,
        ALL_GENDERLESS = 255
    }

    /// <summary>
    /// The different pokeballs a pokemon can be captured in.
    /// Only official balls.
    /// </summary>
    public enum Ball
    {
        // None = 0, //Blue Master_ Ball in DPPt

        // Gen 3: Legacy (ball type recorded starting in gen 3)
        Master = 1,
        Ultra = 2,
        Great = 3,
        Poké = 4,
        Safari = 5,

        // Gen 3
        Net = 6,
        Dive = 7,
        Nest = 8,
        Repeat = 9,
        Timer = 10,
        Luxury = 11,
        Premier = 12,

        // Gen 4
        Dusk = 13,
        Heal = 14,
        Quick = 15,
        Cherish = 16,

        // Gen 4: HGSS
        Fast = 17,
        Level = 18,
        Lure = 19,
        Heavy = 20,
        Love = 21,
        Friend = 22,
        Moon = 23,
        Sport = 24,

        Dream = 25, // Gen 5
        Beast = 26 // Gen 7
    }
}
