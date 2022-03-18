using pkuManager.Formats.Fields;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Friendship_O
{
    public IField<BigInteger> Friendship { get; }
}

public interface Friendship_E : NumericTag_E
{
    public Friendship_O Friendship_Field { get; }

    public int Friendship_Default => 0;
    public bool Friendship_SilentUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessFriendship()
        => ProcessNumericTag("Friendship", pku.Friendship, Friendship_Field.Friendship, Friendship_Default, Friendship_SilentUnspecified);
}