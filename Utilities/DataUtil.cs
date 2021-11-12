using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace pkuManager.Utilities;

/// <summary>
/// Catch-all utility class for useful operations used throughout <see cref="pkuManager"/>.
/// </summary>
public static class DataUtil
{
    /* ------------------------------------
     * JSON Methods
     * ------------------------------------
    */
    /// <summary>
    /// Combines a collection of JObjects into a new JObject.
    /// </summary>
    /// <param name="mergeArrays">Whether or not to merge or replace array values.</param>
    /// <param name="jobjs">The JObjects to be merged, with later entires overwriting previous ones.</param>
    /// <returns>A <b>new</b> JObject that is a combination of all the <paramref name="jobjs"/>. Null if it is null/empty</returns>
    public static JObject CombineJson(bool mergeArrays, params JObject[] jobjs)
    {
        if (jobjs?.Length is not > 0)
            return null;

        JsonMergeSettings jms = new()
        {
            MergeArrayHandling = mergeArrays ? MergeArrayHandling.Union : MergeArrayHandling.Replace
        };

        JObject combined = new();
        foreach (JObject s in jobjs)
            combined.Merge(s, jms);

        return combined;
    }

    /// <summary>
    /// Recursively prunes a <see cref="JToken"/> of all its null properties.
    /// </summary>
    /// <param name="root">The root of the JToken to be pruned.</param>
    private static void PruneNullValues(this JToken root)
    {
        if (root is JValue value)
        {
            if (value.Value is null)
                value.Parent.Remove();
        }
        else if (root is JArray array)
        {
            array.ToList().ForEach(n => n.PruneNullValues());
            if (!array.HasValues)
                root.Parent.Remove();
        }
        else if (root is JProperty property)
            property.Value.PruneNullValues();
        else
        {
            JObject rootObj = (JObject)root;
            var children = rootObj.Properties().ToList();
            children.ForEach(n => n.PruneNullValues());

            if (!rootObj.HasValues)
            {
                if (rootObj.Parent is JArray jarr)
                    jarr.Parent.Where(x => !x.HasValues).ToList().ForEach(n => n.Remove());
                else
                {
                    var propertyParent = rootObj.Parent;
                    while (propertyParent is not JProperty)
                        propertyParent = propertyParent.Parent;
                    propertyParent.Remove();
                }
            }
        }
    }


    /* ------------------------------------
     * File/Directory Methods
     * ------------------------------------
    */
    /// <summary>
    /// Writes a string as a file to the given path. Uses UTF-8 encoding.<br/>
    /// If the file's directory doesn't exist, it will be created.<br/>
    /// If the file already exists, it will be overwritten. 
    /// </summary>
    /// <param name="text">The string to be written to <paramref name="path"/>.</param>
    /// <param name="path">The path the string should be written to.</param>
    public static void WriteToFile(this string text, string path)
    {
        FileInfo file = new(path);
        file.Directory.Create(); // If the directory already exists, this method does nothing.
        File.WriteAllText(file.FullName, text);
    }

    /// <summary>
    /// Creates a directory at the given path. Does nothing if the directory already exists.
    /// </summary>
    /// <param name="path">The path of the new directory.</param>
    public static void CreateDirectory(string path)
    {
        DirectoryInfo dir = new(path);
        dir.Create(); // If the directory already exists, this method does nothing.
    }

    /// <summary>
    /// Given a desired file path, returns the next available file path.<br/>
    /// For example, if "folder\file.txt" already existed, this would return "folder\file (1).txt".
    /// </summary>
    /// <param name="filepath">The desired file path.</param>
    /// <returns>The next available file path.</returns>
    public static string GetNextFilePath(string filepath)
    {
        int i = 1;
        string dir = Path.GetDirectoryName(filepath);
        string file = Path.GetFileNameWithoutExtension(filepath) + "{0}";
        string extension = Path.GetExtension(filepath);

        while (File.Exists(filepath))
            filepath = Path.Combine(dir, string.Format(file, $" ({i++})") + extension);

        return filepath;
    }


    /* ------------------------------------
     * String Methods
     * ------------------------------------
    */
    /// <summary>
    /// Outputs <paramref name="n"/> new line sequences.
    /// </summary>
    /// <param name="n">The number of newline sequences to output. 1 by default.</param>
    /// <returns><paramref name="n"/> newline sequences.</returns>
    public static string Newline(int n = 1) => string.Concat(Enumerable.Repeat(Environment.NewLine, n));

