using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System.Collections.Generic;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Ribbons_O
{
    public IField<bool> Cool_G3_Ribbon => null;
    public IField<bool> Cool_Super_G3_Ribbon => null;
    public IField<bool> Cool_Hyper_G3_Ribbon => null;
    public IField<bool> Cool_Master_G3_Ribbon => null;
    public IField<bool> Beauty_G3_Ribbon => null;
    public IField<bool> Beauty_Super_G3_Ribbon => null;
    public IField<bool> Beauty_Hyper_G3_Ribbon => null;
    public IField<bool> Beauty_Master_G3_Ribbon => null;
    public IField<bool> Cute_G3_Ribbon => null;
    public IField<bool> Cute_Super_G3_Ribbon => null;
    public IField<bool> Cute_Hyper_G3_Ribbon => null;
    public IField<bool> Cute_Master_G3_Ribbon => null;
    public IField<bool> Smart_G3_Ribbon => null;
    public IField<bool> Smart_Super_G3_Ribbon => null;
    public IField<bool> Smart_Hyper_G3_Ribbon => null;
    public IField<bool> Smart_Master_G3_Ribbon => null;
    public IField<bool> Tough_G3_Ribbon => null;
    public IField<bool> Tough_Super_G3_Ribbon => null;
    public IField<bool> Tough_Hyper_G3_Ribbon => null;
    public IField<bool> Tough_Master_G3_Ribbon => null;
    public IField<bool> Cool_G4_Ribbon => null;
    public IField<bool> Cool_Great_G4_Ribbon => null;
    public IField<bool> Cool_Ultra_G4_Ribbon => null;
    public IField<bool> Cool_Master_G4_Ribbon => null;
    public IField<bool> Beauty_G4_Ribbon => null;
    public IField<bool> Beauty_Great_G4_Ribbon => null;
    public IField<bool> Beauty_Ultra_G4_Ribbon => null;
    public IField<bool> Beauty_Master_G4_Ribbon => null;
    public IField<bool> Cute_G4_Ribbon => null;
    public IField<bool> Cute_Great_G4_Ribbon => null;
    public IField<bool> Cute_Ultra_G4_Ribbon => null;
    public IField<bool> Cute_Master_G4_Ribbon => null;
    public IField<bool> Smart_G4_Ribbon => null;
    public IField<bool> Smart_Great_G4_Ribbon => null;
    public IField<bool> Smart_Ultra_G4_Ribbon => null;
    public IField<bool> Smart_Master_G4_Ribbon => null;
    public IField<bool> Tough_G4_Ribbon => null;
    public IField<bool> Tough_Great_G4_Ribbon => null;
    public IField<bool> Tough_Ultra_G4_Ribbon => null;
    public IField<bool> Tough_Master_G4_Ribbon => null;
    public IField<bool> Winning_Ribbon => null;
    public IField<bool> Victory_Ribbon => null;
    public IField<bool> Ability_Ribbon => null;
    public IField<bool> Great_Ability_Ribbon => null;
    public IField<bool> Double_Ability_Ribbon => null;
    public IField<bool> Multi_Ability_Ribbon => null;
    public IField<bool> Pair_Ability_Ribbon => null;
    public IField<bool> World_Ability_Ribbon => null;
    public IField<bool> Kalos_Champion_Ribbon => null;
    public IField<bool> Champion_Ribbon => null;
    public IField<bool> Sinnoh_Champion_Ribbon => null;
    public IField<bool> Best_Friends_Ribbon => null;
    public IField<bool> Training_Ribbon => null;
    public IField<bool> Skillful_Battler_Ribbon => null;
    public IField<bool> Expert_Battler_Ribbon => null;
    public IField<bool> Effort_Ribbon => null;
    public IField<bool> Alert_Ribbon => null;
    public IField<bool> Shock_Ribbon => null;
    public IField<bool> Downcast_Ribbon => null;
    public IField<bool> Careless_Ribbon => null;
    public IField<bool> Relax_Ribbon => null;
    public IField<bool> Snooze_Ribbon => null;
    public IField<bool> Smile_Ribbon => null;
    public IField<bool> Gorgeous_Ribbon => null;
    public IField<bool> Royal_Ribbon => null;
    public IField<bool> Gorgeous_Royal_Ribbon => null;
    public IField<bool> Artist_Ribbon => null;
    public IField<bool> Footprint_Ribbon => null;
    public IField<bool> Record_Ribbon => null;
    public IField<bool> Legend_Ribbon => null;
    public IField<bool> Country_Ribbon => null;
    public IField<bool> National_Ribbon => null;
    public IField<bool> Earth_Ribbon => null;
    public IField<bool> World_Ribbon => null;
    public IField<bool> Classic_Ribbon => null;
    public IField<bool> Premier_Ribbon => null;
    public IField<bool> Event_Ribbon => null;
    public IField<bool> Birthday_Ribbon => null;
    public IField<bool> Special_Ribbon => null;
    public IField<bool> Souvenir_Ribbon => null;
    public IField<bool> Wishing_Ribbon => null;
    public IField<bool> Battle_Champion_Ribbon => null;
    public IField<bool> Regional_Champion_Ribbon => null;
    public IField<bool> National_Champion_Ribbon => null;
    public IField<bool> World_Champion_Ribbon => null;
    public IField<bool> Contest_Memory_Ribbon => null;
    public IField<bool> Battle_Memory_Ribbon => null;
    public IField<bool> Hoenn_Champion_Ribbon => null;
    public IField<bool> Contest_Star_Ribbon => null;
    public IField<bool> Coolness_Master_Ribbon => null;
    public IField<bool> Beauty_Master_Ribbon => null;
    public IField<bool> Cuteness_Master_Ribbon => null;
    public IField<bool> Cleverness_Master_Ribbon => null;
    public IField<bool> Toughness_Master_Ribbon => null;
    public IField<bool> Alola_Champion_Ribbon => null;
    public IField<bool> Battle_Royale_Master_Ribbon => null;
    public IField<bool> Battle_Tree_Great_Ribbon => null;
    public IField<bool> Battle_Tree_Master_Ribbon => null;
    public IField<bool> Galar_Champion_Ribbon => null;
    public IField<bool> Tower_Master_Ribbon => null;
    public IField<bool> Master_Rank_Ribbon => null;
}

