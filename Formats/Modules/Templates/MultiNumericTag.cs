using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface MultiNumericTag_E
{
    public List<Alert> Warnings { get; }

    protected void ProcessMultiNumericTag(string tagName, string[] subTagNames, IField<BigInteger?>[] pkuVals,
        IField<BigInteger[]> formatVals, BigInteger defaultVal, bool alertIfUnspecified)
    {
        AlertType[] valAlerts = new AlertType[pkuVals.Length];
        (BigInteger? max, BigInteger? min) = formatVals is IBoundable<BigInteger> boundable ?
            (boundable.Max, boundable.Min) : (null, null);
        for (int i = 0; i < pkuVals.Length; i++)
        {
            if (pkuVals[i].IsNull())
            {
                formatVals.SetAs(defaultVal, i);
                valAlerts[i] = AlertType.UNSPECIFIED;
            }
            else if (pkuVals[i].Value > max)
            {
                formatVals.SetAs(max.Value, i);
                valAlerts[i] = AlertType.OVERFLOW;
            }
            else if (pkuVals[i].Value < min)
            {
                formatVals.SetAs(min.Value, i);
                valAlerts[i] = AlertType.UNDERFLOW;
            }
            else
            {
                formatVals.SetAs(pkuVals[i].Value.Value, i);
                valAlerts[i] = AlertType.NONE;
            }
        }
        Warnings.Add(GetMultiNumericAlert(tagName, subTagNames, valAlerts, max, min, defaultVal, alertIfUnspecified));
    }

    protected static Alert GetMultiNumericAlert(string tagName, string[] subtags, AlertType[] ats,
        BigInteger? max, BigInteger? min, BigInteger defaultVal, bool alertIfUnspecified)
    {
        if (subtags?.Length != ats?.Length)
            throw new ArgumentException($"{nameof(subtags)} must have the same length as {nameof(ats)}.", nameof(subtags));
        else if (ats.All(x => x is AlertType.UNSPECIFIED))
            return alertIfUnspecified ? new(tagName, $"No {tagName} were specified, setting them all to {defaultVal}.") : null;

        string msgOverflow = subtags.Where((_, id) => ats[id].HasFlag(AlertType.OVERFLOW)).ToArray().JoinGrammatical();
        string msgUnderflow = subtags.Where((_, id) => ats[id].HasFlag(AlertType.UNDERFLOW)).ToArray().JoinGrammatical();
        string msgUnspecifed = subtags.Where((_, id) => ats[id].HasFlag(AlertType.UNSPECIFIED)).ToArray().JoinGrammatical();

        Alert a = new(tagName, null);
        if (!msgOverflow.IsEmpty())
            a += new Alert(tagName, $"The {msgOverflow} {tagName} are too high. Rounding them down to {max}.");
        if (!msgUnderflow.IsEmpty())
            a += new Alert(tagName, $"The {msgUnderflow} {tagName} are too low. Rounding them up to {min}.");
        if (!msgUnspecifed.IsEmpty() && alertIfUnspecified)
            a += new Alert(tagName, $"The {msgUnspecifed} {tagName} are unspecified. Setting them to {defaultVal}.");

        return a.Message?.Length > 0 ? a : null;
    }
}