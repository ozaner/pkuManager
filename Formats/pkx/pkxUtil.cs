using pkuManager.Alerts;
using pkuManager.Formats.Modules;
using pkuManager.Formats.pku;
using pkuManager.Formats.pkx.pk3;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.pkx.pkxUtil.ExportAlerts;

namespace pkuManager.Formats.pkx;

/// <summary>
/// A utility class with common code for exporting and importing pku files to pkx formats.
/// </summary>
public static class pkxUtil
{
    /* ------------------------------------
     * Default Values
     * ------------------------------------
    */
    /// <summary>
    /// The default gender for Pokémon and trainers used in pkx formats.
    /// </summary>
    public const Gender DEFAULT_GENDER = Gender.Male;

    /// <summary>
    /// Maps a <see cref="Language"/> to its translation of "Egg".
    /// </summary>
    public static readonly Dictionary<Language, string> EGG_NICKNAME = new()
    {
        { Language.Japanese, "タマゴ" },
        { Language.English, "Egg" },
        { Language.French, "Œuf" },
        { Language.Italian, "Uovo" },
        { Language.German, "Ei" },
        { Language.Spanish, "Huevo" },
        { Language.Korean, "알" },
        { Language.Chinese_Simplified, "蛋" },
        { Language.Chinese_Traditional, "蛋" },
    };

    /// <summary>
    /// A list of the 6 official Pokémon stats in canonical order.
    /// </summary>
    public static readonly string[] STAT_NAMES = new[] 
        { "HP", "Attack", "Defense", "Sp. Attack", "Sp. Defense", "Speed" };

    /// <summary>
    /// A list of the 6 contest stats in canonical order.
    /// </summary>
    public static readonly string[] CONTEST_STAT_NAMES = new[]
        { "Cool", "Beautiful", "Cute", "Clever", "Tough", "Sheen" };


    /* ------------------------------------
     * Utility Methods
     * ------------------------------------
    */
    /// <summary>
    /// Gets the National dex number of the given <paramref name="species"/>.
    /// </summary>
    /// <param name="species">A Pokémon species name, with any capitalization.</param>
    /// <returns>The national dex # of the given <paramref name="species"/>,
    ///          or <see langword="null"/> if it doesn't have one.</returns>
    public static int? GetNationalDex(string species)
        => SPECIES_DEX.ReadDataDex<int?>(species, "Indices", "main-series");

    /// <summary>
    /// Does the same as <see cref="GetNationalDex(string)"/> but with the<br/>
    /// assumption that, <paramref name="species"/> is an official species.
    /// </summary>
    /// <param name="species">An official Pokémon species, will throw an exception otherwise.</param>
    /// <returns>The national dex # of the given <paramref name="species"/></returns>
    public static int GetNationalDexChecked(string species)
    {
        int? dex = GetNationalDex(species);
        return dex is null ? throw new ArgumentException
            ("Must be an official Pokémon species.", nameof(species)) : dex.Value;
    }

    /// <summary>
    /// Gets the English name of the species with the given <paramref name="dex"/> #.
    /// </summary>
    /// <param name="dex">The national dex # of the desired species.</param>
    /// <returns>The English name of the species with the <paramref name="dex"/> #.
    ///          Null if there is no match.</returns>
    public static string GetSpeciesName(int dex)
        => SPECIES_DEX.SearchDataDex<int?>(dex, "$x", "Indices", "main-series");

    /// <summary>
    /// Gets the gender ratio of the given official <paramref name="species"/>.
    /// </summary>
    /// <param name="species">An official species. Will throw an exception otherwise.</param>
    /// <returns>The gender ratio of <paramref name="species"/>.</returns>
    public static GenderRatio GetGenderRatio(DexUtil.SFA sfa)
        => SPECIES_DEX.ReadSpeciesDex<string>(sfa, "Gender Ratio").ToEnum<GenderRatio>().Value;

