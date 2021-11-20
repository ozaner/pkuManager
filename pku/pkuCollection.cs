using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using pkuManager.Common;
using pkuManager.Formats.pkx;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static pkuManager.pku.pkuCollection.PKUBoxConfig;

namespace pkuManager.pku;

// the use of message boxes here is not really following the model.
public class pkuCollection : Collection
{
    // Cache of the last opened box
    private int cachedBoxID = -1;
    private string cachedBoxName;
    private PKUBoxConfig cachedBoxConfig;
    private SortedDictionary<int, FileInfo> cachedPKUFiles;

    private readonly string path;
    private PKUCollectionConfig config;

    /* ------------------------------------
     * Collection Wide Methods
     * ------------------------------------
    */
    public pkuCollection(string path)
    {
        this.path = path;
        CollectionName = Path.GetFileName(path); //set collection name to folder name
        ReadCollectionConfig(); // Load collection config (collectionConfig.json)
    }

    private void ReadCollectionConfig()
    {
        try
        {
            string collectionConfigString = File.ReadAllText(path + "/collectionConfig.json");
            config = JsonConvert.DeserializeObject<PKUCollectionConfig>(collectionConfigString);
            if (config is null)
                throw new Exception();
        }
        catch
        {
            MessageBox.Show("The collectionConfig.json file is invalid or doesn't exist, a new one will be created.");
            config = new PKUCollectionConfig();
        }

        // Process box list
        List<string> newBoxList = new();

        // remove deleted boxes
        foreach (string box in config.Boxes)
        {
            if (Directory.Exists($@"{path}\{box}"))
                newBoxList.Add(box);
        }

        // add new boxes (i.e. folders with pkus)
        string[] folders = Directory.GetDirectories(path);
        List<string> newContainsPKU = new();
        foreach (string folderPath in folders)
        {
            DirectoryInfo folderInfo = new(folderPath);
            List<FileInfo> allPkus = new(folderInfo.GetFiles("*.pku"));
            if (allPkus.Count > 0)
                newContainsPKU.Add(folderInfo.Name);
        }
        newContainsPKU = newContainsPKU.Except(newBoxList).ToList();

        if(newContainsPKU.Count > 0)
        {
            bool addBoxes = true;
            string msg = "Found .pku files in the following folders: ";
            foreach (string folder in newContainsPKU)
                msg += $"\n - {folder}";
            if (Properties.Settings.Default.Ask_Auto_Add)
            {
                DialogResult dr = MessageBox.Show(msg + "\n Would you like to add them to the collection?", "New boxes detected", MessageBoxButtons.YesNo);
                if (dr == DialogResult.No)
                    addBoxes = false;
            }
            else
                MessageBox.Show(msg + "\n Adding them to the collection.", "New boxes detected");

            if (addBoxes)
                newBoxList.AddRange(newContainsPKU);
        }

        //If box list is empty on load, add default box...
        if (newBoxList.Count < 1)
            newBoxList.Add("Default");

        // save new box list
        config.Boxes = newBoxList;
        WriteCollectionConfig();
    }

    private void WriteCollectionConfig()
    {
        string configPath = $"{path}/collectionConfig.json";
        string newConfigText = JsonConvert.SerializeObject(config, Formatting.Indented);
        try
        {
            File.WriteAllText(configPath, newConfigText);
        }
        catch
        {
            Debug.WriteLine("There was a problem writing the collectionConfig.json file to " + configPath);
        }
    }

    public void SwitchCurrrentBox(int boxID)
    {
        if (boxID >= GetBoxList().Length)
            throw new ArgumentException("BoxID too large!");

        cachedBoxID = boxID;
        cachedBoxName = config.Boxes[cachedBoxID];
        cachedBoxConfig = BoxHelperMethods.ReadBoxConfig(path, cachedBoxName);
        cachedPKUFiles = BoxHelperMethods.ReadPKUFiles(path, cachedBoxName, cachedBoxConfig);

        WriteCachedBoxConfig();
    }

    //public getExportedList()

    public override string[] GetBoxList() => config.Boxes.ToArray();

    public void SetBattleStatOverrideFlag(bool val)
    {
        config.GlobalFlags.Battle_Stat_Override = val;
        WriteCollectionConfig();
    }

