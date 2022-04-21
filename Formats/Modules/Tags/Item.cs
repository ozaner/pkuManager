using OneOf;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Item_O : IndexTag_O
{
    public OneOf<IField<BigInteger>, IField<string>> Item { get; }

    public bool IsValid(string item) => IsValid(ITEM_DEX, item);
    public bool IsValid() => IsValid(AsString);

    public string AsString
    {
        get => AsStringGet(ITEM_DEX, Item);
        set => AsStringSet(ITEM_DEX, Item, value);
    }
}

public interface Item_E : IndexTag_E
{
    public pkuObject pku { get; }
    public Item_O Item_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportItem()
        => ExportIndexTag("Item", pku.Item, "None", false,
            Item_Field.IsValid, x => Item_Field.AsString = x);
}