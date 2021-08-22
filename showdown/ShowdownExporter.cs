using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using pkuManager.pkx;
using System.Linq;
using System.Text;

namespace pkuManager.showdown
{
    public class ShowdownExporter : Exporter
    {
        public ShowdownExporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

        public override string formatName { get { return "Showdown!"; } }

        public override string formatExtension { get { return "txt"; } }

        public override bool canExport()
        {
            //Showdown doesn't support eggs (they can't exactly battle...).
            if (pku.IsEgg())
                return false;

            // Only Pokemon with a valid Showdown name are allowed.
            return ShowdownUtil.GetShowdownName(pku).name != null;
        }


        // Values
        //  - Decided in processAlerts(), encoded in toFile().
        //  - All values can be null except species.
        private string species, nickname, item, ability;
        private string[] moves = new string[4];
        private int level, friendship;
        private Gender? gender;
        private Nature? nature;
        private int[] ivs = new int[4], evs = new int[4];
        private bool gigantamax;

        protected override void processAlerts()
        {
            Alert tempAlert;

            // ----------
            // Process Global Flags
            // ----------

            tempAlert = pkxUtil.ProcessFlags.ProcessBattleStatOverride(pku, globalFlags);
            notes.Add(tempAlert);


            // ----------
            // Process Tags
            // ----------

            // Showdown Name
            //  - Combination of species and form tags (and gender for Meowstic & Indeedee)
            (species, tempAlert) = ShowdownUtil.ProcessTags.ProcessShowdownName(pku);
            warnings.Add(tempAlert);

            // Nickname
            (nickname, tempAlert) = ShowdownUtil.ProcessTags.ProcessNickname(pku);
            warnings.Add(tempAlert);

            // Gender
            //  - Illegal genders are ignored in legal rulesets, but used in illegal ones.
            //  - Genderless is denoted by no gender.
            gender = pkxUtil.GetGender(pku.Gender, false);

            // Item
            (item, tempAlert) = ShowdownUtil.ProcessTags.ProcessItem(pku);
            warnings.Add(tempAlert);

            // Ability
            (ability, tempAlert) = ShowdownUtil.ProcessTags.ProcessAbility(pku);
            warnings.Add(tempAlert);

            // Level
            (level, tempAlert) = ShowdownUtil.ProcessTags.ProcessLevel(pku);
            warnings.Add(tempAlert);

            // Friendship
            (friendship, tempAlert) = ShowdownUtil.ProcessTags.ProcessFriendship(pku); //custom ProcessFriendship method because 255 is default not 0
            warnings.Add(tempAlert);

            // IVs
            (ivs, tempAlert) = ShowdownUtil.ProcessTags.ProcessIVs(pku); //custom ProcessIVs method because 31 is default not 0
            warnings.Add(tempAlert);

            // EVs
            (evs, tempAlert) = pkxUtil.ProcessTags.ProcessEVs(pku);
            warnings.Add(tempAlert);

            // Nature
            (nature, tempAlert) = ShowdownUtil.ProcessTags.ProcessNature(pku);
            warnings.Add(tempAlert);

            // Gigantamax
            (gigantamax, tempAlert) = ShowdownUtil.ProcessTags.ProcessGMax(pku, species);
            warnings.Add(tempAlert);

            // Moves
            (moves, tempAlert) = ShowdownUtil.ProcessTags.ProcessMoves(pku);
            warnings.Add(tempAlert);
        }

        protected override byte[] toFile()
        {
            string txt = "";

            // Species/Nickname
            if (nickname == null || nickname == species)
                txt += species;
            else
                txt += $"{nickname} ({species})";

            // Gender
            if (gender == Gender.Male)
                txt += " (M)";
            else if (gender == Gender.Female)
                txt += " (F)";

            // Item
            if (item != null)
                txt += $" @ {item}";

            // Ability
            if (ability != null)
                txt += $"\nAbility: {ability}";

            // Level
            if (level != 100)
                txt += $"\nLevel: {level}";

            // Shiny (no preprocessing)
            if (pku.IsShiny())
                txt += "\nShiny: true";

            // Friendship
            if (friendship != 255)
                txt += $"\nHappiness: {friendship}";

            // IVs
            if (!ivs.All(x => x == 31))
            {
                txt += $"\nIVs: {(ivs[0] != 31 ? $"{ivs[0]} HP / " : "")}{(ivs[1] != 31 ? $"{ivs[1]} Atk / " : "")}" +
                              $"{(ivs[2] != 31 ? $"{ivs[2]} Def / " : "")}{(ivs[3] != 31 ? $"{ivs[3]} SpA / " : "")}" +
                              $"{(ivs[4] != 31 ? $"{ivs[4]} SpD / " : "")}{(ivs[5] != 31 ? $"{ivs[5]} Spe / " : "")}";
                txt = txt.Substring(0, txt.Length - 3);
            }

            // EVs
            if (!evs.All(x => x == 0))
            {
                txt += $"\nEVs: {(evs[0] != 0 ? $"{evs[0]} HP / " : "")}{(evs[1] != 0 ? $"{evs[1]} Atk / " : "")}" +
                              $"{(evs[2] != 0 ? $"{evs[2]} Def / " : "")}{(evs[3] != 0 ? $"{evs[3]} SpA / " : "")}" +
                              $"{(evs[4] != 0 ? $"{evs[4]} SpD / " : "")}{(evs[5] != 0 ? $"{evs[5]} Spe / " : "")}";
                txt = txt.Substring(0, txt.Length - 3);
            }

            // Nature
            if (nature.HasValue)
                txt += $"\n{nature} Nature";

            // Gigantamax
            if (gigantamax)
                txt += "\nGigantamax: Yes";

            // Moves
            foreach (string move in moves)
                txt += $"\n- {move}";

            // PP Ups not used in Showdown, by default always max PP
            // Found one thread regarding it but nothing seems to have come of it:
            // https://www.smogon.com/forums/threads/allow-moves-to-have-non-max-pp.3653621/

            return Encoding.UTF8.GetBytes(txt);
        }
    }
}
