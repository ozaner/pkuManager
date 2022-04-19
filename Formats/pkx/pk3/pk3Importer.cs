using pkuManager.Formats.pku;

namespace pkuManager.Formats.pkx.pk3;

public class pk3Importer : Importer
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
}