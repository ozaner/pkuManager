using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.pkx.pk3
{
    /// <summary>
    /// Exports a <see cref="pkuObject"/> to a <see cref="pk3Object"/>.
    /// </summary>
    public class pk3Exporter : Exporter
    {
        /// <summary>
        /// Creates an exporter that will attempt to export <paramref name="pku"/>
        /// to a .pk3 file with the given <paramref name="globalFlags"/>.
        /// </summary>
        /// <inheritdoc cref="Exporter(pkuObject, GlobalFlags, FormatObject)"/>
        public pk3Exporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

        protected override pk3Object Data { get; } = new();

        public override (bool canPort, string reason) CanPort()
        {
            // Screen National Dex #
            if (pkxUtil.GetNationalDex(pku.Species) > pk3Object.LAST_DEX_NUM) //Only species gen 3 and below are allowed
                return (false, "Must be a species from Gen 3.");

            // Screen Form
            if (!DexUtil.IsFormDefault(pku) && !DexUtil.CanCastPKU(pku, pk3Object.VALID_FORMS)) // If form isn't default, and uncastable
                return (false, "Must be a form that exists in Gen 3.");

            // Screen Shadow Pokemon
            if (pku.IsShadow())
                return (false, "Cannot be a Shadow Pokémon.");

            return (true, null); //compatible with .pk3
        }

        // Working variables
        protected int dex; //official national dex #
        protected int[] moveIndices; //indices of the chosen moves in the pku
        protected Gender? gender;
        protected Nature? nature;
        protected int? unownForm;
        protected string checkedGameName;
        protected Language checkedLang;
        protected bool legalGen3Egg;


        /* ------------------------------------
         * Pre-Processing Methods
         * ------------------------------------
        */

        // Format Override
        [PorterDirective(ProcessingPhase.FormatOverride)]
        protected virtual void ProcessFormatOverride()
        {
            pku = pkuObject.MergeFormatOverride(pku, "pk3");
        }

        // Battle Stat Override
        [PorterDirective(ProcessingPhase.PreProcessing)]
        protected virtual void ProcessBattleStatOverride()
        {
            Notes.Add(pkxUtil.MetaTags.ApplyBattleStatOverride(pku, GlobalFlags));
        }

        // Dex # [Implicit]
        [PorterDirective(ProcessingPhase.PreProcessing)]
        protected virtual void ProcessDex()
        {
            dex = pkxUtil.GetNationalDexChecked(pku.Species);
        }


        /* ------------------------------------
         * Tag Processing Methods
         * ------------------------------------
        */

        // Species
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessSpecies()
            => Data.Species.Set((ushort)PokeAPIUtil.GetSpeciesIndex(dex, 3));

        // Nature [Implicit]
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessNature()
        {
            Alert alert;
            if (pku.Nature is null)
                alert = GetNatureAlert(AlertType.UNSPECIFIED);
            else if (pku.Nature.ToEnum<Nature>() is null)
                alert = GetNatureAlert(AlertType.INVALID, pku.Nature);
            else
                (nature, alert) = pkxUtil.ExportTags.ProcessNature(pku);

            Warnings.Add(alert);
        }

        // Gender [Implicit]
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessGender()
        {
            GenderRatio gr = PokeAPIUtil.GetGenderRatio(dex);
            bool onlyOneGender = gr is GenderRatio.All_Genderless or GenderRatio.All_Female or GenderRatio.All_Male;

            Alert alert;
            if (pku.Gender is null && !onlyOneGender) //unspecified and has more than one possible gender
                alert = GetGenderAlert(AlertType.UNSPECIFIED);
            else if (pku.Gender.ToEnum<Gender>() is null && !onlyOneGender)
                alert = GetGenderAlert(AlertType.INVALID, null, pku.Gender);
            else
                (gender, alert) = pkxUtil.ExportTags.ProcessGender(pku);

            Warnings.Add(alert);
        }

        // Form [Implicit]
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessForm()
        {
            Alert alert = null; //to return
            if (pku.Forms is not null)
            {
                string properFormName = DexUtil.GetSearchableFormName(pku).ToLowerInvariant();
                if (dex is 201 && pku.Forms.Length is 1 && Regex.IsMatch(properFormName, "[a-z!?]")) //unown
                {
                    if (properFormName[0] is '?')
                        unownForm = 26;
                    else if (properFormName[0] is '!')
                        unownForm = 27;
                    else //all other letters
                        unownForm = properFormName[0] - 97;
                }
                else if (dex is 386 && properFormName is "normal" or "attack" or "defense" or "speed") //deoxys
                    alert = GetFormAlert(AlertType.NONE, null, true);
                else if (dex is 351 && properFormName is "sunny" or "rainy" or "snowy") //castform
                    alert = GetFormAlert(AlertType.IN_BATTLE, pku.Forms);
            }

            Warnings.Add(alert);
        }

        // ID
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessTID()
        {
            var (tid, alert) = pkxUtil.ExportTags.ProcessTID(pku);
            Data.TID.Set(tid);
            Warnings.Add(alert);
        }

        // PID [Requires: Gender, Form, Nature, TID] [ErrorResolver]
        [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessGender), nameof(ProcessForm),
                                                    nameof(ProcessNature), nameof(ProcessTID))]
        protected virtual void ProcessPID()
        {
            var (pids, alert) = pkxUtil.ExportTags.ProcessPID(pku, Data.TID, false, gender, nature, unownForm);
            PIDResolver = new ErrorResolver<uint>(alert, pids, x => Data.PID.Set(x));
            if (alert is RadioButtonAlert)
                Errors.Add(alert);
            else
                Warnings.Add(alert);
        }

        // Origin Game
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessOriginGame()
        {
            var (origingame, gamename, alert) = pkxUtil.ExportTags.ProcessOriginGame(pku, 3);
            Data.Origin_Game.Set((ushort)origingame);
            checkedGameName = gamename;
            Warnings.Add(alert);
        }

        // Met Location [Requires: Origin Game]
        [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessOriginGame))]
        protected virtual void ProcessMetLocation()
        {
            var (location, alert) = pkxUtil.ExportTags.ProcessMetLocation(pku, checkedGameName, (g, l) => pk3Object.EncodeMetLocation(g, l), pk3Object.GetDefaultLocationName(checkedGameName));
            Data.Met_Location.Set((byte)location);
            Warnings.Add(alert);
        }

        // Egg [Requires: Origin Game]
        [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessOriginGame))]
        protected virtual void ProcessEgg()
        {
            Data.Is_Egg.Set(pku.IsEgg());

            //Deal with "Legal Gen 3 eggs"
            if (pku.IsEgg() && Data.Origin_Game.Get() is not 0)
            {
                Language? lang = DataUtil.ToEnum<Language>(pku.Game_Info?.Language);
                if (lang is not null && pkxUtil.EGG_NICKNAME[lang.Value] == pku.Nickname)
                {
                    Data.Egg_Name_Override.Set(pk3Object.EGG_NAME_OVERRIDE_CONST); //override nickname to be 'egg'
                    checkedLang = Language.Japanese;
                    Data.Nickname.Set(pk3Object.CHARACTER_ENCODING
                                    .Encode(pkxUtil.EGG_NICKNAME[checkedLang], pk3Object.MAX_NICKNAME_CHARS, checkedLang).encodedStr);
                    Data.OT.Set(pk3Object.CHARACTER_ENCODING
                                .Encode(pku.Game_Info?.OT, pk3Object.MAX_OT_CHARS, lang.Value).encodedStr);
                    legalGen3Egg = true;
                }
            }
        }

        // Language
        [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessEgg))]
        protected virtual void ProcessLanguage()
        {
            if(!legalGen3Egg)
            {
                Alert alert;
                (checkedLang, alert) = pkxUtil.ExportTags.ProcessLanguage(pku, pk3Object.VALID_LANGUAGES);
                Warnings.Add(alert);
            }
            Data.Language.Set((byte)checkedLang);
        }

        // Nickname [Requires: Language]
        [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessLanguage))]
        protected virtual void ProcessNickname()
        {
            if (!legalGen3Egg)
            {
                var (nick, alert, _, _) = pkxUtil.ExportTags.ProcessNickname(pku, 3, pk3Object.MAX_NICKNAME_CHARS, pk3Object.CHARACTER_ENCODING, checkedLang);
                Data.Nickname.Set(nick);
                Warnings.Add(alert);
            }
        }

        // OT [Requires: Language]
        [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessLanguage))]
        protected virtual void ProcessOT()
        {
            if (!legalGen3Egg)
            {
                var (ot, alert) = pkxUtil.ExportTags.ProcessOT(pku, pk3Object.MAX_OT_CHARS, pk3Object.CHARACTER_ENCODING, checkedLang);
                Data.OT.Set(ot);
                Warnings.Add(alert);
            }
        }

        // Trash Bytes [Requires: Nickname, OT]
        [PorterDirective(ProcessingPhase.FirstPass, nameof(ProcessNickname), nameof(ProcessOT))]
        protected virtual void ProcessTrashBytes()
        {
            pkuObject.Trash_Bytes_Class tb = pku?.Trash_Bytes;
            if(tb is not null)
            {
                ushort[] nicknameTrash = tb?.Nickname?.Length > 0 ? tb.Nickname : null;
                ushort[] otTrash = tb?.OT?.Length > 0 ? tb.OT : null;
                var (nick, ot, alert) = pkxUtil.ExportTags.ProcessTrash(Data.Nickname, nicknameTrash, Data.OT, otTrash, pk3Object.CHARACTER_ENCODING);
                Data.Nickname.Set(nick);
                Data.OT.Set(ot);
                Warnings.Add(alert);
            }
        }

        // Markings
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessMarkings()
        {
            HashSet<Marking> markings = pku.Markings.ToEnumSet<Marking>();
            Data.MarkingCircle.Set(markings.Contains(Marking.Blue_Circle));
            Data.MarkingSquare.Set(markings.Contains(Marking.Blue_Square));
            Data.MarkingTriangle.Set(markings.Contains(Marking.Blue_Triangle));
            Data.MarkingHeart.Set(markings.Contains(Marking.Blue_Heart));
        }

        // Item
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessItem()
        {
            var (item, alert) = pkxUtil.ExportTags.ProcessItem(pku, 3);
            Data.Item.Set((ushort)item);
            Warnings.Add(alert);
        }

        // Experience [ErrorResolver]
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessExperience()
        {
            var (options, alert) = pkxUtil.ExportTags.ProcessEXP(pku);
            ExperienceResolver = new ErrorResolver<uint>(alert, options, x => Data.Experience.Set(x));
            if (alert is RadioButtonAlert)
                Errors.Add(alert);
            else
                Warnings.Add(alert);
        }

        // Moves
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessMoves()
        {
            int[] moves;
            Alert alert;
            (moves, moveIndices, alert) = pkxUtil.ExportTags.ProcessMoves(pku, pk3Object.LAST_MOVE_ID);
            Data.Moves.Set(Array.ConvertAll(moves, x => (ushort)x));
            Warnings.Add(alert);
        }

        // PP-Ups [Requires: Moves]
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessPPUps()
        {
            var (ppups, alert) = pkxUtil.ExportTags.ProcessPPUps(pku, moveIndices);
            Data.PP_Up_1.Set((byte)ppups[0]);
            Data.PP_Up_2.Set((byte)ppups[1]);
            Data.PP_Up_3.Set((byte)ppups[2]);
            Data.PP_Up_4.Set((byte)ppups[3]);
            Warnings.Add(alert);
        }

        // PP [Requires: Moves, PP-Ups]
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessPP()
        {
            ushort[] moves = Data.Moves;
            Data.PP.Set(new[]
            {
                pk3Object.CalculatePP(moves[0], Data.PP_Up_1),
                pk3Object.CalculatePP(moves[1], Data.PP_Up_2),
                pk3Object.CalculatePP(moves[2], Data.PP_Up_3),
                pk3Object.CalculatePP(moves[3], Data.PP_Up_4),
            });
        }

        // Friendship
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessFriendship()
        {
            var (friendship, alert) = pkxUtil.ExportTags.ProcessFriendship(pku);
            Data.Friendship.Set((byte)friendship);
            Warnings.Add(alert);
        }

        // EVs
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessEVs()
        {
            var (evs, alert) = pkxUtil.ExportTags.ProcessEVs(pku);
            Data.EV_HP.Set((byte)evs[0]);
            Data.EV_Attack.Set((byte)evs[1]);
            Data.EV_Defense.Set((byte)evs[2]);
            Data.EV_Sp_Attack.Set((byte)evs[3]);
            Data.EV_Sp_Defense.Set((byte)evs[4]);
            Data.EV_Speed.Set((byte)evs[5]);
            Warnings.Add(alert);
        }

        // Contest Stats
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessContestStats()
        {
            var (contest, alert) = pkxUtil.ExportTags.ProcessContest(pku);
            Data.Cool.Set((byte)contest[0]);
            Data.Beauty.Set((byte)contest[1]);
            Data.Cute.Set((byte)contest[2]);
            Data.Smart.Set((byte)contest[3]);
            Data.Tough.Set((byte)contest[4]);
            Data.Sheen.Set((byte)contest[5]);
            Warnings.Add(alert);
        }

        // Pokérus
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessPokerus()
        {
            var (strain, days, alert) = pkxUtil.ExportTags.ProcessPokerus(pku);
            Data.PKRS_Strain.Set((byte)strain);
            Data.PKRS_Days.Set((byte)days);
            Warnings.Add(alert);
        }

        // Met Level
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessMetLevel()
        {
            var (level, alert) = ((byte, Alert))pkxUtil.ExportTags.ProcessMetLevel(pku);
            Data.Met_Level.Set((byte)level);
            Warnings.Add(alert);
        }

        // Ball
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessBall()
        {
            var (ball, alert) = pkxUtil.ExportTags.ProcessBall(pku, Ball.Premier_Ball);
            Data.Ball.Set((byte)ball);
            Warnings.Add(alert);
        }

        // OT Gender
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessOTGender()
        {
            var (gender, alert) = pkxUtil.ExportTags.ProcessOTGender(pku);
            Data.OT_Gender.Set (gender is Gender.Female); //male otherwise
            Warnings.Add(alert);
        }

        // IVs
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessIVs()
        {
            var (ivs, alert) = pkxUtil.ExportTags.ProcessIVs(pku);
            Data.IV_HP.Set((byte)ivs[0]);
            Data.IV_Attack.Set((byte)ivs[1]);
            Data.IV_Defense.Set((byte)ivs[2]);
            Data.IV_Sp_Attack.Set((byte)ivs[3]);
            Data.IV_Sp_Defense.Set((byte)ivs[4]);
            Data.IV_Speed.Set((byte)ivs[5]);
            Warnings.Add(alert);
        }

        // Ability Slot
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessAbilitySlot()
        {
            Alert alert = null;
            int defaultAbilityID = (int)pk3Object.PK3_ABILITY_DATA["" + dex]["1"];
            string defaultAbility = PokeAPIUtil.GetAbility(defaultAbilityID);
            if (pku.Ability is not null) //ability specified
            {
                int? abilityID = PokeAPIUtil.GetAbilityIndex(pku.Ability);
                if (abilityID is null or > 76) //unofficial ability OR gen4+ ability
                {
                    Data.Ability_Slot.Set(false);
                    alert = pkxUtil.ExportAlerts.GetAbilityAlert(AlertType.INVALID, pku.Ability, defaultAbility);
                }
                else //gen 3- ability
                {
                    (bool slot1valid, bool slot2valid) = pk3Object.IsAbilityValid(dex, abilityID.Value);
                    if (slot1valid) //ability corresponds to slot 1
                        Data.Ability_Slot.Set(false);
                    else if (slot2valid) //ability corresponds to slot 2
                        Data.Ability_Slot.Set(true);
                    else //ability is impossible on this species, fallback on slot 1
                    {
                        Data.Ability_Slot.Set(false);
                        alert = pkxUtil.ExportAlerts.GetAbilityAlert(AlertType.MISMATCH, pku.Ability, defaultAbility);
                    }
                }
            }
            else //ability unspecified
            {
                Data.Ability_Slot.Set(false);
                alert = pkxUtil.ExportAlerts.GetAbilityAlert(AlertType.UNSPECIFIED, pku.Ability, defaultAbility);
            }
            Warnings.Add(alert);
        }

        // Ribbons
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessRibbons()
        {
            (HashSet<Ribbon> ribbons, Alert a) = pkxUtil.ExportTags.ProcessRibbons(pku, pk3Object.IsValidRibbon);

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
                a = AddContestRibbonAlert(a);
            }

            //Add contest ribbons
            Data.Cool_Ribbon_Rank.Set(pk3Object.GetRibbonRank(Ribbon.Cool_G3, ribbons));
            Data.Beauty_Ribbon_Rank.Set(pk3Object.GetRibbonRank(Ribbon.Beauty_G3, ribbons));
            Data.Cute_Ribbon_Rank.Set(pk3Object.GetRibbonRank(Ribbon.Cute_G3, ribbons));
            Data.Smart_Ribbon_Rank.Set(pk3Object.GetRibbonRank(Ribbon.Smart_G3, ribbons));
            Data.Tough_Ribbon_Rank.Set(pk3Object.GetRibbonRank(Ribbon.Tough_G3, ribbons));

            //Add other ribbons
            Data.Champion_Ribbon.Set(ribbons.Contains(Ribbon.Champion));
            Data.Winning_Ribbon.Set(ribbons.Contains(Ribbon.Winning));
            Data.Victory_Ribbon.Set(ribbons.Contains(Ribbon.Victory));
            Data.Artist_Ribbon.Set(ribbons.Contains(Ribbon.Artist));
            Data.Effort_Ribbon.Set(ribbons.Contains(Ribbon.Effort));
            Data.Battle_Champion_Ribbon.Set(ribbons.Contains(Ribbon.Battle_Champion));
            Data.Regional_Champion_Ribbon.Set(ribbons.Contains(Ribbon.Regional_Champion));
            Data.National_Champion_Ribbon.Set(ribbons.Contains(Ribbon.National_Champion));
            Data.Country_Ribbon.Set(ribbons.Contains(Ribbon.Country));
            Data.National_Ribbon.Set(ribbons.Contains(Ribbon.National));
            Data.Earth_Ribbon.Set(ribbons.Contains(Ribbon.Earth));
            Data.World_Ribbon.Set(ribbons.Contains(Ribbon.World));

            Warnings.Add(a);
        }

        // Fateful Encounter [ErrorResolver]
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessFatefulEncounter()
        {
            Alert alert = null;
            bool[] options;
            if (dex is 151 or 386 && pku.Catch_Info?.Fateful_Encounter is not true) //Mew or Deoxys w/ no fateful encounter
            {
                options = new[] { false, true };
                alert = GetFatefulEncounterAlert(dex is 151);
            }
            else
                options = new[] { pku.Catch_Info?.Fateful_Encounter is true };

            FatefulEncounterResolver = new ErrorResolver<bool>(alert, options, x => Data.Fateful_Encounter.Set(x));
            if (alert is RadioButtonAlert)
                Errors.Add(alert);
            else
                Warnings.Add(alert);
        }

        // Byte Override [ErrorResolver]
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessByteOverride()
        {
            Alert a;
            (a, ByteOverrideResolver) = pkxUtil.MetaTags.ApplyByteOverride(pku, Data.NonSubData, Data.G, Data.A, Data.E, Data.M);
            Warnings.Add(a);
        }


        /* ------------------------------------
         * Error Resolvers
         * ------------------------------------
        */

        // PID ErrorResolver
        [PorterDirective(ProcessingPhase.SecondPass)]
        protected virtual ErrorResolver<uint> PIDResolver { get; set; }

        // Experience ErrorResolver
        [PorterDirective(ProcessingPhase.SecondPass)]
        protected virtual ErrorResolver<uint> ExperienceResolver { get; set; }

        // Fateful Encounter ErrorResolver
        [PorterDirective(ProcessingPhase.SecondPass)]
        protected virtual ErrorResolver<bool> FatefulEncounterResolver { get; set; }


        /* ------------------------------------
         * Post-Processing Methods
         * ------------------------------------
        */

        // Byte Override ErrorResolver
        [PorterDirective(ProcessingPhase.PostProcessing)]
        protected virtual ErrorResolver<uint> ByteOverrideResolver { get; set; }


        /* ------------------------------------
         * pk3 Exporting Alerts
         * ------------------------------------
        */

        // Adds gen 3 contest ribbon alert to an existing pkxUtil ribbon alert (or null), if needed.
        public static Alert AddContestRibbonAlert(Alert ribbonAlert)
        {
            string msg = "This pku has a Gen 3 contest ribbon of some category with rank super or higher, " +
                "but doesn't have the ribbons below that rank. This is impossible in this format, adding those ribbons.";
            if (ribbonAlert is not null)
                ribbonAlert.Message += DataUtil.Newline(2) + msg;
            else
                ribbonAlert = new Alert("Ribbons", msg);
            return ribbonAlert;
        }

        public static Alert GetNatureAlert(AlertType at, string invalidNature = null) => at switch
        {
            AlertType.UNSPECIFIED => new Alert("Nature", "No nature specified, using the nature decided by the PID."),
            AlertType.INVALID => new Alert("Nature", $"The nature \"{invalidNature}\" is not valid in this format. Using the nature decided by the PID."),
            _ => pkxUtil.ExportAlerts.GetNatureAlert(at, invalidNature)
        };

        // Changes the UNSPECIFIED & INVALID AlertTypes from the pkxUtil method to account for PID-Nature dependence.
        public static Alert GetGenderAlert(AlertType at, Gender? correctGender = null, string invalidGender = null) => at switch
        {
            AlertType.UNSPECIFIED => new Alert("Gender", "No gender specified, using the gender decided by the PID."),
            AlertType.INVALID => new Alert("Gender", $"The gender \"{invalidGender}\" is not valid in this format. Using the gender decided by the PID."),
            _ => pkxUtil.ExportAlerts.GetGenderAlert(at, correctGender, invalidGender)
        };

        public static RadioButtonAlert GetFatefulEncounterAlert(bool isMew)
        {
            string pkmn = isMew ? "Mew" : "Deoxys";
            string msg = $"This {pkmn} was not met in a fateful encounter. " +
                $"Note that, in the Gen 3 games, {pkmn} will only obey the player if it was met in a fateful encounter.";

            RadioButtonAlert.RBAChoice[] choices =
            {
                new("Keep Fateful Encounter",$"Fateful Encounter: false\n{pkmn} won't obey."),
                new("Set Fateful Encounter",$"Fateful Encounter: true\n{pkmn} will obey.")
            };

            return new RadioButtonAlert("Fateful Encounter", msg, choices);
        }

        public static Alert GetFormAlert(AlertType at, string[] invalidForm = null, bool isDeoxys = false)
        {
            return isDeoxys ? new Alert("Form", "Note that in generation 3, Deoxys' form depends on what game it is currently in.") 
                : pkxUtil.ExportAlerts.GetFormAlert(at, invalidForm);
        }
    }
}