    public void AddNewBox(string boxName)
    {
        //ignore attmepts to create invalid or duplicate boxes
        if (boxName is null || boxName is "" || GetBoxList().Contains(boxName, StringComparer.OrdinalIgnoreCase))
            return;

        config.Boxes.Add(boxName);
        WriteCollectionConfig();
    }

    public void RemoveBox(int id)
    {
        config.Boxes.RemoveAt(id);
        WriteCollectionConfig();
    }

    public void OpenBoxInFileExplorer(int boxID)
        => Process.Start("explorer.exe", @$"{path}\{GetBoxList()[boxID]}");

    public GlobalFlags GetGlobalFlags() => config.GlobalFlags;


    /* ------------------------------------
     * Box Methods
     * ------------------------------------
    */
    private void WriteCachedBoxConfig()
        => BoxHelperMethods.WriteBoxConfig(path, cachedBoxName, cachedBoxConfig, cachedPKUFiles);

    public override BoxInfo GetBoxInfo(int boxID)
    {
        if(boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        //create box object
        BoxInfo box = new()
        {
            Background = BoxHelperMethods.LoadBackground(path, cachedBoxName) //box bg
        };

        //box dimensions
        (box.Width, box.Height) = cachedBoxConfig.BoxType switch
        {
            BoxConfigType.LIST => (0, 0),
            BoxConfigType.THIRTY => (6, 5),
            BoxConfigType.SIXTY => (12, 5),
            BoxConfigType.NINTYSIX => (12, 8),
            _ => throw new NotImplementedException()
        };

        //box slots
        SortedDictionary<int, SlotInfo> slots = new();
        pkuObject pku;
        foreach (var kvp in cachedPKUFiles)
        {
            pku = pkuObject.Deserialize(kvp.Value).pku;
            int? dex = pkxUtil.GetNationalDex(pku.Species);
            Language? lang = pku.Game_Info?.Language.ToEnum<Language>();
            string defaultName = dex.HasValue && lang.HasValue ? PokeAPIUtil.GetSpeciesNameTranslated(dex.Value, lang.Value) : pku.Species;
                
            slots[kvp.Key] = new SlotInfo
            {
                Game = pku.Game_Info?.Origin_Game ?? pku.Game_Info?.Official_Origin_Game,
                Location = kvp.Value.Name,
                LocationIdentifier = "Filename",
                Nickname = pku.Nickname ?? defaultName,
                OT = pku.True_OT.Get() ?? pku.Game_Info?.OT,
                TrueOT = !pku.True_OT.IsNull,
                Species = pku.Species,
                Forms = pku.Forms,
                Appearance = pku.Appearance,
                FrontSprite = ImageUtil.GetSprite(pku, ImageUtil.Sprite_Type.Front),
                BackSprite = ImageUtil.GetSprite(pku, ImageUtil.Sprite_Type.Back),
                IconURL = ImageUtil.GetSprite(pku, ImageUtil.Sprite_Type.Box).url,
                Format = "pku",
                CheckedOut = CurrentBoxContainsExportedName(kvp.Value.Name),
                Ball = pku.Catch_Info?.Ball,
                HasShadowHaze = pku.Shadow_Info?.Shadow is true
            };
        }
        box.Slots = slots;
        return box;
    }

    public override byte[] GetPKMN(int boxID, int slot)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        bool valid = cachedPKUFiles.TryGetValue(slot, out FileInfo fi);
        if(valid)
        {
            try
            {
                byte[] read = File.ReadAllBytes(fi.FullName);
                return valid ? read : null;
            }
            catch
            {
                MessageBox.Show("This .pku file couldn't be read... was it deleted?");
            }
        }
        return null;
    }

    public override void SwapSlots(int boxID, int slotA, int slotB)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        // switch desired pokemon
        bool aSuccess = cachedPKUFiles.TryGetValue(slotA, out FileInfo pkuA);
        bool bSuccess = cachedPKUFiles.TryGetValue(slotB, out FileInfo pkuB);

        if (aSuccess && bSuccess) //Both slots are not empty
        {
            cachedPKUFiles[slotA] = pkuB;
            cachedPKUFiles[slotB] = pkuA;
        }
        else if (aSuccess) //slot b is empty
        {
            cachedPKUFiles[slotB] = pkuA;
            cachedPKUFiles.Remove(slotA);
        }
        else if (bSuccess) //slot a is empty
        {
            cachedPKUFiles[slotA] = pkuB;
            cachedPKUFiles.Remove(slotB);
        }
        //else both slots are empty, do nothing

