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
        Nickname_Field.Nickname.Switch(
            _ => ProcessEncodedNickname(),
            _ => ProcessStringNickname());
    }

    protected void ProcessStringNickname()
    {
        if (Is_Egg_Field?.Is_Egg.Value is true) //is egg
            Nickname_Field.Nickname.AsT1.Value = TagUtil.EGG_NICKNAME[Language_Field?.Value ?? Language.English];
        else
            Nickname_Field.Nickname.AsT1.Value = pku.Nickname.Value;
    }

    protected void ProcessEncodedNickname()
    {
        BAMStringField encodedName = Nickname_Field.Nickname.AsT0;

        BigInteger[] name;
        bool nicknameFlag = pku.Nickname_Flag.Value is true;
        Alert alert = null;
        int dex = TagUtil.GetNationalDexChecked(pku.Species.Value); //must be valid at this point

        if (!pku.Nickname.IsNull()) //specified
        {
            //name
            bool truncated, invalid;
            AlertType at = AlertType.NONE;
            (name, truncated, invalid) = DexUtil.CharEncoding.Encode(pku.Nickname.Value, encodedName.Length, FormatName, Language_Field.Value);
            if (truncated)
                at |= AlertType.TOO_LONG;
            if (invalid)
                at |= AlertType.INVALID;
            alert = GetNicknameAlert(at, encodedName.LangDependent, encodedName.Length);

            //flag
            if (pku.Nickname_Flag.IsNull())
                nicknameFlag = true;

            if (!nicknameFlag)
                alert += GetNicknameFlagAlert(false);
        }
        else //unspecified, get default name for given language
        {
            string defaultName;
            if (Is_Egg_Field?.Is_Egg.Value is true) //use egg name
                defaultName = TagUtil.EGG_NICKNAME[Language_Field.Value.Value];
            else //use default species name
                defaultName = PokeAPIUtil.GetSpeciesNameTranslated(dex, Language_Field.Value.Value);

            if (Nickname_CapitalizeDefault) //e.g. Gens 1-4 are capitalized by default
                defaultName = defaultName.ToUpperInvariant();
            
            (name, _, _) = DexUtil.CharEncoding.Encode(defaultName, encodedName.Length, FormatName, Language_Field.Value); //species names shouldn't be truncated/invalid...

            //flag
            if (pku.Nickname_Flag.IsNull())
                nicknameFlag = false;

            if (nicknameFlag)
                alert += GetNicknameFlagAlert(true, defaultName);
        }
        encodedName.Value = name;
        if (Nickname_Field.Nickname_Flag is not null)
            Nickname_Field.Nickname_Flag.Value = nicknameFlag;
        Warnings.Add(alert);
    }


    public Alert GetNicknameAlert(AlertType at, bool langDep, int? maxChars = null)
        => GetNicknameAlertBase(at, langDep, maxChars);

    public Alert GetNicknameAlertBase(AlertType at, bool langDep, int? maxChars = null)
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

    protected static Alert GetNicknameFlagAlert(bool flagset, string defaultName = null)
    {
        if (flagset && defaultName is null)
            throw new ArgumentNullException(nameof(defaultName), $"A default name must be specified when {nameof(flagset)} true.");
        return new("Nickname", flagset ?
            $"This pku's Nickname Flag is true, yet it doesn't have a nickname. Setting the nickname to: {defaultName}." :
            "This pku's Nickname Flag is false, yet it has a nickname.");
    }
}