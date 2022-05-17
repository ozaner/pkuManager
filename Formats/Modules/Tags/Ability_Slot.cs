using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BackedFields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ability_Slot_O
{
    public IField<BigInteger> Ability_Slot { get; }

    //using official (gen 6+) indices by default
    public Dictionary<AbilitySlot, int> Slot_Mapping => new()
    {
        { AbilitySlot.Slot_1, 1},
        { AbilitySlot.Slot_2, 2},
        { AbilitySlot.Slot_H, 4}
    };
}

public interface Ability_Slot_E : Tag
{
    public const AbilitySlot DEFAULT_SLOT = AbilitySlot.Slot_1; //assuming all formats have a slot 1

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportAbility_Slot()
    {
        Alert alert = null;

        Ability_Slot_O abilitySlotObj = Data as Ability_Slot_O;
        string[] abilitySlots = DexUtil.GetSpeciesIndexedValue<string[]>(pku, FormatName, "Ability Slots"); //must exist
        int getSlotNum(AbilitySlot slot) => abilitySlotObj.Slot_Mapping is null ? (int)slot : abilitySlotObj.Slot_Mapping[slot];

        //process ability slot w/ dummy
        BackedField<AbilitySlot> tempSlotField = new();
        AlertType atSlot = EnumTagUtil<AbilitySlot>.ExportEnumTag(pku.Ability_Slot, tempSlotField, DEFAULT_SLOT, abilitySlotObj.Slot_Mapping);

        if (Data is not Ability_O) //slot only
        {
            bool? match = null; //only set if abil & slot are both specified and valid

            //check if species even has this slot (i.e. SFA MISMATCH)
            if (atSlot is AlertType.NONE && 
                ((int)tempSlotField.Value >= abilitySlots.Length || abilitySlots[(int)tempSlotField.Value] is null))
            {
                tempSlotField.Value = DEFAULT_SLOT;
                atSlot = AlertType.MISMATCH; //species doesn't have this slot
            }

            //process ability
            AlertType atAbil = AlertType.NONE;
            if (pku.Ability.IsNull()) //unspecified
                atAbil = AlertType.UNSPECIFIED;
            else //specified
            {
                if (ABILITY_DEX.ExistsIn(FormatName, pku.Ability.Value)) //valid
                {
                    int abilIndex = Array.IndexOf(abilitySlots, pku.Ability.Value);
                    if (abilIndex < 0) //mismatch
                        atAbil = AlertType.MISMATCH;
                    else //exists on this species
                    {
                        if (atSlot is AlertType.NONE)
                            match = abilitySlots[(int)tempSlotField.Value] == pku.Ability.Value;

                        if (match is false) //both valid but don't match, use abil
                            tempSlotField.Value = (AbilitySlot)abilIndex;
                    }
                }
                else //invalid
                    atAbil = AlertType.INVALID;
            }

            abilitySlotObj.Ability_Slot.Value = getSlotNum(tempSlotField.Value);
            alert = GetAbilitySlotAlert(match, atSlot, atAbil, pku.Ability_Slot.Value, pku.Ability.Value,
                tempSlotField.Value, abilitySlots[(int)tempSlotField.Value]);
        }
        else //both (value taken care of in ability module)
        {
            //if unspecified/invalid, try setting slot to proper slot relative to ability. (so g4/5 -> g6+ makes sense)
            //if abil has no slot, just make a warning of that fact ("no slot coreesponds to ability: "pressure", setting slot to 0.")
            //if slot/abil mismatch, don't even mention or check for it.
        }
        Warnings.Add(alert);
    }

    public static Alert GetAbilitySlotAlert(bool? match, AlertType atS, AlertType atA,
        string valS, string valA, AbilitySlot defaultS, string defaultA)
    {
        string msg = "";
        
        //ability
        if (atA is AlertType.MISMATCH)
            msg += $"This species cannot have the {valA} ability in this format. ";
        else if (atA is AlertType.INVALID)
            msg += $"The {valA} ability does not exist in this format. ";

        //slot
        if (atS is AlertType.MISMATCH)
            msg += $"This species does not have a {valS} ability. ";
        else if (atS is AlertType.INVALID)
            msg += $"The {valS} ability slot does not exist in this format. ";

        //both unspecified or abil/slot mismatch
        if ((atS, atA) is (AlertType.UNSPECIFIED, AlertType.UNSPECIFIED))
            msg += "Both the ability and ability slot were unspecified. ";
        else if ((atS, atA) is (AlertType.NONE, AlertType.NONE) && match is false)
            msg += $"This species cannot both have ability {valS} and the {valA} ability. ";

        //default val
        if (msg != "")
            msg += $"Setting the ability to {defaultS.ToFormattedString()}: {defaultA}.";

        return msg is "" ? null : new("Ability Slot", msg);
    }
}