    /// <summary>
    /// Returns a concatenated string of the following form: "[str1, str2, str3, ..., strn]".<br/>
    /// Unless there is only one string, in which case that string is returned.
    /// </summary>
    /// <param name="strs">An array of strings to be concatenated.</param>
    /// <returns>A concatenated string of each entry in <paramref name="strs"/>.</returns>
    public static string ToFormattedString(this string[] strs)
    {
        if (strs?.Length is not > 0)
            return null;

        if (strs.Length is 1)
            return strs[0];

        string formatted = "[";
        foreach (string str in strs)
            formatted += str + ", ";
            
        return formatted.Remove(formatted.Length - 2) + "]";
    }

    /// <summary>
    /// Checks whether or not a string constitutes a valid URL.
    /// </summary>
    /// <param name="url">The string to be checked.</param>
    /// <returns>A bool denoting whether or not <paramref name="url"/> is a valid URL or not.</returns>
    public static bool IsValidURL(this string url)
        => Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    /// <summary>
    /// A case-insensitive version of <see cref="string.Equals(string)"/>.<br/>
    /// Also works for null strings (i.e. <see langword="true"/> if both are <see langword="null"/>).
    /// </summary>
    /// <param name="str1">First string to be compared.</param>
    /// <param name="str2">Second string to be compared.</param>
    /// <returns>Whether <paramref name="str1"/> equals <paramref name="str2"/> case-insensitively.</returns>
    public static bool EqualsCaseInsensitive(this string str1, string str2)
        => (str1, str2) is (null, null) || str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Concatenates an array of strings in lexicographical order, each delimited by '|'.<br/>
    /// Null values are ignored.
    /// </summary>
    /// <param name="strs">The strings to be concatenated.</param>
    /// <returns>The values of <paramref name="strs"/> concatenated.</returns>
    public static string JoinLexical(params string[] strs)
    {
        strs = strs?.Where(x => x is not null).ToArray();
        if (strs?.Length is not > 0)
            return null;

        Array.Sort(strs, StringComparer.OrdinalIgnoreCase);
        return string.Join('|', strs);
    }


    /* ------------------------------------
     * Value Type Methods
     * ------------------------------------
    */
    private static readonly Random RANDOM = new();

    /// <summary>
    /// Generates a random <see cref="uint"/> from a uniform distribution over all uints.
    /// </summary>
    /// <returns>A random <see cref="uint"/> from 0 to <see cref="uint.MaxValue"/>.</returns>
    public static uint GetRandomUInt()
        => (uint)RANDOM.Next(1 << 30) << 2 | (uint)RANDOM.Next(1 << 2);

    /// <summary>
    /// Reads a contigous region of bits in an unsigned integer.
    /// </summary>
    /// <param name="value">The value whose bits are to be read. Must be nonnegative.</param>
    /// <param name="start">The index of the first bit to be read.</param>
    /// <param name="length">The length of the bit string to read.</param>
    /// <returns>The value the specified region of bits represents.</returns>
    public static BigInteger GetBits(this BigInteger value, int start, int length)
    {
        if (value.Sign < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "value must be nonnegative.");
        return (value >> start) & ((BigInteger.One << length) - 1);
    }

    /// <inheritdoc cref="GetBits(BigInteger, int, int)"/>
    public static uint GetBits(this uint value, int start, int length)
        => (uint)GetBits((BigInteger)value, start, length);

    /// <summary>
    /// Sets a region of bits in an unsigned integer to a given value.
    /// </summary>
    /// <param name="value">The uint whose bits are to be set.</param>
    /// <param name="new_value">The value to be set.</param>
    /// <param name="start">The index of the first bit to be set.</param>
    /// <param name="length">The length of the bit string to set.</param>
    public static void SetBits(this ref BigInteger value, BigInteger new_value, int start, int length)
    {
        if (value.Sign < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "value must be nonnegative.");
        else if (new_value.Sign < 0)
            throw new ArgumentOutOfRangeException(nameof(new_value), "new_value must be nonnegative.");
        BigInteger mask = ((BigInteger.One << length) - 1) << start;
        value = (value & ~mask) + (new_value.GetBits(0, length) << start);
    }

    /// <inheritdoc cref="SetBits(ref BigInteger, BigInteger, int, int)"/>
    public static void SetBits(this ref uint value, uint new_value, int start, int length)
    {
        BigInteger temp = value;
        SetBits(ref temp, new_value, start, length);
        value = (uint)temp;
    }
    
