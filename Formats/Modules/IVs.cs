﻿using pkuManager.Formats.Fields;
using pkuManager.Formats.pkx;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface IVs_O
{
    public IField<BigInteger[]> IVs { get; }
}

public interface IVs_E : MultiNumericTag
{
    public IVs_O IVs_Field { get; }

    public int IVs_Default => 0;
    public bool IVs_SilentUnspecified => false; //Alert when no IVs, in general

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessIVs()
        => ProcessMultiNumericTag("IVs", pkxUtil.STAT_NAMES, pku.IVs_Array, IVs_Field.IVs, IVs_Default, IVs_SilentUnspecified);
}