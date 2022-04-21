using pkuManager.Alerts;
using pkuManager.Formats.Modules;
using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Formats.Modules.Tags;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.pkx.pk3;

public class pk3Importer : Importer, Is_Egg_I, Language_I
{
    public override string FormatName => "pk3";

    protected override pk3Object Data { get; } = new();

    public pk3Importer(byte[] file, GlobalFlags globalFlags, bool checkInMode) : base(file, globalFlags, checkInMode)
    {
        //Must be correct size
        if (File.Length is not (pk3Object.FILE_SIZE_PC or pk3Object.FILE_SIZE_PARTY))
            Reason = $"A .pk3 file must be {pk3Object.FILE_SIZE_PC} or {pk3Object.FILE_SIZE_PARTY} bytes long.";
        else
        {
            Data.FromFile(File); // Init Data
            if (Data.IsBadEggOrInvalidChecksum) //No Bad Eggs
                Reason = "This Pokémon is a Bad Egg, and can't be imported.";
        }

        CanPort = Reason is null;
    }

    //Working Variables
    protected bool legalGen3Egg;


    /* ------------------------------------
     * Tag Import Methods
     * ------------------------------------
    */
    // Egg
    public void ImportIs_Egg()
    {
        (this as Is_Egg_I).ImportIs_EggBase();

        //deal with legal gen 3 egg
        if (Data.Is_Egg.ValueAsBool)
        {
            (string str, bool inv) = DexUtil.CharEncoding.Decode(Data.Nickname.Value, FormatName, "Japanese");
            legalGen3Egg = Data.UseEggName.ValueAsBool //egg name override set
                && Language_Field.AsString == "Japanese" //language set to japanese
                && !inv && str == TagUtil.EGG_NICKNAME["Japanese"]; //nickname set to "タマゴ"
        }

        //byte override use egg name, if not handled by 'legalGen3Egg' thing
        if (!legalGen3Egg && Data.UseEggName.ValueAsBool)
            Warnings.Add(ByteOverride_I.AddByteOverrideCMD("Egg Name Flag", Data.UseEggName.GetOverride(), pku.Byte_Override));
    }

    // Language
    [PorterDirective(ProcessingPhase.FirstPass, nameof(ImportIs_Egg))]
    public void ImportLanguage()
    {
        if (legalGen3Egg) //make language invalid to set off lang dep error
            Language_Field.AsString = "None";

        (this as Language_I).ImportLanguageBase();

        if (legalGen3Egg) //return legal egg lang back to japanese
            Language_Field.AsString = "Japanese";
    }


    /* ------------------------------------
     * Error Resolvers
     * ------------------------------------
    */
    public ErrorResolver<string> Language_Resolver { get; set; }


    /* ------------------------------------
     * Custom Alerts
     * ------------------------------------
    */
    public ChoiceAlert GetLanguageDependencyAlert(AlertType at, string[] langs, int defChoice)
    {
        ChoiceAlert a = (this as Language_I).GetLanguageDependencyAlertBase(at, langs, defChoice);
        a.Message = "Some text values in this format require a language, but Gen 3 eggs don't have a set language. Please choose one:";
        return a;
    }


    /* ------------------------------------
     * Module Parameters
     * ------------------------------------
    */
    public Is_Egg_O Is_Egg_Field => Data;

    public Language_O Language_Field => Data;
    public ChoiceAlert Language_DependencyError { get; set; }
}