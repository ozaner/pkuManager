using Newtonsoft.Json.Linq;
using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AlertType = pkuManager.pkx.pkxUtil.TagAlerts.AlertType;

namespace pkuManager.pkx.pk3
{
    public static class pk3Util
    {
        // ----------
        // Misc.
        // ----------
        // Used in parsing the substructure of .pk3 files
        public static readonly string[] SUBSTRUCTURE_ORDER =
        {
            "GAEM", "GAME", "GEAM", "GEMA", "GMAE", "GMEA",
            "AGEM", "AGME", "AEGM", "AEMG", "AMGE", "AMEG",
            "EGAM", "EGMA", "EAGM", "EAMG", "EMGA", "EMAG",
            "MGAE", "MGEA", "MAGE", "MAEG", "MEGA", "MEAG",
        };

        // doesn't work, use random method in pkxUtil instead
        private static uint generatePID(int? unownForm, GenderRatio gr, Gender gender, Nature nature, bool shiny)
        {
            if (unownForm != null && gender != Gender.Genderless)
                throw new ArgumentException("unownForm is not null yet gender is not genderless. All unown are genderless.");

            // 0 = Unset, U = Unown, G = Gender, N = Nature, S = Shiny

            //P0: initialize to 0 (00000000 00000000 00000000 00000000)
            uint newPID = 0;

            //P1a: Unown (00000000 0000000U 000000UU 000000UU)
            if (unownForm != null)
            {
                newPID = DataUtil.setBits(newPID, DataUtil.getBits((uint)unownForm.Value, 0, 2), 0, 2); //bits 0,1 of form
                newPID = DataUtil.setBits(newPID, DataUtil.getBits((uint)unownForm.Value, 2, 2), 0, 2); //bits 2,3 of form
                newPID = DataUtil.setBits(newPID, DataUtil.getBits((uint)unownForm.Value, 4), 0); //bit 4 of form
                //bits past this wont get set by a number from 0-27;
            }

            //P1b: Gender (00000000 0000000U 000000UU GGGGGGGG)
            if (gr != GenderRatio.ALL_MALE &&
                gr != GenderRatio.ALL_FEMALE &&
                gr != GenderRatio.ALL_GENDERLESS)
            {

            }

            //P2: Nature (00000000 0000000U NNNNNNUU GGGGGG[G/U][G/U])
            //mod equation for 2^11

            //P3: Shiny (SSSSSSSS SSSSSSSU NNNNNNUU GGGGGG[G/U][G/U])
            //if not shiny, make first S =1 in xor calc, if shiny mak all S = 0 in xor calc.

            return newPID;
        }


        // ----------
        // Ribbon Encoding Stuff
        // ----------
        // Which bit in the ribbon bytes correspond to which ribbon. Does not include contest ribbons.
        public static readonly Dictionary<int, Ribbon> RIBBON_INDEX = new Dictionary<int, Ribbon>(){
            {15, Ribbon.Champion},
            {16, Ribbon.Winning},
            {17, Ribbon.Victory},
            {18, Ribbon.Artist},
            {19, Ribbon.Effort},
            {20, Ribbon.Battle_Champion},
            {21, Ribbon.Regional_Champion},
            {22, Ribbon.National_Champion},
            {23, Ribbon.Country},
            {24, Ribbon.National},
            {25, Ribbon.Earth},
            {26, Ribbon.World}
        };

        public static bool IsValidRibbon(Ribbon ribbon)
        {
            return ribbon >= Ribbon.Cool_G3 && ribbon <= Ribbon.Tough_Master_G3 || RIBBON_INDEX.ContainsValue(ribbon);
        }


        // ----------
        // Language Encoding Stuff
        // ----------
        public static readonly Dictionary<uint, Language> LANGUAGE_ENCODING = new Dictionary<uint, Language>
        {
            { 0x0201, Language.Japanese },
            { 0x0202, Language.English },
            { 0x0203, Language.French },
            { 0x0204, Language.Italian },
            { 0x0205, Language.German },
            //{ 0x0206, Common.Language.Korean }, //While this number was reserved for Korean, never saw usage in Gen 3.
            { 0x0207, Language.Spanish }
        };
        public static readonly uint EGG_LANGUAGE_ID = 0x0601;