        WriteCachedBoxConfig();  // write out new boxConfig
    }

    public override void Delete(int boxID, int slotA)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        if (cachedPKUFiles.ContainsKey(slotA))
        {
            try
            {
                if(Properties.Settings.Default.Send_to_Recycle)
                    FileSystem.DeleteFile(cachedPKUFiles[slotA].FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); //send pku file to recycle bin
                else
                    File.Delete(cachedPKUFiles[slotA].FullName); //delete pku file from box folder
            }
            catch
            {
                Debug.WriteLine($"Failed to delete {cachedPKUFiles[slotA].FullName}");
            }
        }

        cachedPKUFiles.Remove(slotA); //remove pku from boxConfig

        WriteCachedBoxConfig();  // write out new boxConfig
    }

    public override bool Add(byte[] file, int boxID, int slotID = -1)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        //slot taken/too big
        if (cachedPKUFiles.ContainsKey(slotID) && slotID <= (int)cachedBoxConfig.BoxType)
            return false;

        pkuObject pku = pkuObject.Deserialize(file).pku;

        string filename = DataUtil.GetNextFilePath(@$"{path}\{cachedBoxName}\{pku.Nickname ?? pku.Species ?? "PKMN"}.pku");
        File.WriteAllBytes(filename, file);

        FileInfo nf = new(filename);

        Debug.WriteLine($"Adding \"{nf.Name}\" to box.");

        //if slotID is -1, then write to first available slot
        int slotToWrite = slotID is -1 ? Enumerable.Range(1, (int)cachedBoxConfig.BoxType).Except(cachedPKUFiles.Keys).First() : slotID;
        cachedPKUFiles.Add(slotToWrite, nf);
        WriteCachedBoxConfig();  // write out new boxConfig

        return true;
    }

    public void ChangeBoxType(int boxID, BoxConfigType type)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        cachedBoxConfig.BoxType = type; //change boxConfig's boxType

        //squeeze box if it's too large
        if (cachedBoxConfig.BoxType is not BoxConfigType.LIST)
        {
            int max = (int)cachedBoxConfig.BoxType;

            foreach (var kp in cachedPKUFiles)
            {
                int tempIndex;
                int key = kp.Key;
                FileInfo val = kp.Value;
                if (key > max)
                {
                    tempIndex = Enumerable.Range(1, max).Except(cachedPKUFiles.Keys).FirstOrDefault();
                    cachedPKUFiles.Remove(key);
                    cachedPKUFiles.Add(tempIndex, val);
                }
            }
        }

        WriteCachedBoxConfig();  // write out new boxConfig
    }

    public bool CanChangeBoxType(int boxID, BoxConfigType type)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        return type switch
        {
            BoxConfigType.LIST => true,
            BoxConfigType.THIRTY => cachedPKUFiles.Keys.Count <= 30,
            BoxConfigType.SIXTY => cachedPKUFiles.Keys.Count <= 60,
            BoxConfigType.NINTYSIX => cachedPKUFiles.Keys.Count <= 96,
            _ => throw new NotImplementedException()
        };
    }

    private bool CurrentBoxContainsExportedName(string filename)
        => cachedBoxConfig.ExportedPku.Any(s => s.EqualsCaseInsensitive(filename));

    public void CheckOut(int boxID, SlotInfo slotInfo)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        if (!CurrentBoxContainsExportedName(slotInfo.Location))
            cachedBoxConfig.ExportedPku.Add(slotInfo.Location);

        //Mark slotInfo as checked out, so UI can be updated.
        slotInfo.CheckedOut = true;

        WriteCachedBoxConfig();  // write out new boxConfig
    }

    public void CheckIn(int boxID, SlotInfo slotInfo)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        if (CurrentBoxContainsExportedName(slotInfo.Location))
            cachedBoxConfig.ExportedPku.Remove(slotInfo.Location);

        //TODO: the pku isn't actualy modified, which it should be

        //Mark slotInfo as checked out, so UI can be updated.
        slotInfo.CheckedOut = false;

        WriteCachedBoxConfig();  // write out new boxConfig
    }

    public bool RoomForOneMore(int boxID)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");

        return (cachedBoxConfig.BoxType is BoxConfigType.LIST) ||
               (cachedBoxConfig.BoxType is BoxConfigType.THIRTY && cachedPKUFiles.Count < 30) ||
               (cachedBoxConfig.BoxType is BoxConfigType.SIXTY && cachedPKUFiles.Count < 60) ||
               (cachedBoxConfig.BoxType is BoxConfigType.NINTYSIX && cachedPKUFiles.Count < 96);
    }

    public BoxConfigType GetCurrentBoxType(int boxID)
    {
        if (boxID != cachedBoxID)
            throw new ArgumentException("pkuCollection must manually SwitchCurrentBox() before referencing a new box!");
        return cachedBoxConfig.BoxType;
    }


    private static class BoxHelperMethods
    {
        // Reads the pku files from the folder, and rectifies them with those in the config
        public static SortedDictionary<int, FileInfo> ReadPKUFiles(string path, string name, PKUBoxConfig boxConfig)
        {
            /* ------------------------------------
             * Read all .pku files from box
             * ------------------------------------
            */
            string boxPathString = $@"{path}\{name}";
            DataUtil.CreateDirectory(boxPathString); //create folder if it doesn't exist
            DirectoryInfo boxPath = new(boxPathString);
            List<FileInfo> allPkus = new(boxPath.GetFiles("*.pku"));


            /* ------------------------------------
             * Notify and remove invalid
             * .pku's from consideration
             * ------------------------------------
            */
            //list of invalid pku file names and their reasons for being invalid
            Dictionary<string, string> invalidPKUs = new();

            // remove invalid pku's from consideration, and add them to invalid message
            foreach (FileInfo fi in allPkus)
            {
                //checks if pku file is valid, if so adds it to box list, else adds it to error dict
                string pkuText = null;
                try
                {
                    pkuText = File.ReadAllText(fi.FullName);
                }
                catch { }
                (_, string erorrMsg) = pkuObject.Deserialize(pkuText);

                if (erorrMsg is not null)
                    invalidPKUs.Add(fi.Name, erorrMsg);
            }
            allPkus.RemoveAll(x => invalidPKUs.ContainsKey(x.Name));

            // alert user of invalid pkus
            if (invalidPKUs.Count > 0)
            {
                string msg = $"Some of the .pku files in the \"{name}\" box are invalid. These will not be included in the boxconfig and ignored. Please fix or delete them:";
                int tempNum = 1;
                foreach (var kvp in invalidPKUs)
                    msg += $"\n          {tempNum++}) {kvp.Key}: {kvp.Value}";
                MessageBox.Show(msg);
            }


            /* ------------------------------------
             * Notify user if new .pkus were added
             * and ask if they want to add them.
             * ------------------------------------
            */
            SortedDictionary<int, string> newBoxConfigNames = new(boxConfig.pkuFileNames);
            List<int> keysToRemove = new();
            foreach (var kvp in newBoxConfigNames)
            {
                var temp = allPkus.Find(x => kvp.Value.EqualsCaseInsensitive(x.Name));
                if (temp is null)
                    keysToRemove.Add(kvp.Key);
            }
            foreach (int key in keysToRemove)
                newBoxConfigNames.Remove(key);

            DialogResult dr = DialogResult.Yes;
            if(allPkus.Count - newBoxConfigNames.Count > 0 && Properties.Settings.Default.Ask_Auto_Add)
                dr = MessageBox.Show($"Some new .pku files were added to the {name} folder since this box was last opened, would you like to add them to the boxconfig?", "New .pku files found", MessageBoxButtons.YesNo);
            bool addNewFiles = dr is DialogResult.Yes;


            /* ------------------------------------
             * Create final pkuFiles dictionary
             * ------------------------------------
            */
            SortedDictionary<int, FileInfo> pkuFiles = new();

            //read all .pku files in listed in the (new) config first
            //only reads until box is full, then ignores the rest
            int numInConfig = 0;
            foreach (var kvp in newBoxConfigNames)
            {
                FileInfo fi = allPkus.Find(x => x.Name.EqualsCaseInsensitive(kvp.Value));
                if (fi is not null && numInConfig < (int)boxConfig.BoxType)
                {
                    pkuFiles.Add(kvp.Key, fi);
                    numInConfig++;
                }
            }

            //add new pku files if there is space leftover
            if (addNewFiles)
            {
                int spaceLeftover = (int)boxConfig.BoxType - pkuFiles.Count;
                var leftoverPkus = new List<FileInfo>(allPkus.Except(pkuFiles.Values));
                while (spaceLeftover > 0 && leftoverPkus.Count > 0)
                {
                    int nextAvailableIndex = Enumerable.Range(1, int.MaxValue).Except(pkuFiles.Keys).FirstOrDefault(); // gets first available slot
                    pkuFiles.Add(nextAvailableIndex, leftoverPkus.First()); // Adds it
                    leftoverPkus.Remove(leftoverPkus.First());
                    spaceLeftover--;
                }

                if(leftoverPkus.Count > 0)
                    MessageBox.Show("There is not enough space in the box to add all the new .pku files. Either make the box larger and refresh, or move these files to another box.");
            }

            return pkuFiles;
        }

        // Trys to read the boxconfig from the box path
        // If it doesn't exist or is malformed, generates a new one in the directory.
        public static PKUBoxConfig ReadBoxConfig(string path, string name)
        {
            PKUBoxConfig boxConfig;
            string configPath = @$"{path}\{name}\boxConfig.json";
            try
            {
                string configText = File.ReadAllText(configPath);
                boxConfig = JsonConvert.DeserializeObject<PKUBoxConfig>(configText);
            }
            catch
            {
                Debug.WriteLine($"Box config for {name} does not exist or is invalid. Generating a new one...");
                boxConfig = new PKUBoxConfig();
                string newConfigText = JsonConvert.SerializeObject(boxConfig, Formatting.Indented);

                newConfigText.WriteToFile(configPath); //Write file
            }

            // remove duplicates from pkuFileNames
            boxConfig.pkuFileNames = new SortedDictionary<int, string>(
                boxConfig.pkuFileNames.GroupBy(pair => pair.Value)
                                        .Select(group => group.First())
                                        //.Where(kv => !kv.Value.ToLower().Contains(".pku")) //only read .pku files from config
                                        .ToDictionary(pair => pair.Key, pair => pair.Value)
            );

            return boxConfig;
        }

        // Write the boxConfig object to the box folder, replacing the old one if it existed.
        public static void WriteBoxConfig(string path, string name, PKUBoxConfig boxConfig, SortedDictionary<int, FileInfo> pkuFiles)
        {
            // Update pkuFiles
            SortedDictionary<int, string> pkfn = new();
            foreach (var kp in pkuFiles)
                pkfn.Add(kp.Key, kp.Value.Name);
            boxConfig.pkuFileNames = pkfn;

            string configPath = path + "/" + name + "/boxConfig.json";
            string newConfigText = JsonConvert.SerializeObject(boxConfig, Formatting.Indented);
            try
            {
                File.WriteAllText(configPath, newConfigText);
            }
            catch
            {
                Debug.WriteLine("There was a problem writing the boxConfig.json file to " + configPath);
            }
        }

        // Loads the boxBG.png as an Image. Returns default grassbox if this fails.
        public static Image LoadBackground(string path, string name)
        {
            DirectoryInfo boxPath = new(path + "/" + name);

            Image boxBG = Properties.Resources.grassbox;
            FileInfo[] boxBGFiles = boxPath.GetFiles("box.png"); //box should exist at this point. 

            // Try reading the box.png to an Image object if it exists.
            if (boxBGFiles.Length is not 0)
            {
                try
                {
                    boxBG = Image.FromFile(boxBGFiles[0].FullName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read the box background for \"" + name + "\" !");
                    Debug.WriteLine(ex.Message);
                }
            }

            return boxBG;
        }
    }

    public class PKUCollectionConfig
    {
        private List<string> boxes = new() { "Default" };

        [JsonProperty("Boxes", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<string> Boxes { get => boxes; set => boxes = value.Distinct(StringComparer.CurrentCultureIgnoreCase).ToList(); }

        [JsonProperty("Global Flags")]
        public GlobalFlags GlobalFlags = new();
    }

    public class PKUBoxConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum BoxConfigType
        {
            LIST = int.MaxValue,
            THIRTY = 30,
            SIXTY = 60,
            NINTYSIX = 96
        }

        public PKUBoxConfig()
        {
            pkuFileNames = new();
            ExportedPku = new();
        }

        [JsonProperty("Box Type")]
        public BoxConfigType BoxType = BoxConfigType.LIST;

        [JsonProperty("Exported")]
        public List<string> ExportedPku;

        [JsonProperty("pku")]
        public SortedDictionary<int, string> pkuFileNames;
    }
}