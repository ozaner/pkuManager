﻿using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Met_Location_O
{
    public IIntField Met_Location { get; }
}

public interface Met_Location_E : LocationTag_E
{
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Origin_Game_E.ExportOrigin_Game))]
    public void ExportMet_Location()
        => ExportLocation("Met Location", pku.Catch_Info.Met_Location, (Data as Met_Location_O).Met_Location);
}