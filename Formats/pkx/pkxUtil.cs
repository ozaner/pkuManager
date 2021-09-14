using Newtonsoft.Json.Linq;
using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.Formats.pkx.pk3;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.pkx.pkxUtil.TagAlerts;

namespace pkuManager.Formats.pkx
{
    public static class pkxUtil
    {
        public static readonly JObject POKESTAR_DATA = DataUtil.GetJson("pokestarData"); //Gen 5: move this to pk5Util when it exists...
        public static readonly JObject NATIONALDEX_DATA = DataUtil.GetJson("nationaldexData");
        public static readonly JObject GAME_DATA = DataUtil.GetJson("gameData");

        // Enum Defaults
        public static readonly Language DEFAULT_LANGUAGE = Language.English;
        public static readonly Nature DEFAULT_NATURE = Nature.Hardy;
        public static readonly Ball DEFAULT_BALL = Ball.Poké_Ball;
        public static readonly Gender DEFAULT_GENDER = Gender.Male;

        public static readonly int LAST_MOVE_INDEX_GEN8 = 826;


        // ----------
        // Helper Methods
        // ----------

        public static string GetSpeciesFromDex(int dex)
        {
            foreach (var x in NATIONALDEX_DATA)
            {
                if ((int?)x.Value.TraverseJTokenCaseInsensitive("National Dex") == dex)
                    return x.Key;
            }
            return null;
        }

        /// <summary>
        /// Returns the National dex number of the given species or null if it has none.
        /// </summary>
        /// <param name="species">An official Pokemon species name, with any capitalization.</param>
        /// <returns></returns>
        public static int? GetNationalDex(string species)
        {
            //uses the nationaldex.json file and not pokeapi
            if (species == null)
                return null;

            //case insensitive species
            return (int?)NATIONALDEX_DATA.TraverseJTokenCaseInsensitive(species, "National Dex");
        }

        public static int GetNationalDexChecked(string species)
        {
            int? dex = GetNationalDex(species);
            if (!dex.HasValue)
                throw new ArgumentException("Must be an official pokemon species.");
            else
                return dex.Value;
        }

        // Returns the ID of the given game version. Null if no ID is found.
        public static (int?, int?) GetGameIDAndGen(string game)
        {
            return ((int?, int?))(GAME_DATA.TraverseJTokenCaseInsensitive(game, "Game ID"), GAME_DATA.TraverseJTokenCaseInsensitive(game, "Generation"));
        }

        // Returns the gen 5 id of a pokestar species, null if it's not a pokestar species (case insensitve)
        // TODO: move to pk5 util when it exists
        public static int? GetPokestarID(pkuObject pku)
        {
            //No species, no pokestar
            if (pku.Species == null)
                return null;

            int? gen5ID = null;

            // Try getting the species+form ID (case insensitive)
            string searchableFormName = DexUtil.GetSearchableFormName(pku);
            if (searchableFormName != null)
                gen5ID = (int?)POKESTAR_DATA.TraverseJTokenCaseInsensitive(pku.Species, "Forms", searchableFormName, "Gen 5 Index");

            // If form is unspecified/invalid (i.e. above didn't work) then just get default form id
            if (!gen5ID.HasValue)
            {
                string defaultForm = DexUtil.GetDefaultForm(pku, POKESTAR_DATA);
                gen5ID = (int?)POKESTAR_DATA.TraverseJTokenCaseInsensitive(pku.Species, "Forms", defaultForm, "Gen 5 Index");
            }
            return gen5ID; //might still be null
        }

        public static Gender GetPIDGender(int dex, uint pid)
        {
            GenderRatio gr = PokeAPIUtil.GetGenderRatio(dex);
            if (gr == GenderRatio.All_Female)
                return Gender.Female;
            else if (gr == GenderRatio.All_Male)
                return Gender.Male;
            else if (gr == GenderRatio.All_Genderless)
                return Gender.Genderless;
            else if (pid % 256 < (int)gr)
                return Gender.Female;
            else
                return Gender.Male;
        }

        //doesnt account for gen6+, but doesnt really matter (since gen 5- shiny pids are a subset of gen 6+ ones)
        public static uint GenerateRandomPID(bool shiny, uint id, GenderRatio? gr = null, Gender? gender = null, Nature? nature = null, int? unownForm = null)
        {
            uint pid;
            while (true)
            {
                pid = DataUtil.GetRandomUInt(); //Generate new PID candidate
                //if (abilitySlot.HasValue) // Ability Slot Check
                //{
                //    if (abilitySlot.Value != (DataUtil.getBits(pid, 0) == 0))
                //        continue;
                //}
                if (unownForm.HasValue) // Unown Form Check
                {
                    if (unownForm != pk3Object.GetUnownFormID(pid))
                        continue;
                }
                if (gr.HasValue && gender.HasValue) // Gender Check
                {
                    if (gr != GenderRatio.All_Male && gr != GenderRatio.All_Female && gr != GenderRatio.All_Genderless)
                    {
                        if (gender == Gender.Male && pid % 256 < (int)gr) //Male but pid is Female
                            continue;
                        if (gender == Gender.Female && pid % 256 >= (int)gr) //Female but pid is Male
                            continue;
                    }
                }
                if (nature.HasValue) // Nature Check
                {
                    if (pid % 25 != (int)nature)
                        continue;
                }
                if ((pid / 65536 ^ pid % 65536 ^ id / 65536 ^ id % 65536) < 8 != shiny) // Shiny Check
                    continue;

                return pid; // all checks out
            }
        }


        // ----------
        // Exporter Stuff
        // ----------

        public static class AuxAlerts
        {
            public static Alert GetBattleStatAlert(bool hasStatNature, bool hasNature, string statNature, string trueNature, bool[] hyperIVs, int?[] IVs)
            {
                string msg = "";

                //Deal with stat nature override
                if (hasStatNature)
                {
                    msg += "The pku's Nature ";
                    if (hasNature)
                        msg += $"({trueNature}), was replaced";
                    else
                        msg += "is unspecified, replacing it";

                    msg += $" with it's Stat Nature ({statNature}).";

                }

                //Deal with hypertraining override
                if (hyperIVs?.Length != 6 || IVs?.Length != 6)
                    throw new ArgumentException("hyperIVs & IVs array must be of length 6 (one for each stat).");

                if (hyperIVs.Contains(true)) //at least one hyper trained IV
                {
                    if (hasStatNature)
                        msg += "\r\n\r\n";
                    msg += "Replacing the pku's ";
                    string[] stats = { "HP", "Attack", "Defense", "Sp. Attack", "Sp. Defense", "Speed" };
                    for (int i = 0; i < 6; i++)
                    {
                        if (hyperIVs[i])
                        {
                            if (IVs[i].HasValue)
                                msg += $"{IVs[i]} {stats[i]} IV, ";
                            else
                                msg += $"unspecified {stats[i]} IV ";
                        }
                    }
                    msg += "with 31s as they are Hyper Trained.";
                }

                return msg == "" ? null : new Alert("Battle Stat Override", msg);
            }
            
            public static Alert GetByteOverrideAlert(AlertType at)
            {
                if (at is AlertType.INVALID)
                    return new Alert("Byte Override", "Some of the byte indices were invalid for this format. Ignoring them.");
                else
                    throw InvalidAlertType(at);
            }
        }

        /// <summary>
        /// Alerts pertaining to all .pkx formats.
        /// </summary>
        public static class TagAlerts
        {
            // Alert method design philosophy:
            //      These Alert methods should take in the minimum amount of info needed.
            //      They shouldn't perform any pku logic. They should just create a relevant alert once the alert is known.

            // ----------
            // Generalized Alert Methods
            // ----------

            public static Alert getNumericalAlert(string name, AlertType at, long defaultVal)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert(name, $"{name} tag not specified, using the default of {defaultVal}.");
                else if (at == AlertType.OVERFLOW)
                    return new Alert(name, $"This pku's {name} is higher than the maximum. Rounding down to {defaultVal}.");
                else if (at == AlertType.UNDERFLOW)
                    return new Alert(name, $"This pku's {name} is lower than the minimum. Rounding up to {defaultVal}.");
                else
                    throw InvalidAlertType(at);
            }

