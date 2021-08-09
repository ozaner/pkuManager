using Newtonsoft.Json.Linq;
using pkuManager.Alerts;
using System;
using System.Text;

namespace pkuManager.Exporters
{
    /// <summary>
    /// The exporter class for the Showdown! file format (.txt)
    /// </summary>
    public class ShowdownExporter : Exporter
    {
        /// <summary>
        /// A JObject corresponding to the showdownData.json data for the Showdown format.
        /// </summary>
        public static readonly JObject SHOWDOWN_DATA = Utilities.getJson("showdownData");

        // Stacked NationalDex+Pokestar+Showdown Data
        private static readonly JObject COMBINED_SHOWDOWN_DATA = Utilities.getCombinedJson(new JObject[] 
        { 
            pkCommons.NATIONALDEX_DATA,
            pkCommons.POKESTAR_DATA,
            SHOWDOWN_DATA
        });

        // Cached attributes from processAlerts phase. Cuts down on redudant processing.
        private string species, item, ability, nature;
        private string[] moves = new string[4];
        private int level, friendship;
        private Gender? gender;
        private int?[] ivs = new int?[4], evs = new int?[4];

        /// <summary>
        /// Creates a new one-time Showdown exporter for the given pku.
        /// </summary>
        /// <param name="pku">The PKUObject to be converted.</param>
        public ShowdownExporter(PKUObject pku) : base(pku) { }

        public override string formatName { get { return "Showdown!"; } }

        public override string formatExtension { get { return "txt"; } }

        // Returns whether the given pku is a valid CAP pokemon
        private static bool isCAP(PKUObject pku)
        {
            if (pku.Species == null)
                return false;

            return pkCommons.POKESTAR_DATA[pku.Species] != null;
        }

        // Returns the showdown species name of the given pku with its forms, Returns null otherwise
        // Also returns if the pku form is invalid in showdown, so a warning can be thrown
        private static (string, bool) getShowdownSpecies(PKUObject pku)
        {
            // Check if species is null
            if (pku.Species == null)
                return (null, false); // no species, no use of form or not

            // Check if species is valid in showdown
            JToken speciesjson = COMBINED_SHOWDOWN_DATA?[pku.Species];
            if (speciesjson == null)
                return (null, false); // invalid species, no use of form or not

            string defaultForm = Utilities.getDefaultForm(pku, COMBINED_SHOWDOWN_DATA); //get default form of this species

            // Check if given form is valid in showdown
            JToken formjson = null;
            bool formInvalid = false;
            if (pku.Form != null)
            {
                formjson = speciesjson?["Forms"]?[pku.Form]?["Showdown Species"];
                formInvalid = formjson == null;
            }

            // If given form is invalid, use default form
            if (formjson != null)
                return ((string)(formjson), formInvalid);

            // returns the showdown name of this species' default form
            string defaultShowdownName = (string)speciesjson?["Forms"]?[defaultForm]?["Showdown Species"];
            return defaultShowdownName == null ? (pku.Species, formInvalid) : (defaultShowdownName, formInvalid);
        }

        // Implementation for exporter.
        // Allows: Gen 1-8 (1-4 for now), CAP (Create-a-Pokemon), Pokestar Pokemon
        public override bool canExport()
        {
            if (pkCommons.isAnEgg(pku))
                return false; //Eggs cannot battle until they are hatched.

            // Only Pokemon from gens 1-8, CAP, or pokestar are allowed.
            return pkCommons.GetNationalDex(pku).HasValue ||
                   isCAP(pku) ||
                   pkCommons.GetPokestarID(pku).Item1.HasValue; //If pokestar id isn't null
        }

