using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using System;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface PP_Ups_O
{
    public IField<BigInteger[]> PP_Ups { get; }
}

public interface PP_Ups_E : MultiNumericTag_E
{
    public int[] Moves_Indices { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Moves_E.ExportMoves))]
    public void ExportPP_Ups()
    {
        PP_Ups_O ppUpsObj = Data as PP_Ups_O;
        BackedField<BigInteger?>[] ppupFields = new BackedField<BigInteger?>[ppUpsObj.PP_Ups.Value.Length];
        var x = pku.PP_Up_ArrayFromIndices(Moves_Indices);
        Array.Copy(x, ppupFields, x.Length);
        for (int i = x.Length; i < ppupFields.Length; i++)
            ppupFields[i] = new();
        ExportMultiNumericTag("PP Ups", new[] { "move 1", "move 2", "move 3", "move 4" }, ppupFields, ppUpsObj.PP_Ups, 0, false);
    }
}