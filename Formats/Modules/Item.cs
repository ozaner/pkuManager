using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Item_O
{
    public Union<IntegralField, Field<string>> Item { get; }
}

public interface Item_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }
    public string FormatName { get; }

    public Item_O Data { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessItem()
    {
        if (!pku.Item.IsNull && ITEM_DEX.ExistsIn(FormatName, pku.Item)) //item specified & exists
        {
            if(Data.Item.IsLeft) //numeric item
            {
                int? id = ITEM_DEX.GetIndexedValue<int?>(FormatName, pku.Item, "Indices");
                if (id is null)
                    throw new Exception($"pkuData Error: Item {pku.Item} exists, yet has no ID.");
                Data.Item.Left.Set(id.Value);
            }
            else //string item
                Data.Item.Right.Set(pku.Item);
        }
        else if(!pku.Item.IsNull) //item specified & DNE
            Warnings.Add(GetInvalidItemAlert(pku.Item));
    }

    protected Alert GetInvalidItemAlert(string invalidItem)
        => new("Item", $"The held item {invalidItem} is not valid in this format. Setting the held item to none.");
}