using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using pkuManager.WinForms.Utilities;
using System.Collections.Generic;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

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

    public Dictionary<Ribbon, IField<bool>> GetMapping() => new()
    {
        { Ribbon.Cool_G3, Cool_G3_Ribbon },
        { Ribbon.Cool_Super_G3, Cool_Super_G3_Ribbon },
        { Ribbon.Cool_Hyper_G3, Cool_Hyper_G3_Ribbon },
        { Ribbon.Cool_Master_G3, Cool_Master_G3_Ribbon },
        { Ribbon.Beauty_G3, Beauty_G3_Ribbon },
        { Ribbon.Beauty_Super_G3, Beauty_Super_G3_Ribbon },
        { Ribbon.Beauty_Hyper_G3, Beauty_Hyper_G3_Ribbon },
        { Ribbon.Beauty_Master_G3, Beauty_Master_G3_Ribbon },
        { Ribbon.Cute_G3, Cute_G3_Ribbon },
        { Ribbon.Cute_Super_G3, Cute_Super_G3_Ribbon },
        { Ribbon.Cute_Hyper_G3, Cute_Hyper_G3_Ribbon },
        { Ribbon.Cute_Master_G3, Cute_Master_G3_Ribbon },
        { Ribbon.Smart_G3, Smart_G3_Ribbon },
        { Ribbon.Smart_Super_G3, Smart_Super_G3_Ribbon },
        { Ribbon.Smart_Hyper_G3, Smart_Hyper_G3_Ribbon },
        { Ribbon.Smart_Master_G3, Smart_Master_G3_Ribbon },
        { Ribbon.Tough_G3, Tough_G3_Ribbon },
        { Ribbon.Tough_Super_G3, Tough_Super_G3_Ribbon },
        { Ribbon.Tough_Hyper_G3, Tough_Hyper_G3_Ribbon },
        { Ribbon.Tough_Master_G3, Tough_Master_G3_Ribbon },
        { Ribbon.Cool_G4, Cool_G4_Ribbon },
        { Ribbon.Cool_Great_G4, Cool_Great_G4_Ribbon },
        { Ribbon.Cool_Ultra_G4, Cool_Ultra_G4_Ribbon },
        { Ribbon.Cool_Master_G4, Cool_Master_G4_Ribbon },
        { Ribbon.Beauty_G4, Beauty_G4_Ribbon },
        { Ribbon.Beauty_Great_G4, Beauty_Great_G4_Ribbon },
        { Ribbon.Beauty_Ultra_G4, Beauty_Ultra_G4_Ribbon },
        { Ribbon.Beauty_Master_G4, Beauty_Master_G4_Ribbon },
        { Ribbon.Cute_G4, Cute_G4_Ribbon },
        { Ribbon.Cute_Great_G4, Cute_Great_G4_Ribbon },
        { Ribbon.Cute_Ultra_G4, Cute_Ultra_G4_Ribbon },
        { Ribbon.Cute_Master_G4, Cute_Master_G4_Ribbon },
        { Ribbon.Smart_G4, Smart_G4_Ribbon },
        { Ribbon.Smart_Great_G4, Smart_Great_G4_Ribbon },
        { Ribbon.Smart_Ultra_G4, Smart_Ultra_G4_Ribbon },
        { Ribbon.Smart_Master_G4, Smart_Master_G4_Ribbon },
        { Ribbon.Tough_G4, Tough_G4_Ribbon },
        { Ribbon.Tough_Great_G4, Tough_Great_G4_Ribbon },
        { Ribbon.Tough_Ultra_G4, Tough_Ultra_G4_Ribbon },
        { Ribbon.Tough_Master_G4, Tough_Master_G4_Ribbon },
        { Ribbon.Winning, Winning_Ribbon },
        { Ribbon.Victory, Victory_Ribbon },
        { Ribbon.Ability, Ability_Ribbon },
        { Ribbon.Great_Ability, Great_Ability_Ribbon },
        { Ribbon.Double_Ability, Double_Ability_Ribbon },
        { Ribbon.Multi_Ability, Multi_Ability_Ribbon },
        { Ribbon.Pair_Ability, Pair_Ability_Ribbon },
        { Ribbon.World_Ability, World_Ability_Ribbon },
        { Ribbon.Kalos_Champion, Kalos_Champion_Ribbon },
        { Ribbon.Champion, Champion_Ribbon },
        { Ribbon.Sinnoh_Champion, Sinnoh_Champion_Ribbon },
        { Ribbon.Best_Friends, Best_Friends_Ribbon },
        { Ribbon.Training, Training_Ribbon },
        { Ribbon.Skillful_Battler, Skillful_Battler_Ribbon },
        { Ribbon.Expert_Battler, Expert_Battler_Ribbon },
        { Ribbon.Effort, Effort_Ribbon },
        { Ribbon.Alert, Alert_Ribbon },
        { Ribbon.Shock, Shock_Ribbon },
        { Ribbon.Downcast, Downcast_Ribbon },
        { Ribbon.Careless, Careless_Ribbon },
        { Ribbon.Relax, Relax_Ribbon },
        { Ribbon.Snooze, Snooze_Ribbon },
        { Ribbon.Smile, Smile_Ribbon },
        { Ribbon.Gorgeous, Gorgeous_Ribbon },
        { Ribbon.Royal, Royal_Ribbon },
        { Ribbon.Gorgeous_Royal, Gorgeous_Royal_Ribbon },
        { Ribbon.Artist, Artist_Ribbon },
        { Ribbon.Footprint, Footprint_Ribbon },
        { Ribbon.Record, Record_Ribbon },
        { Ribbon.Legend, Legend_Ribbon },
        { Ribbon.Country, Country_Ribbon },
        { Ribbon.National, National_Ribbon },
        { Ribbon.Earth, Earth_Ribbon },
        { Ribbon.World, World_Ribbon },
        { Ribbon.Classic, Classic_Ribbon },
        { Ribbon.Premier, Premier_Ribbon },
        { Ribbon.Event, Event_Ribbon },
        { Ribbon.Birthday, Birthday_Ribbon },
        { Ribbon.Special, Special_Ribbon },
        { Ribbon.Souvenir, Souvenir_Ribbon },
        { Ribbon.Wishing, Wishing_Ribbon },
        { Ribbon.Battle_Champion, Battle_Champion_Ribbon },
        { Ribbon.Regional_Champion, Regional_Champion_Ribbon },
        { Ribbon.National_Champion, National_Champion_Ribbon },
        { Ribbon.World_Champion, World_Champion_Ribbon },
        { Ribbon.Contest_Memory, Contest_Memory_Ribbon },
        { Ribbon.Battle_Memory, Battle_Memory_Ribbon },
        { Ribbon.Hoenn_Champion, Hoenn_Champion_Ribbon },
        { Ribbon.Contest_Star, Contest_Star_Ribbon },
        { Ribbon.Coolness_Master, Coolness_Master_Ribbon },
        { Ribbon.Beauty_Master, Beauty_Master_Ribbon },
        { Ribbon.Cuteness_Master, Cuteness_Master_Ribbon },
        { Ribbon.Cleverness_Master, Cleverness_Master_Ribbon },
        { Ribbon.Toughness_Master, Toughness_Master_Ribbon },
        { Ribbon.Alola_Champion, Alola_Champion_Ribbon },
        { Ribbon.Battle_Royale_Master, Battle_Royale_Master_Ribbon },
        { Ribbon.Battle_Tree_Great, Battle_Tree_Great_Ribbon },
        { Ribbon.Battle_Tree_Master, Battle_Tree_Master_Ribbon },
        { Ribbon.Galar_Champion, Galar_Champion_Ribbon },
        { Ribbon.Tower_Master, Tower_Master_Ribbon },
        { Ribbon.Master_Rank, Master_Rank_Ribbon },
    };
}

public interface Ribbons_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportRibbons() => ExportRibbonsBase();

    public void ExportRibbonsBase()
        => EnumTagUtil<Ribbon>.ExportMultiEnumTag((Data as Ribbons_O).GetMapping(), pku.Ribbons.ToEnumSet<Ribbon>());
}