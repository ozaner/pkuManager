using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Trash_Bytes_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }
    public List<Alert> Warnings { get; }

    public Nickname_O Nickname_Field { get; }
    public Encoded_OT_O OT_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Nickname_E.ProcessNickname),
                                                nameof(Encoded_OT_E.ProcessOT))]
    public void ProcessTrash_Bytes()
    {
        BAMStringField encodedNickname = Nickname_Field.Nickname.AsT0;

        AlertType atName = ProcessTrash_BytesSingle(encodedNickname, pku.Trash_Bytes.Nickname);
        AlertType atOT = ProcessTrash_BytesSingle(OT_Field.OT, pku.Trash_Bytes.OT);

        Alert alert = GetTrashAlert(atName, "Nickname") + GetTrashAlert(atOT, "OT");
        Warnings.Add(alert);
    }

    protected AlertType ProcessTrash_BytesSingle<T>(T encodedField, IField<BigInteger[]> trashField)
        where T : IField<BigInteger[]>, IBoundable<BigInteger>
    {
        if (!trashField.IsNull())
        {
            AlertType at = AlertType.NONE;
            if (trashField.Value.Any(x => x > encodedField.Max) == true) //some entries too large
                at |= AlertType.OVERFLOW;
            if (trashField.Value.Any(x => x < encodedField.Min) == true) //some entries too small
                at |= AlertType.UNDERFLOW;
            if (trashField.Value.Length != encodedField.Value.Length)
                at |= AlertType.TOO_LONG;

            BigInteger[] trashedStr = new BigInteger[encodedField.Value.Length];
            BigInteger terminator = DexUtil.CharEncoding.GetTerminator(FormatName);
            bool terminatorFound = false;
            for (int i = 0; i < encodedField.Value.Length; i++)
            {
                if (terminatorFound && trashField.Value.Length > i)
                {
                    if (trashField.Value[i] > encodedField.Max)
                        trashedStr[i] = encodedField.Max.Value;
                    else if (trashField.Value[i] > encodedField.Max)
                        trashedStr[i] = encodedField.Min.Value;
                    else
                        trashedStr[i] = trashField.Value[i];
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
        if (at.HasFlag(AlertType.OVERFLOW) || at.HasFlag(AlertType.UNDERFLOW))
            msg += $"One or more of the entries in {stringName} Trash Bytes are too high/low, rounding them up/down.";
        if (at.HasFlag(AlertType.TOO_LONG))
            msg += (msg != "" ? DataUtil.Newline(2) : "") + $"There are too many entries in the {stringName} Trash Bytes. Ignoring the extra ones.";

        return new("Trash Bytes", msg);
    }
}