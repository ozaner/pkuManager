using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.Fields.BAMFields;
using pkuManager.Formats.pku;
using pkuManager.Formats.pkx;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface Nickname_O<T> where T : struct
{
    public BAMStringField<T> Nickname { get; }
    public virtual IField<bool> Nickname_Flag => null; //no nickname flag by default
}

public interface Nickname_E<T> where T : struct
{
    public pkuObject pku { get; }
    public List<Alert> Warnings { get; }
    public string FormatName { get; }

    public Nickname_O<T> Data { get; }
    public Language_O Language_Data { get; }
    public virtual bool Nickname_CapitalizeDefault => false;

    [PorterDirective(ProcessingPhase.FirstPass, nameof(Language_E.ProcessLanguage))]
    public virtual void ProcessNickname() => ProcessNicknameBase();

    public void ProcessNicknameBase()
    {
        T[] name;
        bool nicknameFlag = pku.Nickname_Flag is true;
        Alert alert = null;
        Alert flagAlert = null;
        int dex = pkxUtil.GetNationalDexChecked(pku.Species); //must be valid at this point

        if (pku.Nickname is not null) //specified
        {
            //name
            bool truncated, invalid;
            (name, truncated, invalid) = DexUtil.CharEncoding<T>.Encode(pku.Nickname, Data.Nickname.Length, FormatName, Language_Data.Value);
            if (truncated && invalid)
                alert = GetNicknameAlert(AlertType.OVERFLOW, Data.Nickname.Length, AlertType.INVALID);
            else if (truncated)
                alert = GetNicknameAlert(AlertType.OVERFLOW, Data.Nickname.Length);
            else if (invalid)
                alert = GetNicknameAlert(AlertType.INVALID);

            //flag
            if (pku.Nickname_Flag is null)
                nicknameFlag = true;

            if (!nicknameFlag)
                flagAlert = GetNicknameFlagAlert(false);
        }
        else //unspecified, get default name for given language
        {
            //name
            string defaultName = PokeAPIUtil.GetSpeciesNameTranslated(dex, Language_Data.Value.Value);

            if (Nickname_CapitalizeDefault) //e.g. Gens 1-4 are capitalized by default
                defaultName = defaultName.ToUpperInvariant();

            (name, _, _) = DexUtil.CharEncoding<T>.Encode(defaultName, Data.Nickname.Length, FormatName, Language_Data.Value); //species names shouldn't be truncated/invalid...

            //flag
            if (pku.Nickname_Flag is null)
                nicknameFlag = false;

            if (nicknameFlag)
                flagAlert = GetNicknameFlagAlert(true, defaultName);
        }
        Data.Nickname.SetAs(name);
        Warnings.Add(alert);
        
        //flag stuff
        if (Data.Nickname_Flag is not null)
        {
            Data.Nickname_Flag.Value = nicknameFlag;
            Warnings.Add(flagAlert);
        }
    }

    protected static Alert GetNicknameAlert(AlertType at1, int? maxCharacters = null, AlertType at2 = AlertType.NONE)
    {
        AlertType[] ats = new AlertType[] { at1, at2 };
        string msg = "";
        if (ats.Contains(AlertType.INVALID)) //some characters invalid, removing them
            msg += $"Some of the characters in the nickname are invalid in this format, removing them.";
        if (ats.Contains(AlertType.OVERFLOW)) //too many characters, truncating
        {
            if (maxCharacters is null)
                throw new ArgumentNullException(nameof(maxCharacters), "maximum # of chars must be specified for OVERFLOW alerts.");
            if (msg is not "")
                msg += DataUtil.Newline();
            msg += $"Nickname can only have {maxCharacters} characters in this format, truncating it.";
        }
        return msg is not "" ? new("Nickname", msg) : throw InvalidAlertType();
    }

    protected static Alert GetNicknameFlagAlert(bool flagset, string defaultName = null)
    {
        if (flagset && defaultName is null)
            throw new ArgumentNullException(nameof(defaultName), $"A default name must be specified when {nameof(flagset)} true.");
        return new("Nickname Flag", flagset ?
            $"This pku's Nickname Flag is true, yet it doesn't have a nickname. Setting the nickname to: {defaultName}." :
            "This pku's Nickname Flag is false, yet it has a nickname.");
    }
}