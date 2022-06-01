using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System.Linq;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public static class NumericTagUtil
{
    /* ------------------------------------
     * Common Export Functionality
     * ------------------------------------
    */
    private static (BigInteger checkedVal, AlertType at)
        CheckNumericField(IField<BigInteger?> pkuVal, IBoundable bounds, BigInteger defaultVal)
    {
        //round value & get alerttype
        return pkuVal.Value switch
        {
            null => (defaultVal, AlertType.UNSPECIFIED),
            var x when x > bounds?.Max => (bounds.Max.Value, AlertType.OVERFLOW),
            var x when x < bounds?.Min => (bounds.Min.Value, AlertType.UNDERFLOW),
            _ => (pkuVal.Value.Value, AlertType.NONE)
        };
    }


    /* ------------------------------------
     * Exporting
     * ------------------------------------
    */
    public static AlertType ExportNumericTag(IField<BigInteger?> pkuVal, IIntField formatVal, BigInteger defaultVal)
    {
        (BigInteger checkedVal, AlertType at) = CheckNumericField(pkuVal, formatVal, defaultVal);
        formatVal.Value = checkedVal;
        return at;
    }

    public static AlertType[] ExportMultiNumericTag(IField<BigInteger?>[] pkuVals, IIntField[] formatVals, BigInteger[] defaultVals)
    {
        AlertType[] ats = new AlertType[pkuVals.Length];
        for (int i = 0; i < pkuVals.Length; i++)
            (formatVals[i].Value, ats[i]) = CheckNumericField(pkuVals[i], formatVals[i], defaultVals[i]);
        return ats;
    }

    public static AlertType[] ExportNumericArrayTag(IField<BigInteger?>[] pkuVals, IIntArrayField formatVals, BigInteger defaultVal)
    {
        AlertType[] ats = new AlertType[formatVals.Value.Length];
        for (int i = 0; i < ats.Length; i++)
        {
            if (i < pkuVals.Length)
            {
                (BigInteger checkedVal, ats[i]) = CheckNumericField(pkuVals[i], formatVals, defaultVal);
                formatVals.SetAs(checkedVal, i);
            }
            else
            {
                ats[i] = AlertType.UNSPECIFIED; //no pku field given
                formatVals.SetAs(defaultVal, i);
            }
        }
        return ats;
    }


    /* ------------------------------------
     * Importing
     * ------------------------------------
    */
    public static void ImportNumericTag(IField<BigInteger?> pkuVal, IIntField formatVal)
        => pkuVal.Value = formatVal.Value; //can't be invalid, all ints fit in pku fields


    /* ------------------------------------
     * Alerting
     * ------------------------------------
    */
    public static Alert GetNumericAlert(string name, AlertType at, BigInteger defaultVal, BigInteger? max, BigInteger? min) => at switch
    {
        AlertType.OVERFLOW => new(name, $"This pku's {name} is higher than the maximum. Rounding down to {max}."),
        AlertType.UNDERFLOW => new(name, $"This pku's {name} is lower than the minimum. Rounding up to {min}."),
        AlertType.UNSPECIFIED => new(name, $"{name} tag not specified, using the default of {defaultVal}."),
        AlertType.NONE => null,
        _ => throw InvalidAlertType(at)
    };

    public static Alert GetNumericAlert(string name, AlertType at, BigInteger defaultVal, IBoundable bounds = null)
        => GetNumericAlert(name, at, defaultVal, bounds?.Max, bounds?.Min);

    public static Alert GetMultiNumericAlert(string tagName, string[] subTagNames, AlertType[] ats,
        BigInteger[] defaultVals, IBoundable[] bounds, bool ignoreUnspecified)
    {
        Alert a = null;
        for (int i = 0; i < subTagNames.Length; i++)
        {
            if (!ignoreUnspecified || ats[i] is not AlertType.UNSPECIFIED)
                a += GetNumericAlert(subTagNames[i], ats[i], defaultVals[i], bounds[i]);
        }
        if (a is not null)
            a.Title = tagName;
        return a;
    }

    public static Alert GetNumericArrayAlert(string tagName, string[] subtags, AlertType[] ats,
        IBoundable bounds, BigInteger defaultVal, bool ignoreUnspecified)
    {
        if (ats.All(x => x is AlertType.UNSPECIFIED))
            return !ignoreUnspecified ? new(tagName, $"No {tagName} were specified, setting them all to {defaultVal}.") : null;

        string msgOverflow = subtags.Where((_, id) => ats[id].HasFlag(AlertType.OVERFLOW)).ToArray().JoinGrammatical();
        string msgUnderflow = subtags.Where((_, id) => ats[id].HasFlag(AlertType.UNDERFLOW)).ToArray().JoinGrammatical();
        string msgUnspecifed = subtags.Where((_, id) => ats[id].HasFlag(AlertType.UNSPECIFIED)).ToArray().JoinGrammatical();

        Alert a = new(tagName, null);
        if (!msgOverflow.IsEmpty())
            a += new Alert(tagName, $"The {msgOverflow} {tagName} are too high. Rounding them down to {bounds.Max}.");
        if (!msgUnderflow.IsEmpty())
            a += new Alert(tagName, $"The {msgUnderflow} {tagName} are too low. Rounding them up to {bounds.Min}.");
        if (!msgUnspecifed.IsEmpty() && !ignoreUnspecified)
            a += new Alert(tagName, $"The {msgUnspecifed} {tagName} are unspecified. Setting them to {defaultVal}.");

        return a.Message?.Length > 0 ? a : null;
    }
}
