using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public static class DateTagUtil
{
    /* ------------------------------------
     * Common Export Functionality
     * ------------------------------------
    */
    private const string defaultDate = "2000-01-01";

    private static (pkuTime, AlertType) ProcessPkuTime(IField<string> pkuVal)
    {
        AlertType at = AlertType.NONE;
        pkuTime timeToEncode;
        pkuTime? dtzp = null;

        //get alert type
        if (pkuVal.IsNull())
            at = AlertType.UNSPECIFIED;
        else //Validate date
        {
            dtzp = pkuTime.Parse(pkuVal.Value);
            if (!dtzp.HasValue) //invalid
                at = AlertType.INVALID;
            else if (!dtzp.Value.Date.HasValue) //time only
                at = AlertType.UNDERSPECIFIED | AlertType.MODIFIER_A;
        }

        //calculate processed pkuTime
        if (at is AlertType.NONE) //valid
            timeToEncode = dtzp.Value;
        else //unspecified, invalid, time only
        {
            string temp = defaultDate;
            if (at.HasFlag(AlertType.UNDERSPECIFIED | AlertType.MODIFIER_A))
                temp += dtzp.Value.TimeToString();
            timeToEncode = pkuTime.Parse(temp).Value; //should always work
        }

        return (timeToEncode, at);
    }

    private static AlertType ExportUnixTime(pkuTime pkuTime, IIntField formatField)
    {
        long val = pkuTime.ToUnixTime();
        AlertType at = AlertType.NONE;
        if (val < formatField.Min) //underflow
        {
            formatField.Value = formatField.Min.Value;
            at = AlertType.UNDERFLOW;
        }
        else if (val > formatField.Max) //overflow
        {
            formatField.Value = formatField.Max.Value;
            at = AlertType.OVERFLOW;
        }
        else //valid
            formatField.Value = val;

        return at;
    }

    private static AlertType ExportDateOnly(pkuTime pkuTime, IIntField formatY, IIntField formatM, IIntField formatD)
    {
        //Assuming formatM can hold 1-12 and formatD can hold 0-31
        AlertType at = AlertType.NONE;
        if (pkuTime.Date.Value.Year < formatY.Min) //underflow
        {
            formatY.Value = formatY.Min.Value;
            formatM.Value = 1;
            formatD.Value = 1;
            at = AlertType.UNDERFLOW;
        }
        else if (pkuTime.Date.Value.Year > formatY.Max) //overflow
        {
            formatY.Value = formatY.Max.Value;
            formatM.Value = 12;
            formatD.Value = 31;
            at = AlertType.OVERFLOW;
        }
        else //valid
        {
            formatY.Value = pkuTime.Date.Value.Year;
            formatM.Value = pkuTime.Date.Value.Month;
            formatD.Value = pkuTime.Date.Value.Day;
        }
        return at;
    }


    /* ------------------------------------
     * Exporting
     * ------------------------------------
    */
    public static Alert ExportDate(string tagName, IField<string> pkuTimeField,
        OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> formatVal)
    {
        (pkuTime preparedTime, AlertType at) = ProcessPkuTime(pkuTimeField);
        formatVal.Switch(
            dateField => at |= ExportDateOnly(preparedTime, dateField.Y, dateField.M, dateField.D),
            unixField => at |= ExportUnixTime(preparedTime, unixField)
        );

        return GetDateAlert(tagName, at);
    }


    /* ------------------------------------
     * Alerting
     * ------------------------------------
    */
    public static Alert GetDateAlert(string tagName, AlertType at)
    {
        if (at is AlertType.NONE)
            return null;

        string msg = $"{tagName} ";
        if (at.HasFlag(AlertType.UNSPECIFIED))
            msg += $"unspecified, setting to {defaultDate}.";
        else if (at.HasFlag(AlertType.INVALID))
            msg += $"invalid, setting to {defaultDate}.";
        else if (at.HasFlag(AlertType.UNDERSPECIFIED))
        {
            if (at.HasFlag(AlertType.MODIFIER_A)) //TimeOnly (doesn't mention no offset)
                msg += $"only specifies a time, setting the date to {defaultDate}.";
            else if(at.HasFlag(AlertType.MODIFIER_B)) //No Offset
                msg += $"does not specify an offset, assuming UTC (+00:00).";
        }
        else if (at.HasFlag(AlertType.OVERFLOW))
            msg += $"too far in the future, setting date to highest possible.";
        else if (at.HasFlag(AlertType.UNDERFLOW))
            msg += $"too far in the past, setting date to lowest possible.";
        else
            throw InvalidAlertType(at);
        
        return new(tagName, msg);
    }
}