        // Implementation for exporter.
        // Warnings: form, gender, item, ability, level, friendship IVs, EVs, nature, moves
        // Errors: None
        protected override void processAlerts()
        {
            AlertBox tempAlert; // used for unpacking the alerts from their pairs

            // Form Warning
            // Uses Form and Species Tag
            // Range: Any valid showdown form for the given species
            // Default: whatever the default showdown form is for the species
            // Alert on Unspecified: No
            bool formInvalid;
            (species, formInvalid) = getShowdownSpecies(pku);
            if(formInvalid)
                warnings.Add(new AlertBox("Form", pku.Form + " is not a valid form for " + pku.Species + " in Showdown. Using the base form."));

            // Gender Warning
            // Uses Gender tag
            // Range: 0 (M), 1 (F), -1 (anything else)
            // Default: -1 => None (i.e. no line for it) (Showdown picks a random one)
            // Alert on Unspecified: No
            (tempAlert, _, gender) = CommonAlerts.getGenderAlert(pku, "None", false, "Showdown uses a random gender when none is specified.");
            warnings.Add(tempAlert);

            // Item Warning
            // Uses Item tag
            // Range: Everything in showdownItems.txt (gen 2-8 battle items + cap item) or "None"
            // Default: None (i.e. no line for it)
            // Alert on Unspecified: No
            (tempAlert, item) = CommonAlerts.getTextAlert("Item", pku.Item, (x) =>
            {
                return ReadingUtil.getIDFromFile("showdownItems", x);
            }, "None", false);
            warnings.Add(tempAlert);

            // Ability Warning
            // Uses Ability tag
            // Range: Every Gen 3-8 ability (i.e. everything in Abilities.txt) or "None"
            // Default: None (Showdown can handle it but may require adding an ability later)
            // Alert on Unspecified: No
            (tempAlert, ability, _) = CommonAlerts.getAbilityAlert(pku, true, "Showdown may require you choose an ability depending on the format.");
            warnings.Add(tempAlert);

            // Level Warning
            // Uses Level tag
            // Range: [1..100]
            // Default: 100 (uses no line)
            // Alert on Unspecified: No
            // Comments: had to use a boilerplate getTextAlert call since the actual level alert is entagled with exp.
            //           Design choice to not use exp. tag not referenced in showdown so no need to make use of it here.
            (tempAlert, level) = CommonAlerts.getNumericalAlert("Level", pku.Level, 100, 1, 100, true);
            warnings.Add(tempAlert);

            // Friendship Warning
            // Uses HT_Friendship tag
            // Range: [0..255]
            // Default: 255
            // Alert on Unspecified: No
            (tempAlert, friendship) = CommonAlerts.getFriendshipAlert(false, pku, 255, false);
            warnings.Add(tempAlert);

            // IVs Warning
            // Uses IV tag
            // Range: 0-31
            // Default: None (31)
            // Alert on Unspecified: Yes
            (tempAlert, ivs) = CommonAlerts.getIVAlert(pku, null, true, null); ;
            //(tempAlert, ivs) = CommonAlerts.getIVAlert(pku, null, true, "Note that IVs of 31 will be not be listed as Showdown assumes them to be 31.");
            warnings.Add(tempAlert);

            // EVs Warning
            // Uses EV tag
            // Range: 0-255
            // Default: None (0)
            // Alert on Unspecified: Yes
            (tempAlert, evs) = CommonAlerts.getEVAlert(pku, null, true, null);
            //(tempAlert, evs) = CommonAlerts.getEVAlert(pku, null, true, "Note that EVs of 0 will be not be listed as Showdown assumes them to be 0.");
            warnings.Add(tempAlert);

            // Nature Warning
            // Uses Nature tag
            // Range: Any valid nature or "None"
            // Default: "None"
            // Alert on Unspecified: Yes
            (tempAlert, nature, _) = CommonAlerts.getNatureAlert(pku, "None", true, "Showdown uses the Serious nature when none is specified.");
            warnings.Add(tempAlert);

            // Moves Warning
            (tempAlert, moves, _) = CommonAlerts.getMoveAlert(pku, new string[]
            {
                pku.Moves?.Move1,
                pku.Moves?.Move2,
                pku.Moves?.Move3,
                pku.Moves?.Move4
            }, pkCommons.GetMoveID, true);
            warnings.Add(tempAlert);
        }

