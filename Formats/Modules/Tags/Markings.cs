using pkuManager.Formats.Fields;
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

public interface Markings_E
{
    public pkuObject pku { get; }
    public string FormatName { get; }

    public Markings_O Markings_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public sealed void ProcessMarkings()
    {
        HashSet<Marking> markings = pku.Markings.ToEnumSet<Marking>();

        //blue markings
        if (Markings_Field.Marking_Blue_Circle is not null)
            Markings_Field.Marking_Blue_Circle.Value = markings.Contains(Marking.Blue_Circle);

        if (Markings_Field.Marking_Blue_Triangle is not null)
            Markings_Field.Marking_Blue_Triangle.Value = markings.Contains(Marking.Blue_Triangle);

        if (Markings_Field.Marking_Blue_Square is not null)
            Markings_Field.Marking_Blue_Square.Value = markings.Contains(Marking.Blue_Square);

        if (Markings_Field.Marking_Blue_Heart is not null)
            Markings_Field.Marking_Blue_Heart.Value = markings.Contains(Marking.Blue_Heart);

        if (Markings_Field.Marking_Blue_Star is not null)
            Markings_Field.Marking_Blue_Star.Value = markings.Contains(Marking.Blue_Star);

        if (Markings_Field.Marking_Blue_Diamond is not null)
            Markings_Field.Marking_Blue_Diamond.Value = markings.Contains(Marking.Blue_Diamond);

        //pink markings
        if (Markings_Field.Marking_Pink_Circle is not null)
            Markings_Field.Marking_Pink_Circle.Value = markings.Contains(Marking.Pink_Circle);

        if (Markings_Field.Marking_Pink_Triangle is not null)
            Markings_Field.Marking_Pink_Triangle.Value = markings.Contains(Marking.Pink_Triangle);

        if (Markings_Field.Marking_Pink_Square is not null)
            Markings_Field.Marking_Pink_Square.Value = markings.Contains(Marking.Pink_Square);

        if (Markings_Field.Marking_Pink_Heart is not null)
            Markings_Field.Marking_Pink_Heart.Value = markings.Contains(Marking.Pink_Heart);

        if (Markings_Field.Marking_Pink_Star is not null)
            Markings_Field.Marking_Pink_Star.Value = markings.Contains(Marking.Pink_Star);

        if (Markings_Field.Marking_Pink_Diamond is not null)
            Markings_Field.Marking_Pink_Diamond.Value = markings.Contains(Marking.Pink_Diamond);

        //favorite
        if (Markings_Field.Marking_Favorite is not null)
            Markings_Field.Marking_Favorite.Value = markings.Contains(Marking.Favorite);
    }
}