using OneOf;
using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.Tags;

public interface Nickname_O
{
    public OneOf<BAMStringField, IField<string>> Nickname { get; }
    public IField<bool> Nickname_Flag => null; //no nickname flag by default
}

public interface Nickname_E
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }
    public string FormatName { get; }

    public Nickname_O Nickname_Field { get; }
    public Language_O Language_Field => null;
    public Is_Egg_O Is_Egg_Field => null;
    public bool Nickname_CapitalizeDefault => false;

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ProcessLanguage),
                                                nameof(Is_Egg_E.ProcessIs_Egg))]
    public void ProcessNickname() => ProcessNicknameBase();

    public void ProcessNicknameBase()
    {
        //calculate nickname
        string nickname = null;
        if (pku.Nickname.IsNull()) //invalid langs have no default name
            nickname = TagUtil.GetDefaultName(pku.Species.Value, Is_Egg_Field?.Is_Egg.Value is true, Language_Field?.AsString);
        nickname ??= pku.Nickname.Value;

        Alert alert = null;
        alert += Nickname_Field.Nickname.Match(
            x => ProcessEncodedNickname(nickname, x),
            x => ProcessStringNickname(nickname, x));

        Warnings.Add(alert);
    }

    protected Alert ProcessStringNickname(string nickname, IField<string> nicknameField)
    {
        nicknameField.Value = nickname;
        return null;
    }

    protected Alert ProcessEncodedNickname(string nickname, IField<BigInteger[]> nicknameField)
    {
        BigInteger[] name;
        bool nicknameFlag = pku.Nickname_Flag.Value is true;
        Alert alert = null;

        string checkedLang = Language_Field?.AsString;
        bool langDep = DexUtil.CharEncoding.IsLangDependent(FormatName);
        if (langDep && !Language_Field.IsValid())
        {
            checkedLang = TagUtil.DEFAULT_SEMANTIC_LANGUAGE;
            alert += GetNicknameLangAlert(checkedLang);
        }

        if (!pku.Nickname.IsNull()) //specified
        {
            //name
            bool truncated, invalid;
            AlertType at = AlertType.NONE;
            (name, truncated, invalid) = DexUtil.CharEncoding.Encode(pku.Nickname.Value,
                nicknameField.Value.Length, FormatName, checkedLang);
            if (truncated)
                at |= AlertType.TOO_LONG;
            if (invalid)
                at |= AlertType.INVALID;
            alert += GetNicknameAlert(at, DexUtil.CharEncoding.IsLangDependent(FormatName), nicknameField.Value.Length);

            //flag
            if (pku.Nickname_Flag.IsNull())
                nicknameFlag = true;

            if (!nicknameFlag)
                alert += GetNicknameFlagAlert(false);
        }
        else //unspecified, get default name for given language
        {
            string defaultName = TagUtil.GetDefaultName(pku.Species.Value, Is_Egg_Field?.Is_Egg.Value is true, checkedLang);
            if (Nickname_CapitalizeDefault) //e.g. Gens 1-4 are capitalized by default
                defaultName = defaultName.ToUpperInvariant();

            //species names shouldn't be truncated/invalid...
            (name, _, _) = DexUtil.CharEncoding.Encode(defaultName, nicknameField.Value.Length, FormatName, checkedLang);

            //flag
            if (pku.Nickname_Flag.IsNull())
                nicknameFlag = false;

            if (nicknameFlag)
                alert += GetNicknameFlagAlert(true, defaultName);
        }
        nicknameField.Value = name;
        if (Nickname_Field.Nickname_Flag is not null)
            Nickname_Field.Nickname_Flag.Value = nicknameFlag;
        return alert;
    }


    public Alert GetNicknameAlert(AlertType at, bool langDep, int? maxChars = null)
    {
        if (at == AlertType.NONE)
            return null;

        string msg = "";
        if (at.HasFlag(AlertType.INVALID)) //some characters invalid, removing them
            msg += $"Some of the characters in the nickname are invalid in this format{(langDep ? "/language" : "")}, removing them.";
        if (at.HasFlag(AlertType.TOO_LONG)) //too many characters, truncating
        {
            if (maxChars is null)
                throw new ArgumentNullException(nameof(maxChars), "maximum # of chars must be specified for TOO_LONG alerts.");
            if (msg is not "")
                msg += DataUtil.Newline();
            msg += $"Nickname can only have {maxChars} characters in this format, truncating it.";
        }
        return msg is not "" ? new("Nickname", msg) : throw InvalidAlertType();
    }

    public Alert GetNicknameLangAlert(string encodingLang)
        => new("Nickname", $"Language is invalid, encoding the nickname using {encodingLang}.");

    protected static Alert GetNicknameFlagAlert(bool flagset, string defaultName = null)
    {
        if (flagset && defaultName is null)
            throw new ArgumentNullException(nameof(defaultName), $"A default name must be specified when {nameof(flagset)} true.");
        return new("Nickname", flagset ?
            $"This pku's Nickname Flag is true, yet it doesn't have a nickname. Setting the nickname to: {defaultName}." :
            "This pku's Nickname Flag is false, yet it has a nickname.");
    }
}