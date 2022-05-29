using System.Collections.Generic;
using System.Text;

namespace pkuManager.Formats;

/// <summary>
/// The base class for all Pokemon data structure implementations.
/// </summary>
public abstract class FormatObject
{
    /// <summary>
    /// The name of the format this object represents.
    /// </summary>
    public abstract string FormatName { get; }

    /// <summary>
    /// Exports this object as a file in the implemented format.
    /// </summary>
    /// <returns>A <see cref="byte"/> array representation of the
    ///          file this object represents.</returns>
    public abstract byte[] ToFile();

    /// <summary>
    /// Tries to initalize this object using a file version of this format,
    /// overwriting any data that was there previously.<br/>
    /// If this fails (i.e. the file is invalid), a reason is returned. Otherwise null is returned.
    /// </summary>
    /// <param name="file">A file that satisfies <see cref="IsFile(byte[])"/>.</param>
    public abstract string TryFromFile(byte[] file);
}

public abstract class StringFormatObject : FormatObject
{
    /// <summary>
    /// Compiles a list of strings that will be added to the final .txt file upon calling <see cref="ToFile"/>.
    /// </summary>
    protected abstract List<string> CompileLines();

    protected abstract string DecompileLines(List<string> lines);

    public sealed override byte[] ToFile()
    {
        string txt = string.Join("\n", CompileLines());
        return Encoding.UTF8.GetBytes(txt);
    }

    public sealed override string TryFromFile(byte[] file)
    {
        string txtFile;
        try
        {
            txtFile = Encoding.UTF8.GetString(file);
        }
        catch
        {
            return "This is not a valid UTF-8 text file.";
        }
        return DecompileLines(new(txtFile.Split('\n')));
    }
}