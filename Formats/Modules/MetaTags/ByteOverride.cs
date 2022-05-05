using Newtonsoft.Json.Linq;
using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.MetaTags;

public interface ByteOverride_O
{
    public ByteArrayManipulator BAM { get; }
}

public interface ByteOverride_E : Tag
{
    // Processing
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ApplyByteOverride()
    {
        ByteOverride_O byteOverrideObj = Data as ByteOverride_O;

        pkuObject.Format_Dict forDict = FormatSpecificUtil.GetFormatDict(pku, FormatName);
        if (forDict?.Byte_Override.Count is not > 0) //byte override null/empty
            return;

        ByteOverride_Action = () => { };
        List<string> invalidCmds = new();
        foreach ((string comment, JToken token) in forDict.Byte_Override)
        {
            ByteOverrideCMD cmd = ByteOverrideCMD.FromJToken(token);
            if (cmd is null) //cmd invalid
                invalidCmds.Add(comment);
            else //cmd valid
                ByteOverride_Action += () => cmd.Apply(byteOverrideObj.BAM);
        }
        Warnings.Add(GetByteOverrideAlert(invalidCmds));
    }

    // Action
    [PorterDirective(ProcessingPhase.PostProcessing)]
    public Action ByteOverride_Action { get; set; }

    protected static Alert GetByteOverrideAlert(List<string> invalidCmds)
    {
        if (invalidCmds?.Count > 0)
            return new("Byte Override", $"The '{string.Join(", ", invalidCmds)}' byte override command(s) are invalid. Ignoring them.");
        return null;
    }
}

public static class ByteOverrideUtil
{
    public static Alert AddByteOverrideCMD(string tag, ByteOverrideCMD cmd, pkuObject pku, string FormatName)
    {
        FormatSpecificUtil.EnsureFormatDictExists(pku, FormatName);

        pku.Format_Specific[FormatName].Byte_Override.Add(tag, cmd.ToJToken());
        return GetByteOverrideAlert(tag);
    }

    public static Alert GetByteOverrideAlert(string tag)
        => new(tag, $"The {tag} value is invalid, an override will be added for this format.");
}

public class ByteOverrideCMD
{
    public bool IsBitType { get; }

    public int StartByte { get; }
    public int StartBit { get; }
    public int ByteOrBitLength { get; }

    public (int, int)[] VirtualIndices { get; }

    public OneOf<BigInteger, BigInteger[]> Value { get; }

    private ByteOverrideCMD(OneOf<BigInteger, BigInteger[]> value, bool isBitType,
        int byteAddr, int bitAddr, int length, (int, int)[] virtualIndices)
    {
        Value = value;
        IsBitType = isBitType;
        StartByte = byteAddr;
        StartBit = bitAddr;
        ByteOrBitLength = length;
        VirtualIndices = virtualIndices;
    }

    public ByteOverrideCMD(BigInteger value, int byteAddr, int length, (int, int)[] virtualIndices = null)
        : this(value, false, byteAddr, -1, length, virtualIndices) { }

    public ByteOverrideCMD(BigInteger value, int byteAddr, int bitAddr, int length, (int, int)[] virtualIndices = null)
        : this(value, true, byteAddr, bitAddr, length, virtualIndices) { }

    public ByteOverrideCMD(BigInteger[] value, int byteAddr, int length, (int, int)[] virtualIndices = null)
        : this(value, false, byteAddr, -1, length, virtualIndices) { }

    public ByteOverrideCMD(BigInteger[] value, int byteAddr, int bitAddr, int length, (int, int)[] virtualIndices = null)
        : this(value, true, byteAddr, bitAddr, length, virtualIndices) { }

