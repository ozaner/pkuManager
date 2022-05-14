using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;
using System.Collections.Generic;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Markings_O
{
    public IField<bool> Marking_Blue_Circle => null;
    public IField<bool> Marking_Blue_Square => null;
    public IField<bool> Marking_Blue_Triangle => null;
    public IField<bool> Marking_Blue_Heart => null;
    public IField<bool> Marking_Blue_Star => null;
    public IField<bool> Marking_Blue_Diamond => null;

    public IField<bool> Marking_Pink_Circle => null;
    public IField<bool> Marking_Pink_Square => null;
    public IField<bool> Marking_Pink_Triangle => null;
    public IField<bool> Marking_Pink_Heart => null;
    public IField<bool> Marking_Pink_Star => null;
    public IField<bool> Marking_Pink_Diamond => null;

    public IField<bool> Marking_Favorite => null;
}

public interface Markings_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMarkings()
        => EnumTagUtil<Marking>.ExportMultiEnumTag(GetMapping(), pku.Markings.ToEnumSet<Marking>());

    private Dictionary<Marking, IField<bool>> GetMapping()
    {
        Markings_O markingsObj = Data as Markings_O;
        return new()
        {
            { Marking.Blue_Circle, markingsObj.Marking_Blue_Circle },
            { Marking.Blue_Square, markingsObj.Marking_Blue_Square },
            { Marking.Blue_Triangle, markingsObj.Marking_Blue_Triangle },
            { Marking.Blue_Heart, markingsObj.Marking_Blue_Heart },
            { Marking.Blue_Star, markingsObj.Marking_Blue_Star },
            { Marking.Blue_Diamond, markingsObj.Marking_Blue_Diamond },

            { Marking.Pink_Circle, markingsObj.Marking_Pink_Circle },
            { Marking.Pink_Square, markingsObj.Marking_Pink_Square },
            { Marking.Pink_Triangle, markingsObj.Marking_Pink_Triangle },
            { Marking.Pink_Heart, markingsObj.Marking_Pink_Heart },
            { Marking.Pink_Star, markingsObj.Marking_Pink_Star },
            { Marking.Pink_Diamond, markingsObj.Marking_Pink_Diamond },

            { Marking.Favorite, markingsObj.Marking_Favorite }
        };
    } 
}