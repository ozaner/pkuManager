using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.Formats.pkx;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.showdown
{
    /// <summary>
    /// Exports a <see cref="pkuObject"/> to a <see cref="ShowdownObject"/>.
    /// </summary>
    public class ShowdownExporter : Exporter
    {
        /// <summary>
        /// Creates an exporter that will attempt to export <paramref name="pku"/>
        /// to a .txt (Showdown!) file, encoded in UTF-8, with the given <paramref name="globalFlags"/>.
        /// </summary>
        /// <inheritdoc cref="Exporter(pkuObject, GlobalFlags, FormatObject)"/>
        public ShowdownExporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

        protected override Type DataType { get => typeof(ShowdownObject); }

        /// <summary>
        /// <see cref="Exporter.Data"/> casted as a <see cref="ShowdownObject"/>.
        /// </summary>
        protected ShowdownObject ShowdownData { get => Data as ShowdownObject; }

        public override (bool, string) CanPort()
        {
            //Showdown doesn't support eggs (they can't exactly battle...).
            if (pku.IsEgg())
                return (false, "Cannot be an Egg.");

            // Only Pokemon with a valid Showdown name are allowed.
            return (ShowdownObject.GetShowdownName(pku).name is not null,
                "Species+Form doesn't exist in Showdown, and cannot be casted to a form that does.");
        }


        /* ------------------------------------
         * Pre-Processing Methods
         * ------------------------------------
        */

        // Battle Stat Override
        [PorterDirective(ProcessingPhase.PreProcessing)]
        protected virtual void ProcessBattleStatOverride()
        {
            Notes.Add(pkxUtil.PreProcess.ProcessBattleStatOverride(pku, GlobalFlags));
        }


        /* ------------------------------------
         * Tag Processing Methods
         * ------------------------------------
        */

        // Showdown Name
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessShowdownName()
        {
            // Notes:
            //  - Combination of species and form tags (and gender for Meowstic & Indeedee)

            bool casted;
            (ShowdownData.ShowdownName, casted) = ShowdownObject.GetShowdownName(pku);
            if (ShowdownData.ShowdownName is null)
                throw new ArgumentException("Expected a pku with a valid Showdown species here.");

            if (casted)
                Warnings.Add(pkxUtil.TagAlerts.GetFormAlert(AlertType.CASTED, pku.Forms));
        }

        // Nickname
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessNickname()
        {
            // Notes:
            //  - Practically no character limit
            //  - Can use parenthesis (only checks at the end of first line)
            //  - Empty nickname interpreted as no nickname
            //  - Leading spaces are ignored

            if (pku.Nickname is "") //empty counts as null
                ShowdownData.Nickname = null;
            else
                ShowdownData.Nickname = pku.Nickname;

            if (ShowdownData.Nickname?.Length > 0 && ShowdownData.Nickname[0] is ' ') //if first character is a space
                Warnings.Add(GetNicknameAlert(AlertType.INVALID));
        }

        // Gender
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessGender()
        {
            // Notes:
            //  - Illegal genders are ignored in legal rulesets, but used in illegal ones.
            //  - Genderless is denoted by no gender.

            ShowdownData.Gender = pku.Gender.ToEnum<Gender>();
        }

        // Item
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessItem()
        {
            bool itemValid = ShowdownObject.IsItemValid(pku.Item);
            if (pku.Item is not null && !itemValid) //check for invalid alert
                Warnings.Add(pkxUtil.TagAlerts.GetItemAlert(AlertType.INVALID, pku.Item));

            if (itemValid)
                ShowdownData.Item = pku.Item;
        }

        // Ability
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessAbility()
        {
            bool abilityValid = ShowdownObject.IsAbilityValid(pku.Ability);
            if (pku.Ability is not null && !abilityValid) //check for invalid alert
                Warnings.Add(pkxUtil.TagAlerts.GetAbilityAlert(AlertType.INVALID, pku.Ability, "None (Showdown will pick one)."));
            else
                ShowdownData.Ability = pku.Ability;
        }

        // Level
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessLevel()
        {
            var (level, alert) = pkxUtil.ProcessTags.ProcessNumericTag(pku.Level, pkxUtil.TagAlerts.GetLevelAlert, false, 100, 1, 100);
            ShowdownData.Level = (byte)level;
            Warnings.Add(alert);
        }

        // Friendship
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessFriendship()
        {
            var (friendship, alert) = pkxUtil.ProcessTags.ProcessNumericTag(pku.Friendship, GetFriendshipAlert, false, 255, 0, 255);
            ShowdownData.Friendship = (byte)friendship;
            Warnings.Add(alert);
        }

        // IVs
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessIVs()
        {
            int?[] vals = { pku.IVs?.HP, pku.IVs?.Attack, pku.IVs?.Defense, pku.IVs?.Sp_Attack, pku.IVs?.Sp_Defense, pku.IVs?.Speed };
            var (ivs, alert) = pkxUtil.ProcessTags.ProcessMultiNumericTag(pku.IVs is not null, vals, pkxUtil.TagAlerts.GetIVsAlert, 31, 0, 31, false);
            ShowdownData.IVs = Array.ConvertAll(ivs, x => (byte)x);
            Warnings.Add(alert);
        }

        // EVs
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessEVs()
        {
            var (evs, alert) = pkxUtil.ProcessTags.ProcessEVs(pku);
            ShowdownData.EVs = Array.ConvertAll(evs, x => (byte)x);
            Warnings.Add(alert);
        }

        // Nature
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessNature()
        {
            ShowdownData.Nature = pku.Nature.ToEnum<Nature>();
            if (ShowdownData.Nature is null)
            {
                if (pku.Nature is null)
                    Warnings.Add(GetNatureAlert(AlertType.UNSPECIFIED));
                else
                    Warnings.Add(GetNatureAlert(AlertType.INVALID, pku.Nature));
            }
        }

        // Gigantamax Factor
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessGigantamaxFactor()
        {
            if (pku.Gigantamax_Factor is true)
            {
                if (ShowdownObject.IsGMaxValid(ShowdownData.ShowdownName))
                    ShowdownData.Gigantamax_Factor = true;
                else
                    Warnings.Add(GetGMaxAlert(AlertType.INVALID));
            }
        }

        // Moves
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void ProcessMoves()
        {
            //doesnt get gmax moves, but showdown doesn't allow them either
            List<string> moves = new();
            (int[] moveIDs, _, Alert alert) = pkxUtil.ProcessTags.ProcessMoves(pku, pkxUtil.LAST_MOVE_INDEX_GEN8);
            foreach (int id in moveIDs)
            {
                if (id is not 0)
                    moves.Add(PokeAPIUtil.GetMoveName(id));
            }
            ShowdownData.Moves = moves.ToArray();
            Warnings.Add(alert);
        }


        /* ------------------------------------
         * Showdown Exporting Alerts
         * ------------------------------------
        */

        public static Alert GetNicknameAlert(AlertType at) => at switch
        {
            AlertType.INVALID => new Alert("Nickname", $"Showdown does not recoginize leading spaces in nicknames."),
            _ => throw InvalidAlertType(at)
        };

        public static Alert GetLevelAlert(AlertType at) => at switch
        {
            //override pkx's unspecified level of 1 to 100
            AlertType.UNSPECIFIED => new Alert("Level", "No level specified, using the default: 100."),
            _ => pkxUtil.TagAlerts.GetLevelAlert(at)
        };

        public static Alert GetFriendshipAlert(AlertType at) => at switch
        {
            //override pkx's unspecified friendship of 0 to 255
            AlertType.UNSPECIFIED => pkxUtil.TagAlerts.getNumericalAlert("Friendship", at, 255),
            _ => pkxUtil.TagAlerts.GetFriendshipAlert(at)
        };

        public static Alert GetNatureAlert(AlertType at, string invalidNature = null)
        {
            Alert a = new("Nature", $"Using the default: None (Showdown uses Serious when no nature is specified.)");
            if (at is AlertType.INVALID)
            {
                if (invalidNature == null)
                    throw new ArgumentException("If INVALID AlertType given, invalidNature must also be given.");
                a.Message = $"The Nature \"{invalidNature}\" is not valid in this format. " + a.Message;
                return a;
            }
            else if (at is AlertType.UNSPECIFIED)
            {
                a.Message = $"No nature was specified. " + a.Message;
                return a;
            }
            else
                throw InvalidAlertType(at);
        }

        public static Alert GetGMaxAlert(AlertType at) => at switch
        {
            AlertType.INVALID => new Alert("Gigantamax Factor", "This species does not have a Gigantamax form in this format. Setting Gigantamax factor to false."),
            _ => throw InvalidAlertType(at)
        };
    }
}
