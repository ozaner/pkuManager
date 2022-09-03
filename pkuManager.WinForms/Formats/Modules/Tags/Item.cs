using OneOf;
using pkuManager.Data.Dexes;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using static pkuManager.WinForms.Alerts.Alert;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

public interface Item_O
{
    public OneOf<IIntField, IField<string>> Item { get; }
}

public interface Item_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportItem()
    {
        AlertType at = IndexTagUtil.ExportIndexTag(pku.Item, (Data as Item_O).Item, "None",
            (v) =>
            {
                bool a = DDM.TryGetItemID(FormatName, v, out int ID);
                return (a, ID);
            },
            (v) =>
            {
                bool a = DDM.TryGetItemID(FormatName, v, out string ID);
                return (a, ID);
            });
        Warnings.Add(IndexTagUtil.GetIndexAlert("Item", at, pku.Item.Value, "None", true));
    }
}