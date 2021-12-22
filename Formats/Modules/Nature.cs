using OneOf;
using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.Formats.Fields;
using pkuManager.pku;
using System;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Nature_O
{
    public OneOf<IntegralField, Field<Nature>, Field<Nature?>> Nature { get; }
}

public interface Nature_E : EnumTag_E
{
    public pkuObject pku { get; }
    public Nature_O Data { get; }

    public Nature? Nature_Default => Nature.Hardy;
    public bool Nature_AlertIfUnspecified => true;
    public Func<AlertType, string, string, Alert> Nature_Alert_Func => null;

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessNature()
        => ProcessEnumTag("Nature", pku.Nature, Nature_Default, Data.Nature, Nature_AlertIfUnspecified, Nature_Alert_Func);
}