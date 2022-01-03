using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules;

public interface NumericTag_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }

    protected void ProcessNumericTag(string tagName, Field<BigInteger?> pkuVal,
        IntegralField formatVal, BigInteger defaultVal, bool silentUnspecified)
    {
        (BigInteger checkedVal, AlertType at) = pkuVal.Get() switch {
            null => (defaultVal, AlertType.UNSPECIFIED),
            var x when x > formatVal.Max => (formatVal.Max, AlertType.OVERFLOW),
            var x when x < formatVal.Min => (formatVal.Min, AlertType.UNDERFLOW),
            _ => (pkuVal.Get().Value, AlertType.NONE)
        };
        formatVal.Set(checkedVal);
        Warnings.Add(GetNumericalAlert(tagName, at, formatVal.Max, formatVal.Min, defaultVal, silentUnspecified));
    }

    protected Alert GetNumericalAlert(string name, AlertType at, BigInteger max, BigInteger min, BigInteger defaultVal, bool silentUnspecified) => at switch
    {
        AlertType.OVERFLOW => new(name, $"This pku's {name} is higher than the maximum. Rounding down to {max}."),
        AlertType.UNDERFLOW => new(name, $"This pku's {name} is lower than the minimum. Rounding up to {min}."),
        AlertType.UNSPECIFIED => silentUnspecified ? null : new(name, $"{name} tag not specified, using the default of {defaultVal}."),
        AlertType.NONE => null,
        _ => throw InvalidAlertType(at)
    };
}