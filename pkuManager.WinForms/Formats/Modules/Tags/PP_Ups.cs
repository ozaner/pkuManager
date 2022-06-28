using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Formats.PorterDirective;
using static pkuManager.WinForms.Alerts.Alert;
using pkuManager.WinForms.Alerts;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface PP_Ups_O
{
    public IIntArrayField PP_Ups { get; }
}

public interface PP_Ups_E : Tag
{
    public string[] Moves_Indices { get; }

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Moves_E.ExportMoves))]
    public void ExportPP_Ups()
    {
        var ppUps = (Data as PP_Ups_O).PP_Ups;

        string[] tagNames = new string[ppUps.Value.Length];
        for (int i = 0; i < tagNames.Length; i++)
            tagNames[i] = $"move {i}";
        
        AlertType[] ats = NumericTagUtil.ExportNumericArrayTag(pku.PP_Up_ArrayFromIndices(Moves_Indices), ppUps, 0);
        Alert a = NumericTagUtil.GetNumericArrayAlert("PP Ups", tagNames, ats, ppUps, 0, true);
        Warnings.Add(a);
    }
}