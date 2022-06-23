using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public static class DateTagUtil
{
    /* ------------------------------------
     * Common Export Functionality
     * ------------------------------------
    */
    private const string defaultDate = "2000-01-01";

    public static (pkuTime, AlertType) ProcessPkuTime(IField<string> pkuVal)
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
                at = AlertType.UNDERSPECIFIED;
        }

        //calculate processed pkuTime
        if (at is AlertType.NONE) //valid
            timeToEncode = dtzp.Value;
        else //unspecified, invalid, time only
        {
            string temp = defaultDate;
            if (at is AlertType.UNDERSPECIFIED)
                temp += dtzp.Value.TimeToString();
            timeToEncode = pkuTime.Parse(temp).Value; //should always work
        }

        return (timeToEncode, at);
    }

    public static (string, AlertType) ProcessTimeZone(IField<string> timezone)
    {
        if (timezone.IsNull()) //unspecified
            return (null, AlertType.UNSPECIFIED);
        else
        {
            try //valid
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone.Value);
                if (tz.HasIanaId)
                    return (tz.Id, AlertType.NONE);
                else
                    throw new Exception();
            }
            catch //invalid
            {
                return (null, AlertType.INVALID);
            }
        }
    }


    /* ------------------------------------
     * Exporting
     * ------------------------------------
    */
    public static Alert ExportAnyDate(string tagName, IField<string> pkuTimeField, IField<string> pkuTZField,
        OneOf<(IIntField Y, IIntField M, IIntField D), IIntField> formatVal)
    {
        (pkuTime preparedTime, AlertType atD) = ProcessPkuTime(pkuTimeField);
        AlertType atTZ = AlertType.NONE;

        formatVal.Switch(
            dateField => atD |= ExportDate(preparedTime, dateField.Y, dateField.M, dateField.D), //Y-M-D formats
            unixField => { //Unix formats
                (string timezone, atTZ) = ProcessTimeZone(pkuTZField);
                atD |= ExportUnixTime(preparedTime, timezone, unixField);
            }
        );

        Alert a = GetDateAlert(tagName, atD);
        a += GetTimeZoneAlert(tagName, atTZ);
        return a;
    }

    public static AlertType ExportUnixTime(pkuTime pkuTime, string timezone, IIntField formatField)
    {
        long val = pkuTime.ToUnixTime(timezone);
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

    public static AlertType ExportDate(pkuTime pkuTime, IIntField formatY, IIntField formatM, IIntField formatD)
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
        else if (at.HasFlag(AlertType.TOO_SHORT))
            msg += $"only specifies a time, setting the date to {defaultDate}.";
        else if (at.HasFlag(AlertType.OVERFLOW))
            msg += $"too far in the future, setting date to highest possible.";
        else if (at.HasFlag(AlertType.UNDERFLOW))
            msg += $"too far in the past, setting date to lowest possible.";
        else
            throw InvalidAlertType(at);
        
        return new(tagName, msg);
    }

    public static Alert GetTimeZoneAlert(string tagName, AlertType at)
    {
        if (at is AlertType.NONE)
            return null;

        string defaultVal = pkuTime.LOCAL_TIMEZONE is null ? "UTC" : $"the local timezone: {pkuTime.LOCAL_TIMEZONE}";
        string msg = at switch
        {
            AlertType.UNSPECIFIED => $"No timezone specified for {tagName}",
            AlertType.INVALID => $"The timezone for {tagName} was invalid",
            _ => throw InvalidAlertType(at)
        } + $", using {defaultVal}";

        return new(tagName, msg);
    }
}
