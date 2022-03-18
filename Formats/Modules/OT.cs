using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Encoded_OT_O
{
    public BAMStringField OT { get; }
    public virtual IField<bool> OT_Flag => null; //no nickname flag by default
}

public interface Encoded_OT_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }
    public string FormatName { get; }

    public Encoded_OT_O OT_Field { get; }
    public Language_O Language_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ProcessLanguage))]
    public virtual void ProcessOT() => ProcessOTBase();

    public void ProcessOTBase()
    {
        BigInteger[] otName;
        Alert alert = null;

        if (pku.Game_Info?.OT is not null) //OT specified
        {
            bool truncated, invalid;
            (otName, truncated, invalid) = DexUtil.CharEncoding.Encode(pku.Game_Info.OT, OT_Field.OT.Length, FormatName, Language_Field.Value);
            if (truncated && invalid)
                alert = GetOTAlert(OT_Field.OT.Length, AlertType.OVERFLOW, AlertType.INVALID);
            else if (truncated)
                alert = GetOTAlert(OT_Field.OT.Length, AlertType.OVERFLOW);
            else if (invalid)
                alert = GetOTAlert(AlertType.INVALID);
        }
        else //OT not specified
        {
            (otName, _, _) = DexUtil.CharEncoding.Encode(null, OT_Field.OT.Length, FormatName, Language_Field.Value); //blank array
            alert = GetOTAlert(AlertType.UNSPECIFIED);
        }
        OT_Field.OT.Value = otName;
        Warnings.Add(alert);
    }

    public static Alert GetOTAlert(int maxChars, params AlertType[] ats)
    {
        string msg = "";
        if (ats.Contains(AlertType.UNSPECIFIED))
            msg = "OT was not specified, leaving blank.";
        else
        {
            if (ats.Contains(AlertType.INVALID)) //invalid characters, removing
                msg += $"Some of the characters in the OT are invalid in this format, removing them.";
            if (ats.Contains(AlertType.OVERFLOW)) //too many characters, truncating
            {
                if (msg is not "")
                    msg += DataUtil.Newline(2);
                msg += $"OTs can only have {maxChars} characters in this format, truncating it.";
            }
        }
        return msg is not "" ? new("OT", msg) : throw InvalidAlertType();
    }

    public static Alert GetOTAlert(params AlertType[] ats)
        => ats.Contains(AlertType.OVERFLOW) ?
           throw new ArgumentNullException("maxChars", "Overflow OT Alerts must include the character limit.") : GetOTAlert(-1, ats);
}