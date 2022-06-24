using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Egg_Location_O
{
    public IIntField Egg_Location { get; }
}

public interface Egg_Location_E : LocationTag_E
{
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Origin_Game_E.ExportOrigin_Game))]
    public void ExportEgg_Location()
        => ExportLocation("Egg Location", pku.Egg_Info.Met_Location, (Data as Egg_Location_O).Egg_Location);
}