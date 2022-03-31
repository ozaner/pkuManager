using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Encoded_OT_O
{
    public BAMStringField OT { get; }
    public IField<bool> OT_Flag => null; //no nickname flag by default
}

public interface Encoded_OT_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }
    public string FormatName { get; }

    public Encoded_OT_O OT_Field { get; }
    public Language_O Language_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ProcessLanguage))]
    public void ProcessOT() => ProcessOTBase();

    public void ProcessOTBase()
    {
        BigInteger[] otName;
        AlertType at = AlertType.NONE;
        if (!pku.Game_Info.OT.IsNull()) //OT specified
        {
            bool truncated, invalid;
            (otName, truncated, invalid) = DexUtil.CharEncoding.Encode(pku.Game_Info.OT.Value, OT_Field.OT.Length, FormatName, Language_Field.Value);
            if (truncated)
                at |= AlertType.TOO_LONG;
            if (invalid)
                at |= AlertType.INVALID;
        }
        else //OT not specified
        {
            (otName, _, _) = DexUtil.CharEncoding.Encode(null, OT_Field.OT.Length, FormatName, Language_Field.Value); //blank array
            at = AlertType.UNSPECIFIED;
        }
        OT_Field.OT.Value = otName;
        Warnings.Add(GetOTAlert(at, OT_Field.OT.LangDependent, OT_Field.OT.Length));
    }

    public static Alert GetOTAlert(AlertType at, bool langDep, int? maxChars = null)
    {
        if (at == AlertType.NONE)
            return null;

        string msg = "";
        if (at.HasFlag(AlertType.UNSPECIFIED))
            msg = "OT was not specified, leaving blank.";
        else
        {
            if (at.HasFlag(AlertType.INVALID)) //invalid characters, removing
                msg += $"Some of the characters in the OT are invalid in this format{(langDep ? "/language": "")}, removing them.";
            if (at.HasFlag(AlertType.TOO_LONG)) //too many characters, truncating
            {
                if (msg is not "")
                    msg += DataUtil.Newline(2);
                msg += $"OTs can only have {maxChars} characters in this format, truncating it.";
            }
        }
        return msg is not "" ? new("OT", msg) : throw InvalidAlertType();
    }
}