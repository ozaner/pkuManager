using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Formats.pku;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Nickname_O
{
    public OneOf<BAMStringField, IField<string>> Nickname { get; }
}

public interface Nickname_E : StringTag_E
{
    public pkuObject pku { get; }
    public Nickname_O Nickname_Field { get; }
    public Is_Egg_O Is_Egg_Field => null;
    public bool Nickname_CapitalizeDefault => false;

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ProcessLanguage),
                                                nameof(Is_Egg_E.ProcessIs_Egg))]
    public void ProcessNickname() => ProcessNicknameBase();

    public void ProcessNicknameBase()
    {
        string GetNickname(string lang)
        {
            string nickname;
            if (pku.Nickname.IsNull()) //invalid langs have no default name
            {
                nickname = TagUtil.GetDefaultName(pku.Species.Value, Is_Egg_Field?.Is_Egg.Value is true, lang);
                if (Nickname_CapitalizeDefault)
                    nickname = nickname?.ToUpperInvariant();
            }
            else
                nickname = pku.Nickname.Value;
            return nickname;
        }
        Nickname_Resolver = ProcessString("Nickname", GetNickname, Nickname_Field.Nickname);
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger[]> Nickname_Resolver { get => null; set { } }
}