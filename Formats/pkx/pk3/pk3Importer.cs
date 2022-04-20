using pkuManager.Formats.Modules;
using pkuManager.Formats.Modules.MetaTags;
using pkuManager.Formats.Modules.Tags;
using pkuManager.Formats.pku;
using pkuManager.Utilities;

namespace pkuManager.Formats.pkx.pk3;

public class pk3Importer : Importer, Is_Egg_I
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


    /* ------------------------------------
     * Module Parameters
     * ------------------------------------
    */
    public Is_Egg_O Is_Egg_Field => Data;
    public Language_O Language_Field => Data;
}