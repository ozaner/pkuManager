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
    public Ribbons_O Ribbons_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportRibbons() => ExportRibbonsBase();

    public void ExportRibbonsBase()
        => ExportMultiEnumTag(GetMapping(), pku.Ribbons.ToEnumSet<Ribbon>());

    private Dictionary<Ribbon, IField<bool>> GetMapping() => new()
    {
        { Ribbon.Cool_G3, Ribbons_Field.Cool_G3_Ribbon },
        { Ribbon.Cool_Super_G3, Ribbons_Field.Cool_Super_G3_Ribbon },
        { Ribbon.Cool_Hyper_G3, Ribbons_Field.Cool_Hyper_G3_Ribbon },
        { Ribbon.Cool_Master_G3, Ribbons_Field.Cool_Master_G3_Ribbon },
        { Ribbon.Beauty_G3, Ribbons_Field.Beauty_G3_Ribbon },
        { Ribbon.Beauty_Super_G3, Ribbons_Field.Beauty_Super_G3_Ribbon },
        { Ribbon.Beauty_Hyper_G3, Ribbons_Field.Beauty_Hyper_G3_Ribbon },
        { Ribbon.Beauty_Master_G3, Ribbons_Field.Beauty_Master_G3_Ribbon },
        { Ribbon.Cute_G3, Ribbons_Field.Cute_G3_Ribbon },
        { Ribbon.Cute_Super_G3, Ribbons_Field.Cute_Super_G3_Ribbon },
        { Ribbon.Cute_Hyper_G3, Ribbons_Field.Cute_Hyper_G3_Ribbon },
        { Ribbon.Cute_Master_G3, Ribbons_Field.Cute_Master_G3_Ribbon },
        { Ribbon.Smart_G3, Ribbons_Field.Smart_G3_Ribbon },
        { Ribbon.Smart_Super_G3, Ribbons_Field.Smart_Super_G3_Ribbon },
        { Ribbon.Smart_Hyper_G3, Ribbons_Field.Smart_Hyper_G3_Ribbon },
        { Ribbon.Smart_Master_G3, Ribbons_Field.Smart_Master_G3_Ribbon },
        { Ribbon.Tough_G3, Ribbons_Field.Tough_G3_Ribbon },
        { Ribbon.Tough_Super_G3, Ribbons_Field.Tough_Super_G3_Ribbon },
        { Ribbon.Tough_Hyper_G3, Ribbons_Field.Tough_Hyper_G3_Ribbon },
        { Ribbon.Tough_Master_G3, Ribbons_Field.Tough_Master_G3_Ribbon },
        { Ribbon.Cool_G4, Ribbons_Field.Cool_G4_Ribbon },
        { Ribbon.Cool_Great_G4, Ribbons_Field.Cool_Great_G4_Ribbon },
        { Ribbon.Cool_Ultra_G4, Ribbons_Field.Cool_Ultra_G4_Ribbon },
        { Ribbon.Cool_Master_G4, Ribbons_Field.Cool_Master_G4_Ribbon },
        { Ribbon.Beauty_G4, Ribbons_Field.Beauty_G4_Ribbon },
        { Ribbon.Beauty_Great_G4, Ribbons_Field.Beauty_Great_G4_Ribbon },
        { Ribbon.Beauty_Ultra_G4, Ribbons_Field.Beauty_Ultra_G4_Ribbon },
        { Ribbon.Beauty_Master_G4, Ribbons_Field.Beauty_Master_G4_Ribbon },
        { Ribbon.Cute_G4, Ribbons_Field.Cute_G4_Ribbon },
        { Ribbon.Cute_Great_G4, Ribbons_Field.Cute_Great_G4_Ribbon },
        { Ribbon.Cute_Ultra_G4, Ribbons_Field.Cute_Ultra_G4_Ribbon },
        { Ribbon.Cute_Master_G4, Ribbons_Field.Cute_Master_G4_Ribbon },
        { Ribbon.Smart_G4, Ribbons_Field.Smart_G4_Ribbon },
        { Ribbon.Smart_Great_G4, Ribbons_Field.Smart_Great_G4_Ribbon },
        { Ribbon.Smart_Ultra_G4, Ribbons_Field.Smart_Ultra_G4_Ribbon },
        { Ribbon.Smart_Master_G4, Ribbons_Field.Smart_Master_G4_Ribbon },
        { Ribbon.Tough_G4, Ribbons_Field.Tough_G4_Ribbon },
        { Ribbon.Tough_Great_G4, Ribbons_Field.Tough_Great_G4_Ribbon },
        { Ribbon.Tough_Ultra_G4, Ribbons_Field.Tough_Ultra_G4_Ribbon },
        { Ribbon.Tough_Master_G4, Ribbons_Field.Tough_Master_G4_Ribbon },
        { Ribbon.Winning, Ribbons_Field.Winning_Ribbon },
        { Ribbon.Victory, Ribbons_Field.Victory_Ribbon },
        { Ribbon.Ability, Ribbons_Field.Ability_Ribbon },
        { Ribbon.Great_Ability, Ribbons_Field.Great_Ability_Ribbon },
        { Ribbon.Double_Ability, Ribbons_Field.Double_Ability_Ribbon },
        { Ribbon.Multi_Ability, Ribbons_Field.Multi_Ability_Ribbon },
        { Ribbon.Pair_Ability, Ribbons_Field.Pair_Ability_Ribbon },
        { Ribbon.World_Ability, Ribbons_Field.World_Ability_Ribbon },
        { Ribbon.Kalos_Champion, Ribbons_Field.Kalos_Champion_Ribbon },
        { Ribbon.Champion, Ribbons_Field.Champion_Ribbon },
        { Ribbon.Sinnoh_Champion, Ribbons_Field.Sinnoh_Champion_Ribbon },
        { Ribbon.Best_Friends, Ribbons_Field.Best_Friends_Ribbon },
        { Ribbon.Training, Ribbons_Field.Training_Ribbon },
        { Ribbon.Skillful_Battler, Ribbons_Field.Skillful_Battler_Ribbon },
        { Ribbon.Expert_Battler, Ribbons_Field.Expert_Battler_Ribbon },
        { Ribbon.Effort, Ribbons_Field.Effort_Ribbon },
        { Ribbon.Alert, Ribbons_Field.Alert_Ribbon },
        { Ribbon.Shock, Ribbons_Field.Shock_Ribbon },
        { Ribbon.Downcast, Ribbons_Field.Downcast_Ribbon },
        { Ribbon.Careless, Ribbons_Field.Careless_Ribbon },
        { Ribbon.Relax, Ribbons_Field.Relax_Ribbon },
        { Ribbon.Snooze, Ribbons_Field.Snooze_Ribbon },
        { Ribbon.Smile, Ribbons_Field.Smile_Ribbon },
        { Ribbon.Gorgeous, Ribbons_Field.Gorgeous_Ribbon },
        { Ribbon.Royal, Ribbons_Field.Royal_Ribbon },
        { Ribbon.Gorgeous_Royal, Ribbons_Field.Gorgeous_Royal_Ribbon },
        { Ribbon.Artist, Ribbons_Field.Artist_Ribbon },
        { Ribbon.Footprint, Ribbons_Field.Footprint_Ribbon },
        { Ribbon.Record, Ribbons_Field.Record_Ribbon },
        { Ribbon.Legend, Ribbons_Field.Legend_Ribbon },
        { Ribbon.Country, Ribbons_Field.Country_Ribbon },
        { Ribbon.National, Ribbons_Field.National_Ribbon },
        { Ribbon.Earth, Ribbons_Field.Earth_Ribbon },
        { Ribbon.World, Ribbons_Field.World_Ribbon },
        { Ribbon.Classic, Ribbons_Field.Classic_Ribbon },
        { Ribbon.Premier, Ribbons_Field.Premier_Ribbon },
        { Ribbon.Event, Ribbons_Field.Event_Ribbon },
        { Ribbon.Birthday, Ribbons_Field.Birthday_Ribbon },
        { Ribbon.Special, Ribbons_Field.Special_Ribbon },
        { Ribbon.Souvenir, Ribbons_Field.Souvenir_Ribbon },
        { Ribbon.Wishing, Ribbons_Field.Wishing_Ribbon },
        { Ribbon.Battle_Champion, Ribbons_Field.Battle_Champion_Ribbon },
        { Ribbon.Regional_Champion, Ribbons_Field.Regional_Champion_Ribbon },
        { Ribbon.National_Champion, Ribbons_Field.National_Champion_Ribbon },
        { Ribbon.World_Champion, Ribbons_Field.World_Champion_Ribbon },
        { Ribbon.Contest_Memory, Ribbons_Field.Contest_Memory_Ribbon },
        { Ribbon.Battle_Memory, Ribbons_Field.Battle_Memory_Ribbon },
        { Ribbon.Hoenn_Champion, Ribbons_Field.Hoenn_Champion_Ribbon },
        { Ribbon.Contest_Star, Ribbons_Field.Contest_Star_Ribbon },
        { Ribbon.Coolness_Master, Ribbons_Field.Coolness_Master_Ribbon },
        { Ribbon.Beauty_Master, Ribbons_Field.Beauty_Master_Ribbon },
        { Ribbon.Cuteness_Master, Ribbons_Field.Cuteness_Master_Ribbon },
        { Ribbon.Cleverness_Master, Ribbons_Field.Cleverness_Master_Ribbon },
        { Ribbon.Toughness_Master, Ribbons_Field.Toughness_Master_Ribbon },
        { Ribbon.Alola_Champion, Ribbons_Field.Alola_Champion_Ribbon },
        { Ribbon.Battle_Royale_Master, Ribbons_Field.Battle_Royale_Master_Ribbon },
        { Ribbon.Battle_Tree_Great, Ribbons_Field.Battle_Tree_Great_Ribbon },
        { Ribbon.Battle_Tree_Master, Ribbons_Field.Battle_Tree_Master_Ribbon },
        { Ribbon.Galar_Champion, Ribbons_Field.Galar_Champion_Ribbon },
        { Ribbon.Tower_Master, Ribbons_Field.Tower_Master_Ribbon },
        { Ribbon.Master_Rank, Ribbons_Field.Master_Rank_Ribbon },
    };
}