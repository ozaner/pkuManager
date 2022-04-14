using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using System;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface PP_Ups_O
{
    public IField<BigInteger[]> PP_Ups { get; }
}

public interface PP_Ups_E : MultiNumericTag_E
{
    protected static readonly string[] TagNames = new[] { "move 1", "move 2", "move 3", "move 4" };

    public pkuObject pku { get; }

    public PP_Ups_O PP_Ups_Field { get; }
    public int[] Moves_Indices { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Moves_E.ExportMoves))]
    public void ExportPP_Ups()
    {
        BackedField<BigInteger?>[] ppupFields = new BackedField<BigInteger?>[PP_Ups_Field.PP_Ups.Value.Length];
        var x = pku.PP_Up_ArrayFromIndices(Moves_Indices);
        Array.Copy(x, ppupFields, x.Length);
        for (int i = x.Length; i < ppupFields.Length; i++)
            ppupFields[i] = new();
        ExportMultiNumericTag("PP Ups", TagNames, ppupFields, PP_Ups_Field.PP_Ups, 0, false);
    }
}