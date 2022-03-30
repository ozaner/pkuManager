using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface RelatedNumericTag_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }

    protected void ProcessRelatedNumericTag(string tagName, string[] subTagNames, IField<BigInteger?>[] pkuVals,
        IField<BigInteger>[] formatVals, BigInteger[] defaultVals, bool alertIfUnspecified)
    {
        Alert a = null;
        for (int i = 0; i < pkuVals.Length; i++)
        {
            (BigInteger? max, BigInteger? min) = formatVals[i] is IBoundable<BigInteger> boundable ?
                (boundable.Max, boundable.Min) : (null, null);
            (BigInteger checkedVal, AlertType at) = pkuVals[i].Value switch
            {
                null => (defaultVals[i], AlertType.UNSPECIFIED),
                var x when x > max => (max.Value, AlertType.OVERFLOW),
                var x when x < min => (min.Value, AlertType.UNDERFLOW),
                _ => (pkuVals[i].Value.Value, AlertType.NONE)
            };
            formatVals[i].Value = checkedVal;
            a += NumericTag_E.GetNumericalAlert(subTagNames[i], at, max, min, defaultVals[i], alertIfUnspecified);
        }
        if (a is not null)
            a.Title = tagName;
        Warnings.Add(a);
    }
}