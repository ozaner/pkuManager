using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;
using static pkuManager.Alerts.Alert;
using pkuManager.Alerts;

namespace pkuManager.Formats.Modules.Tags;

public interface PP_Ups_O
{
    public IField<BigInteger[]> PP_Ups { get; }
}

public interface PP_Ups_E : Tag
{
    public int[] Moves_Indices { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Moves_E.ExportMoves))]
    public void ExportPP_Ups()
    {
        var ppUps = (Data as PP_Ups_O).PP_Ups;

        string[] tagNames = new string[ppUps.Value.Length];
        for (int i = 0; i < tagNames.Length; i++)
            tagNames[i] = $"move {i}";
        
        AlertType[] ats = NumericTagUtil.ExportNumericArrayTag(pku.PP_Up_ArrayFromIndices(Moves_Indices), ppUps, 0);
        Alert a = NumericTagUtil.GetNumericArrayAlert("PP Ups", tagNames, ats, ppUps as IBoundable, 0, true);
        Warnings.Add(a);
    }
}