        public static uint EncodeLanguage(Language lang)
        {
            try
            {
                return LANGUAGE_ENCODING.First(x => x.Value == lang).Key;
            }
            catch
            {
                throw new Exception("Must be a valid gen 3 language");
            }
        }


        // ----------
        // Character Encoding Stuff
        // ----------
        private static readonly JObject PK3_CHARACTER_ENCODING_DATA = DataUtil.getJson("pk3CharEncoding");
        private static readonly Dictionary<byte?, char> INTERNATIONAL_CHARSET = PK3_CHARACTER_ENCODING_DATA["International"].ToObject<Dictionary<byte?, char>>();
        private static readonly Dictionary<byte?, char> GERMAN_CHARSET = DataUtil.getCombinedJson(new JObject[]
        {
            (JObject)PK3_CHARACTER_ENCODING_DATA["International"],
            (JObject)PK3_CHARACTER_ENCODING_DATA["German"]
        }).ToObject<Dictionary<byte?, char>>();
        private static readonly Dictionary<byte?, char> FRENCH_CHARSET = DataUtil.getCombinedJson(new JObject[]
        {
            (JObject)PK3_CHARACTER_ENCODING_DATA["International"],
            (JObject)PK3_CHARACTER_ENCODING_DATA["French"]
        }).ToObject<Dictionary<byte?, char>>();
        private static readonly Dictionary<byte?, char> JAPANESE_CHARSET = PK3_CHARACTER_ENCODING_DATA["Japanese"].ToObject<Dictionary<byte?, char>>();
        public static readonly int MAX_NICKNAME_CHARS = 10; //japan only displays 6 but they are still there
        public static readonly int MAX_OT_CHARS = 7;

        public static byte? EncodeCharacter(char c, Language lang)
        {
            return lang switch
            {
                Language.Japanese => JAPANESE_CHARSET.FirstOrDefault(x => x.Value == c).Key,
                Language.German => GERMAN_CHARSET.FirstOrDefault(x => x.Value == c).Key,
                Language.French => FRENCH_CHARSET.FirstOrDefault(x => x.Value == c).Key,
                var l when l == Language.English || l == Language.Italian || l == Language.Spanish
                        => INTERNATIONAL_CHARSET.FirstOrDefault(x => x.Value == c).Key,
                _ => null //Korean, Chinese, and any future language characters DNE in Gen 3.
            };
        }


        // ----------
        // Met Location Encoding Stuff
        // ----------
        private static readonly JObject PK3_LOCATION_DATA = DataUtil.getJson("gen3Locations");
        private static readonly Dictionary<byte?, string> RS_LOCATION_TABLE = DataUtil.getCombinedJson(new JObject[]
        {
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["RS"]
        }).ToObject<Dictionary<byte?, string>>();
        private static readonly Dictionary<byte?, string> FRLG_LOCATION_TABLE = DataUtil.getCombinedJson(new JObject[]
        {
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["FRLG"]
        }).ToObject<Dictionary<byte?, string>>();
        private static readonly Dictionary<byte?, string> E_LOCATION_TABLE = DataUtil.getCombinedJson(new JObject[]
        {
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["RS"],
            (JObject)PK3_LOCATION_DATA["E"]
        }).ToObject<Dictionary<byte?, string>>();
        private static readonly Dictionary<int?, string> COLO_LOCATION_TABLE = DataUtil.getCombinedJson(new JObject[]
        {
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["Colo"]
        }).ToObject<Dictionary<int?, string>>();
        private static readonly Dictionary<int?, string> XD_LOCATION_TABLE = DataUtil.getCombinedJson(new JObject[]
{
            (JObject)PK3_LOCATION_DATA["Base"],
            (JObject)PK3_LOCATION_DATA["XD"]
        }).ToObject<Dictionary<int?, string>>();

        public static string GetDefaultLocationName(string checkedLocation) => checkedLocation?.ToLowerInvariant() switch
        {
            "colosseum" => null,
            "xd" => XD_LOCATION_TABLE[0],
            _ => RS_LOCATION_TABLE[0],
        };