            private static Alert getMultiNumericalAlert(string tag, string[] subtags, AlertType[] ats, int max, int min, int defaultVal, bool allUnspecified)
            {
                if (tag == null || ats == null || subtags == null)
                    throw new ArgumentException("tag, subtags, and ats cannot be null.");
                else if (allUnspecified)
                    return new Alert(tag, $"No {tag} were specified, setting them all to {defaultVal}.");
                else if (subtags.Length != ats.Length)
                    throw new ArgumentException("subtags must have the same length as ats.");

                string msgOverflow = "";
                string msgUnderflow = "";
                string msgUnspecifed = "";

                for (int i = 0; i < subtags.Length; i++)
                {
                    if (ats[i] == AlertType.OVERFLOW)
                        msgOverflow += $"{subtags[i]}, ";
                    else if (ats[i] == AlertType.UNDERFLOW)
                        msgUnderflow += $"{subtags[i]}, ";
                    else if (ats[i] == AlertType.UNSPECIFIED)
                        msgUnspecifed += $"{subtags[i]}, ";
                    else if (ats[i] != AlertType.NONE)
                        throw InvalidAlertType(ats[i]);
                }

                string msg = "";
                if (msgOverflow != "")
                    msg += $"The {msgOverflow.Substring(0, msgOverflow.Length - 2)} tag(s) were too high. Rounding them down to {max}\r\n\r\n";
                if (msgUnderflow != "")
                    msg += $"The {msgUnderflow.Substring(0, msgUnderflow.Length - 2)} tag(s) were too low. Rounding them up to {min}\r\n\r\n";
                if (msgUnspecifed != "")
                    msg += $"The {msgUnspecifed.Substring(0, msgUnspecifed.Length - 2)} tag(s) were unspecified. Setting them to {defaultVal}\r\n\r\n";

                if (msg == "")
                    return null;
                else
                    return new Alert(tag, msg.Substring(0, msg.Length - 4) + ".");
            }

            private static Alert GetEnumAlert(string tagName, string defaultVal, AlertType at, string invalidVal = null)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert(tagName, $"No {tagName.ToLowerInvariant()} was specified, using the default: {defaultVal}.");
                else if (invalidVal == null)
                    throw new ArgumentException("Must give the invalid value if there is a specified enum Alert.");
                else if (at == AlertType.INVALID)
                    return new Alert(tagName, $"The {tagName.ToLowerInvariant()} \"{invalidVal}\" is not supported by this format, using the default: {defaultVal}.");
                else
                    throw InvalidAlertType(at);
            }


            // ----------
            // Game Info Alert Methods
            // ----------

            public static Alert GetIDAlert(AlertType at)
            {
                long val = 0;
                if (at == AlertType.UNSPECIFIED)
                    val = 0;
                else if (at == AlertType.OVERFLOW)
                    val = 4294967295;
                else if (at == AlertType.UNDERFLOW)
                    val = 0;
                return getNumericalAlert("ID", at, val);
            }

            public static Alert GetOTAlert(int maxChars, params AlertType[] ats)
            {
                string msg = "";
                if (ats.Contains(AlertType.UNSPECIFIED))
                {
                    return new Alert("OT", $"OT was not specified, leaving blank.");
                }
                if (ats.Contains(AlertType.INVALID)) //invalid characters, removing
                {
                    msg += $"Some of the characters in the OT are invalid in this format, removing them.";
                }
                if (ats.Contains(AlertType.OVERFLOW)) //too many characters, truncating
                {
                    if (msg != "")
                        msg += "\r\n\r\n";
                    msg += $"OTs can only have {maxChars} characters in this format, truncating it.";
                }

                if (msg != "")
                    return new Alert("OT", msg);
                else
                    throw InvalidAlertType();
            }

            public static Alert GetOTAlert(params AlertType[] ats)
            {
                if (ats.Contains(AlertType.OVERFLOW))
                    throw new ArgumentException("Overflow OT Alerts must include the character limit.");
                else
                    return GetOTAlert(-1, ats);
            }

            public static Alert GetLanguageAlert(AlertType at, string invalidLang = null)
            {
                return GetEnumAlert("Language", DEFAULT_LANGUAGE.ToFormattedString(), at, invalidLang);
            }

