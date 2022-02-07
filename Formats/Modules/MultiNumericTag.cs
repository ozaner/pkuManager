﻿using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static pkuManager.Alerts.Alert;

namespace pkuManager.Formats.Modules;

public interface MultiNumericTag
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }

    protected void ProcessMultiNumericTag(string tagName, string[] subTagNames, IField<BigInteger?>[] pkuVals,
        IField<BigInteger[]> formatVals, BigInteger defaultVal, bool silentUnspecified)
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
        Warnings.Add(GetMultiNumericAlert(tagName, subTagNames, valAlerts, max, min, defaultVal, silentUnspecified));
    }

    protected static Alert GetMultiNumericAlert(string tagName, string[] subtags, AlertType[] ats,
        BigInteger? max, BigInteger? min, BigInteger defaultVal, bool silentUnspecified)
    {
        if (subtags?.Length != ats?.Length)
            throw new ArgumentException($"{nameof(subtags)} must have the same length as {nameof(ats)}.", nameof(subtags));
        else if (ats.All(x => x is AlertType.UNSPECIFIED))
            return silentUnspecified ? null : new(tagName, $"No {tagName} were specified, setting them all to {defaultVal}.");

        string msgOverflow = "";
        string msgUnderflow = "";
        string msgUnspecifed = "";

        for (int i = 0; i < subtags.Length; i++)
        {
            switch (ats[i])
            {
                case AlertType.OVERFLOW:
                    msgOverflow += $"{subtags[i]}, ";
                    break;
                case AlertType.UNDERFLOW:
                    msgUnderflow += $"{subtags[i]}, ";
                    break;
                case AlertType.UNSPECIFIED:
                    msgUnspecifed += $"{subtags[i]}, ";
                    break;
                case AlertType.NONE:
                    break;
                default:
                    throw InvalidAlertType(ats[i]);
            }
        }

        string msg = "";
        if (msgOverflow is not "")
            msg += $"The {msgOverflow[0..^2]} tag(s) were too high. Rounding them down to {max}.";
        if (msgUnderflow is not "")
            msg += (msg is not "" ? DataUtil.Newline(2) : "") +
                    $"The {msgUnderflow[0..^2]} tag(s) were too low. Rounding them up to {min}.";
        if (msgUnspecifed is not "")
            msg += (msg is not "" ? DataUtil.Newline(2) : "") +
                    $"The {msgUnspecifed[0..^2]} tag(s) were unspecified. Setting them to {defaultVal}.";

        return msg is "" ? null : new(tagName, msg);
    }
}