using pkuManager.Formats.pku;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.pkx.pk3;

public class pk3Importer : Importer
{
    public override string FormatName => "pk3";

    protected override pk3Object Data { get; } = new();

    public pk3Importer(byte[] file, GlobalFlags globalFlags, bool checkInMode) : base(file, globalFlags, checkInMode) { }

    public override (bool, string) CanPort()
    {
        if (File.Length is not (pk3Object.FILE_SIZE_PC or pk3Object.FILE_SIZE_PARTY))
            return (false, $"A .pk3 file must be {pk3Object.FILE_SIZE_PC} or {pk3Object.FILE_SIZE_PARTY} bytes long.");

        // Note that pk3Importer ignores cheksum mismatches (i.e. "Bad Eggs" are recovered)
        return (true, null);
    }
    // Init Data
    [PorterDirective(ProcessingPhase.PreProcessing)]
    protected virtual void InitData()
        => Data.FromFile(File);
}