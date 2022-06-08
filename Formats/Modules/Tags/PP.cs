using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface PP_O
{
    public IIntArrayField PP { get; }
}

public interface PP_E : Tag
{
    public string[] Moves_Indices { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Moves_E.ExportMoves),
                                                nameof(PP_Ups_E.ExportPP_Ups))]
    public void ExportPP()
    {
        var pp = (Data as PP_O).PP;
        var ppups = (Data as PP_Ups_O).PP_Ups;
        int[] calculatedpp = new int[pp.Value.Length];

        for (int i = 0; i < calculatedpp.Length; i++)
        {
            if (i < Moves_Indices.Length)
            {
                string trueMove = MoveTagUtil.GetTrueMove(Moves_Indices[i]);
                calculatedpp[i] = TagUtil.CalculatePP(trueMove, ppups.GetAs<byte>(i), FormatName);
            }
        }
        pp.SetAs(calculatedpp);
    }
}