using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Item_O
{
    public OneOf<IField<BigInteger>, IField<string>> Item { get; }
    public string FormatName { get; }

    public bool IsValid(string item) => ITEM_DEX.ExistsIn(FormatName, item);
    public bool IsValid() => IsValid(AsString);

    public string AsString
    {
        get => Item.Match(
            x => ITEM_DEX.SearchIndexedValue<int?>(x.GetAs<int>(), FormatName, "Indices", "$x"),
            x => x.Value);
        set => Item.Switch(
            x => x.Value = ITEM_DEX.GetIndexedValue<int?>(FormatName, value, "Indices") ?? 0,
            x => x.Value = value);
    }
}

public interface Item_E : IndexTag_E
{
    public pkuObject pku { get; }
    public Item_O Item_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessItem()
        => ProcessIndexTag("Item", pku.Item, "None", false,
            Item_Field.IsValid, x => Item_Field.AsString = x);
}