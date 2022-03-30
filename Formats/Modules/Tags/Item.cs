using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Item_O
{
    public OneOf<IField<BigInteger>, IField<string>> Item { get; }
}

public interface Item_E : IndexTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public Item_O Item_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ProcessItem()
        => ProcessIndexTag("Item", pku.Item, null, Item_Field.Item, false,
            x => ITEM_DEX.ExistsIn(FormatName, x),
            Item_Field.Item.Match<OneOf<Func<string, int>, Func<string, string>>>(
                _ => (Func<string, int>)(x => ITEM_DEX.GetIndexedValue<int?>(FormatName, x, "Indices") ?? 0),
                _ => (Func<string, string>)(x => ITEM_DEX.GetIndexedValue<string>(FormatName, x, "Indices"))
            ));
}