    /// <summary>
    /// Casts an integral value type, or JValue, to a BigInteger.
    /// </summary>
    /// <param name="val">The value to cast.</param>
    /// <returns><paramref name="val"/> as a BigInteger.</returns>
    /// <exception cref="ArgumentException"><paramref name="val"/> is not a supported integral type.</exception>
    public static BigInteger ToBigInteger(this ValueType val) => val switch
    {
        bool x => x ? BigInteger.One : BigInteger.Zero,
        char x => x,
        byte x => x,
        sbyte x => x,
        ushort x => x,
        short x => x,
        uint x => x,
        int x => x,
        ulong x => x,
        long x => x,
        _ => throw new ArgumentException("obj must be an integral valuetype, or JValue.", nameof(val)),
    };

    /// <inheritdoc cref="ToBigInteger(ValueType)"/>
    public static BigInteger ToBigInteger(this JValue val) => Type.GetTypeCode(val.Value.GetType()) switch
    {
        TypeCode.UInt64 => (ulong)val.Value,
        TypeCode.Int64 => (long)val.Value,
        _ => throw new ArgumentException("obj must be an integral valuetype, or JValue.", nameof(val))
    };

    /// <summary>
    /// Casts a BigInteger to an integral value type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">An integral value type.</typeparam>
    /// <param name="val">The value to cast.</param>
    /// <returns><paramref name="val"/> casted to type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> is not a supported value type.</exception>
    public static T BigIntegerTo<T>(this BigInteger val) where T : struct => Type.GetTypeCode(typeof(T)) switch
    {
        TypeCode.Boolean => (T)(object)(val > BigInteger.Zero), // positive is true
        TypeCode.Char => (T)(object)(char)val,
        TypeCode.Byte => (T)(object)(byte)val,
        TypeCode.SByte => (T)(object)(sbyte)val,
        TypeCode.UInt16 => (T)(object)(ushort)val,
        TypeCode.Int16 => (T)(object)(short)val,
        TypeCode.UInt32 => (T)(object)(uint)val,
        TypeCode.Int32 => (T)(object)(int)val,
        TypeCode.UInt64 => (T)(object)(ulong)val,
        TypeCode.Int64 => (T)(object)(long)val,
        _ => throw new ArgumentException("T must be a integral valuetype.", nameof(T)),
    };


    /* ------------------------------------
     * Enum Methods
     * ------------------------------------
    */
    /// <summary>
    /// Converts an enum to a string, replacing underscores with spaces.
    /// </summary>
    /// <param name="e">An enum.</param>
    /// <returns>The string form of <paramref name="e"/>.</returns>
    public static string ToFormattedString(this Enum e)
        => e.ToString().Replace('_', ' '); //Underscores are spaces

    /// <summary>
    /// Attempts to convert a string to an enum of type <typeparamref name="T"/>. Returns null if not possible.
    /// </summary>
    /// <typeparam name="T">An enum type.</typeparam>
    /// <param name="str">A string representing an enum of type <typeparamref name="T"/>.</param>
    /// <returns><paramref name="str"/> as an enum of type <typeparamref name="T"/>,
    ///          or <see langword="null"/> if there was no match.</returns>
    public static T? ToEnum<T>(this string str) where T : struct
    {
        if (!typeof(T).IsEnum) //Only Enum's allowed
            throw new ArgumentException($"{nameof(T)} must be an {nameof(Enum)}", nameof(T));

        if (str is null)
            return null;

        //Replace spaces with underscores
        str = str.Replace(' ', '_');

        //Returns enum, or null if it DNE
        return Enum.TryParse(str, true, out T e) ? e : null;
    }

    /// <summary>
    /// Attempts to convert an array of strings to a list of enums of type <typeparamref name="T"/>.<br/>
    /// If some strings are not valid enums, they are excluded.
    /// </summary>
    /// <typeparam name="T">An enum type.</typeparam>
    /// <param name="strs">A string representing an enum of type <typeparamref name="T"/>.</param>
    /// <returns>A list of enums of type <typeparamref name="T"/> corresponding to the
    ///          valid entries of <paramref name="strs"/>.</returns>
    public static List<T> ToEnumList<T>(this string[] strs) where T : struct
    {
        List<T> enums = new();
        foreach (string str in strs ?? Array.Empty<string>())
        {
            T? e = str.ToEnum<T>();
            if (e.HasValue)
                enums.Add(e.Value);
        }
        return enums;
    }

