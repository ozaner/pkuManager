using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.pku;
using System;
using static pkuManager.WinForms.Formats.PorterDirective;

namespace pkuManager.WinForms.Formats;

/// <summary>
/// The base class for all Importers. All formats that can be<br/>
/// imported must have a corresponding class implements this.
/// </summary>
public abstract class Importer : Porter
{
    /// <summary>
    /// <see langword="true"/> importer instance is meant to check-in the given file.<br/>
    /// <see langword="false"/> if it is meant to just import it.
    /// </summary>
    public bool CheckInMode { get; }

    /// <summary>
    /// Reference to the passed file.
    /// </summary>
    protected byte[] File { get; }

    /// <summary>
    /// The base importer constructor.
    /// </summary>
    /// <inheritdoc cref="Porter(pkuObject, GlobalFlags, FormatObject)"/>
    public Importer(byte[] file, GlobalFlags globalFlags, bool checkInMode) : base(new(), globalFlags)
    {
        File = file;
        CheckInMode = checkInMode;
    }

    /// <summary>
    /// Returns the imported <see cref="pkuObject"/> generated from the given file.<br/>
    /// Should only be run after <see cref="Porter.FirstHalf"/>.
    /// </summary>
    /// <returns>A <see cref="byte"/> array representation of the exported file.</returns>
    public pkuObject ToPKU()
    {
        SecondHalf();
        return pku;
    }


    /* ------------------------------------
     * Universal Import Questions
     * ------------------------------------
    */
    // OT Override
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void AskOT_Override()
    {
        ChoiceAlert rba = new("OT Override", "Do you want to include a different OT on this pku? " +
            "The original (official) OT will only be used on official formats.", new ChoiceAlert.SingleChoice[]
        {
            new("Add OT Override:", null, true),
            new("Don't Add", null)
        }, true);
        OT_Override_Resolver = () =>
        {
            if (rba.SelectedIndex is 0) //use override
            {
                pku.Game_Info.Official_OT.Value = pku.Game_Info.OT.Value;
                pku.Game_Info.OT.Value = rba.Choices[0].TextEntry;
            }
        };
        Errors.Add(rba);
    }

    // OT Override ErrorResolver
    [PorterDirective(ProcessingPhase.SecondPass)]
    protected virtual Action OT_Override_Resolver { get; set; }
}