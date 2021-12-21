using pkuManager.Formats.Fields;
using pkuManager.pku;
using pkuManager.Utilities;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Item_O
{
    public Union<IntegralField, Field<string>> Item { get; }
}

public interface Item_E : IndexTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public Item_O Data { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessItem()
        => ProcessIndexTag("Item", pku.Item, "None", Data.Item, false,
            x => ITEM_DEX.ExistsIn(FormatName, x), x => ITEM_DEX.GetIndexedValue<int?>(FormatName, x, "Indices"));
}