        protected override byte[] toFile()
        {
            string txt = "";

            // Nickname/Species/Form (TODO FIX FORMS FOR gens 1-8)
            // Default: Use Species name if there is no nickname, canExport ensures Species is specified
            // Alerts: Warning if form is invalid, uses default form for species
            if (pku.Nickname != null)
                txt += pku.Nickname + " (" + species + ")";
            else // No nickname or it matches
                txt += species;

            // Gender
            // Default: None (Showdown chooses a random gender)
            // Alerts: Warning if gender is invalid
            if (gender == Gender.Male)
                txt += " (M)";
            else if (gender == Gender.Female)
                txt += " (F)";

            // Item
            // Default: None (No Item)
            // Alerts: Warning if item is invalid (unrecoginized by Showdown)
            if (item != "None")
                txt += " @ " + pku.Item;

            txt += "\n"; //next line

            // Ability
            // Default: None (No Ability)
            // Alerts: Warning if ability is invalid or unspecified.
            if (ability != "None")
                txt += "Ability: " + pku.Ability + "\n";

            // Level
            // Default: None (100)
            // Alerts: Warning if level is not valid.
            if (level != 100 && level != -1)
                txt += "Level: " + pku.Level + "\n";

            // Shiny
            // Default: None (false)
            // Alerts: None
            // Comments: unlike the other values, not cached since there's nothing to check in processAlerts()
            if (pku.Shiny)
                txt += "Shiny: Yes\n";

            // Friendship
            // Default: None (255)
            // Alerts: Warning if over/under 255/0
            if (friendship != 255 && friendship != -1)
                txt += "Happiness: " + friendship + "\n";

            // IVs
            // Default: None
            // Alerts: Warning if over/under 31/0 or unspecified
            // TODO: USE BATTLE IVS
            string ivstring = "";
            if (ivs[0].HasValue)// && ivs[0] != 31)
                ivstring += ivs[0] + " Atk / ";
            if (ivs[1].HasValue)// && ivs[1] != 31)
                ivstring += ivs[1] + " Def / ";
            if (ivs[2].HasValue)// && ivs[2] != 31)
                ivstring += ivs[2] + " SpA / ";
            if (ivs[3].HasValue)// && ivs[3] != 31)
                ivstring += ivs[3] + " SpD / ";
            if (ivs[4].HasValue)// && ivs[4] != 31)
                ivstring += ivs[4] + " Spd / ";
            if (ivstring != "")
            {
                ivstring = ivstring.Substring(0, ivstring.Length - 3); //remove last " /" chars
                txt += "IVs: " + ivstring + "\n";
            }

            // EVs
            // Default: None
            // Alerts: Warning if over/under 255/0 or unspecified
            string evstring = "";
            if (evs[0].HasValue && evs[0] != 0)
                evstring += evs[0] + " Atk / ";
            if (evs[1].HasValue && evs[1] != 0)
                evstring += evs[1] + " Def / ";
            if (evs[2].HasValue && evs[2] != 0)
                evstring += evs[2] + " SpA / ";
            if (evs[3].HasValue && evs[3] != 0)
                evstring += evs[3] + " SpD / ";
            if (evs[4].HasValue && evs[4] != 0)
                evstring += evs[4] + " Spd / ";
            if (evstring != "")
            {
                evstring = evstring.Substring(0, evstring.Length - 3); //remove last " /" chars
                txt += "EVs: " + evstring + "\n";
            }

            // Nature
            // Default: None (Serious)
            // Alerts: Warning if invalid nature.
            if (nature != "None")
                txt += pku.Nature + " Nature\n";

            // Gigantamax
            // Default: None (false)
            // Alerts: None
            // Comments: not cached, nothing to check. Showdown ignores illegal gigantamax factors.
            if (pku.Gigantamax_Factor)
                txt += "Gigantamax: Yes";

            // Moves
            // Default: None
            // Alerts: When all are unspecified, or when any are invalid.
            if (pku.Moves != null)
            {
                if (moves[0] != null)
                    txt += "- " + moves[0] + "\n";
                if (moves[1] != null)
                    txt += "- " + moves[1] + "\n";
                if (moves[2] != null)
                    txt += "- " + moves[2] + "\n";
                if (moves[3] != null)
                    txt += "- " + moves[3] + "\n";
            }

            // PP Ups not used in showdown, by default always max pp
            // Found one thread regarding it but nothing seems to have come of it:
            // https://www.smogon.com/forums/threads/allow-moves-to-have-non-max-pp.3653621/

            return Encoding.UTF8.GetBytes(txt);
        }
    }
}
