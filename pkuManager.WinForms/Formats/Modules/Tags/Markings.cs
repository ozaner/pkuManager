using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Modules.Templates;
using pkuManager.WinForms.Utilities;
using System.Collections.Generic;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats.Modules.Tags;

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

    public Dictionary<Marking, IField<bool>> GetMapping() => new()
    {
        { Marking.Blue_Circle, Marking_Blue_Circle },
        { Marking.Blue_Square, Marking_Blue_Square },
        { Marking.Blue_Triangle, Marking_Blue_Triangle },
        { Marking.Blue_Heart, Marking_Blue_Heart },
        { Marking.Blue_Star, Marking_Blue_Star },
        { Marking.Blue_Diamond, Marking_Blue_Diamond },

        { Marking.Pink_Circle, Marking_Pink_Circle },
        { Marking.Pink_Square, Marking_Pink_Square },
        { Marking.Pink_Triangle, Marking_Pink_Triangle },
        { Marking.Pink_Heart, Marking_Pink_Heart },
        { Marking.Pink_Star, Marking_Pink_Star },
        { Marking.Pink_Diamond, Marking_Pink_Diamond },

        { Marking.Favorite, Marking_Favorite }
    };
}

public interface Markings_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMarkings()
        => EnumTagUtil<Marking>.ExportMultiEnumTag((Data as Markings_O).GetMapping(), pku.Markings.ToEnumSet<Marking>());
}