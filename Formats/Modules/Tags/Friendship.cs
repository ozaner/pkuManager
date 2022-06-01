using pkuManager.Formats.Fields;
using pkuManager.Formats.Modules.Templates;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Friendship_O
{
    public IIntField Friendship { get; }
    public int Friendship_Default => 0;
}

public interface Friendship_E : Tag
{
    [PorterDirective(ProcessingPhase.FirstPass)]
    public void ExportFriendship()
    {
        //if egg steps field is stored in friendship, dont process friendship unless if pku is egg
        if (pku.IsEgg() && (Data as Egg_Steps_O)?.Egg_Steps_StoredInFriendship == true)
            return;

        Friendship_O friendshipObj = Data as Friendship_O;
        AlertType at = NumericTagUtil.ExportNumericTag(pku.Friendship, friendshipObj.Friendship, friendshipObj.Friendship_Default);
        if (at is not AlertType.UNSPECIFIED) //ignore unspecified
            Warnings.Add(NumericTagUtil.GetNumericAlert("Friendship", at, friendshipObj.Friendship_Default, friendshipObj.Friendship));
    }
}