        public static byte? EncodeMetLocation(string game, string location)
        {
            if (location == null)
                return null;

            // Match explicit location IDs (i.e. "\0xXX...X\")
            if (Regex.IsMatch(location, @"^\\0x[a-fA-F0-9]+\\$"))
                return Convert.ToByte(location[1..^1], 16);

            // Game must be specified to find a location (i.e. full path of a location is "Game:Location")
            if (game == null)
                return null;

            // Get .pk3 Game Location
            switch (game.ToLowerInvariant())
            {
                case "ruby":
                case "sapphire":
                    return RS_LOCATION_TABLE.FirstOrDefault(x => x.Value.ToLowerInvariant() == location.ToLowerInvariant()).Key;
                case "firered":
                case "leafgreen":
                    return FRLG_LOCATION_TABLE.FirstOrDefault(x => x.Value.ToLowerInvariant() == location.ToLowerInvariant()).Key;
                case "emerald":
                    return E_LOCATION_TABLE.FirstOrDefault(x => x.Value.ToLowerInvariant() == location.ToLowerInvariant()).Key;
                case "colosseum":
                    return (byte?)COLO_LOCATION_TABLE.FirstOrDefault(x => x.Value.ToLowerInvariant() == location.ToLowerInvariant()).Key;
                case "xd":
                    return (byte?)XD_LOCATION_TABLE.FirstOrDefault(x => x.Value.ToLowerInvariant() == location.ToLowerInvariant()).Key;
                default:
                    return null; //invalid game
            }
        }


        // ----------
        // Form Encoding Stuff
        // ----------
        public static readonly JObject VALID_FORMS = DataUtil.getJson("gen3Forms");

        public static int GetUnownFormID(uint pid)
        {
            uint formID = 0;
            formID = DataUtil.setBits(formID, DataUtil.getBits(pid, 0, 2), 0, 2); //first two bits of byte 0
            formID = DataUtil.setBits(formID, DataUtil.getBits(pid, 8, 2), 2, 2); //first two bits of byte 1
            formID = DataUtil.setBits(formID, DataUtil.getBits(pid, 16, 2), 4, 2); //first two bits of byte 2
            formID = DataUtil.setBits(formID, DataUtil.getBits(pid, 24, 2), 6, 2); //first two bits of byte 3

            return (byte)formID % 28;
        }

        public static string GetUnownFormName(int id)
        {
            if (id == 26)
                return "!";
            else if (id == 27)
                return "?";
            else
                return ("" + (char)(97 + id)).ToUpperInvariant();
        }


        // ----------
        // Ability Encoding Stuff
        // ----------
        private static readonly JObject PK3_ABILITY_DATA = DataUtil.getJson("pk3Abilities");

        public static (bool slot1, bool slot2) IsAbilityValid(int dex, int abilityID)
        {
            if (dex < 1 || dex > 386) //must be a valid gen 3 dex
                throw new ArgumentException("pk3Util.GetAbilityID must be given a valid gen 3 dex number.");

            int slot1 = (int)PK3_ABILITY_DATA["" + dex]["1"];
            int? slot2 = (int?)PK3_ABILITY_DATA["" + dex]?["2"];

            return (slot1 == abilityID, slot2 == abilityID);
        }


        // ----------
        // Encryption Stuff
        // ----------
        public static uint CalculateChecksum(byte[] subData)
        {
            if (subData.Length != 48)
                throw new ArgumentException("Expected a 48 byte pk3 subdata array.");

            uint checksum = 0;
            for (int i = 0; i < 24; i++) // Sum over subData, w/ 2 byte window
                checksum += subData[i + 1] * (uint)256 + subData[i];
            checksum %= 65536; //should be 2 bytes

            return checksum;
        }

        public static void EncryptSubData(byte[] subData, uint id, uint pid)
        {
            uint encryptionKey = id ^ pid;
            for (int i = 0; i < 12; i++) //xor subData with key in 4 byte chunks
            {
                int index = 4 * i;
                uint chunk = (uint)(subData[index + 3] << 24 | subData[index + 2] << 16 | subData[index + 1] << 8 | subData[index]);
                chunk ^= encryptionKey;
                DataUtil.toByteArray(chunk).CopyTo(subData, index); //copy the encrypted chunk bytes to the subData array
            }
        }


        // ----------
        // Exporter Stuff
        // ----------

