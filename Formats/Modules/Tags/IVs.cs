﻿using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface IVs_O
{
    public IField<BigInteger[]> IVs { get; }
}

public interface IVs_E : MultiNumericTag_E
{
    public pkuObject pku { get; }

    public IVs_O IVs_Field { get; }
    public int IVs_Default => 0;
    public bool IVs_AlertIfUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessIVs()
        => ProcessMultiNumericTag("IVs", TagUtil.STAT_NAMES, pku.IVs_Array, IVs_Field.IVs, IVs_Default, IVs_AlertIfUnspecified);
}