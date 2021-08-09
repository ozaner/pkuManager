using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace pkuManager.Utilities
{
    static class DataUtil
    {
        private static Random random = new Random();

        // -----------------------
        // JSON Methods
        // -----------------------

        /// <summary>
        /// Gets a JObject from the given json file located in the resources.resx file
        /// </summary>
        /// <param name="filename">The filename (sans the .json) of the desired json.</param>
        /// <returns></returns>
        public static JObject getJson(string filename)
        {
            return JObject.Parse(Properties.Resources.ResourceManager.GetString(filename));
        }

        /// <summary>
        /// Returns a merged JObject with the mergings taking place one after the other.
        /// </summary>
        /// <param name="jobjs">The JObjects to be merged, in order.</param>
        /// <returns></returns>
        public static JObject getCombinedJson(params JObject[] jobjs)
        {
            if (jobjs == null || jobjs.Length == 0)
                return null;

            JObject combined = (JObject)jobjs[0].DeepClone();
            foreach (JObject s in jobjs.Skip(1))
                combined.Merge(s);

            return combined;
        }

        // Traverses a given JToken using the provided keys. If it reaches a value/array, returns it and ignores the rest of the keys.
        public static JToken TraverseJTokenCaseInsensitive(JToken jobj, params string[] keys)
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

        //Prunes null nodes from a JToken
        private static void RemoveNullNodes(JToken root)
        {
            if (root is JValue)
            {
                if (((JValue)root).Value == null)
                {
                    ((JValue)root).Parent.Remove();
                }
            }
            else if (root is JArray)
            {
                ((JArray)root).ToList().ForEach(n => RemoveNullNodes(n));
                if (!(((JArray)root)).HasValues)
                {
                    root.Parent.Remove();
                }
            }
            else if (root is JProperty)
            {
                RemoveNullNodes(((JProperty)root).Value);
            }
            else
            {
                var children = ((JObject)root).Properties().ToList();
                children.ForEach(n => RemoveNullNodes(n));

                if (!((JObject)root).HasValues)
                {
                    if (((JObject)root).Parent is JArray)
                    {
                        ((JArray)root.Parent).Where(x => !x.HasValues).ToList().ForEach(n => n.Remove());
                    }
                    else
                    {
                        var propertyParent = ((JObject)root).Parent;
                        while (!(propertyParent is JProperty))
                        {
                            propertyParent = propertyParent.Parent;
                        }
                        propertyParent.Remove();
                    }
                }
            }
        }

        // -----------------------
        // File/Directory Methods
        // -----------------------
        public static void LockFile(string path)
        {

        }

        public static void UnlockFile(string path)
        {

        }

        public static void WriteStringToFileChecked(string path, string text)
        {
            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            File.WriteAllText(file.FullName, text);
        }

        public static void CreateDirectory(string path)
        {
            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.
        }

        public static string GetNextFilename(string filename)
        {
            int i = 1;
            string dir = Path.GetDirectoryName(filename);
            string file = Path.GetFileNameWithoutExtension(filename) + "{0}";
            string extension = Path.GetExtension(filename);

            while (File.Exists(filename))
                filename = Path.Combine(dir, string.Format(file, "(" + i++ + ")") + extension);

            return filename;
        }


        // -----------------------
        // String realted methods
        // -----------------------

        public static readonly char[] VOWELS = new char[] { 'a', 'e', 'i', 'o', 'u' };

        public static string FormatArrayPrint(string[] strs)
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

        public static bool isValidURL(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool stringEqualsCaseInsensitive(string str1, string str2)
        {
            return str1?.ToLowerInvariant() == str2?.ToLowerInvariant();
        }

        public static string uppercaseFirstChar(string str)
        {
            if (str == null || str == "")
                return str;
            else if (str.Length == 1)
                return "" + char.ToUpper(str[0]);
            return char.ToUpper(str[0]) + str.Substring(1);
        }


        // -----------------------
        // Data type methods
        // -----------------------

        public static uint GetRandomUint()
        {
            int number = random.Next(Int32.MinValue, Int32.MaxValue);
            return (uint)(number + (uint)Int32.MaxValue);
        }

        public static uint setByte(uint original, byte val, int index)
        {
            if (index > 4 || index < 0)
                throw new ArgumentException("index must be between 0-3");

            return setBits(original, val, 8 * index, 8);
        }

        public static uint getByte(uint original, int index)
        {
            if (index > 4 || index < 0)
                throw new ArgumentException("index must be between 0-3");

            //return (original >> (8 * index)) & 255;
            return getBits(original, 8 * index, 8);
        }

        public static uint getBits(uint original, int start, int length = 1)
        {
            if (length + start > 32 || start < 0 || start > 32 || length < 0)
                throw new ArgumentException("start must be between 0-32, length must be positive, length+start be larger than 32.");

            return (original >> start) & (((uint)1 << length) - 1);
        }

        public static uint setBits(uint original, uint val, int start, int length = 1)
        {
            if (start > 32 || start < 0 || length + start > 32 || length < 0)
                throw new ArgumentException("start must be between 0-32 inclusive, length must be positive, length must be between 1-32.");

            uint mask = (((uint)1 << length) - 1) << start;
            return (original & ~mask) + (getBits(val, 0, length) << start);
        }

        public static byte[] toByteArray(uint num)
        {
            byte[] bytes = new byte[4];
            bytes[3] = (byte)((num >> 24) & 0xFF);
            bytes[2] = (byte)((num >> 16) & 0xFF);
            bytes[1] = (byte)((num >> 8) & 0xFF);
            bytes[0] = (byte)(num & 0xFF);
            return bytes;
        }

        public static void ShiftCopy(uint i, byte[] arr, int offset, int numBytes = 4)
        {
            if (numBytes < 1 || numBytes > 4)
                throw new ArgumentException("numBytes must be between 1-4.");

            for (int j = 0; j < numBytes; j++)
            {
                arr[offset + j] = (byte)((i >> (8 * j)) & 0xFF);
            }
        }

        public static void ShiftCopy(int i, byte[] arr, int offset, int numBytes = 4)
        {
            if (i < 0)
                throw new ArgumentException("Can only use ShiftCopy with positive integers.");
            ShiftCopy(Convert.ToUInt32(i), arr, offset, numBytes);
        }


        // -----------------------
        // Misc.
        // -----------------------

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

        /// <summary>
        /// Given a GBA ROM, creates a JObject of the different possible ability IDs a pokemon species can have.
        /// </summary>
        /// <param name="path">Path to a main series Pokemon GBA ROM.</param>
        /// <param name="offset">The starting index of the species table in this ROM.</param>
        /// <returns></returns>
        public static JObject ProduceGBAAbilityJSON(string path, int offset)
        {
            // offset = 0x3203E8 for english emerald
            byte[] ROM = File.ReadAllBytes(path);
            JObject abiltyDex = new JObject();

            for (int i = 1; i <= 385; i++) //For each Gen 3 Pokemon (except deoxys)
            {
                int index = PokeAPIUtil.GetSpeciesIndex(i, 3).Value; //These are all valid pokedex #s
                byte slot1 = ROM[offset + 28 * (index - 1) + 22];
                byte slot2 = ROM[offset + 28 * (index - 1) + 23];

                // Adjust for Air Lock (77 in Gen 3 -> 76 in Gen 4+)
                slot1 = slot1 == 77 ? (byte)76 : slot1;
                slot2 = slot2 == 77 ? (byte)76 : slot2;

                // Add Entry to AbilityDex
                JObject jo = new JObject();
                jo.Add("1", slot1);
                if (slot2 != 0) //No entry for blank Slot 2's
                    jo.Add("2", slot2);
                abiltyDex.Add("" + i, jo);
            }

            return abiltyDex;
        }
    }
}