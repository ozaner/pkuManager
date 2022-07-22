using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Fields.BAMFields;
using pkuManager.WinForms.Formats.Modules.MetaTags;
using pkuManager.WinForms.Utilities;
using System.Linq;
using System.Numerics;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Trash_Bytes_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Nickname_E.ExportNickname),
                                                nameof(OT_E.ExportOT))]
    public void ExportTrash_Bytes()
    {
        BAMStringField encodedNickname = (Data as Nickname_O).Nickname.AsT0;
        BAMStringField encodedOT = (Data as OT_O).OT.AsT0;

        (BigInteger[] nicknameTrash, bool nickInvalid) = FormatSpecificUtil.GetValue<BigInteger[]>(pku, FormatName, "Trash Bytes", "Nickname");
        (BigInteger[] otTrash, bool otInvalid) = FormatSpecificUtil.GetValue<BigInteger[]>(pku, FormatName, "Trash Bytes", "OT");

        AlertType atName = nickInvalid ? AlertType.INVALID : ExportTrash_BytesSingle(encodedNickname, nicknameTrash);
        AlertType atOT = otInvalid ? AlertType.INVALID : ExportTrash_BytesSingle(encodedOT, otTrash);

        Alert alert = GetTrashAlert(atName, "Nickname") + GetTrashAlert(atOT, "OT");
        Warnings.Add(alert);
    }

    protected AlertType ExportTrash_BytesSingle(IIntArrayField encodedField, BigInteger[] trashField)
    {
        if (trashField is not null)
        {
            AlertType at = AlertType.NONE;
            if (trashField.Any(x => x > encodedField.Max) == true) //some entries too large
                at |= AlertType.OVERFLOW;
            if (trashField.Any(x => x < encodedField.Min) == true) //some entries too small
                at |= AlertType.UNDERFLOW;
            if (trashField.Length != encodedField.Value.Length)
                at |= AlertType.TOO_LONG;

            BigInteger[] trashedStr = new BigInteger[encodedField.Value.Length];
            BigInteger terminator = DDM.FormatDex.GetTerminator(FormatName);
            bool terminatorFound = false;
            for (int i = 0; i < encodedField.Value.Length; i++)
            {
                if (terminatorFound && trashField.Length > i)
                {
                    if (trashField[i] > encodedField.Max)
                        trashedStr[i] = encodedField.Max.Value;
                    else if (trashField[i] > encodedField.Max)
                        trashedStr[i] = encodedField.Min.Value;
                    else
                        trashedStr[i] = trashField[i];
                }
                else
                    trashedStr[i] = encodedField.Value[i];
                terminatorFound |= encodedField.Value[i] == terminator;
            }
            encodedField.Value = trashedStr;
            return at;
        }
        return AlertType.NONE; //no trash
    }

    protected static Alert GetTrashAlert(AlertType at, string stringName)
    {
        if (at is AlertType.NONE)
            return null;

        string msg = "";
        if (at.HasFlag(AlertType.INVALID))
            msg += $"The {stringName} trash bytes are invalid, ignoring them.";
        if (at.HasFlag(AlertType.OVERFLOW) || at.HasFlag(AlertType.UNDERFLOW))
            msg += $"One or more of the entries in {stringName} Trash Bytes are too high/low, rounding them up/down.";
        if (at.HasFlag(AlertType.TOO_LONG))
            msg += (msg != "" ? DataUtil.Newline(2) : "") + $"There are too many entries in the {stringName} Trash Bytes. Ignoring the extra ones.";

        return new("Trash Bytes", msg);
    }
}