        /// <summary>
        /// Gen 3-specific alert generation methods. See <see cref="pkxUtil.TagAlerts"/> for more.
        /// </summary>
        public static class TagAlerts
        {
            /// <summary>
            /// Adds the gen 3 contest ribbon Alert onto an existing pkxUtil ribbon Alert.
            /// Creates a new Alert if passed alert is null.
            /// </summary>
            /// <param name="ribbonAlert">A ribbon Alert generated by pkxUtil.</param>
            /// <returns></returns>
            public static Alert AddContestRibbonAlert(Alert ribbonAlert)
            {
                string msg = "This pku has a Gen 3 contest ribbon of some category with rank super or higher, " +
                    "but doesn't have the ribbons below that rank. This is impossible in this format, adding those ribbons.";
                if (ribbonAlert != null)
                    ribbonAlert.message += $"\r\n\r\n{ msg}";
                else
                    ribbonAlert = new Alert("Ribbons", msg);
                return ribbonAlert;
            }

            public static Alert GetNatureAlert(AlertType at, string invalidNature = null)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert("Nature", "No nature specified, using the nature decided by the PID.");
                else if (at == AlertType.INVALID)
                    return new Alert("Nature", $"The nature \"{invalidNature}\" is not valid in this format. Using the nature decided by the PID.");
                else
                    return pkxUtil.TagAlerts.GetNatureAlert(at, invalidNature);
            }

            /// <summary>
            /// Changes the UNSPECIFIED & INVALID AlertTypes from the pkxUtil method to account for PID-Nature dependence.
            /// </summary>
            /// <param name="at">The type of Alert.</param>
            /// <param name="invalidGender">The invalid gender specified in the pku, if any.</param>
            /// <returns></returns>
            public static Alert GetGenderAlert(AlertType at, Gender? correctGender = null, string invalidGender = null)
            {
                if (at == AlertType.UNSPECIFIED)
                    return new Alert("Gender", "No gender specified, using the gender decided by the PID.");
                else if (at == AlertType.INVALID)
                    return new Alert("Gender", $"The gender \"{invalidGender}\" is not valid in this format. Using the gender decided by the PID.");
                else
                    return pkxUtil.TagAlerts.GetGenderAlert(at, correctGender, invalidGender);
            }

            public static RadioButtonAlert GetFatefulEncounterAlert(bool isMew)
            {
                string pkmn = isMew ? "Mew" : "Deoxys";
                string msg = $"This {pkmn} was not met in a fateful encounter. " +
                    $"Note that, in the Gen 3 games, {pkmn} will only obey the player if it was met in a fateful encounter.";

                (string, string)[] choices =
                {
                    ("Keep Fateful Encounter",$"Fateful Encounter: false\n{pkmn} won't obey."),
                    ("Set Fateful Encounter",$"Fateful Encounter: true\n{pkmn} will obey.")
                };

                return new RadioButtonAlert("Fateful Encounter", msg, choices);
            }

            public static Alert GetFormAlert(AlertType at, string[] invalidForm = null, bool isDeoxys = false)
            {
                if (isDeoxys)
                    return new Alert("Form", "Note that in generation 3, Deoxys' form depends on what game it is currently in.");
                else
                    return pkxUtil.TagAlerts.GetFormAlert(at, invalidForm);
            }
        }

        /// <summary>
        /// Gen 3-specific tag processing methods. See <see cref="pkxUtil.ProcessTags"/> for more.
        /// </summary>
        public static class ProcessTags
        {
            public static (int?, Alert) ProcessForm(pkuObject pku)
            {
                int? unownForm = null; //to return
                Alert alert = null; //to return

                int dex = pkxUtil.GetNationalDexChecked(pku.Species); //Must be valid to use this method

                if (pku.Forms != null)
                {
                    string properFormName = DexUtil.GetSearchableFormName(pku).ToLowerInvariant();
                    if (dex == 201 && pku.Forms.Length == 1 && Regex.IsMatch(properFormName, "[a-z!?]")) //unown
                    {
                        if (properFormName[0] == '?')
                            unownForm = 26;
                        else if (properFormName[0] == '!')
                            unownForm = 27;
                        else //all other letters
                            unownForm = properFormName[0] - 97;
                    }
                    else if (dex == 386 && new string[] { "normal", "attack", "defense", "speed" }.Contains(properFormName)) //deoxys
                        alert = TagAlerts.GetFormAlert(AlertType.NONE, null, true);
                    else if (dex == 351 && new string[] { "sunny", "rainy", "snowy" }.Contains(properFormName)) //castform
                        alert = TagAlerts.GetFormAlert(AlertType.IN_BATTLE, pku.Forms);
                }

                return (unownForm, alert);
            }

