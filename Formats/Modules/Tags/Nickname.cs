using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.Modules.Templates;
using System;
using System.Numerics;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Nickname_O
{
    public OneOf<BAMStringField, IField<string>> Nickname { get; }
}

public interface Nickname_E : StringTag_E
{
    public Nickname_O Nickname_Field { get; }
    public Is_Egg_O Is_Egg_Field => null;
    public bool Nickname_CapitalizeDefault => false;

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ExportLanguage),
                                                nameof(Is_Egg_E.ExportIs_Egg))]
    public void ExportNickname() => ExportNicknameBase();

    public void ExportNicknameBase()
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
        Nickname_Resolver = ExportString("Nickname", GetNickname, Nickname_Field.Nickname);
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger[]> Nickname_Resolver { get => null; set { } }
}

public interface Nickname_I : StringTag_I
{
    public Nickname_O Nickname_Field { get; }
    public Is_Egg_O Is_Egg_Field => null;
    public bool Nickname_CapitalizeDefault => false;

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_I.ImportLanguage),
                                                "ImportSpecies", "ImportIs_Egg")]
    public void ImportNickname() => ImportNicknameBase();

    public void ImportNicknameBase()
    {
        var errRes = ImportString("Nickname", pku.Nickname, Nickname_Field.Nickname);
        if (errRes is not null)
            Nickname_Resolver = () => errRes.DecideValue();

        //deal with default names (invalid langs have no default name)
        void dealWithDefault()
        {
            string checkName = TagUtil.GetDefaultName(pku.Species.Value, pku.IsEgg(), pku.Game_Info.Language.Value);
            if (Nickname_CapitalizeDefault)
                checkName = checkName?.ToUpperInvariant();
            if (pku.Nickname.Value == checkName)
                pku.Nickname.Value = null;
        }
        if (errRes is null)
            dealWithDefault();
        else
            Nickname_Resolver += dealWithDefault;
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public Action Nickname_Resolver { get => null; set { } }
}