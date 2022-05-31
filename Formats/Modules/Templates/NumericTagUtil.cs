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
        CheckNumericField(IField<BigInteger?> pkuVal, IBoundable<BigInteger> bounds, BigInteger defaultVal)
    {
        //Get bounds, if any
        (BigInteger? max, BigInteger? min) = bounds is null ? (null, null) : (bounds.Max, bounds.Min);

        //round value & get alerttype
        return pkuVal.Value switch
        {
            null => (defaultVal, AlertType.UNSPECIFIED),
            var x when x > max => (max.Value, AlertType.OVERFLOW),
            var x when x < min => (min.Value, AlertType.UNDERFLOW),
            _ => (pkuVal.Value.Value, AlertType.NONE)
        };
    }


    /* ------------------------------------
     * Exporting
     * ------------------------------------
    */
    public static AlertType ExportNumericTag(IField<BigInteger?> pkuVal, IField<BigInteger> formatVal, BigInteger defaultVal)
    {
        (BigInteger checkedVal, AlertType at) = CheckNumericField(pkuVal, formatVal as IBoundable<BigInteger>, defaultVal);
        formatVal.Value = checkedVal;
        return at;
    }

    public static AlertType[] ExportMultiNumericTag(IField<BigInteger?>[] pkuVals, IField<BigInteger>[] formatVals, BigInteger[] defaultVals)
    {
        AlertType[] ats = new AlertType[pkuVals.Length];
        for (int i = 0; i < pkuVals.Length; i++)
            (formatVals[i].Value, ats[i]) = CheckNumericField(pkuVals[i], formatVals[i] as IBoundable<BigInteger>, defaultVals[i]);
        return ats;
    }

    public static AlertType[] ExportNumericArrayTag(IField<BigInteger?>[] pkuVals, IField<BigInteger[]> formatVals, BigInteger defaultVal)
    {
        AlertType[] ats = new AlertType[formatVals.Value.Length];
        for (int i = 0; i < ats.Length; i++)
        {
            if (i < pkuVals.Length)
            {
                (BigInteger checkedVal, ats[i]) = CheckNumericField(pkuVals[i], formatVals as IBoundable<BigInteger>, defaultVal);
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
    public static void ImportNumericTag(IField<BigInteger?> pkuVal, IField<BigInteger> formatVal)
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

    public static Alert GetNumericAlert(string name, AlertType at, BigInteger defaultVal, IBoundable<BigInteger> boundable = null)
        => GetNumericAlert(name, at, defaultVal, boundable?.Max, boundable?.Min);

    public static Alert GetMultiNumericAlert(string tagName, string[] subTagNames, AlertType[] ats,
        BigInteger[] defaultVals, BigInteger?[] maxes, BigInteger?[] mins, bool ignoreUnspecified)
    {
        Alert a = null;
        for (int i = 0; i < subTagNames.Length; i++)
        {
            if (!ignoreUnspecified || ats[i] is not AlertType.UNSPECIFIED)
                a += GetNumericAlert(subTagNames[i], ats[i], defaultVals[i], maxes[i], mins[i]);
        }
        if (a is not null)
            a.Title = tagName;
        return a;
    }

    public static Alert GetMultiNumericAlert(string tagName, string[] subTagNames, AlertType[] ats,
        BigInteger[] defaultVals, IBoundable<BigInteger>[] boundables, bool ignoreUnspecified)
    {
        BigInteger?[] maxes = new BigInteger?[boundables.Length];
        BigInteger?[] mins = new BigInteger?[boundables.Length];
        for (int i = 0; i < boundables.Length; i++)
        {
            maxes[i] = boundables[i]?.Max;
            mins[i] = boundables[i]?.Min;
        }
        return GetMultiNumericAlert(tagName, subTagNames, ats, defaultVals, maxes, mins, ignoreUnspecified);
    }

    public static Alert GetNumericArrayAlert(string tagName, string[] subtags, AlertType[] ats,
        BigInteger? max, BigInteger? min, BigInteger defaultVal, bool ignoreUnspecified)
    {
        if (ats.All(x => x is AlertType.UNSPECIFIED))
            return !ignoreUnspecified ? new(tagName, $"No {tagName} were specified, setting them all to {defaultVal}.") : null;

        string msgOverflow = subtags.Where((_, id) => ats[id].HasFlag(AlertType.OVERFLOW)).ToArray().JoinGrammatical();
        string msgUnderflow = subtags.Where((_, id) => ats[id].HasFlag(AlertType.UNDERFLOW)).ToArray().JoinGrammatical();
        string msgUnspecifed = subtags.Where((_, id) => ats[id].HasFlag(AlertType.UNSPECIFIED)).ToArray().JoinGrammatical();

        Alert a = new(tagName, null);
        if (!msgOverflow.IsEmpty())
            a += new Alert(tagName, $"The {msgOverflow} {tagName} are too high. Rounding them down to {max}.");
        if (!msgUnderflow.IsEmpty())
            a += new Alert(tagName, $"The {msgUnderflow} {tagName} are too low. Rounding them up to {min}.");
        if (!msgUnspecifed.IsEmpty() && !ignoreUnspecified)
            a += new Alert(tagName, $"The {msgUnspecifed} {tagName} are unspecified. Setting them to {defaultVal}.");

        return a.Message?.Length > 0 ? a : null;
    }

    public static Alert GetNumericArrayAlert(string tagName, string[] subtags, AlertType[] ats,
        IBoundable<BigInteger> boundable, BigInteger defaultVal, bool ignoreUnspecified)
        => GetNumericArrayAlert(tagName, subtags, ats, boundable?.Max, boundable?.Min, defaultVal, ignoreUnspecified);
}
