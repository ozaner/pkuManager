using Newtonsoft.Json.Linq;
using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.MetaTags;

public interface ByteOverride_O
{
    public ByteArrayManipulator BAM { get; }
}

public interface ByteOverride_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }

    public ByteOverride_O ByteOverride_Field { get; }

    // Processing
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ApplyByteOverride()
    {
        List<Action> validCommands = new();
        List<int> invalidIndices = new();
        int index = -1;
        foreach ((string cmd, JToken token) in pku.Byte_Override)
        {
            index++;

            //process value (JToken -> bigint)
            OneOf<BigInteger, BigInteger[]> value;
            if (token is JValue jval && jval.Type is JTokenType.Integer)
                value = jval.ToBigInteger();
            else if (token is JArray jarr)
            {
                BigInteger[] array = new BigInteger[jarr.Count];
                for (int i = 0; i < array.Length; i++)
                {
                    if (jarr[i].Type is JTokenType.Integer)
                        array[i] = jarr[i].ToBigInteger();
                    else
                    {
                        invalidIndices.Add(index);
                        continue;
                    }
                }
                value = array;
            }
            else
            {
                invalidIndices.Add(index);
                continue;
            }

            //process CMD
            var boCMD = ByteOverrideCMD.FromString(cmd, value);
            if (boCMD?.IsValid(ByteOverride_Field.BAM) == true)
                validCommands.Add(() => boCMD.Apply(ByteOverride_Field.BAM));
            else
            {
                invalidIndices.Add(index);
                continue;
            }
            boCMD.ToString();
        }

        void action()
        {
            foreach (Action a in validCommands)
                a.Invoke();
        }
        ByteOverride_Action = action;
        Warnings.Add(GetByteOverrideAlert(invalidIndices));
    }

    // Action
    [PorterDirective(ProcessingPhase.PostProcessing)]
    public Action ByteOverride_Action { get; set; }

    protected static Alert GetByteOverrideAlert(List<int> invalidIndices)
    {
        if (invalidIndices?.Count > 0)
            return new("Byte Override", $"Byte Override command(s) {string.Join(", ", invalidIndices)} are invalid. Ignoring them.");
        return null;
    }
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


    private const string BYTE_TYPE_REGEX = @"^(.*) ([0-9]+):([0-9]+)(?:\|((?:\([0-9]+,[0-9]+\))+))*$";
    private const string BIT_TYPE_REGEX = @"^(.*) ([0-9]+):([0-9]+):([0-9]+)(?:\|((?:\([0-9]+,[0-9]+\))+))*$";
    private const string VIRTUAL_INDEX_REGEX = @"\(([0-9]+),([0-9]+)\)";

    public static ByteOverrideCMD FromString(string cmd, OneOf<BigInteger, BigInteger[]> value)
    {
        bool isBitType = false;
        int startByte = -1, startBit = -1, length = -1;
        (int, int)[] virtualIndices = null;

        bool cmdTypeValid(Match match)
        {
            string cmdtype = match.Groups[1].Value.ToLowerInvariant();
            return cmdtype is "set" && value.IsT0 || cmdtype is "set array" && value.IsT1;
        }

        (int, int)[] processVirtualIndices(string group)
        {
            (int, int)[] virtualIndices = null;
            MatchCollection matches = Regex.Matches(group, VIRTUAL_INDEX_REGEX);
            if (matches.Count > 0)
            {
                virtualIndices = new (int, int)[matches.Count];
                for (int i = 0; i < virtualIndices.Length; i++)
                {
                    if (!int.TryParse(matches[i].Groups[1].Value, out int a) ||
                        !int.TryParse(matches[i].Groups[2].Value, out int b))
                        return null;
                    virtualIndices[i] = (a, b);
                }
            }
            return virtualIndices;
        }

        //try byte type
        Match match = Regex.Match(cmd, BYTE_TYPE_REGEX);
        if (match.Success)
        {
            if (!cmdTypeValid(match))
                return null;
            isBitType = false;
            if (!int.TryParse(match.Groups[2].Value, out startByte) ||
                !int.TryParse(match.Groups[3].Value, out length))
                return null;
            if (match.Groups[4].Success)
                virtualIndices = processVirtualIndices(match.Groups[4].Value);
        }

        //try bit type
        match = Regex.Match(cmd, BIT_TYPE_REGEX);
        if (match.Success)
        {
            if (!cmdTypeValid(match))
                return null;
            isBitType = true;
            if (int.TryParse(match.Groups[2].Value, out startByte) ||
                int.TryParse(match.Groups[3].Value, out startBit) ||
                int.TryParse(match.Groups[4].Value, out length))
                return null;
            if (match.Groups[5].Success)
                virtualIndices = processVirtualIndices(match.Groups[5].Value);
        }

        return value.Match(
            x =>
            {
                if (isBitType)
                    return new ByteOverrideCMD(x, startByte, startBit, length, virtualIndices);
                else
                    return new ByteOverrideCMD(x, startByte, length, virtualIndices);
            },
            x =>
            {
                if (isBitType)
                    return new ByteOverrideCMD(x, startByte, startBit, length, virtualIndices);
                else
                    return new ByteOverrideCMD(x, startByte, length, virtualIndices);
            }
        );
    }

    public override string ToString()
    {
        string cmd = "Set ";
        if (Value.IsT1)
            cmd += "Array ";
        cmd += $"{StartByte}:";
        if (IsBitType)
            cmd += $"{StartBit}:";
        cmd += $"{ByteOrBitLength}";
        if (VirtualIndices?.Length > 0)
        {
            cmd += "|";
            foreach ((int offset, int size) in VirtualIndices)
                cmd += $"({offset},{size})";
        }
        return cmd;
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