    public static ByteOverrideCMD FromJToken(JToken token)
    {
        OneOf<BigInteger, BigInteger[]> value;
        bool bitType;
        int startByte;
        int startBit = -1;
        int length;
        (int, int)[] virtualIndices = null;

        //array & valid size
        if (token is not JArray tokenArr || tokenArr.Count is not (3 or 4))
                return null; //not array of size 3/4

        //value
        if (tokenArr[0] is JArray) //array
        {
            try { value = tokenArr[0].ToObject<BigInteger[]>(); }
            catch { return null; }
        }
        else //single
        {
            try { value = tokenArr[0].ToObject<BigInteger>(); }
            catch { return null; }
        }

        //start byte/bit
        if (tokenArr[1] is JArray startToken) //array
        {
            if (startToken.Count is not 2) //must be [byte, bit]
                return null;
            try {
                startByte = startToken[0].ToObject<int>();
                startBit = startToken[1].ToObject<int>();
            }
            catch { return null; }
            bitType = true;
        }
        else //single
        {
            try { startByte = tokenArr[1].ToObject<int>(); }
            catch { return null; }
            bitType = false;
        }

        //length
        try { length = tokenArr[2].ToObject<int>(); }
        catch { return null; }

        //virtual indices
        if (tokenArr.Count > 3) //has 4th element
        {
            if (tokenArr[3] is not JArray indicesToken)
                return null; //must be an array

            virtualIndices = new (int, int)[indicesToken.Count];

            for (int i = 0; i < virtualIndices.Length; i++)
            {
                if (indicesToken[i] is not JArray indexToken || indexToken.Count != 2)
                    return null; //all elements must be 2 long arrays

                try { virtualIndices[i] = (indexToken[0].ToObject<int>(), indexToken[1].ToObject<int>()); }
                catch { return null; }
            }
        }

        return value.Match(
            x =>
            {
                if (bitType)
                    return new ByteOverrideCMD(x, startByte, startBit, length, virtualIndices);
                else
                    return new ByteOverrideCMD(x, startByte, length, virtualIndices);
            },
            x =>
            {
                if (bitType)
                    return new ByteOverrideCMD(x, startByte, startBit, length, virtualIndices);
                else
                    return new ByteOverrideCMD(x, startByte, length, virtualIndices);
            }
        );
    }

    public JToken ToJToken()
    {
        JArray tok = new();

        //Value
        tok.Add(Value.Match(
            x => JToken.FromObject(x),
            x => JArray.FromObject(x)));

        //start
        if (IsBitType)
            tok.Add(JArray.FromObject(new[] { StartByte, StartBit }));
        else
            tok.Add(StartByte);

        //length
        tok.Add(ByteOrBitLength);

        //virtual indices
        if (VirtualIndices != null)
        {
            JArray indicesToken = new();
            foreach ((int a, int b) in VirtualIndices)
                indicesToken.Add(JArray.FromObject(new[] { a, b }));
            tok.Add(indicesToken);
        }

        return tok;
    }

    public bool IsValid(ByteArrayManipulator bam)
    {
        //get (potentially virtual) bam size
        var temp = bam.VirtualIndices;
        bam.VirtualIndices = VirtualIndices;
        int bamSize = bam.Length;
        bam.VirtualIndices = temp;

        //check if virtual indices are valid (not out of range)
        if (VirtualIndices?.Length > 0)
            foreach ((int offset, int size) in VirtualIndices)
                if (offset + size > bamSize)
                    return false;

        //get byte length of a single value
        int singleByteSize = IsBitType ? (ByteOrBitLength - 1) / 8 + 1 : ByteOrBitLength;
        if (ByteOrBitLength == 0) //special case for 0 byte/bitlength
            singleByteSize = 0;

        // check if value is in bounds (nonnegative + fits in given bits)
        BigInteger singleMax = BigInteger.Pow(2, ByteOrBitLength * (IsBitType ? 1 : 8)) - 1;
        bool InBounds(BigInteger bi)
            => bi.Sign >= 0 && bi <= singleMax;

        return Value.Match(
            x => InBounds(x) && StartByte + singleByteSize <= bamSize,
            x => x != null && x.All(x => InBounds(x)) && StartByte + singleByteSize * x.Length <= bamSize
        );
    }

    public void Apply(ByteArrayManipulator bam)
    {
        //temporarily swap virtual indices
        var temp = bam.VirtualIndices;
        bam.VirtualIndices = VirtualIndices;

        //apply override
        Value.Switch(
            x =>
            {
                if (IsBitType)
                    bam.Set(x, StartByte, StartBit, ByteOrBitLength);
                else
                    bam.Set(x, StartByte, ByteOrBitLength);
            },
            x =>
            {
                if (IsBitType)
                    bam.SetArray(StartByte, StartBit, ByteOrBitLength, x);
                else
                    bam.SetArray(StartByte, ByteOrBitLength, x);
            }
        );
        bam.VirtualIndices = temp; //revert virtual indices
    }
}