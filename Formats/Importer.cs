using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using System;
using System.Collections.Generic;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats;

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
    /// A list of questions to be displayed on the exporter window.<br/>
    /// A question is an alert about a value that, generally, requires input from the user.
    /// </summary>
    public List<Alert> Questions { get; } = new();

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
    // TrueOT
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void AskTrueOT()
    {
        RadioButtonAlert rba = new("True OT", "Do you want to include a True OT on this pku?", new RadioButtonAlert.RBAChoice[]
        {
            new("Include True OT:", null, true),
            new("Don't Include", null)
        });
        TrueOTResolver = new(rba, pku.True_OT, (Func<string>)(() => rba.Choices[0].TextEntry), (string)null);
        Questions.Add(rba);
    }

    // TrueOT ErrorResolver
    [PorterDirective(ProcessingPhase.SecondPass)]
    protected virtual ErrorResolver<string> TrueOTResolver { get; set; }
}