using pkuManager.WinForms.Formats.Modules.MetaTags;

namespace pkuManager.WinForms.Formats.Fields.BAMFields;

/// <summary>
/// To be implemented by a <see cref="Field{T}"/> that can output "Byte Override" commands to a pku.
/// </summary>
public interface IByteOverridable
{
    /// <summary>
    /// Gets the byte override commmand corresponding to this field.
    /// </summary>
    /// <returns>A byte override command.</returns>
    public abstract ByteOverrideCMD GetOverride();
}