public interface Ribbons_E : MultiEnumTag_E
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportRibbons() => ExportRibbonsBase();

    public void ExportRibbonsBase()
        => ExportMultiEnumTag(GetMapping(), pku.Ribbons.ToEnumSet<Ribbon>());

    private Dictionary<Ribbon, IField<bool>> GetMapping()
    {
        Ribbons_O ribbonsObj = Data as Ribbons_O;
        return new()
        {
            { Ribbon.Cool_G3, ribbonsObj.Cool_G3_Ribbon },
            { Ribbon.Cool_Super_G3, ribbonsObj.Cool_Super_G3_Ribbon },
            { Ribbon.Cool_Hyper_G3, ribbonsObj.Cool_Hyper_G3_Ribbon },
            { Ribbon.Cool_Master_G3, ribbonsObj.Cool_Master_G3_Ribbon },
            { Ribbon.Beauty_G3, ribbonsObj.Beauty_G3_Ribbon },
            { Ribbon.Beauty_Super_G3, ribbonsObj.Beauty_Super_G3_Ribbon },
            { Ribbon.Beauty_Hyper_G3, ribbonsObj.Beauty_Hyper_G3_Ribbon },
            { Ribbon.Beauty_Master_G3, ribbonsObj.Beauty_Master_G3_Ribbon },
            { Ribbon.Cute_G3, ribbonsObj.Cute_G3_Ribbon },
            { Ribbon.Cute_Super_G3, ribbonsObj.Cute_Super_G3_Ribbon },
            { Ribbon.Cute_Hyper_G3, ribbonsObj.Cute_Hyper_G3_Ribbon },
            { Ribbon.Cute_Master_G3, ribbonsObj.Cute_Master_G3_Ribbon },
            { Ribbon.Smart_G3, ribbonsObj.Smart_G3_Ribbon },
            { Ribbon.Smart_Super_G3, ribbonsObj.Smart_Super_G3_Ribbon },
            { Ribbon.Smart_Hyper_G3, ribbonsObj.Smart_Hyper_G3_Ribbon },
            { Ribbon.Smart_Master_G3, ribbonsObj.Smart_Master_G3_Ribbon },
            { Ribbon.Tough_G3, ribbonsObj.Tough_G3_Ribbon },
            { Ribbon.Tough_Super_G3, ribbonsObj.Tough_Super_G3_Ribbon },
            { Ribbon.Tough_Hyper_G3, ribbonsObj.Tough_Hyper_G3_Ribbon },
            { Ribbon.Tough_Master_G3, ribbonsObj.Tough_Master_G3_Ribbon },
            { Ribbon.Cool_G4, ribbonsObj.Cool_G4_Ribbon },
            { Ribbon.Cool_Great_G4, ribbonsObj.Cool_Great_G4_Ribbon },
            { Ribbon.Cool_Ultra_G4, ribbonsObj.Cool_Ultra_G4_Ribbon },
            { Ribbon.Cool_Master_G4, ribbonsObj.Cool_Master_G4_Ribbon },
            { Ribbon.Beauty_G4, ribbonsObj.Beauty_G4_Ribbon },
            { Ribbon.Beauty_Great_G4, ribbonsObj.Beauty_Great_G4_Ribbon },
            { Ribbon.Beauty_Ultra_G4, ribbonsObj.Beauty_Ultra_G4_Ribbon },
            { Ribbon.Beauty_Master_G4, ribbonsObj.Beauty_Master_G4_Ribbon },
            { Ribbon.Cute_G4, ribbonsObj.Cute_G4_Ribbon },
            { Ribbon.Cute_Great_G4, ribbonsObj.Cute_Great_G4_Ribbon },
            { Ribbon.Cute_Ultra_G4, ribbonsObj.Cute_Ultra_G4_Ribbon },
            { Ribbon.Cute_Master_G4, ribbonsObj.Cute_Master_G4_Ribbon },
            { Ribbon.Smart_G4, ribbonsObj.Smart_G4_Ribbon },
            { Ribbon.Smart_Great_G4, ribbonsObj.Smart_Great_G4_Ribbon },
            { Ribbon.Smart_Ultra_G4, ribbonsObj.Smart_Ultra_G4_Ribbon },
            { Ribbon.Smart_Master_G4, ribbonsObj.Smart_Master_G4_Ribbon },
            { Ribbon.Tough_G4, ribbonsObj.Tough_G4_Ribbon },
            { Ribbon.Tough_Great_G4, ribbonsObj.Tough_Great_G4_Ribbon },
            { Ribbon.Tough_Ultra_G4, ribbonsObj.Tough_Ultra_G4_Ribbon },
            { Ribbon.Tough_Master_G4, ribbonsObj.Tough_Master_G4_Ribbon },
            { Ribbon.Winning, ribbonsObj.Winning_Ribbon },
            { Ribbon.Victory, ribbonsObj.Victory_Ribbon },
            { Ribbon.Ability, ribbonsObj.Ability_Ribbon },
            { Ribbon.Great_Ability, ribbonsObj.Great_Ability_Ribbon },
            { Ribbon.Double_Ability, ribbonsObj.Double_Ability_Ribbon },
            { Ribbon.Multi_Ability, ribbonsObj.Multi_Ability_Ribbon },
            { Ribbon.Pair_Ability, ribbonsObj.Pair_Ability_Ribbon },
            { Ribbon.World_Ability, ribbonsObj.World_Ability_Ribbon },
            { Ribbon.Kalos_Champion, ribbonsObj.Kalos_Champion_Ribbon },
            { Ribbon.Champion, ribbonsObj.Champion_Ribbon },
            { Ribbon.Sinnoh_Champion, ribbonsObj.Sinnoh_Champion_Ribbon },
            { Ribbon.Best_Friends, ribbonsObj.Best_Friends_Ribbon },
            { Ribbon.Training, ribbonsObj.Training_Ribbon },
            { Ribbon.Skillful_Battler, ribbonsObj.Skillful_Battler_Ribbon },
            { Ribbon.Expert_Battler, ribbonsObj.Expert_Battler_Ribbon },
            { Ribbon.Effort, ribbonsObj.Effort_Ribbon },
            { Ribbon.Alert, ribbonsObj.Alert_Ribbon },
            { Ribbon.Shock, ribbonsObj.Shock_Ribbon },
            { Ribbon.Downcast, ribbonsObj.Downcast_Ribbon },
            { Ribbon.Careless, ribbonsObj.Careless_Ribbon },
            { Ribbon.Relax, ribbonsObj.Relax_Ribbon },
            { Ribbon.Snooze, ribbonsObj.Snooze_Ribbon },
            { Ribbon.Smile, ribbonsObj.Smile_Ribbon },
            { Ribbon.Gorgeous, ribbonsObj.Gorgeous_Ribbon },
            { Ribbon.Royal, ribbonsObj.Royal_Ribbon },
            { Ribbon.Gorgeous_Royal, ribbonsObj.Gorgeous_Royal_Ribbon },
            { Ribbon.Artist, ribbonsObj.Artist_Ribbon },
            { Ribbon.Footprint, ribbonsObj.Footprint_Ribbon },
            { Ribbon.Record, ribbonsObj.Record_Ribbon },
            { Ribbon.Legend, ribbonsObj.Legend_Ribbon },
            { Ribbon.Country, ribbonsObj.Country_Ribbon },
            { Ribbon.National, ribbonsObj.National_Ribbon },
            { Ribbon.Earth, ribbonsObj.Earth_Ribbon },
            { Ribbon.World, ribbonsObj.World_Ribbon },
            { Ribbon.Classic, ribbonsObj.Classic_Ribbon },
            { Ribbon.Premier, ribbonsObj.Premier_Ribbon },
            { Ribbon.Event, ribbonsObj.Event_Ribbon },
            { Ribbon.Birthday, ribbonsObj.Birthday_Ribbon },
            { Ribbon.Special, ribbonsObj.Special_Ribbon },
            { Ribbon.Souvenir, ribbonsObj.Souvenir_Ribbon },
            { Ribbon.Wishing, ribbonsObj.Wishing_Ribbon },
            { Ribbon.Battle_Champion, ribbonsObj.Battle_Champion_Ribbon },
            { Ribbon.Regional_Champion, ribbonsObj.Regional_Champion_Ribbon },
            { Ribbon.National_Champion, ribbonsObj.National_Champion_Ribbon },
            { Ribbon.World_Champion, ribbonsObj.World_Champion_Ribbon },
            { Ribbon.Contest_Memory, ribbonsObj.Contest_Memory_Ribbon },
            { Ribbon.Battle_Memory, ribbonsObj.Battle_Memory_Ribbon },
            { Ribbon.Hoenn_Champion, ribbonsObj.Hoenn_Champion_Ribbon },
            { Ribbon.Contest_Star, ribbonsObj.Contest_Star_Ribbon },
            { Ribbon.Coolness_Master, ribbonsObj.Coolness_Master_Ribbon },
            { Ribbon.Beauty_Master, ribbonsObj.Beauty_Master_Ribbon },
            { Ribbon.Cuteness_Master, ribbonsObj.Cuteness_Master_Ribbon },
            { Ribbon.Cleverness_Master, ribbonsObj.Cleverness_Master_Ribbon },
            { Ribbon.Toughness_Master, ribbonsObj.Toughness_Master_Ribbon },
            { Ribbon.Alola_Champion, ribbonsObj.Alola_Champion_Ribbon },
            { Ribbon.Battle_Royale_Master, ribbonsObj.Battle_Royale_Master_Ribbon },
            { Ribbon.Battle_Tree_Great, ribbonsObj.Battle_Tree_Great_Ribbon },
            { Ribbon.Battle_Tree_Master, ribbonsObj.Battle_Tree_Master_Ribbon },
            { Ribbon.Galar_Champion, ribbonsObj.Galar_Champion_Ribbon },
            { Ribbon.Tower_Master, ribbonsObj.Tower_Master_Ribbon },
            { Ribbon.Master_Rank, ribbonsObj.Master_Rank_Ribbon },
        };
    }
}