            public static Alert GetOriginGameAlert(AlertType at, string originGame = null, string officialOriginGame = null)
            {
                if (at == AlertType.INVALID)
                {
                    string msg = "";
                    if (originGame != null && officialOriginGame != null)
                        msg += $"Neither the specified origin game {originGame} nor the official origin game {officialOriginGame}";
                    else if (originGame != null)
                        msg += $"The specified origin game {originGame} doesn't";
                    else if (officialOriginGame != null)
                        msg += $"The specified official origin game {officialOriginGame} doesn't";
                    return new Alert("Origin Game", msg + $" exist in this format. Using the default of None.");
                }
                else if (at == AlertType.UNSPECIFIED)
                    return new Alert("Origin Game", $"The met location was unspecified. Using the default of None.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetOTGenderAlert(AlertType at, string invalidGender = null)
            {
                return GetEnumAlert("OT Gender", DEFAULT_GENDER.ToFormattedString(), at, invalidGender);
            }


            // ----------
            // Catch Info Alert Methods
            // ----------

            public static Alert GetMetLocationAlert(AlertType at, string defaultLoc, string invalidLocation = null)
            {
                if (at == AlertType.INVALID)
                    return new Alert("Met Location", $"The location \"{invalidLocation}\" doesn't exist in specified origin game. Using the default location: {defaultLoc ?? "None"}.");
                else if (at == AlertType.UNSPECIFIED)
                    return new Alert("Met Location", $"The met location was unspecified. Using the default location: {defaultLoc ?? "None"}.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetMetLevelAlert(AlertType at)
            {
                int val = 0;
                if (at == AlertType.UNSPECIFIED)
                    val = 0;
                else if (at == AlertType.OVERFLOW)
                    val = 127;
                else if (at == AlertType.UNDERFLOW)
                    val = 0;

                return getNumericalAlert("Met Level", at, val);
            }

            public static Alert GetBallAlert(AlertType at, string invalidBall = null)
            {
                return GetEnumAlert("Ball", DEFAULT_BALL.ToFormattedString(), at, invalidBall);
            }


            // ----------
            // Pokemon Attribute Alert Methods
            // ----------

            public static Alert GetPIDAlert(AlertType at, List<(string, object, object)> tags = null)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert("PID", $"PID not specified, generating one that matches this pku's other tags.");
                else if (at == AlertType.OVERFLOW)
                    return new Alert("PID", $"This pku's PID is higher than the maximum, generating one that matches this pku's other tags.");
                else if (at == AlertType.UNDERFLOW)
                    return new Alert("PID", $"This pku's PID is lower than the minimum, generating one that matches this pku's other tags.");
                else if (tags == null || tags.Count == 0)
                    throw new ArgumentException("if getPIDAlert recieves a MISMATCH alert, it must also recieve the tags parameter.");
                else if (at == AlertType.MISMATCH)
                {
                    (string, string)[] choices = new (string, string)[2];
                    string choice1msg = "";
                    string choice2msg = "";
                    foreach ((string name, object a, object b) in tags)
                    {
                        choice1msg += $"{name}: {a}\n";
                        choice2msg += $"{name}: {b}\n";
                    }
                    choice1msg = choice1msg.Substring(0, choice1msg.Length - 1);
                    choice2msg = choice2msg.Substring(0, choice2msg.Length - 1);
                    choices[0] = ("Use original PID", choice1msg);
                    choices[1] = ("Generate new PID", choice2msg);

                    return new RadioButtonAlert("PID-Mismatch", "This pku's PID is incompatible with some of its other " +
                        "tags (in this format). Choose whether to keep the PID or generate a compatible one.", choices);
                }
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetNicknameAlert(AlertType at1, int? maxCharacters = null, AlertType at2 = AlertType.NONE)
            {
                AlertType[] ats = new AlertType[] { at1, at2 };
                string msg = "";
                if (ats.Contains(AlertType.INVALID)) //invalid characters, removing
                {
                    msg += $"Some of the characters in the nickname are invalid in this format, removing them.";
                }
                if (ats.Contains(AlertType.OVERFLOW)) //too many characters, truncating
                {
                    if (maxCharacters == null)
                        throw new ArgumentNullException("maxCharacters", "maxCharacters must be specified if OVERFLOW error is given.");
                    if (msg != "")
                        msg += "\r\n\r\n";
                    msg += $"Nickname can only have {maxCharacters} characters in this format, truncating it.";
                }

                if (msg != "")
                    return new Alert("Nickname", msg);
                else
                    throw InvalidAlertType();
            }

            public static Alert GetNicknameFlagAlert(AlertType at, bool flagset, string defaultName = null)
            {
                if (at == AlertType.MISMATCH)
                {
                    if (flagset)
                    {
                        if (defaultName == null)
                            throw new ArgumentException("defaultName must be specified on a MISMATCH alert where flagst == true.");
                        return new Alert("Nickname Flag", $"This pku's Nickname Flag is true, yet it doesn't have a nickname. Setting the nickname to: {defaultName}.");
                    }
                    else
                        return new Alert("Nickname Flag", "This pku's Nickname Flag is false, yet it has a nickname.");
                }
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetItemAlert(AlertType at, string invalidItem)
            {
                if (at == AlertType.INVALID)
                    return new Alert("Item", $"The held item {invalidItem} is not valid in this format. Setting the held item to none.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetLevelAlert(AlertType at)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert("Level", $"No level specified, using the default: 1.");
                else if (at == AlertType.OVERFLOW)
                    return new Alert("Level", $"This pku's level is too high. Rounding down to 100.");
                else if (at == AlertType.UNDERFLOW)
                    return new Alert("Level", $"This pku's level is too low. Rounding up to 1.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetEXPAlert(AlertType at, int? level100exp = null)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert("EXP", $"EXP tag not specified, using the default of 0, i.e. level 1.");
                else if (at == AlertType.OVERFLOW)
                {
                    if (!level100exp.HasValue)
                        throw new ArgumentException("If getEXPAlert is given an OVERFLOW alert, it must also receive the species' level100exp.");
                    return new Alert("EXP", $"This pku's EXP is higher than the maximum. Rounding down to {level100exp}, i.e. level 100.");
                }
                else if (at == AlertType.UNDERFLOW)
                    return new Alert("EXP", $"This pku's EXP is lower than the minimum. Rounding up to 0, i.e. level 1.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetLevelExpAlert(AlertType at, (AlertType atLevel, AlertType atEXP, int level, int exp, int levelToExp, int expToLevel, int level100Exp)? mismatchData = null)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert("Level/Experience", "Neither the level nor experience was specified. Defaulting to level 1.");
                else if (at == AlertType.OVERFLOW) //both overflow
                    return new Alert("Level/Experience", "Both the Level and EXP tags are too high, rounding down to level 100.");
                else if (at == AlertType.UNDERFLOW) //both underflow
                    return new Alert("Level/Experience", "Both the Level and EXP tags are too low, rounding up to level 1.");
                else if (at == AlertType.MISMATCH) //mismatch (can't both be over/underflow)
                {
                    //levelData and expData must be given for MISMATCH alert
                    if (!mismatchData.HasValue)
                        throw new Exception("If the MISMATCH AlertType is given to getLevelExpAlert, then mismatchData must be given.");

                    // Deal with phrasing the level option
                    (string, string)[] choices = new (string, string)[2];
                    if (mismatchData.Value.atLevel == AlertType.OVERFLOW)
                        choices[0] = ("Use Level Tag", $"Set to level 100, i.e. {mismatchData.Value.level100Exp} experience (rounded down because level tag was too high).");
                    else if (mismatchData.Value.atLevel == AlertType.UNDERFLOW)
                        choices[0] = ("Use Level Tag", $"Set to level 1, i.e. 0 experience (rounded up because level tag was too low).");
                    else if (mismatchData.Value.atLevel == AlertType.NONE)
                        choices[0] = ("Use Level Tag", $"Set to level {mismatchData.Value.level}, i.e. {mismatchData.Value.levelToExp} exp.");
                    else
                        throw new Exception("No valid AlertTypes were given to the LEVEL part of getLevelExpAlert's MISMATCH alert");

                    // Deal with phrasing the exp option
                    if (mismatchData.Value.atEXP == AlertType.OVERFLOW)
                        choices[1] = ("Use Experience Tag", $"Set experience to {mismatchData.Value.level100Exp} experience, i.e. level 100 (rounded down because experience tag was too high).");
                    else if (mismatchData.Value.atEXP == AlertType.UNDERFLOW)
                        choices[1] = ("Use Experience Tag", $"Set experience to 0, i.e. level 1 (rounded up because level tag was too low).");
                    else if (mismatchData.Value.atEXP == AlertType.NONE)
                        choices[1] = ("Use Experience Tag", $"Set experience to {mismatchData.Value.exp}, i.e. level {mismatchData.Value.expToLevel}.");
                    else
                        throw new Exception("No valid AlertTypes were given to the EXP part of getLevelExpAlert's MISMATCH alert");

                    //put 2 options together
                    return new RadioButtonAlert("Level/Experience", $"The given level and experience don't match. Choose which one to use.", choices);
                }
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetPPUpAlert(AlertType at, (int[] overflow, int[] underflow)? invalidData = null)
            {
                if (at == AlertType.UNSPECIFIED) //shouldn't be called, should be silent
                    return new Alert("PP Ups", $"PP Ups tag not specified, giving each move 0 PP ups.");
                else if (at == AlertType.INVALID)
                {
                    if (!invalidData.HasValue || invalidData.Value.overflow.Length < 1 && invalidData.Value.underflow.Length < 1)
                        throw new ArgumentException("If getPPUpAlert is given an INVALID alert, it must also receive invalidData and one of those arrays must be non-empty.");

                    string msgOF = "";
                    string msgUF = "";

                    if (invalidData.Value.overflow.Length > 0)
                    {
                        msgOF += "The PP-Ups of ";
                        msgOF += invalidData.Value.overflow.Length == 1 ? "move " : "moves "; //plural check
                        foreach (int i in invalidData.Value.overflow) //add moves
                            msgOF += i + 1 + ","; //starts at move 1 not move 0
                        msgOF = msgOF.Substring(0, msgOF.Length - 1); //remove extra comma
                        msgOF += " are too high, rounding them down to 3.";
                    }
                    if (invalidData.Value.underflow.Length > 0)
                    {
                        msgUF += "The PP-Ups of ";
                        msgUF += invalidData.Value.underflow.Length == 1 ? "move " : "moves "; //plural check
                        foreach (int i in invalidData.Value.underflow) //add moves
                            msgUF += i + 1 + ","; //starts at move 1 not move 0
                        msgUF = msgUF.Substring(0, msgUF.Length - 1); //remove extra comma
                        msgUF += " are too low, rounding them up to 0.";
                    }

                    string msg;
                    if (msgOF != "" && msgUF != "")
                        msg = msgOF + "\r\n\r\n" + msgUF;
                    else if (msgOF != "")
                        msg = msgOF;
                    else //must be underflow
                        msg = msgUF;

                    return new Alert("PP Ups", msg);
                }
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetFriendshipAlert(AlertType at)
            {
                int val = 0;
                if (at == AlertType.UNSPECIFIED)
                    val = 0;
                else if (at == AlertType.OVERFLOW)
                    val = 255;
                else if (at == AlertType.UNDERFLOW)
                    val = 0;

                return getNumericalAlert("Friendship", at, val);
            }

            public static Alert GetMoveAlert(AlertType at, int? movesUsed = null)
            {
                if (at == AlertType.UNSPECIFIED) // Move tag is unspecified, or empty
                    return new Alert("Moves", $"This pku has no moves, the Pokemon's moveset will be empty.");
                else if (at == AlertType.INVALID) // One or more moves is invalid
                {
                    if (movesUsed == null)
                        throw new ArgumentException("Must specify movesUsed (0-4), if INVALID AlertType used.");

                    string msg = movesUsed == 0 ? "None of the pku's moves are valid in this format, the Pokemon's moveset will be empty." :
                        $"Some of the pku's moves are invalid in this format, using the first {movesUsed} valid moves.";
                    return new Alert("Moves", msg);
                }
                else if(at == AlertType.OVERFLOW)
                    return new Alert("Moves", $"This pku has more than 4 valid moves, using the first 4.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetGenderAlert(AlertType at, Gender? correctGender = null, string invalidGender = null)
            {
                if (at == AlertType.UNSPECIFIED) //only if it has no mandatory gender, otherwise dont even alert
                    return new Alert("Gender", $"This species can be either male or female, yet no gender was specified. Setting to male.");
                else if (!correctGender.HasValue || invalidGender == null)
                    throw new ArgumentException("If getGenderAlert is given MISMATCH or INVALID, the correctGender and invalidGender must be given.");
                else if (at == AlertType.MISMATCH) //mismatch with species
                    return new Alert("Gender", $"This species cannot be {invalidGender}. Setting gender to {correctGender}.");
                else if (at == AlertType.INVALID)
                    return new Alert("Gender", $"\"{invalidGender}\" is not a valid gender. Setting gender to {correctGender}.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetFormAlert(AlertType at, string[] invalidForm = null)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert("Form", $"No form specified, using the default form.");
                else if (invalidForm == null)
                    throw new ArgumentException("Must give invalidForm if there is a specified form Alert.");
                else if (at == AlertType.CASTED)
                    return new Alert("Form", $"The form \"{invalidForm.ToFormattedString()}\" does not exist in this format and has been casted to its default form.");
                else if (at == AlertType.IN_BATTLE)
                    return new Alert("Form", $"The form \"{invalidForm.ToFormattedString()}\" only exists in-battle, using its out of battle form.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetNatureAlert(AlertType at, string invalidNature = null)
            {
                return GetEnumAlert("Nature", DEFAULT_NATURE.ToFormattedString(), at, invalidNature);
            }

            public static Alert GetEVsAlert(params AlertType[] ats)
            {
                if (ats == null || ats.Length != 1 && ats.Length != 6)
                    throw new ArgumentException("getEVsAlert() only accepts a single UNSPECFIED AlertType, or six OVERFLOW/UNDERFLOW/UNSPECIFIED AlertTypes.");
                return getMultiNumericalAlert("EVs", new string[]
                {
                    "HP", "Attack", "Defense", "Sp. Attack", "Sp. Defense", "Speed"
                }, ats, 255, 0, 0, ats.Length == 1 && ats[0] == AlertType.UNSPECIFIED);
            }

            public static Alert GetIVsAlert(params AlertType[] ats)
            {
                if (ats == null || ats.Length != 1 && ats.Length != 6)
                    throw new ArgumentException("getIVsAlert() only accepts a single UNSPECFIED AlertType, or six OVERFLOW/UNDERFLOW/UNSPECIFIED AlertTypes.");
                return getMultiNumericalAlert("IVs", new string[]
                {
                    "HP", "Attack", "Defense", "Sp. Attack", "Sp. Defense", "Speed"
                }, ats, 31, 0, 0, ats.Length == 1 && ats[0] == AlertType.UNSPECIFIED);
            }

            public static Alert GetContestAlert(params AlertType[] ats)
            {
                if (ats == null || ats.Length != 1 && ats.Length != 6)
                    throw new ArgumentException("getContestAlert() only accepts a single UNSPECFIED AlertType, or six OVERFLOW/UNDERFLOW/UNSPECIFIED AlertTypes.");
                return getMultiNumericalAlert("Contest Stats", new string[]
                {
                    "Cool", "Beautiful", "Cute", "Clever", "Tough", "Sheen"
                }, ats, 255, 0, 0, ats.Length == 1 && ats[0] == AlertType.UNSPECIFIED);
            }

            public static Alert GetPokerusAlert(AlertType atStrain, AlertType atDays)
            {
                if (atStrain == AlertType.NONE && atDays == AlertType.NONE)
                    return null;

                Alert s = null, d = null;
                if (atStrain == AlertType.OVERFLOW)
                    s = getNumericalAlert("Pokérus strain", atStrain, 15);
                else if (atStrain == AlertType.UNDERFLOW)
                    s = getNumericalAlert("Pokérus strain", atStrain, 0);
                else if (atStrain != AlertType.NONE)
                    InvalidAlertType(atStrain);

                if (atDays == AlertType.OVERFLOW)
                    d = getNumericalAlert("Pokérus strain", atDays, 4);
                else if (atStrain == AlertType.UNDERFLOW)
                    d = getNumericalAlert("Pokérus strain", atDays, 0);
                else if (atDays != AlertType.NONE)
                    InvalidAlertType(atDays);

                string msg = "";
                if (s != null)
                    msg += s.Message;
                if (msg.Length > 0)
                    msg += "\r\n\r\n";
                if (d != null)
                    msg += d.Message;

                return new Alert("Pokérus", msg);
            }

            public static Alert GetAbilityAlert(AlertType at, string invalidAbility = null, string defaultAbility = "None")
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert("Ability", $"No ability was specified, using the default ability: {defaultAbility}.");
                else if (invalidAbility == null)
                    throw new ArgumentException("Must give the invalid value if there is a specified ability Alert.");
                else if (at == AlertType.MISMATCH) //only in gen 3
                    return new Alert("Ability", $"This species cannot have the ability {invalidAbility} in this format. Using the default ability: {defaultAbility}.");
                else if (at == AlertType.INVALID)
                    return new Alert("Ability", $"The ability {invalidAbility} is not supported by this format, using the default ability: {defaultAbility}.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetRibbonAlert(AlertType at)
            {
                if (at == AlertType.INVALID)
                    return new Alert("Ribbons", "Some of the pku's ribbons are not valid in this format. Ignoring them.");
                else
                    throw InvalidAlertType(at);
            }

            public static Alert GetTrashAlert(AlertType atNickname, AlertType atOT, int nickEntries, int OTEntries, int? maxVal = null)
            {
                if ((atNickname, atOT) is (AlertType.NONE, AlertType.NONE))
                    throw InvalidAlertType();

                if((atNickname is AlertType.OVERFLOW || atOT is AlertType.OVERFLOW) && maxVal is null)
                    throw new ArgumentException("maxVal cannot be null if there is an overflow error.", nameof(maxVal));

                string msg = "";
                if (atNickname is AlertType.OVERFLOW)
                    msg += $"One or more of the entries in Nickname Trash Bytes is too high, the maximum value is ${maxVal}. Ignoring them.";
                else if (atNickname is AlertType.MISMATCH)
                    msg += $"The number of entries in the Nickname Trash Bytes must be exactly {nickEntries}. Ignoring them.";

                if (msg.Length > 0 && atOT is not AlertType.NONE)
                    msg += "\r\n\r\n";

                if (atOT is AlertType.OVERFLOW)
                    msg += $"One or more of the entries in OT Trash Bytes is too high, the maximum value is ${maxVal}. Ignoring them.";
                else if (atOT is AlertType.MISMATCH)
                    msg += $"The number of entries in the OT Trash Bytes must be exactly {OTEntries}. Ignoring them.";

                return new Alert("Trash Bytes", msg);
            }
        }

        public static class AuxProcess
        {
            public static Alert ProcessBattleStatOverride(pkuObject pku, GlobalFlags flags)
            {
                //generate alert, BEFORE modifying pku
                Alert alert = AuxAlerts.GetBattleStatAlert(pku.Stat_Nature != null, pku.Nature != null, pku.Stat_Nature, pku.Nature, new bool[]
                {
                    pku.Hyper_Training?.HP == true,
                    pku.Hyper_Training?.Attack == true,
                    pku.Hyper_Training?.Defense == true,
                    pku.Hyper_Training?.Sp_Attack == true,
                    pku.Hyper_Training?.Sp_Defense == true,
                    pku.Hyper_Training?.Speed == true
                }, new int?[]
                {
                    pku.IVs?.HP,
                    pku.IVs?.Attack,
                    pku.IVs?.Defense,
                    pku.IVs?.Sp_Attack,
                    pku.IVs?.Sp_Defense,
                    pku.IVs?.Speed
                });

                if (flags?.Battle_Stat_Override == true)
                {
                    //If stat nature is specified, replace nature with it
                    if (pku.Stat_Nature != null)
                        pku.Nature = pku.Stat_Nature;

                    //If any hyper training is specified, make sure IV object is not null.
                    if (pku.IVs == null && (pku.Hyper_Training?.HP == true || pku.Hyper_Training?.Attack == true ||
                                           pku.Hyper_Training?.Defense == true || pku.Hyper_Training?.Sp_Attack == true ||
                                           pku.Hyper_Training?.Sp_Defense == true || pku.Hyper_Training?.Speed == true))
                        pku.IVs ??= new pkuObject.IVs_Class();

                    //If any hyper training is specified, replace corresponding IVs with 31
                    if (pku.Hyper_Training?.HP == true)
                        pku.IVs.HP = 31;
                    if (pku.Hyper_Training?.Attack == true)
                        pku.IVs.Attack = 31;
                    if (pku.Hyper_Training?.Defense == true)
                        pku.IVs.Defense = 31;
                    if (pku.Hyper_Training?.Sp_Attack == true)
                        pku.IVs.Sp_Attack = 31;
                    if (pku.Hyper_Training?.Sp_Defense == true)
                        pku.IVs.Sp_Defense = 31;
                    if (pku.Hyper_Training?.Speed == true)
                        pku.IVs.Speed = 31;

                    return alert;
                }
                else
                    return null;
            }

            public static Alert ProcessByteOverride(pkuObject pku, params ByteArrayManipulator[] bams)
            {
                bool invalid = false;
                static bool singleBam(ByteArrayManipulator bam, Dictionary<int, byte> bo)
                {
                    bool invalid = false;
                    foreach (var kvp in bo)
                    {
                        if (kvp.Key >= bam.Length)
                        {
                            invalid = true;
                            continue;
                        }
                        bam.SetByte(kvp.Value, kvp.Key);
                    }
                    return invalid;
                }
                if (bams.Length > 0 && pku.Byte_Override?.Main_Data is not null)
                    invalid &= singleBam(bams[0], pku.Byte_Override.Main_Data);
                if (bams.Length > 1 && pku.Byte_Override?.A is not null)
                    invalid &= singleBam(bams[1], pku.Byte_Override.A);
                if (bams.Length > 2 && pku.Byte_Override?.B is not null)
                    invalid &= singleBam(bams[2], pku.Byte_Override.B);
                if (bams.Length > 3 && pku.Byte_Override?.C is not null)
                    invalid &= singleBam(bams[3], pku.Byte_Override.C);
                if (bams.Length > 4 && pku.Byte_Override?.D is not null)
                    invalid &= singleBam(bams[4], pku.Byte_Override.D);
                if (bams.Length > 5)
                    throw new ArgumentException($"At most 5 different {nameof(ByteArrayManipulator)}s should have been passed.", nameof(bams));

                return invalid ? AuxAlerts.GetByteOverrideAlert(AlertType.INVALID) : null;
            }
        }


        public static class ProcessTags
        {
            // ----------
            // Generalized Processing Methods
            // ----------

            public static (int[], Alert) ProcessMultiNumericTag(bool specified, int?[] vals, Func<AlertType[], Alert> alertFunc, int max, int min, int defaultVal, bool silentUnspecified)
            {
                int[] checkedVals = new int[vals.Length];
                Alert alert = null;

                AlertType[] valAlerts = new AlertType[vals.Length];
                if (specified)
                {
                    for (int i = 0; i < vals.Length; i++)
                    {
                        if (vals[i].HasValue)
                        {
                            if (vals[i] > max)
                            {
                                checkedVals[i] = max; //max val
                                valAlerts[i] = AlertType.OVERFLOW;
                            }
                            else if (vals[i] < min)
                            {
                                checkedVals[i] = min; //min val
                                valAlerts[i] = AlertType.UNDERFLOW;
                            }
                            else //valid val
                            {
                                checkedVals[i] = vals[i].Value;
                                valAlerts[i] = AlertType.NONE;
                            }
                        }
                        else
                        {
                            checkedVals[i] = defaultVal; //default val
                            valAlerts[i] = AlertType.UNSPECIFIED;
                        }
                    }
                    alert = alertFunc(valAlerts);
                }
                else if (!silentUnspecified)
                    alert = alertFunc(new AlertType[] { AlertType.UNSPECIFIED });

                return (checkedVals, alert);
            }

            public static (int, Alert) ProcessNumericTag(int? tag, Func<AlertType, Alert> getAlertFunc, bool silentUnspecified, int max, int min, int defaultVal)
            {
                int val = defaultVal;
                Alert alert = null;

                if (tag.HasValue)
                {
                    if (tag > max) //overflow
                    {
                        val = max;
                        alert = getAlertFunc(AlertType.OVERFLOW);
                    }
                    else if (tag < min) //underflow
                        alert = getAlertFunc(AlertType.UNDERFLOW);
                    else //tag is within valid range
                        val = tag.Value;
                }
                else if (!silentUnspecified)
                    alert = getAlertFunc(AlertType.UNSPECIFIED);

                return (val, alert);
            }

            public static (uint, Alert) ProcessNumericTag(uint? tag, Func<AlertType, Alert> getAlertFunc, bool silentUnspecified, uint max, uint min, uint defaultVal)
            {
                uint val = defaultVal;
                Alert alert = null;

                if (tag.HasValue)
                {
                    if (tag > max) //overflow
                    {
                        val = max;
                        alert = getAlertFunc(AlertType.OVERFLOW);
                    }
                    else if (tag < min) //underflow
                        alert = getAlertFunc(AlertType.UNDERFLOW);
                    else //tag is within valid range
                        val = tag.Value;
                }
                else if (!silentUnspecified)
                    alert = getAlertFunc(AlertType.UNSPECIFIED);

                return (val, alert);
            }

            private static (T, Alert) ProcessEnumTag<T>(string tag, T? tagTest, Func<AlertType, string, Alert> getAlertFunc,
                bool silentUnspecified, T defaultVal, Func<T, bool> isValidFunc = null) where T : struct
            {
                if (tag != null) //tag specified
                {
                    if (tagTest != null && (isValidFunc == null || isValidFunc(tagTest.Value)))
                        return (tagTest.Value, null); //valid enum, no alerts
                    else //tag invalid, use default
                        return (defaultVal, getAlertFunc(AlertType.INVALID, tag));
                }
                else if (silentUnspecified) //tag unspecified (don't alert), use default
                    return (defaultVal, null);
                else //tag unspecified (do alert), use default
                    return (defaultVal, getAlertFunc(AlertType.UNSPECIFIED, tag));
            }


            // ----------
            // String Processing Methods
            // ----------

            private static ushort GetTerminator(Dictionary<ushort, char> charset)
            {
                return charset.FirstOrDefault(x => x.Value == '\0').Key;
            }

            private static Dictionary<ushort, char> UpCastCharset(Dictionary<byte, char> charset)
            {
                return charset.ToDictionary(kvp => (ushort)kvp.Key, kvp => kvp.Value);
            }

            //Helper method for processnickname and processOT
            //this just encodes strings and adds a terminator, it doesn't deal with trash.
            private static (ushort[] encodedString, bool truncated, bool hasInvalidChars) EncodeString(string str, int maxLength, Dictionary<ushort, char> charset)
            {
                bool truncated = false, hasInvalidChars = false;
                ushort[] encodedStr = new ushort[maxLength];

                //Encode string
                int successfulChars = 0;
                while (str?.Length > 0 && successfulChars < maxLength)
                {
                    ushort? encodedChar = charset?.FirstOrDefault(x => x.Value == str[0]).Key; //get next character
                    str = str[1..]; //chop off current character

                    //if character invalid
                    if (encodedChar is null)
                    {
                        hasInvalidChars = true;
                        continue;
                    }

                    //else character not invalid
                    encodedStr[successfulChars] = encodedChar.Value;
                    successfulChars++;

                    //stop encoding when limit reached
                    if (successfulChars >= maxLength)
                        break;
                }

                //Deal with terminator
                if (successfulChars < maxLength)
                    encodedStr[successfulChars] = GetTerminator(charset);
                return (encodedStr, truncated, hasInvalidChars);
            }

            public static (ushort[] nickname, Alert nicknameAlert, bool nicknameFlag, Alert nicknameFlagAlert) ProcessNickname(pkuObject pku, int gen, Language checkedLang, int maxLength, Dictionary<ushort, char> charset)
            {
                ushort[] name;
                bool nicknameFlag = pku.Nickname_Flag is true;
                Alert alert = null;
                Alert flagAlert = null;
                int dex = GetNationalDexChecked(pku.Species); //must be valid at this point

                if (pku.Nickname is not null) //specified
                {
                    //name
                    bool truncated, invalid;
                    (name, truncated, invalid) = EncodeString(pku.Nickname, maxLength, charset);
                    if (truncated && invalid)
                        alert = GetNicknameAlert(AlertType.OVERFLOW, maxLength, AlertType.INVALID);
                    else if (truncated)
                        alert = GetNicknameAlert(AlertType.OVERFLOW, maxLength);
                    else if (invalid)
                        alert = GetNicknameAlert(AlertType.INVALID);

                    //flag
                    if (pku.Nickname_Flag is null)
                        nicknameFlag = true;

                    if (!nicknameFlag)
                        flagAlert = GetNicknameFlagAlert(AlertType.MISMATCH, false);
                }
                else //unspecified, get default name for given language
                {
                    //name
                    string defaultName = PokeAPIUtil.GetSpeciesNameTranslated(dex, checkedLang);

                    if (gen < 5) //Capitalize Gens 1-4
                        defaultName = defaultName.ToUpperInvariant();

                    if (gen < 8 && dex is 83) //farfetch'd uses ’ in Gens 1-7
                        defaultName = defaultName.Replace('\'', '’'); //Gen 8: verify this once pokeAPI updates

                    (name, _, _) = EncodeString(defaultName, maxLength, charset); //species names shouldn't be truncated/invalid...

                    //flag
                    if (pku.Nickname_Flag is null)
                        nicknameFlag = false;

                    if (nicknameFlag)
                        flagAlert = GetNicknameFlagAlert(AlertType.MISMATCH, true, defaultName);
                }

                return (name, alert, nicknameFlag, flagAlert);
            }

            //1-byte encoding override
            public static (byte[] nickname, Alert nicknameAlert, bool nicknameFlag, Alert nicknameFlagAlert) ProcessNickname(pkuObject pku, int gen, Language checkedLang, int maxLength, Dictionary<byte, char> charset)
            {
                var (a, b, c, d) = ProcessNickname(pku, gen, checkedLang, maxLength, UpCastCharset(charset));
                return (Array.ConvertAll(a, s => (byte)s), b, c, d);
            }

            public static (ushort[], Alert) ProcessOT(pkuObject pku, int maxLength, Dictionary<ushort, char> charset)
            {
                ushort[] otName;
                Alert alert = null;

                if (pku.Game_Info?.OT is not null) //OT specified
                {
                    bool truncated, invalid;
                    (otName, truncated, invalid) = EncodeString(pku.Game_Info.OT, maxLength, charset);
                    if (truncated && invalid)
                        alert = GetOTAlert(maxLength, AlertType.OVERFLOW, AlertType.INVALID);
                    else if (truncated)
                        alert = GetOTAlert(maxLength, AlertType.OVERFLOW);
                    else if (invalid)
                        alert = GetOTAlert(AlertType.INVALID);
                }
                else //OT not specified
                {
                    (otName, _, _) = EncodeString(null, maxLength, charset); //blank array
                    alert = GetOTAlert(AlertType.UNSPECIFIED);
                }
                return (otName, alert);
            }

            //1-byte encoding override
            public static (byte[], Alert) ProcessOT(pkuObject pku, int maxLength, Dictionary<byte, char> charset)
            {
                var (a, b) = ProcessOT(pku, maxLength, UpCastCharset(charset));
                return (Array.ConvertAll(a, s => (byte)s), b);
            }

            //helper method for process trash.
            private static (ushort[], AlertType) ProcessTrashSingle(ushort[] encodedStr, ushort[] trash, Dictionary<ushort, char> charset, ushort maxVal)
            {
                AlertType at = AlertType.NONE;
                if (trash is null)
                    return (encodedStr, at);
                else if (trash.Any(x => x > maxVal))
                    at = AlertType.OVERFLOW;
                else if (trash.Length != encodedStr.Length)
                    at = AlertType.MISMATCH;

                if (at is not AlertType.NONE)
                    return (encodedStr, at);

                ushort[] trashedStr = trash.Clone() as ushort[];
                ushort terminator = GetTerminator(charset);
                for (int i = 0; i < encodedStr.Length; i++)
                {
                    trashedStr[i] = encodedStr[i];
                    if (encodedStr[i] == terminator)
                        break;
                }
                return (trashedStr, at);
            }

            public static (ushort[] trashedName, ushort[] trashedOT, Alert) ProcessTrash(ushort[] encodedName, ushort[] nameTrash, ushort[] encodedOT, ushort[] otTrash, Dictionary<ushort, char> charset, ushort maxVal = ushort.MaxValue)
            {
                (ushort[] trashedName, AlertType atName) = ProcessTrashSingle(encodedName, nameTrash, charset, maxVal);
                (ushort[] trashedOT, AlertType atOT) = ProcessTrashSingle(encodedOT, otTrash, charset, maxVal);
                Alert alert = (atName, atOT) is (AlertType.NONE, AlertType.NONE) ? null : GetTrashAlert(atName, atOT, encodedName.Length, encodedOT.Length, maxVal);
                return (trashedName, trashedOT, alert);
            }

            //1-byte encoding override
            public static (byte[] trashedName, byte[] trashedOT, Alert) ProcessTrash(byte[] encodedName, ushort[] nameTrash, byte[] encodedOT, ushort[] otTrash, Dictionary<byte, char> charset)
            {
                static ushort[] castUp(byte[] a) => Array.ConvertAll(a, x => (ushort)x);
                static byte[] castDown(ushort[] a) => Array.ConvertAll(a, x => (byte)x);
                var (a, b, c) = ProcessTrash(castUp(encodedName), nameTrash, castUp(encodedOT), otTrash, UpCastCharset(charset), byte.MaxValue);
                return (castDown(a), castDown(b), c);
            }


            // ----------
            // Game Info Processing Methods
            // ----------

            public static (uint, Alert) ProcessID(pkuObject pku)
            {
                return ProcessNumericTag(pku.Game_Info?.ID, GetIDAlert, false, 4294967295, 0, 0);
            }

            public static (int gameID, string game, Alert alert) ProcessOriginGame(pkuObject pku, int gen)
            {
                bool triedOfficialOriginGame = false;

                (int? id, int? genIntroduced) = GetGameIDAndGen(pku.Game_Info?.Origin_Game); //try origin game

                if (!id.HasValue) //origin game unspecified/didn't work
                {
                    (id, genIntroduced) = GetGameIDAndGen(pku.Game_Info?.Official_Origin_Game); //try official origin game
                    triedOfficialOriginGame = true;
                }

                // Future games don't exist in past generations
                if (genIntroduced > gen)
                    id = null;

                string game;
                if (!id.HasValue) //neither game worked
                    game = null;
                else //one game worked
                    game = triedOfficialOriginGame ? pku.Game_Info?.Official_Origin_Game : pku.Game_Info?.Origin_Game;

                Alert alert = null;
                if (pku.Game_Info?.Origin_Game == null && pku.Game_Info?.Official_Origin_Game == null) //no origin game specified
                    alert = GetOriginGameAlert(AlertType.UNSPECIFIED);
                else if (!id.HasValue) //origin game specified, just not valid in this gen
                    alert = GetOriginGameAlert(AlertType.INVALID, pku.Game_Info?.Origin_Game, pku.Game_Info?.Official_Origin_Game);

                return (id ?? 0, game, alert); //default gameID is None (0)
            }

            public static (Language, Alert) ProcessLanguage(pkuObject pku, Language[] validLanguages)
            {
                return ProcessEnumTag(pku.Game_Info?.Language, pku.Game_Info?.Language.ToEnum<Language>(), GetLanguageAlert, false, DEFAULT_LANGUAGE, (x) =>
                {
                    return validLanguages.Contains(x);
                });
            }

            public static (Gender, Alert) ProcessOTGender(pkuObject pku)
            {
                Gender? checkedGender = pku.Game_Info?.Gender.ToEnum<Gender>();
                checkedGender = checkedGender is Gender.Genderless ? null : checkedGender;
                return ProcessEnumTag(pku.Game_Info?.Gender, checkedGender, GetOTGenderAlert, false, DEFAULT_GENDER, (x) =>
                {
                    return x != Gender.Genderless;
                });
            }


            // ----------
            // Catch Info Processing Methods
            // ----------

            public static (int, Alert) ProcessMetLevel(pkuObject pku)
            {
                return ProcessNumericTag(pku.Catch_Info?.Met_Level, GetMetLevelAlert, false, 127, 0, 0);
            }

            public static (Ball, Alert) ProcessBall(pkuObject pku, Ball maxBall)
            {
                return ProcessEnumTag(pku.Catch_Info?.Ball, pku.Catch_Info?.Ball.ToEnum<Ball>(), GetBallAlert, false, DEFAULT_BALL, (x) =>
                {
                    return x <= maxBall;
                });
            }

            public static (int, Alert) ProcessMetLocation(pkuObject pku, string checkedGameName, Func<string, string, int?> GetLocationID, string defaultLocation)
            {
                //override game for met location
                if (pku.Catch_Info?.Met_Game_Override != null)
                    checkedGameName = pku.Catch_Info.Met_Game_Override;

                int? locID;

                if (pku.Catch_Info?.Met_Location != null) //location specified
                    locID = GetLocationID(checkedGameName, pku.Catch_Info.Met_Location);
                else //location unspecified
                    return (0, GetMetLocationAlert(AlertType.UNSPECIFIED, defaultLocation));

                if (!locID.HasValue) //location specified but invalid
                    return (0, GetMetLocationAlert(AlertType.INVALID, defaultLocation, pku.Catch_Info.Met_Location));
                else //location specified and valid
                    return (locID.Value, null);
            }


            // ----------
            // Pokemon Attribute Processing Methods
            // ----------

            //Gen 6: account for gen6+ pid change on shiny mismatch
            public static (uint[], Alert) ProcessPID(pkuObject pku, uint checkedID, bool gen6Plus, Gender? checkedGender = null, Nature? checkedNature = null, int? checkedUnownForm = null)
            {
                uint pid, newPID;
                Alert alert = null;
                bool pidInBounds = true;

                // deal with PID bounds
                if (!pku.PID.HasValue)
                {
                    pid = 0;
                    pidInBounds = false;
                    alert = GetPIDAlert(AlertType.UNSPECIFIED);
                }
                else if (pku.PID > 4294967295) //overflow
                {
                    pid = 4294967295;
                    pidInBounds = false;
                    alert = GetPIDAlert(AlertType.OVERFLOW);
                }
                else if (pku.PID < 0) //underflow
                {
                    pid = 0;
                    pidInBounds = false;
                    alert = GetPIDAlert(AlertType.UNDERFLOW);
                }
                else
                    pid = (uint)pku.PID;


                // Check if any value has a pid-mismatch
                int dex = GetNationalDexChecked(pku.Species);
                bool genderMismatch = false, natureMismatch = false, unownMismatch = false, shinyMismatch;
                int oldunownform = 0;
                Nature oldnature = 0;
                Gender oldgender = 0;

                if (checkedGender.HasValue) //gender mismatch check
                {
                    oldgender = GetPIDGender(dex, pid);
                    genderMismatch = checkedGender != oldgender;
                }
                if (checkedNature.HasValue) //nature mismatch check
                {
                    oldnature = (Nature)(pid % 25);
                    natureMismatch = checkedNature != oldnature;
                }
                if (checkedUnownForm.HasValue) //unown form mismatch check
                {
                    oldunownform = pk3Object.GetUnownFormID(pid);
                    unownMismatch = checkedUnownForm != null && checkedUnownForm != oldunownform;
                }
                //always check shiny
                bool oldshiny = (checkedID / 65536 ^ checkedID % 65536 ^ pid / 65536 ^ pid % 65536) < (gen6Plus ? 16 : 8);
                shinyMismatch = pku.IsShiny() != oldshiny;

                // Deal with pid-mismatches
                if (unownMismatch || genderMismatch || natureMismatch || shinyMismatch)
                {
                    newPID = GenerateRandomPID(pku.IsShiny(), checkedID, PokeAPIUtil.GetGenderRatio(dex), checkedGender, checkedNature, checkedUnownForm);

                    if (pidInBounds) //two options: old & new, need error
                    {
                        List<(string, object, object)> tags = new List<(string, object, object)>();
                        if (unownMismatch)
                            tags.Add(("Unown Form", pk3Object.GetUnownFormName(oldunownform), pk3Object.GetUnownFormName(checkedUnownForm.Value)));
                        if (genderMismatch)
                            tags.Add(("Gender", oldgender, checkedGender));
                        if (natureMismatch)
                            tags.Add(("Nature", oldnature, checkedNature));
                        if (shinyMismatch)
                            tags.Add(("Shiny", oldshiny, pku.Shiny));
                        alert = GetPIDAlert(AlertType.MISMATCH, tags); //RadioButtonAlert
                        return (new[] { pid, newPID }, alert); //error: pid mismatched, choose old or new.
                    }
                    else
                        return (new[] { newPID }, alert); //warning: pid out of bounds, generating new one that deals with mismatches.
                }
                return (new[] { pid }, alert); //either:
                                               //   warning: pid unspecified or out of bounds, rounding it.
                                               //no warning: pid is in bounds w/ no mismatches.
            }

            public static (Nature, Alert) ProcessNature(pkuObject pku)
            {
                return ProcessEnumTag(pku.Nature, pku.Nature.ToEnum<Nature>(), GetNatureAlert, false, DEFAULT_NATURE);
            }

            //Gen 6: allow impossible genders in gen 6+ (I think they allow impossible genders...)
            public static (Gender, Alert) ProcessGender(pkuObject pku)
            {
                int dex = GetNationalDexChecked(pku.Species);

                Gender gender;
                Alert alert = null;
                GenderRatio genderRatio = PokeAPIUtil.GetGenderRatio(dex);
                Gender? mandatoryGender = genderRatio switch
                {
                    GenderRatio.All_Genderless => Gender.Genderless,
                    GenderRatio.All_Female => Gender.Female,
                    GenderRatio.All_Male => Gender.Male,
                    _ => null
                };
                if (pku.Gender != null)
                {
                    Gender? readGender = pku.Gender.ToEnum<Gender>();
                    if (readGender.HasValue)
                    {
                        if (mandatoryGender.HasValue && mandatoryGender != readGender) //impossible gender
                        {
                            gender = mandatoryGender.Value;
                            alert = GetGenderAlert(AlertType.MISMATCH, mandatoryGender, readGender.ToFormattedString());
                        }
                        else //no mismatch
                            gender = readGender.Value;
                    }
                    else //gender was invalid, just make it male (or mandatoryGender if it cant be male).
                    {
                        gender = mandatoryGender ?? Gender.Male;
                        alert = GetGenderAlert(AlertType.INVALID); //invalid gender
                    }
                }
                else
                {
                    gender = mandatoryGender ?? Gender.Male;
                    if (!mandatoryGender.HasValue)
                        alert = GetGenderAlert(AlertType.UNSPECIFIED); //only alert if there is no mandatory gender.
                }

                return (gender, alert);
            }

            public static (int, Alert) ProcessFriendship(pkuObject pku)
            {
                return ProcessNumericTag(pku.Friendship, GetFriendshipAlert, false, 255, 0, 0);
            }

            // silent on unspecified
            public static (int[], Alert) ProcessEVs(pkuObject pku)
            {
                int?[] vals = { pku.EVs?.HP, pku.EVs?.Attack, pku.EVs?.Defense, pku.EVs?.Sp_Attack, pku.EVs?.Sp_Defense, pku.EVs?.Speed };
                return ProcessMultiNumericTag(pku.EVs != null, vals, GetEVsAlert, 255, 0, 0, true);
            }

            // not silent on unspecified
            public static (int[], Alert) ProcessIVs(pkuObject pku)
            {
                int?[] vals = { pku.IVs?.HP, pku.IVs?.Attack, pku.IVs?.Defense, pku.IVs?.Sp_Attack, pku.IVs?.Sp_Defense, pku.IVs?.Speed };
                return ProcessMultiNumericTag(pku.IVs != null, vals, GetIVsAlert, 31, 0, 0, false);
            }

            // silent on unspecified
            public static (int[], Alert) ProcessContest(pkuObject pku)
            {
                int?[] vals = { pku.Contest_Stats?.Cool, pku.Contest_Stats?.Beauty, pku.Contest_Stats?.Cute, pku.Contest_Stats?.Clever, pku.Contest_Stats?.Tough, pku.Contest_Stats?.Sheen };
                return ProcessMultiNumericTag(pku.Contest_Stats != null, vals, GetContestAlert, 255, 0, 0, true);
            }

            public static (int, Alert) ProcessItem(pkuObject pku, int gen)
            {
                return ProcessEnumTag(pku.Item, PokeAPIUtil.GetItemIndex(pku.Item, gen), GetItemAlert, true, 0);
            }

            //gmax moves use a different index and cannot even be stored out of battle. Thus, they are irrelevant.
            public static (int[] moveIDs, int[] moveIndicies, Alert) ProcessMoves(pkuObject pku, int lastMoveIndex)
            {
                List<int> moveIndices = new List<int>(); //indicies in pku
                int[] moveIDs = new int[4]; //index numbers for format
                Alert alert = null;
                bool invalid = false;
                if (pku.Moves != null)
                {
                    int confirmedMoves = 0;

                    int? moveIDTemp;
                    for (int i = 0; i < pku.Moves.Length; i++)
                    {
                        if (pku.Moves[i].Name != null) //move has a name
                        {
                            moveIDTemp = PokeAPIUtil.GetMoveIndex(pku.Moves[i].Name);
                            moveIDTemp = moveIDTemp > lastMoveIndex ? null : moveIDTemp;
                            if (moveIDTemp.HasValue && confirmedMoves < 4)
                            {
                                moveIDs[confirmedMoves] = moveIDTemp.Value;
                                moveIndices.Add(i);
                                confirmedMoves++;
                            }
                            else if(moveIDTemp == null)
                                invalid = true;
                        }
                    }

                    if (confirmedMoves != pku.Moves.Length) //not a perfect match
                    {
                        if(invalid)
                            alert = GetMoveAlert(AlertType.INVALID, confirmedMoves);
                        else //if its not invalid but numbers don't match, then must be an overflow
                            alert = GetMoveAlert(AlertType.OVERFLOW);
                    }
                }
                else
                    alert = GetMoveAlert(AlertType.UNSPECIFIED);

                return (moveIDs, moveIndices.ToArray(), alert);
            }

            public static (int[], Alert) ProcessPPUps(pkuObject pku, int[] moveIndicies)
            {
                int[] ppups = new int[4];
                Alert alert = null;
                List<int> overflow = new List<int>();
                List<int> underflow = new List<int>();
                if (moveIndicies.Length > 4)
                    throw new ArgumentException("moveIndicies should only be of length 0-4.");
                for (int i = 0; i < moveIndicies.Length; i++)
                {
                    if ((pku?.Moves?[moveIndicies[i]]?.PP_Ups).HasValue) //this move exists and has a ppup value
                    {
                        if (pku.Moves[moveIndicies[i]].PP_Ups > 3)
                        {
                            ppups[i] = 3;
                            overflow.Add(i);
                        }
                        else if (pku.Moves[moveIndicies[i]].PP_Ups < 0)
                            underflow.Add(i);
                        else
                            ppups[i] = pku.Moves[moveIndicies[i]].PP_Ups.Value;
                    }
                }
                if (overflow.Count > 0 || underflow.Count > 0)
                    alert = GetPPUpAlert(AlertType.INVALID, (overflow.ToArray(), underflow.ToArray()));
                return (ppups, alert);
            }

            public static (uint[], Alert) ProcessEXP(pkuObject pku)
            {
                uint exp;
                uint? expFromLevel = null;
                Alert alert = null;
                int? dexTest = GetNationalDex(pku.Species); //must be valid at this point
                if (!dexTest.HasValue)
                    throw new ArgumentException("ProcessNickname expects an official pku species.");
                int dex = dexTest.Value;

                int level100EXP = PokeAPIUtil.GetEXPFromLevel(dex, 100);

                //data collection - Level
                int? levelChecked = null;
                int levelAsEXP = -1;
                AlertType atLevel = AlertType.NONE;
                if (pku.Level.HasValue) //sets level alert, and gets levelChecked + levelAsExp
                {
                    if (pku.Level > 100) //exp overflow
                    {
                        levelChecked = 100;
                        levelAsEXP = level100EXP;
                        atLevel = AlertType.OVERFLOW;
                    }
                    else if (pku.Level < 1) //level underflow
                    {
                        levelChecked = 1;
                        levelAsEXP = 0;
                        atLevel = AlertType.UNDERFLOW;
                    }
                    else //level valid
                    {
                        levelChecked = pku.Level;
                        levelAsEXP = PokeAPIUtil.GetEXPFromLevel(dex, levelChecked.Value);
                    }
                }

                //data collection - EXP
                int? EXPChecked = null;
                int EXPAsLevel = -1;
                AlertType atEXP = AlertType.NONE;
                if (pku.EXP.HasValue) //sets EXP alert, and gets EXPChecked + EXPAsLevel
                {
                    if (pku.EXP > level100EXP) //exp overflow
                    {
                        EXPChecked = level100EXP;
                        EXPAsLevel = 100;
                        atEXP = AlertType.OVERFLOW;
                    }
                    else if (pku.EXP < 0) //exp underflow
                    {
                        EXPChecked = 0;
                        EXPAsLevel = 1;
                        atEXP = AlertType.UNDERFLOW;
                    }
                    else //exp valid
                    {
                        EXPChecked = pku.EXP;
                        EXPAsLevel = PokeAPIUtil.GetLevelFromEXP(dex, EXPChecked.Value);
                    }
                }

                //make Level-EXP alerts
                if (EXPChecked.HasValue && levelChecked.HasValue) //both specified
                {
                    if (atLevel == AlertType.OVERFLOW && atEXP == AlertType.OVERFLOW)
                    {
                        exp = (uint)level100EXP;
                        alert = GetLevelExpAlert(AlertType.OVERFLOW);
                    }
                    else if (atLevel == AlertType.UNDERFLOW && atEXP == AlertType.UNDERFLOW)
                    {
                        exp = 0;
                        alert = GetLevelExpAlert(AlertType.UNDERFLOW);
                    }
                    else if (EXPAsLevel != levelChecked || atLevel != AlertType.NONE || atEXP != AlertType.NONE) //mismatch
                    {
                        exp = (uint)EXPChecked.Value;
                        expFromLevel = (uint)levelAsEXP;
                        alert = GetLevelExpAlert(AlertType.MISMATCH, ((AlertType, AlertType, int, int, int, int, int)?)(atLevel, atEXP, levelChecked, EXPChecked, levelAsEXP, EXPAsLevel, level100EXP));
                    }
                    else //no mismatch, no alert
                        exp = (uint)EXPChecked;
                }
                else if (levelChecked.HasValue) //only level specified
                {
                    exp = (uint)levelAsEXP;
                    if (atLevel != AlertType.NONE)
                        alert = GetLevelAlert(atLevel);
                }
                else if (EXPChecked.HasValue) //only exp specified
                {
                    exp = (uint)EXPChecked;
                    if (atEXP != AlertType.NONE)
                        alert = GetEXPAlert(atEXP, level100EXP);
                }
                else //neither specified
                {
                    exp = 0;
                    alert = GetLevelExpAlert(AlertType.UNSPECIFIED);
                }

                if (expFromLevel.HasValue)
                    return (new[] { exp, expFromLevel.Value }, alert);
                else
                    return (new[] { exp }, alert);
            }

            public static (int strain, int days, Alert) ProcessPokerus(pkuObject pku)
            {
                int strain = 0, days = 0;
                AlertType atStrain = AlertType.NONE, atDays = AlertType.NONE;

                if ((pku.Pokerus?.Strain).HasValue)
                {
                    if (pku.Pokerus.Strain > 15) //overflow
                    {
                        strain = 15;
                        atStrain = AlertType.OVERFLOW;
                    }
                    else if (pku.Pokerus.Strain < 0) //underflow
                    {
                        strain = 0;
                        atStrain = AlertType.UNDERFLOW;
                    }
                    else
                        strain = pku.Pokerus.Strain.Value;
                }

                if ((pku.Pokerus?.Days).HasValue)
                {
                    if (pku.Pokerus.Days > 4) //overflow
                    {
                        days = 4;
                        atDays = AlertType.OVERFLOW;
                    }
                    else if (pku.Pokerus.Days < 0) //underflow
                    {
                        days = 0;
                        atDays = AlertType.UNDERFLOW;
                    }
                    else
                        days = pku.Pokerus.Days.Value;
                }

                return (strain, days, GetPokerusAlert(atStrain, atDays));
            }

            public static (HashSet<Ribbon>, Alert) ProcessRibbons(pkuObject pku, Func<Ribbon, bool> isValidRibbon)
            {
                bool anyInvalid = false;
                HashSet<Ribbon> ribbons = pku.Ribbons.ToEnumSet<Ribbon>();
                if(pku.Ribbons is not null)
                    anyInvalid = pku.Ribbons.Distinct().Count() > ribbons.Count; //some ribbons not official

                int oldCount = ribbons.Count;
                ribbons.RemoveWhere(x => !isValidRibbon(x)); //removes invalid ribbons from set
                anyInvalid = oldCount > ribbons.Count || anyInvalid;
                
                return (ribbons, anyInvalid ? GetRibbonAlert(AlertType.INVALID) : null);
            }

            //Gen 4: implement this for gen4+, slot/ability independent in all gens, EXCEPT for gen 3
            //public static (int abilityID, int slot, Alert) ProcessAbility(PKUObject pku, int maxAbility)
            //{
            //    
            //}
        }

    }
}
