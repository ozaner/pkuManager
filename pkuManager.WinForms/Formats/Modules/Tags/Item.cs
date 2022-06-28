using OneOf;
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
        AlertType at = IndexTagUtil.ExportIndexTag(pku.Item, (Data as Item_O).Item, "None", ITEM_DEX, FormatName);
        Warnings.Add(IndexTagUtil.GetIndexAlert("Item", at, pku.Item.Value, "None", true));
    }
}