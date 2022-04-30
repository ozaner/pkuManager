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
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportFriendship()
    {
        Friendship_O friendshipObj = Data as Friendship_O;
        ExportNumericTag("Friendship", pku.Friendship, friendshipObj.Friendship, friendshipObj.Friendship_Default, false);
    }
}