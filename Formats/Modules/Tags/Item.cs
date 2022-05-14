using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Item_O
{
    public OneOf<IField<BigInteger>, IField<string>> Item { get; }
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