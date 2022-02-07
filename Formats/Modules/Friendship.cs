using pkuManager.Formats.Fields;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Friendship_O
{
    public IIntegralField Friendship { get; }
}

public interface Friendship_E : NumericTag_E
{
    public Friendship_O Data { get; }

    public int Friendship_Default => 0;
    public bool Friendship_SilentUnspecified => true;

    [PorterDirective(ProcessingPhase.FirstPass)]
    protected void ProcessFriendship()
        => ProcessNumericTag("Friendship", pku.Friendship, Data.Friendship, Friendship_Default, Friendship_SilentUnspecified);
}