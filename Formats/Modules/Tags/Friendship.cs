using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Friendship_O
{
    public IField<BigInteger> Friendship { get; }
    public int Friendship_Default => 0;
}

public interface Friendship_E : NumericTag_E
{
    public Friendship_O Friendship_Field { get; }

    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportFriendship()
        => ExportNumericTag("Friendship", pku.Friendship, Friendship_Field.Friendship, Friendship_Field.Friendship_Default, false);
}