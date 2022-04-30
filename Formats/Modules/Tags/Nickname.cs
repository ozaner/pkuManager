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
    public bool Nickname_CapitalizeDefault => false;
}

public interface Nickname_E : StringTag_E
{
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ExportLanguage),
                                                nameof(Is_Egg_E.ExportIs_Egg))]
    public void ExportNickname() => ExportNicknameBase();

    public void ExportNicknameBase()
    {
        Nickname_O nicknameObj = Data as Nickname_O;
        string GetNickname(string lang)
        {
            string nickname;
            if (pku.Nickname.IsNull()) //invalid langs have no default name
            {
                nickname = TagUtil.GetDefaultName(pku.Species.Value, (Data as Is_Egg_O)?.Is_Egg.Value is true, lang);
                if (nicknameObj.Nickname_CapitalizeDefault)
                    nickname = nickname?.ToUpperInvariant();
            }
            else
                nickname = pku.Nickname.Value;
            return nickname;
        }
        Nickname_Resolver = ExportString("Nickname", GetNickname, nicknameObj.Nickname);
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public ErrorResolver<BigInteger[]> Nickname_Resolver { get => null; set { } }
}

public interface Nickname_I : StringTag_I
{
    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_I.ImportLanguage),
                                                "ImportSpecies", nameof(Is_Egg_I.ImportIs_Egg))]
    public void ImportNickname() => ImportNicknameBase();

    public void ImportNicknameBase()
    {
        var errRes = ImportString("Nickname", pku.Nickname, (Data as Nickname_O).Nickname);

        //deal with default names (invalid langs have no default name)
        void dealWithDefault()
        {
            string checkName = TagUtil.GetDefaultName(pku.Species.Value, pku.IsEgg(), pku.Game_Info.Language.Value);
            if ((Data as Nickname_O).Nickname_CapitalizeDefault)
                checkName = checkName?.ToUpperInvariant();
            if (pku.Nickname.Value == checkName)
                pku.Nickname.Value = null;
        }
        if (errRes is null)
            dealWithDefault();
        else
            Nickname_Resolver = (Action)errRes + dealWithDefault;
    }

    [PorterDirective(ProcessingPhase.SecondPass)]
    public Action Nickname_Resolver { get => null; set { } }
}