    /// <summary>
    /// Returns the gender of a Pokémon with the given <paramref name="pid"/> as determined by Gens 3-5. 
    /// </summary>
    /// <param name="gr">The gender ratio of the Pokémon.</param>
    /// <param name="pid">The PID of the Pokémon.</param>
    /// <returns>The gender of a Pokémon with the given gender ratio
    ///          and <paramref name="pid"/> in Gens 3-5.</returns>
    public static Gender GetPIDGender(GenderRatio gr, uint pid) => gr switch
    {
        GenderRatio.All_Female => Gender.Female,
        GenderRatio.All_Male => Gender.Male,
        GenderRatio.All_Genderless => Gender.Genderless,
        GenderRatio x when (int)x > pid % 256 => Gender.Female,
        _ => Gender.Male
    };

    /// <summary>
    /// Generates a PID that satisfies the given constraints in all generations.<br/>
    /// If a constraint is null, it will be ignored.
    /// </summary>
    /// <param name="shiny">The desired shinyness.</param>
    /// <param name="tid">The Pokémon's TID.</param>
    /// <param name="gender">The desired gender.</param>
    /// <param name="gr">The species' gender ratio.</param>
    /// <param name="nature">The desired nature.</param>
    /// <param name="unownForm">The desired form, if the species is Unown.</param>
    /// <returns>A random PID satisfying all the given constraints.</returns>
    public static uint GenerateRandomPID(bool shiny, uint tid, Gender? gender = null,
        GenderRatio? gr = null, Nature ? nature = null, int? unownForm = null)
    {
        //Notice no option for ability slot.
        //Getting legality is the user's problem. Preserving legality is pku's problem.
        while (true)
        {
            uint pid = DataUtil.GetRandomUInt(); //Generate new PID candidate
                
            // Unown Form Check
            if (unownForm is not null)
            {
                if (unownForm != pk3Object.GetUnownFormID(pid))
                    continue;
            }

            // Gender Check
            if (gender is not null && gr is null)
                throw new ArgumentException($"If {nameof(gender)} is specified, a gender ratio must also be specified.", nameof(gr));
            else if (gender is not null) //gr is not null
            {
                if (gr is not (GenderRatio.All_Male or GenderRatio.All_Female or GenderRatio.All_Genderless))
                {
                    Gender pidGender = GetPIDGender(gr.Value, pid);

                    //Male but pid is Female
                    if ((gender, pidGender) is (Gender.Male, not Gender.Male))
                        continue;

                    //Female but pid is Male
                    if ((gender, pidGender) is (Gender.Female, not Gender.Female))
                        continue;
                }
            }

            // Nature Check
            if (nature is not null)
            {
                if ((int)nature != pid % 25)
                    continue;
            }

            // Shiny Check
            if ((pid / 65536 ^ pid % 65536 ^ tid / 65536 ^ tid % 65536) < 8 != shiny) //In gen 6+ that 8->16.
                continue;                                                             //No harm keeping it 8 for backwards compat.

            return pid; // everything checks out
        }
    }

    public static bool IsPIDShiny(uint pid, uint tid, bool gen6Plus)
        => (tid / 65536 ^ tid % 65536 ^ pid / 65536 ^ pid % 65536) < (gen6Plus ? 16 : 8);

    /// <summary>
    /// Calculates the PP of the given <paramref name="move"/> with the given number of
    /// <paramref name="ppups"/>, under the given <paramref name="format"/>.
    /// </summary>
    /// <param name="move">The name of move.</param>
    /// <param name="ppups">The number of PP Ups the move has.</param>
    /// <param name="format">The format the move is being considered under.</param>
    /// <returns>The PP <paramref name="move"/> would have with <paramref name="ppups"/> PP Ups.</returns>
    public static int CalculatePP(string move, int ppups, string format)
        => (5 + ppups) * MOVE_DEX.GetIndexedValue<int?>(format, move, "Base PP").Value / 5;


