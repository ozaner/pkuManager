using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Item_O
{
    public OneOf<IIntegralField, IField<string>> Item { get; }
}

public interface Item_E : IndexTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public string Item_Default => null;
    public Item_O Data { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessItem()
        => ProcessIndexTag("Item", pku.Item, Item_Default, Data.Item, false,
            x => ITEM_DEX.ExistsIn(FormatName, x), x => ITEM_DEX.GetIndexedValue<int?>(FormatName, x, "Indices") ?? 0);
}