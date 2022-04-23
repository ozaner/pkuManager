using pkuManager.Formats.Fields;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface PP_O
{
    public IField<BigInteger[]> PP { get; }
}

public interface PP_E : Tag
{
    public PP_O PP_Field { get; }
    public PP_Ups_O PP_Ups_Field { get; }
    public int[] Moves_Indices { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Moves_E.ExportMoves),
                                                nameof(PP_Ups_E.ExportPP_Ups))]
    public void ExportPP()
    {
        int[] pp = new int[4];
        for (int i = 0; i < Moves_Indices.Length; i++)
            pp[i] = TagUtil.CalculatePP(pku.Moves[Moves_Indices[i]].Name.Value, PP_Ups_Field.PP_Ups.GetAs<byte>(i), FormatName);
        PP_Field.PP.SetAs(pp);
    }
}