    /* ------------------------------------
     * Alert Generator Methods
     * ------------------------------------
    */        
    /// <summary>
    /// Alert generating methods for <see cref="ExportTags"/> methods.
    /// </summary>
    public static class ExportAlerts
    {
        // ----------
        // Generalized Alert Methods
        // ----------
        public static Alert GetNumericalAlert(string name, AlertType at, long defaultVal) => at switch
        {
            AlertType.OVERFLOW => new(name, $"This pku's {name} is higher than the maximum. Rounding down to {defaultVal}."),
            AlertType.UNDERFLOW => new(name, $"This pku's {name} is lower than the minimum. Rounding up to {defaultVal}."),
            AlertType.UNSPECIFIED => new(name, $"{name} tag not specified, using the default of {defaultVal}."),
            _ => throw InvalidAlertType(at)
        };


        // ----------
        // Pokemon Attribute Alert Methods
        // ----------
        public static Alert GetPIDAlert(AlertType at, List<(string, object, object)> tags = null)
        {
            // PID-Mismatch Alert
            if (at is AlertType.MISMATCH)
            {
                if (tags?.Count is not > 0)
                    throw new ArgumentException($"If {nameof(at)} is MISMATCH, {nameof(tags)} must be non-empty.", nameof(tags));
                string choice1msg = "";
                string choice2msg = "";
                foreach ((string name, object a, object b) in tags)
                {
                    choice1msg += $"{name}: {a}{DataUtil.Newline()}";
                    choice2msg += $"{name}: {b}{DataUtil.Newline()}";
                }
                choice1msg = choice1msg[0..^1];
                choice2msg = choice2msg[0..^1];
                RadioButtonAlert.RBAChoice[] choices =
                {
                    new("Use original PID", choice1msg),
                    new("Generate new PID", choice2msg)
                };

                return new RadioButtonAlert("PID-Mismatch", "This pku's PID is incompatible with some of its other " +
                    "tags (in this format). Choose whether to keep the PID or generate a compatible one.", choices);
            }

            // PID Alert
            return new("PID", at switch
            {
                AlertType.UNSPECIFIED => "PID not specified",
                AlertType.OVERFLOW => "This pku's PID is higher than the maximum",
                AlertType.UNDERFLOW => "This pku's PID is lower than the minimum",
                _ => throw InvalidAlertType(at)
            } + ", generating one that matches this pku's other tags.");
        }

        public static Alert GetLevelAlert(AlertType at) => GetNumericalAlert("Level", at, at switch
        {
            AlertType.UNSPECIFIED or AlertType.UNDERFLOW => 1,
            AlertType.OVERFLOW => 100,
            _ => throw InvalidAlertType(at)
        });

        public static Alert GetEXPAlert(AlertType at, int? level100exp = null)
        {
            if (at is AlertType.OVERFLOW && level100exp is null)
                throw new ArgumentNullException(nameof(level100exp), "The level 100 exp must be given for OVERFLOW alerts.");
            return new("EXP", at switch
            {
                AlertType.UNSPECIFIED => "EXP tag not specified, using the default of 0, i.e. level 1.",
                AlertType.OVERFLOW => $"This pku's EXP is higher than the maximum. Rounding down to {level100exp}, i.e. level 100.",
                AlertType.UNDERFLOW => "This pku's EXP is lower than the minimum. Rounding up to 0, i.e. level 1.",
                _ => throw InvalidAlertType(at)
            });
        }