            public static (byte[], Alert) ProcessNickname(pkuObject pku, Language checkedLang)
            {
                return pkxUtil.ProcessTags.ProcessNickname(pku, 3, checkedLang, MAX_NICKNAME_CHARS, 1, (c) => { return EncodeCharacter(c, checkedLang); });
            }

            public static (byte[], Alert) ProcessOT(pkuObject pku, Language checkedLang)
            {
                return pkxUtil.ProcessTags.ProcessOT(pku, MAX_OT_CHARS, 1, (c) => { return EncodeCharacter(c, checkedLang); });
            }

            public static (byte[] trashedNickname, byte[] trashedOT, Alert) ProcessTrash(pkuObject pku, byte[] encodedNickname, byte[] encodedOT)
            {
                //Get Nickname trash
                byte[] nicknameTrash = null;
                if (pku.Trash_Bytes?.Gen == 3 && pku.Trash_Bytes?.Nickname?.Length > 0) //If trash bytes exist and are gen 3
                    nicknameTrash = pku.Trash_Bytes.Nickname;

                //Get OT trash
                byte[] otTrash = null;
                if (pku.Trash_Bytes?.Gen == 3 && pku.Trash_Bytes?.OT?.Length > 0) //If trash bytes exist and are gen 3
                    otTrash = pku.Trash_Bytes.OT;

                return pkxUtil.ProcessTags.ProcessTrash(encodedNickname, nicknameTrash, encodedOT, otTrash, new byte[] { 0xFF });
            }

            public static (int, Alert) ProcessAbility(pkuObject pku)
            {
                int dex = pkxUtil.GetNationalDexChecked(pku.Species);
                int defaultAbilityID = (int)PK3_ABILITY_DATA["" + dex]["1"];
                string defaultAbility = PokeAPIUtil.GetAbility(defaultAbilityID);
                if (pku.Ability != null) //ability specified
                {
                    int? abilityID = PokeAPIUtil.GetAbilityIndex(pku.Ability);
                    if (!abilityID.HasValue || abilityID.Value > 76) //unofficial ability OR gen4+ ability
                        return (0, pkxUtil.TagAlerts.GetAbilityAlert(AlertType.INVALID, pku.Ability, defaultAbility));

                    (bool slot1valid, bool slot2valid) = IsAbilityValid(dex, abilityID.Value);
                    if (slot1valid) //ability corresponds to slot 1
                        return (0, null);
                    else if (slot2valid) //ability corresponds to slot 2
                        return (1, null);
                    else //ability is impossible on this species
                        return (0, pkxUtil.TagAlerts.GetAbilityAlert(AlertType.MISMATCH, pku.Ability, defaultAbility));
                }
                else //ability unspecified
                    return (0, pkxUtil.TagAlerts.GetAbilityAlert(AlertType.UNSPECIFIED, null, defaultAbility));
            }

