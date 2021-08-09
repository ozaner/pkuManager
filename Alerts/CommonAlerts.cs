using pkuManager.Alerts;
using System;
using System.Linq;

namespace pkuManager.Exporters
{
    public static class CommonAlerts
    {
        // Generic Alert patterns below

        public static (AlertBox, string) getTextAlert(string tagName, string tagValue, Func<string, int?> checker, string defaultVal, bool alertUnspecified, string defaultMsg = null)
        {
            string msg = "";
            string newVal = tagValue;
            if (tagValue == null)
            {
                newVal = defaultVal; //Turn null into defaultValue regardless of whether we warn them about it
                if (alertUnspecified) // If value is unspecified AND we give warnings for that (i.e. unspecifiedWarning)
                    msg = tagName + " is unspecified. Using the default: " + defaultVal + ".";
            }
            else // non null value, check if its valid
            {
                if (checker(tagValue) == null) // If value is invalid
                    msg = "The " + tagName.ToLower() + " " + tagValue + " is invalid. Using the default: " + defaultVal + ".";
            }
                
            if (msg != "" && defaultMsg != null) // If there is a default message and a warning was determined.
                msg += "\r\n\r\n" + defaultMsg;

            // Return the alert and default val (warning), or null and the given tag value / default if it was null (no warning).
            return msg != "" ? (new AlertBox(tagName, msg), defaultVal) : (null, newVal);
        }

        public static (AlertBox, int) getNumericalAlert(string tagName, int? tagValue, int upper, int lower, int defaultVal, bool alertUnspecified)
        {
            string msg = "";
            if (tagValue.HasValue)
            {
                if (tagValue.Value < lower)
                {
                    msg = "The " + tagName.ToLower() + " value " + tagValue.Value + " is too low. Rounding up to the minimum value of " + lower + ".";
                    return (new AlertBox(tagName, msg), lower); // round up warning
                }
                else if (tagValue > upper)
                {
                    msg = "The " + tagName.ToLower() + " value " + tagValue.Value + " is too high. Rounding down to the maximum value of " + upper + ".";
                    return (new AlertBox(tagName, msg), upper); // round down warning
                }
            }
            else if (alertUnspecified)
            { 
                msg = tagName + " is unspecified. Using the default: " + defaultVal + ".";
                return (new AlertBox(tagName, msg), defaultVal); // unspecified warning
            }

            return (null, -1); // No warning msg
        }

        // Generic shared implementation of Alert for both IVs and EVs. Used below in getIVAlert and getEVAlert.
        private static (AlertBox, int?[]) getIVEVAlert(PKUObject pku, bool isIV, int? defaultVal, bool alertUnspecified, string defaultMsg)
        {
            //Get IVs/EVs
            string tagName = (isIV ? "IVs" : "EVs");
            string defaultName = (defaultVal.HasValue ? "" + defaultVal.Value : "None"); //If default is null, then write "None"
            int valMax = isIV ? 31 : 255;
            int? hp = isIV ? pku.IVs?.HP : pku.EVs?.HP;
            int? atk = isIV ? pku.IVs?.Attack : pku.EVs?.Attack;
            int? def = isIV ? pku.IVs?.Defense : pku.EVs?.Defense;
            int? spa = isIV ? pku.IVs?.Sp_Attack : pku.EVs?.Sp_Attack;
            int? spd = isIV ? pku.IVs?.Sp_Defense : pku.EVs?.Sp_Defense;
            int? spe = isIV ? pku.IVs?.Speed : pku.EVs?.Speed;

            // IVs Warning
            string msg = "";
            if (alertUnspecified && !atk.HasValue && !def.HasValue && !spa.HasValue && !spd.HasValue && !spe.HasValue)
                msg = "No " + tagName + " specified. Setting all " + tagName + " to the default value: " + defaultName + ".";
            else
            {
                string ivsTooHigh = "";
                string ivsTooLow = "";
                string ivsDNE = "";
                if (hp.HasValue)
                {
                    if (hp > valMax)
                    {
                        ivsTooHigh += "HP/";
                        hp = valMax;
                    }
                    else if (hp < 0)
                    {
                        ivsTooLow += "HP/";
                        hp = 0;
                    }
                }
                else
                    ivsDNE += "HP/";
                if (atk.HasValue)
                {
                    if (atk > valMax)
                    {
                        ivsTooHigh += "Attack/";
                        atk = valMax;
                    }
                    else if (atk < 0)
                    {
                        ivsTooLow += "Attack/";
                        atk = 0;
                    }
                }
                else
                    ivsDNE += "Attack/";
                if (def.HasValue)
                {
                    if (def > valMax)
                    {
                        ivsTooHigh += "Defense/";
                        def = valMax;
                    }
                    else if (def < 0)
                    {
                        ivsTooLow += "Defense/";
                        def = 0;
                    }
                }
                else
                    ivsDNE += "Defense/";
                if (spa.HasValue)
                {
                    if (spa > valMax)
                    {
                        ivsTooHigh += "Sp. Attack/";
                        spa = valMax;
                    }
                    else if (spa < 0)
                    {
                        ivsTooLow += "Sp. Attack/";
                        spa = 0;
                    }
                }
                else
                    ivsDNE += "Sp. Attack/";
                if (spd.HasValue)
                {
                    if (spd > valMax)
                    {
                        ivsTooHigh += "Sp. Defense/";
                        spd = valMax;
                    }
                    else if (spd < 0)
                    {
                        ivsTooLow += "Sp. Defense/";
                        spd = 0;
                    }
                }
                else
                    ivsDNE += "Sp. Defense/";
                if (spe.HasValue)
                {
                    if (spe > valMax)
                    {
                        ivsTooHigh += "Speed/";
                        spe = valMax;
                    }
                    else if (spd < 0)
                    {
                        ivsTooLow += "Speed/";
                        spe = 0;
                    }
                }
                else
                    ivsDNE += "Speed/";

                // For grammar, get # of IVs/EVs that:
                int high = ivsTooHigh.Split('/').Length - 1; //overflowed
                int low = ivsTooLow.Split('/').Length - 1; //underflowed
                int dne = ivsDNE.Split('/').Length - 1; //weren't specified

                if (high > 1)
                    msg += "The " + ivsTooHigh.Substring(0, ivsTooHigh.Length - 1) + " IVs are too high. Rounding them down to " + valMax + ".\r\n\r\n";
                else if (high == 1)
                    msg += "The " + ivsTooHigh.Substring(0, ivsTooHigh.Length - 1) + " IV is too high. Rounding it down to " + valMax + ".\r\n\r\n";

                if (low > 1)
                    msg += "The " + ivsTooLow.Substring(0, ivsTooLow.Length - 1) + " IVs are too low. Rounding them up to 0.\r\n\r\n";
                else if (low == 1)
                    msg += "The " + ivsTooLow.Substring(0, ivsTooLow.Length - 1) + " IV is too low. Rounding it up to 0.\r\n\r\n";

                if (alertUnspecified)
                {
                    if (dne > 1)
                        msg += "The " + ivsDNE.Substring(0, ivsDNE.Length - 1) + " IVs were not specified. Giving them the default value 0.\r\n\r\n";
                    else if (dne == 1)
                        msg += "The " + ivsDNE.Substring(0, ivsDNE.Length - 1) + " IV was not specified. Giving it the default value 0.\r\n\r\n";
                }

                if (msg != "")
                    msg = msg.Substring(0, msg.Length - 4); //Remove extra \r\n\r\n

                if (msg != "" && defaultMsg != null) //default message appended when an alert is raised
                    msg += "\r\n\r\n" + defaultMsg;
            }

            int?[] vals = { hp, atk, def, spa, spd, spe };
            return msg == "" ? (null, vals) : (new AlertBox(tagName, msg), vals);
        }