        public static Alert GetLevelExpAlert(AlertType at, (AlertType atLevel, AlertType atEXP, 
            int level, int exp, int levelToExp, int expToLevel, int level100Exp)? mismatchData = null)
        {
            //mismatch alert
            if (at is AlertType.MISMATCH)
            {
                //levelData and expData must be given for MISMATCH alert
                if (mismatchData is null)
                    throw new ArgumentNullException(nameof(mismatchData), $"{nameof(mismatchData)} must be given for MISMATCH alerts.");

                RadioButtonAlert.RBAChoice[] choices = new RadioButtonAlert.RBAChoice[2];

                // Deal with phrasing the level option
                choices[0] = new("Use Level Tag", mismatchData.Value.atLevel switch
                {
                    AlertType.OVERFLOW => $"Set to level 100, i.e. {mismatchData.Value.level100Exp} experience (rounded down because level tag was too high).",
                    AlertType.UNDERFLOW => "Set to level 1, i.e. 0 experience (rounded up because level tag was too low).",
                    AlertType.NONE => $"Set to level {mismatchData.Value.level}, i.e. {mismatchData.Value.levelToExp} exp.",
                    _ => throw new ArgumentException("No valid AlertTypes were given to the LEVEL part of the MISMATCH alert.", nameof(mismatchData.Value.atLevel))
                });

                // Deal with phrasing the exp option
                choices[1] = new("Use Experience Tag", mismatchData.Value.atEXP switch
                {
                    AlertType.OVERFLOW => $"Set experience to {mismatchData.Value.level100Exp} experience, i.e. level 100 (rounded down because experience tag was too high).",
                    AlertType.UNDERFLOW => "Set experience to 0, i.e. level 1 (rounded up because level tag was too low).",
                    AlertType.NONE => $"Set experience to {mismatchData.Value.exp}, i.e. level {mismatchData.Value.expToLevel}.",
                    _ => throw new ArgumentException("No valid AlertTypes were given to the EXP part the MISMATCH alert.", nameof(mismatchData.Value.atEXP))
                });

                //put 2 options together
                return new RadioButtonAlert("Level/Experience", "The given level and experience don't match, choose which one to use.", choices);
            }

            //other alerts
            return new("Level/Experience", at switch
            {
                AlertType.UNSPECIFIED => "Neither the level nor experience was specified. Defaulting to level 1.",
                AlertType.OVERFLOW => "Both the Level and EXP tags are too high, rounding down to level 100.",
                AlertType.UNDERFLOW => "Both the Level and EXP tags are too low, rounding up to level 1.",
                _ => throw InvalidAlertType(at)
            });
        }

        public static Alert GetPPUpAlert(AlertType at, (int[] overflow, int[] underflow)? invalidData = null)
        {
            static string helper(int[] xFlow)
            {
                string msgXF = "The PP-Ups of "+ (xFlow.Length is 1 ? "move " : "moves "); //plural check
                foreach (int i in xFlow) //add moves
                    msgXF += i + 1 + ","; //starts at move 1 not move 0
                return msgXF;
            }
            string msg;
            switch (at)
            {
                case AlertType.UNSPECIFIED: //shouldn't be called, should be silent
                    msg = "PP Ups tag not specified, giving each move 0 PP ups.";
                    break;
                case AlertType.INVALID:
                    if (invalidData is null || invalidData.Value.overflow.Length < 1 && invalidData.Value.underflow.Length < 1)
                        throw new ArgumentException($"At least one of {nameof(invalidData)} must be non-empty for INVALID alerts.", nameof(invalidData));
                    string msgOF = null, msgUF = null;
                    if (invalidData.Value.overflow.Length > 0)
                        msgOF = helper(invalidData.Value.overflow)[0..^1] + " are too high, rounding them down to 3.";
                    if (invalidData.Value.underflow.Length > 0)
                        msgUF = helper(invalidData.Value.underflow)[0..^1] + " are too low, rounding them up to 0.";
                    msg = string.Join(DataUtil.Newline(2), msgOF, msgUF);
                    break;
                default:
                    throw InvalidAlertType(at);
            }
            return new("PP Ups", msg);
        }

