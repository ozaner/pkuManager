using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
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

public interface Markings_E : MultiEnumTag_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public Markings_O Markings_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportMarkings()
        => ExportMultiEnumTag(GetMapping(), pku.Markings.ToEnumSet<Marking>());

    private Dictionary<Marking, IField<bool>> GetMapping() => new()
    {
        { Marking.Blue_Circle, Markings_Field.Marking_Blue_Circle },
        { Marking.Blue_Square, Markings_Field.Marking_Blue_Square },
        { Marking.Blue_Triangle, Markings_Field.Marking_Blue_Triangle },
        { Marking.Blue_Heart, Markings_Field.Marking_Blue_Heart },
        { Marking.Blue_Star, Markings_Field.Marking_Blue_Star },
        { Marking.Blue_Diamond, Markings_Field.Marking_Blue_Diamond },

        { Marking.Pink_Circle, Markings_Field.Marking_Pink_Circle },
        { Marking.Pink_Square, Markings_Field.Marking_Pink_Square },
        { Marking.Pink_Triangle, Markings_Field.Marking_Pink_Triangle },
        { Marking.Pink_Heart, Markings_Field.Marking_Pink_Heart },
        { Marking.Pink_Star, Markings_Field.Marking_Pink_Star },
        { Marking.Pink_Diamond, Markings_Field.Marking_Pink_Diamond },

        { Marking.Favorite, Markings_Field.Marking_Favorite}
    };
}