        // Specific Alert Implementations Below

        public static (AlertBox, string, Gender?) getGenderAlert(PKUObject pku, string defaultVal, bool alertUnspecified=true, string defaultMessage=null)
        {
            (AlertBox ab, string gen) = getTextAlert("Gender", pku.Gender, (x) =>
            {
                return (int?)pkCommons.GetGenderID(x, false);
            }, defaultVal, alertUnspecified, defaultMessage);
            return (ab, gen, pkCommons.GetGenderID(gen, false));
        }

        public static (AlertBox, int) getFriendshipAlert(bool OT, PKUObject pku, int defaultVal = 0, bool alertUnspecified=true)
        {
            string tagName = OT ? "OT Friendship" : "HT Friendship";
            int? friendship = OT ? pku.OT_Friendship : pku.HT_Friendship;
            return getNumericalAlert(tagName, friendship, 255, 0, defaultVal, alertUnspecified);
        }

        public static (AlertBox, string, int?) getAbilityAlert(PKUObject pku, bool alertUnspecified = true, string defaultMessage=null)
        {
            (AlertBox ab, string abil) = getTextAlert("Ability", pku.Ability, pkCommons.GetAbilityID, "None", alertUnspecified, defaultMessage);
            return (ab, abil, pkCommons.GetAbilityID(abil));
        }

        public static (AlertBox, string, int?) getNatureAlert(PKUObject pku, string defaultVal = "Hardy", bool alertUnspecified = true, string defaultMessage = null)
        {
            (AlertBox ab, string nat) = getTextAlert("Nature", pku.Nature, pkCommons.GetNatureID, defaultVal, alertUnspecified, defaultMessage);
            return (ab, nat, pkCommons.GetNatureID(nat));
        }

        public static (AlertBox, int?[]) getIVAlert(PKUObject pku, int? defaultVal = 0, bool alertUnspecified = true, string defaultMsg = null)
        {
            return getIVEVAlert(pku, true, defaultVal, alertUnspecified, defaultMsg);
        }

        public static (AlertBox, int?[]) getEVAlert(PKUObject pku, int? defaultVal = 0, bool alertUnspecified = true, string defaultMsg = null)
        {
            return getIVEVAlert(pku, false, defaultVal, alertUnspecified, defaultMsg);
        }

        public static (AlertBox, string[], int?[]) getMoveAlert(PKUObject pku, string[] moves, Func<string, int?> checker, bool alertUnspecified)
        {
            int?[] moveIDs = new int?[4];

            if (moves == null || moves.All(x => x == null)) //check for length 0?
                return (new AlertBox("Moves", "No moves specified, leaving them all empty."), new string[4], new int?[4]);

            string msg = "";
            string invalidMoves = "";
            for (int i = 0; i < 4; i++)
            {
                if (moves[i] == null)
                    continue; //ignore empty moves

                int? moveID = checker(moves[i]);
                moveIDs[i] = moveID;
                if (moveID == null) //If ith move is invalid
                {
                    invalidMoves += moves[i] + ", ";
                    moves[i] = null;
                }
            }

            if (invalidMoves.Length > 0)
                invalidMoves = invalidMoves.Substring(0, invalidMoves.Length - 2);

            msg = "The move(s) " + invalidMoves + " are not valid in this format. They will not be included.";

            if (moves.All(x => x == null))
                msg += "\r\n\r\nNo valid moves were given, leaving them all empty.";

            if (msg != "")
                return (new AlertBox("Moves", msg), moves, moveIDs);

            return (null, moves, moveIDs);
        }
    }
}