            public static (HashSet<Ribbon>, Alert) ProcessRibbons(pkuObject pku)
            {
                (HashSet<Ribbon> ribbons, Alert a) = pkxUtil.ProcessTags.ProcessRibbons(pku, IsValidRibbon);

                //In other words, if the pku has a contest ribbon at level x, but not at level x-1 (when x-1 exists).
                if (ribbons.Contains(Ribbon.Cool_Super_G3) && !ribbons.Contains(Ribbon.Cool_G3) ||
                    ribbons.Contains(Ribbon.Cool_Hyper_G3) && !ribbons.Contains(Ribbon.Cool_Super_G3) ||
                    ribbons.Contains(Ribbon.Cool_Master_G3) && !ribbons.Contains(Ribbon.Cool_Hyper_G3) ||
                    ribbons.Contains(Ribbon.Beauty_Super_G3) && !ribbons.Contains(Ribbon.Beauty_G3) ||
                    ribbons.Contains(Ribbon.Beauty_Hyper_G3) && !ribbons.Contains(Ribbon.Beauty_Super_G3) ||
                    ribbons.Contains(Ribbon.Beauty_Master_G3) && !ribbons.Contains(Ribbon.Beauty_Hyper_G3) ||
                    ribbons.Contains(Ribbon.Cute_Super_G3) && !ribbons.Contains(Ribbon.Cute_G3) ||
                    ribbons.Contains(Ribbon.Cute_Hyper_G3) && !ribbons.Contains(Ribbon.Cute_Super_G3) ||
                    ribbons.Contains(Ribbon.Cute_Master_G3) && !ribbons.Contains(Ribbon.Cute_Hyper_G3) ||
                    ribbons.Contains(Ribbon.Smart_Super_G3) && !ribbons.Contains(Ribbon.Smart_G3) ||
                    ribbons.Contains(Ribbon.Smart_Hyper_G3) && !ribbons.Contains(Ribbon.Smart_Super_G3) ||
                    ribbons.Contains(Ribbon.Smart_Master_G3) && !ribbons.Contains(Ribbon.Smart_Hyper_G3) ||
                    ribbons.Contains(Ribbon.Tough_Super_G3) && !ribbons.Contains(Ribbon.Tough_G3) ||
                    ribbons.Contains(Ribbon.Tough_Hyper_G3) && !ribbons.Contains(Ribbon.Tough_Super_G3) ||
                    ribbons.Contains(Ribbon.Tough_Master_G3) && !ribbons.Contains(Ribbon.Tough_Hyper_G3))
                {
                    a = TagAlerts.AddContestRibbonAlert(a);
                }
                return (ribbons, a);
            }

            public static (Alert, bool[]) ProcessFatefulEncounter(pkuObject pku)
            {
                int dex = pkxUtil.GetNationalDexChecked(pku.Species);
                if (dex == 151 || dex == 386) //Mew or Deoxys
                {
                    if (pku.Catch_Info?.Fateful_Encounter == false) //no fateful encounter, won't obey
                        return (TagAlerts.GetFatefulEncounterAlert(dex == 151), new bool[] { false, true });
                }

                return (null, new bool[] { pku.Catch_Info?.Fateful_Encounter == true });
            }


            //Stuff that can be used for gen 4 (and maybe 5) as well, due to pid dependents

            /// <summary>
            /// Different from pkxUtil version: 1) If nature is unspecified/invalid doesn't pick Hardy by default but
            /// instead just returns null since ProcessPID will implicitly pick one. 2) Changes the Alerts to account for this.
            /// </summary>
            /// <param name="pku">The pku whose gender is being processed.</param>
            /// <returns></returns>
            public static (Nature?, Alert) ProcessNature(pkuObject pku)
            {
                if (pku.Nature == null)
                    return (null, TagAlerts.GetNatureAlert(AlertType.UNSPECIFIED));
                else if (!pkxUtil.GetNature(pku.Nature).HasValue)
                    return (null, TagAlerts.GetNatureAlert(AlertType.INVALID, pku.Nature));
                else
                    return pkxUtil.ProcessTags.ProcessNature(pku);
            }

            /// <summary>
            /// Different from pkxUtil version: 1) If gender is unspecified/invalid doesn't pick Male by default but
            /// instead just returns null since ProcessPID will implicitly pick one. 2) Changes the Alerts to account for this.
            /// </summary>
            /// <param name="pku">The pku whose gender is being processed.</param>
            /// <returns></returns>
            public static (Gender?, Alert) ProcessGender(pkuObject pku)
            {
                int dex = pkxUtil.GetNationalDexChecked(pku.Species);
                GenderRatio gr = PokeAPIUtil.GetGenderRatio(dex);
                bool onlyOneGender = gr == GenderRatio.ALL_GENDERLESS || gr == GenderRatio.ALL_FEMALE || gr == GenderRatio.ALL_MALE;

                if (pku.Gender == null && onlyOneGender) //Unspecified but only one gender anyway
                    return (null, null);
                else if (pku.Gender == null) //unspecified
                    return (null, TagAlerts.GetGenderAlert(AlertType.UNSPECIFIED));
                else if (!pkxUtil.GetGender(pku.Gender, false).HasValue)
                    return (null, TagAlerts.GetGenderAlert(AlertType.INVALID, null, pku.Gender));
                else
                    return pkxUtil.ProcessTags.ProcessGender(pku);
            }
        }
    }
}