        public static Alert GetGenderAlert(AlertType at, Gender? correctGender = null, string invalidGender = null)
        {
            if (at is AlertType.MISMATCH or AlertType.INVALID && (correctGender is null || invalidGender is null))
                throw new ArgumentNullException($"{nameof(correctGender)}, {nameof(invalidGender)}",
                    "The correct and invalid gender must be given for INVALID & MSIMATCH alerts.");
            return new("Gender", at switch
            {
                AlertType.UNSPECIFIED => "This species can be either male or female, yet no gender was specified." +
                                            $" Setting to {DEFAULT_GENDER.ToFormattedString()}.",
                AlertType.MISMATCH => $"This species cannot be {invalidGender}. Setting gender to {correctGender}.",
                AlertType.INVALID => $"\"{invalidGender}\" is not a valid gender. Setting gender to {correctGender}.",
                _ => throw InvalidAlertType(at)
            });
        }

        public static Alert GetFormAlert(AlertType at, string[] invalidForm = null)
        {
            if (at is AlertType.CASTED or AlertType.IN_BATTLE && invalidForm is null)
                throw new ArgumentException($"{nameof(invalidForm)} must be given for CASTED and IN_BATTLE alerts.", nameof(invalidForm));
            return new("Form", at switch
            {
                AlertType.UNSPECIFIED => "No form specified, using the default form.",
                AlertType.CASTED => $"The form \"{invalidForm.ToFormattedString()}\" does not exist in this format and has been casted to its default form.",
                AlertType.IN_BATTLE => $"The form \"{invalidForm.ToFormattedString()}\" only exists in-battle, using its out of battle form.",
                _ => throw InvalidAlertType(at)
            });
        }

        public static Alert GetPokerusAlert(AlertType atStrain, AlertType atDays)
        {
            if ((atStrain, atDays) is (AlertType.NONE, AlertType.NONE))
                return null;
            Alert helper(string name, AlertType at, int maxVal) => at switch
            {
                AlertType.OVERFLOW => GetNumericalAlert($"Pokérus {name}", at, maxVal),
                AlertType.UNDERFLOW => GetNumericalAlert($"Pokérus {name}", at, 0),
                AlertType.NONE => null,
                _ => throw InvalidAlertType(atStrain)
            };
            return new("Pokérus", string.Join(DataUtil.Newline(2), helper("strain", atStrain, 15)?.Message, helper("days", atDays, 4)?.Message));
        }

        public static Alert GetAbilityAlert(AlertType at, string invalidAbility = null, string defaultAbility = "None")
        {
            if (at is AlertType.MISMATCH or AlertType.INVALID && invalidAbility is null)
                throw new ArgumentNullException(nameof(invalidAbility), "Must give the invalid ability for MISMATCH and INVALID alerts.");
            return new("Ability", at switch
            {
                AlertType.UNSPECIFIED => $"No ability was specified, using the default ability: {defaultAbility}.",
                AlertType.MISMATCH => $"This species cannot have the ability {invalidAbility} in this format. Using the default ability: {defaultAbility}.",
                AlertType.INVALID => $"The ability {invalidAbility} is not supported by this format, using the default ability: {defaultAbility}.",
                _ => throw InvalidAlertType(at)
            });
        }

        public static Alert GetRibbonAlert()
            => new("Ribbons", "Some of the pku's ribbons are not valid in this format. Ignoring them.");

        public static Alert GetTrashAlert(AlertType atNickname, AlertType atOT, int nickEntries, int OTEntries, int? maxVal = null)
        {
            if (atNickname is not AlertType.OVERFLOW or AlertType.MISMATCH ||
                atOT is not AlertType.OVERFLOW or AlertType.MISMATCH)
                throw InvalidAlertType();

            if ((atNickname is AlertType.OVERFLOW || atOT is AlertType.OVERFLOW) && maxVal is null)
                throw new ArgumentNullException(nameof(maxVal), "Must give maximum value if there is an OVERFLOW alert.");

            string helper(string name, AlertType at, int entries) => at switch
            {
                AlertType.OVERFLOW => $"One or more of the entries in {name} Trash Bytes is too high, the maximum value is ${maxVal}. Ignoring them.",
                AlertType.MISMATCH => $"The number of entries in the {name} Trash Bytes must be exactly {entries}. Ignoring them.",
                _ => null
            };

            return new("Trash Bytes", string.Join(DataUtil.Newline(2), 
                helper("Nickname", atNickname, nickEntries), helper("OT", atOT, OTEntries)));
        }
    }


