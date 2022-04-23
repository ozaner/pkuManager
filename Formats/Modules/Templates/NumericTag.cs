using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public interface NumericTag_E : Tag
{
    protected void ExportNumericTag(string tagName, IField<BigInteger?> pkuVal,
        IField<BigInteger> formatVal, BigInteger defaultVal, bool alertIfUnspecified)
    {
        (BigInteger? max, BigInteger? min) = formatVal is IBoundable<BigInteger> boundable ?
            (boundable.Max, boundable.Min) : (null, null);
        (BigInteger checkedVal, AlertType at) = pkuVal.Value switch
        {
            null => (defaultVal, AlertType.UNSPECIFIED),
            var x when x > max => (max.Value, AlertType.OVERFLOW),
            var x when x < min => (min.Value, AlertType.UNDERFLOW),
            _ => (pkuVal.Value.Value, AlertType.NONE)
        };
        formatVal.Value = checkedVal;
        Warnings.Add(GetNumericalAlert(tagName, at, max, min, defaultVal, alertIfUnspecified));
    }

    public static Alert GetNumericalAlert(string name, AlertType at, BigInteger? max, BigInteger? min,
        BigInteger defaultVal, bool alertIfUnspecified) => at switch
    {
        AlertType.OVERFLOW => new(name, $"This pku's {name} is higher than the maximum. Rounding down to {max}."),
        AlertType.UNDERFLOW => new(name, $"This pku's {name} is lower than the minimum. Rounding up to {min}."),
        AlertType.UNSPECIFIED => alertIfUnspecified ? new(name, $"{name} tag not specified, using the default of {defaultVal}.") : null,
        AlertType.NONE => null,
        _ => throw InvalidAlertType(at)
    };
}