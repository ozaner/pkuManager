using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules.Templates;

public static class NumericTagUtil
{
    public static AlertType ExportNumericTag(IField<BigInteger?> pkuVal, IField<BigInteger> formatVal, BigInteger defaultVal)
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
        return at;
    }

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
}
