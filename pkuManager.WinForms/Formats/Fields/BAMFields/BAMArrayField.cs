﻿using pkuManager.WinForms.Formats.Modules.MetaTags;
using pkuManager.WinForms.Utilities;
using System.Numerics;

namespace pkuManager.WinForms.Formats.Fields.BAMFields;

public class BAMArrayField : BAMField, IIntArrayField
{
    public int Length { get; set; }

    public BigInteger[] Value
    {
        get => BitType ? BAM.GetArray(StartByte, StartBit, ByteOrBitLength, Length)
                       : BAM.GetArray(StartByte, ByteOrBitLength, Length);
        set
        {
            if (BitType)
                BAM.SetArray(StartByte, StartBit, ByteOrBitLength, value, Length);
            else
                BAM.SetArray(StartByte, ByteOrBitLength, value, Length);
        }
    }

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int byteLength, int length)
        : base(bam, startByte, byteLength) => Length = length;

    public BAMArrayField(ByteArrayManipulator bam, int startByte, int startBit, int bitLength, int length)
        : base(bam, startByte, startBit, bitLength) => Length = length;

    public override ByteOverrideCMD GetOverride()
        => BitType ? new ByteOverrideCMD(Value, StartByte, StartBit, ByteOrBitLength, BAM.VirtualIndices)
                   : new ByteOverrideCMD(Value, StartByte, ByteOrBitLength, BAM.VirtualIndices);
}