    /* ------------------------------------
     * Tag Processing Methods
     * ------------------------------------
    */
    /// <summary>
    /// Generalized methods for processing attributes of pkx files.
    /// </summary>
    public static class ExportTags
    {
        // ----------
        // Generalized Processing Methods
        // ----------
        public static (int, Alert) ProcessNumericTag(int? tag, Func<AlertType, Alert> getAlertFunc,
            bool silentUnspecified, int max, int min, int defaultVal) => tag switch
        {
            null => (defaultVal, silentUnspecified ? null : getAlertFunc(AlertType.UNSPECIFIED)),
            var x when x > max => (max, getAlertFunc(AlertType.OVERFLOW)),
            var x when x < min => (min, getAlertFunc(AlertType.UNDERFLOW)),
            _ => (tag.Value, null)
        };


        // ----------
        // String Processing Methods
        // ----------
        public static (BigInteger[] trashedName, BigInteger[] trashedOT, Alert)
            ProcessTrash(BigInteger[] encodedName, BigInteger[] nameTrash, BigInteger[] encodedOT, BigInteger[] otTrash, int byteLength, string format, Language? checkedLang = null)
        {
            ushort max = byteLength switch
            {
                1 => byte.MaxValue,
                2 => ushort.MaxValue,
                _ => throw new ArgumentException("only 1 and 2 byte encodings are supported.", nameof(byteLength))
            };
            (BigInteger[], AlertType) helper(BigInteger[] encodedStr, BigInteger[] trash)
            {
                AlertType at = AlertType.NONE;
                if (trash is null)
                    return (encodedStr, at);
                else if (trash.Any(x => x > max))
                    at = AlertType.OVERFLOW;
                else if (trash.Length != encodedStr.Length)
                    at = AlertType.MISMATCH;

                if (at is not AlertType.NONE)
                    return (encodedStr, at);

                //T is definitely byte, ushort, or byte at this point.
                return (DexUtil.CharEncoding.Trash(encodedStr, trash, format, checkedLang), at);
            }

            (BigInteger[] trashedName, AlertType atName) = helper(encodedName, nameTrash);
            (BigInteger[] trashedOT, AlertType atOT) = helper(encodedOT, otTrash);
            Alert alert = (atName, atOT) is (AlertType.NONE, AlertType.NONE) ? null :
                GetTrashAlert(atName, atOT, encodedName.Length, encodedOT.Length, max);
            return (trashedName, trashedOT, alert);
        }


