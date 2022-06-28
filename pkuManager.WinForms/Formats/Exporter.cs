using pkuManager.WinForms.Formats.pku;

namespace pkuManager.WinForms.Formats;

/// <summary>
/// The base class for all Exporters. All formats that can be<br/>
/// exported must have a corresponding class implements this.
/// </summary>
public abstract class Exporter : Porter
{
    /// <summary>
    /// The base exporter constructor.
    /// </summary>
    /// <param name="data">A data structure representing the non-pku format.<br/>
    ///                    This parameter should be hidden by any implementation of a porter.</param>
    /// <inheritdoc cref="Porter(pkuObject, GlobalFlags, FormatObject)"/>
    public Exporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags)
        => this.pku = pkuObject.MergePkuOverride(this.pku, FormatName); //apply format override

    /// <summary>
    /// Returns the exported file generated from the given <see cref="pku"/>.<br/>
    /// Should only be run after <see cref="Porter.FirstHalf"/>.
    /// </summary>
    /// <returns>A <see cref="byte"/> array representation of the exported file.</returns>
    public byte[] ToFile()
    {
        SecondHalf();
        return Data.ToFile();
    }
}