    /// <summary>
    /// Attempts to convert an array of strings to a set of enums of type <typeparamref name="T"/>.<br/>
    /// If some strings are not valid enums, they are excluded.
    /// </summary>
    /// <returns>A set of enums of type <typeparamref name="T"/> corresponding to the
    ///          valid entries of <paramref name="strs"/>.</returns>
    /// <inheritdoc cref="ToEnumList{T}(string[])"/>
    public static HashSet<T> ToEnumSet<T>(this string[] strs) where T : struct
        => new(ToEnumList<T>(strs));


    /* ------------------------------------
     * Misc. Methods
     * ------------------------------------
    */
    /// <summary>
    /// Shows a message box with the given <paramref name="title"/> and
    /// <paramref name="message"/>, then terminates the application.
    /// </summary>
    /// <param name="title">The title of the message box.</param>
    /// <param name="message">The message of the message box.</param>
    public static void TerminateProgram(string title, string message)
    {
        MessageBox.Show(message, title);
        Environment.Exit(1);
    }

    /// <summary>
    /// Opens a dialog window to obtain some string input from the user.
    /// </summary>
    /// <param name="title">The title of the input window.</param>
    /// <param name="promptText">The prompt in the input window.</param>
    /// <param name="value">A reference to the inputed value.<br/>
    ///                     Giving a non-null value beforehand sets the default input.</param>
    /// <returns><see cref="DialogResult.OK"/> if an input was successfully given or
    ///          <see cref="DialogResult.Cancel"/> if the input was aborted.</returns>
    public static DialogResult InputBox(string title, string promptText, ref string value)
    {
        Form form = new();
        Label label = new();
        TextBox textBox = new();
        Button buttonOk = new();
        Button buttonCancel = new();

        form.Text = title;
        label.Text = promptText;
        textBox.Text = value;

        buttonOk.Text = "OK";
        buttonCancel.Text = "Cancel";
        buttonOk.DialogResult = DialogResult.OK;
        buttonCancel.DialogResult = DialogResult.Cancel;

        label.SetBounds(9, 20, 372, 13);
        textBox.SetBounds(12, 36, 372, 20);
        buttonOk.SetBounds(228, 72, 75, 23);
        buttonCancel.SetBounds(309, 72, 75, 23);
            
        label.AutoSize = true;
        textBox.Anchor |= AnchorStyles.Right;
        buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        form.ClientSize = new Size(396, 107);
        form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
        form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.AcceptButton = buttonOk;
        form.CancelButton = buttonCancel;

        DialogResult dialogResult = form.ShowDialog();
        value = textBox.Text;
        return dialogResult;
    }

    /// <summary>
    /// Casts <paramref name="obj"/> to the given generic type <typeparamref name="T"/>.<br/>
    /// Will throw an exception if cast is invalid.
    /// </summary>
    /// <typeparam name="T">The type to cast <paramref name="obj"/> to.</typeparam>
    /// <param name="obj">An object to be casted.</param>
    /// <returns><paramref name="obj"/> casted as type <typeparamref name="T"/>.</returns>
    public static T CastTo<T>(this object obj)
    {
        var t = typeof(T);

        //nullable check
        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (obj is null) //if its a null nullable, return null
                return default;

            t = Nullable.GetUnderlyingType(t); //if not then just act like its not nullable.
        }
        return (T)Convert.ChangeType(obj, t);
    }

    /// <summary>
    /// Composes two funcs with the appripriate typings.
    /// </summary>
    /// <typeparam name="T1">Domain of f1.</typeparam>
    /// <typeparam name="T2">Codomain of f1/Domain of f2.</typeparam>
    /// <typeparam name="T3">Codomain of f2.</typeparam>
    /// <param name="f1">A function from <typeparamref name="T1"/> to <typeparamref name="T2"/>.</param>
    /// <param name="f2">A function from <typeparamref name="T2"/> to <typeparamref name="T3"/>.</param>
    /// <returns>The composition of f1 and f2.</returns>
    public static Func<T1, T3> Compose<T1, T2, T3>(this Func<T1, T2> f1, Func<T2, T3> f2) => x => f2(f1(x));

    /// <summary>
    /// Permutates a list with the given pair swaps.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    /// <param name="list">A list implementing IList.</param>
    /// <param name="perms">A list of indices to swap in list.</param>
    public static void Permutate<T>(this IList<T> list, params (int, int)[] perms)
    {
        foreach ((int x, int y) in perms)
            (list[x], list[y]) = (list[y], list[x]);
    }
}