        // ----------
        // Pokemon Attribute Processing Methods
        // ----------
        //TODO Gen 6: account for gen6+ pid change on shiny mismatch
        public static (uint[], Alert) ProcessPID(pkuObject pku, uint checkedTID, bool gen6Plus,
            Gender? checkedGender = null, Nature? checkedNature = null, int? checkedUnownForm = null)
        {
            // Deal with null PID. (Always in-bounds since it is a uint)
            (uint pid, bool pidInBounds, Alert alert) = pku.PID is null ?
                (uint.MinValue, false, GetPIDAlert(AlertType.UNSPECIFIED)) : (pku.PID.Value, true, null);

            // Check if any value has a pid-mismatch
            bool genderMismatch = false, natureMismatch = false, unownMismatch = false, shinyMismatch;
            int oldunownform = 0;
            Nature oldnature = Nature.Hardy;
            Gender oldgender = DEFAULT_GENDER;

            if (checkedGender is not null) //gender mismatch check
            {
                oldgender = GetPIDGender(GetGenderRatio(pku), pid);
                genderMismatch = checkedGender != oldgender;
            }
            if (checkedNature is not null) //nature mismatch check
            {
                oldnature = (Nature)(pid % 25);
                natureMismatch = checkedNature != oldnature;
            }
            if (checkedUnownForm is not null) //unown form mismatch check
            {
                oldunownform = pk3Object.GetUnownFormID(pid);
                unownMismatch = checkedUnownForm is not null && checkedUnownForm != oldunownform;
            }
            //always check shiny
            bool oldshiny = IsPIDShiny(pid, checkedTID, gen6Plus);
            shinyMismatch = pku.IsShiny() != oldshiny;

            // Deal with pid-mismatches
            if (unownMismatch || genderMismatch || natureMismatch || shinyMismatch)
            {
                uint newPID = GenerateRandomPID(pku.IsShiny(), checkedTID, checkedGender,
                    GetGenderRatio(pku), checkedNature, checkedUnownForm);

                if (pidInBounds) //two options: old & new, need error
                {
                    List<(string, object, object)> tags = new();
                    if (unownMismatch)
                        tags.Add(("Unown Form", pk3Object.GetUnownFormName(oldunownform),
                            pk3Object.GetUnownFormName(checkedUnownForm.Value)));
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

        //Gen 6: allow impossible genders in gen 6+ (I think they allow impossible genders...)
        public static (Gender, Alert) ProcessGender(pkuObject pku)
        {
            GenderRatio genderRatio = GetGenderRatio(pku);
            Gender? mandatoryGender = genderRatio switch
            {
                GenderRatio.All_Genderless => Gender.Genderless,
                GenderRatio.All_Female => Gender.Female,
                GenderRatio.All_Male => Gender.Male,
                _ => null
            };
            Gender? readGender = pku.Gender.ToEnum<Gender>();

            return pku.Gender switch
            {
                null => mandatoryGender is null ? (DEFAULT_GENDER, GetGenderAlert(AlertType.UNSPECIFIED)) : (mandatoryGender.Value, null),
                _ => readGender switch //gender invalid, make it mandatory/male
                {
                    null => (mandatoryGender ?? DEFAULT_GENDER, GetGenderAlert(AlertType.INVALID)), //invalid gender
                    _ => mandatoryGender switch
                    {
                        var x when x is null || x == readGender => (readGender.Value, null), //matches mandatory gender/doesn't have one 
                        _ => (mandatoryGender.Value, GetGenderAlert(AlertType.MISMATCH, mandatoryGender, readGender.ToFormattedString()))
                    }
                }
            };
        }

        public static (int[], Alert) ProcessPPUps(pkuObject pku, int[] moveIndicies)
        {
            //Calculate ppups
            int[] ppups = new int[4];
            List<int> overflow = new();
            List<int> underflow = new();
            if (moveIndicies?.Length > 4)
                throw new ArgumentException($"{nameof(moveIndicies)} should be of length 0-4.", nameof(moveIndicies));
            for (int i = 0; i < moveIndicies.Length; i++)
            {
                if (pku?.Moves?[moveIndicies[i]]?.PP_Ups is not null) //this move exists and has a ppup value
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

            //Generate alert
            Alert alert = overflow.Count > 0 || underflow.Count > 0 ? 
                GetPPUpAlert(AlertType.INVALID, (overflow.ToArray(), underflow.ToArray())) : null;
            return (ppups, alert);
        }

        public static (uint[], Alert) ProcessEXP(pkuObject pku)
        {
            int dex = GetNationalDexChecked(pku.Species); //must be valid at this point
            int level100EXP = PokeAPIUtil.GetEXPFromLevel(dex, 100);

            //data collection - Level
            (int? levelChecked, int levelAsEXP, AlertType atLevel) = pku.Level switch
            {
                null => (null, -1, AlertType.NONE), //no level specified
                var x when x > 100 => (100, level100EXP, AlertType.OVERFLOW), //level overflow
                var x when x < 1 => (1, 0, AlertType.UNDERFLOW), //level underflow
                _ => (pku.Level, PokeAPIUtil.GetEXPFromLevel(dex, pku.Level.Value), AlertType.NONE) //valid level
            };

            //data collection - EXP
            (int? EXPChecked, int EXPAsLevel, AlertType atEXP) = pku.EXP switch
            {
                null => (null, -1, AlertType.NONE), //no exp specified
                var x when x > level100EXP => (level100EXP, 100, AlertType.OVERFLOW), //exp overflow
                var x when x < 0 => (0, 1, AlertType.UNDERFLOW), //exp underflow
                _ => (pku.EXP, PokeAPIUtil.GetLevelFromEXP(dex, pku.EXP.Value), AlertType.NONE) //valid exp
            };

            //make Level-EXP alerts
            (uint exp, uint? expFromLevel, Alert alert) = (EXPChecked, levelChecked) switch
            {
                (null, null) => (0, null, GetLevelExpAlert(AlertType.UNSPECIFIED)),
                (null, not null) => ((uint)levelAsEXP, null, atLevel is AlertType.NONE ? null : GetLevelAlert(atLevel)),
                (not null, null) => ((uint)EXPChecked, null, atEXP is AlertType.NONE ? null : GetEXPAlert(atEXP, level100EXP)),
                _ => (atEXP, atLevel) switch
                {
                    (AlertType.OVERFLOW, AlertType.OVERFLOW) => ((uint)level100EXP, null, GetLevelExpAlert(AlertType.OVERFLOW)),
                    (AlertType.UNDERFLOW, AlertType.UNDERFLOW) => (0, null, GetLevelExpAlert(AlertType.UNDERFLOW)),
                    var (x, y) when x is not AlertType.NONE || y is not AlertType.NONE || EXPAsLevel != levelChecked
                        => ((uint)EXPChecked, (uint?)levelAsEXP, GetLevelExpAlert(AlertType.MISMATCH,
                            (atLevel, atEXP, (int)levelChecked, (int)EXPChecked, levelAsEXP, EXPAsLevel, level100EXP))),
                    _ => ((uint)EXPChecked, null, null)
                }
            };

            return (expFromLevel is null ? new[]{ exp } : new[]{ exp, expFromLevel.Value }, alert);
        }

        public static (int strain, int days, Alert) ProcessPokerus(pkuObject pku)
        {
            static (int, AlertType) helper(int? val, int max) => val switch
            {
                null => (0, AlertType.NONE),
                var x when x > max => (max, AlertType.OVERFLOW),
                var x when x < 0 => (0, AlertType.UNDERFLOW),
                _ => (val.Value, AlertType.NONE)
            };
            (int strain, AlertType atStrain) = helper(pku.Pokerus?.Strain, 15);
            (int days, AlertType atDays) = helper(pku.Pokerus?.Days, 4);
            return (strain, days, GetPokerusAlert(atStrain, atDays));
        }

        public static (HashSet<Ribbon>, Alert) ProcessRibbons(pkuObject pku, Func<Ribbon, bool> isValidRibbon)
        {
            bool anyInvalid = false;
            HashSet<Ribbon> ribbons = pku.Ribbons.ToEnumSet<Ribbon>();
            if(pku.Ribbons is not null)
                anyInvalid = pku.Ribbons.Distinct(StringComparer.InvariantCultureIgnoreCase).Count() > ribbons.Count;

            int oldCount = ribbons.Count;
            ribbons.RemoveWhere(x => !isValidRibbon(x)); //removes invalid ribbons from set
            anyInvalid = oldCount > ribbons.Count || anyInvalid;
                
            return (ribbons, anyInvalid ? GetRibbonAlert() : null);
        }

        //TODO Gen 4: implement this for gen4+, slot/ability independent in all gens, EXCEPT for gen 3
        //public static (int abilityID, int slot, Alert) ProcessAbility(PKUObject pku, int maxAbility)
    }
}