using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace pkuManager.Utilities
{
    /// <summary>
    /// Catch-all utility class for useful operations used throughout <see cref="pkuManager"/>.
    /// </summary>
    public static class DataUtil
    {
        // -----------------------
        // JSON Methods
        // -----------------------
        /// <summary>
        /// Loads JSON files from <see cref="Properties.Resources"/>.
        /// </summary>
        /// <param name="filename">The filename of the desired JSON (sans the ".json").</param>
        /// <returns>A <see cref="JObject"/> representation of the specified JSON file.</returns>
        public static JObject GetJson(string filename)
        {
            return JObject.Parse(Properties.Resources.ResourceManager.GetString(filename));
        }

        /// <summary>
        /// Merges a collection of JObjects.
        /// </summary>
        /// <param name="jobjs">The JObjects to be merged, with later entires overwriting previous ones.</param>
        /// <returns>A JObject that is a combination of all the <paramref name="jobjs"/>. Null if it is null/empty</returns>
        public static JObject GetCombinedJson(params JObject[] jobjs)
        {
            if (jobjs == null || jobjs.Length == 0)
                return null;

            JObject combined = (JObject)jobjs[0].DeepClone();
            foreach (JObject s in jobjs.Skip(1))
                combined.Merge(s);

            return combined;
        }

        /// <summary>
        /// Reads a value from a JToken using a list of keys, in a case-insensitive manner.<br/>
        /// If the traversal reaches a value/array, it is returned and the rest of the keys are ignored.
        /// </summary>
        /// <param name="jobj">The JToken to read from.</param>
        /// <param name="keys">An array of keys to traverse <paramref name="jobj"/> with.</param>
        /// <returns>The resulting value of the traversal. Null if no value was found.</returns>
        public static JToken TraverseJTokenCaseInsensitive(this JToken jobj, params string[] keys)
        {
            JToken temp = jobj;
            foreach (string k in keys)
            {
                if (!(temp is JObject))
                    break;
                temp = ((JObject)temp)?.GetValue(k, StringComparison.OrdinalIgnoreCase);
            }

            return temp;
        }

        /// <summary>
        /// Recursively prunes a <see cref="JToken"/> of all its null properties.
        /// </summary>
        /// <param name="root">The root of the JToken to be pruned.</param>
        private static void PruneNullValues(this JToken root)
        {
            if (root is JValue value)
            {
                if (value.Value == null)
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
                    if (rootObj.Parent is JArray)
                        ((JArray)rootObj.Parent).Where(x => !x.HasValues).ToList().ForEach(n => n.Remove());
                    else
                    {
                        var propertyParent = rootObj.Parent;
                        while (!(propertyParent is JProperty))
                            propertyParent = propertyParent.Parent;
                        propertyParent.Remove();
                    }
                }
            }
        }


        // -----------------------
        // File/Directory Methods
        // -----------------------
        /// <summary>
        /// Writes a string as a file to the given path. Uses UTF-8 encoding.<br/>
        /// If the file's directory doesn't exist, it will be created.<br/>
        /// If the file already exists, it will be overwritten. 
        /// </summary>
        /// <param name="text">The string to be written to <paramref name="path"/>.</param>
        /// <param name="path">The path the string should be written to.</param>
        public static void WriteToFile(this string text, string path)
        {
            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            File.WriteAllText(file.FullName, text);
        }

        /// <summary>
        /// Creates a directory at the given path. Does nothing if the directory already exists.
        /// </summary>
        /// <param name="path">The path of the new directory.</param>
        public static void CreateDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            dir.Create(); // If the directory already exists, this method does nothing.
        }

        /// <summary>
        /// Given a desired file path, returns the next available file path.<br/>
        /// For example, if "folder\file.txt" already existed, this would return "folder\file (1).txt".
        /// </summary>
        /// <param name="filepath">The desired file path.</param>
        /// <returns></returns>
        public static string GetNextFilePath(string filepath)
        {
            int i = 1;
            string dir = Path.GetDirectoryName(filepath);
            string file = Path.GetFileNameWithoutExtension(filepath) + "{0}";
            string extension = Path.GetExtension(filepath);

            while (File.Exists(filepath))
                filepath = Path.Combine(dir, string.Format(file, " (" + i++ + ")") + extension);

            return filepath;
        }


        // -----------------------
        // String Methods
        // -----------------------
        /// <summary>
        /// Returns a concatenated string of the following form: "[str1, str2, str3, ..., strn]".<br/>
        /// Unless there is only one string, in which case that string is returned.
        /// </summary>
        /// <param name="strs">An array of strings to be concatenated.</param>
        /// <returns>A concatenated string of each entry in <paramref name="strs"/>.</returns>
        public static string ToFormattedString(this string[] strs)
        {
            if (strs == null || strs.Length == 0)
                return null;

            if (strs.Length == 1)
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
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// A case-insensitive version of <see cref="string.Equals(string)"/>.<br/>
        /// Also works for null strings (i.e. <see langword="true"/> if both are <see langword="null"/>).
        /// </summary>
        /// <param name="str1">First string to be compared.</param>
        /// <param name="str2">Second string to be compared.</param>
        /// <returns>Whether <paramref name="str1"/> equals <paramref name="str2"/> case-insensitively.</returns>
        public static bool EqualsCaseInsensitive(this string str1, string str2)
        {
            return (str1, str2) is (null, null) || str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
        }


        // -----------------------
        // Value Type Methods
        // -----------------------
        private static readonly Random RANDOM = new Random();

        /// <summary>
        /// Generates a random <see cref="uint"/> from a uniform distribution over all uints.
        /// </summary>
        /// <returns>A random <see cref="uint"/> from 0 to <see cref="uint.MaxValue"/>.</returns>
        public static uint GetRandomUInt()
        {
            int number = RANDOM.Next(Int32.MinValue, Int32.MaxValue);
            return (uint)number + Int32.MaxValue;
        }

        /// <summary>
        /// Reads a contigous region of bits in a uint.
        /// </summary>
        /// <param name="original">The uint whose bits are to be read.</param>
        /// <param name="start">The index of the first bit to be read. A value from 0-31.</param>
        /// <param name="length">The length of the bit string to read.
        ///                      This value + <paramref name="start"/> must be less than 32.</param>
        /// <returns>The value the specified region of bits represents.</returns>
        public static uint GetBits(this uint original, int start, int length)
        {
            if (length + start > 32 || start < 0 || start > 32 || length < 0)
                throw new ArgumentException("start must be between 0-32, length must be positive, length+start be larger than 32.");

            return (original >> start) & (((uint)1 << length) - 1);
        }

        /// <summary>
        /// Sets a region of bits in a uint to a given value.
        /// </summary>
        /// <param name="original">The uint whose bits are to be set.</param>
        /// <param name="val">The value to be set.</param>
        /// <param name="start">The index of the first bit to be set. A value from 0-31.</param>
        /// <param name="length">The length of the bit string to set.
        ///                      This value + <paramref name="start"/> must be less than 32.</param>
        public static void SetBits(this ref uint original, uint val, int start, int length)
        {
            if (start > 32 || start < 0 || length + start > 32 || length < 0)
                throw new ArgumentException("start must be between 0-32 inclusive, length must be positive, length must be between 1-32.");

            uint mask = (((uint)1 << length) - 1) << start;
            original = (original & ~mask) + (val.GetBits(0, length) << start);
        }


        // -----------------------
        // GUI Methods
        // -----------------------
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
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

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
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
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
    }
}