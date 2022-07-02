using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using pkuManager.WinForms.Formats.Fields;
using pkuManager.WinForms.Formats.Fields.LambdaFields;
using pkuManager.WinForms.Formats.Modules;
using pkuManager.WinForms.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static pkuManager.WinForms.Formats.pku.pkuBox.pkuBoxConfig;

namespace pkuManager.WinForms.Formats.pku;

// the use of message boxes here is not really following the model.
public class pkuCollection : Collection
{
    public override string FormatName => "pku";

    public override string Name => Path.GetFileName(Location);
    public override int BoxCount => config.Boxes.Count;
    public override IIntField CurrentBoxID { get; protected set; }

    private PKUCollectionConfig config;
    public pkuBox CurrentPKUBox => CurrentBox as pkuBox;

    public pkuCollection(string path) : base(path) { }

    protected override bool DetermineValidity()
        => CollectionConfigExistsIn(Location);

    protected override void Init()
    {
        CurrentBoxID = new LambdaIntField(
            () => config.CurrentBoxID,
            x => {
                config.CurrentBoxID = (int)x;
                WriteCollectionConfig();
            }
        );
        ReadCollectionConfig(); // Load collection config (collectionConfig.json)
    }


    /* ------------------------------------
     * Collection Config Methods
     * ------------------------------------
    */
    public class PKUCollectionConfig
    {
        [JsonProperty("Current Box ID")]
        public int CurrentBoxID;

        private List<string> boxes = new() { "Default" };

        [JsonProperty("Boxes", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<string> Boxes { get => boxes; set => boxes = value.Distinct(StringComparer.CurrentCultureIgnoreCase).ToList(); }

        [JsonProperty("Global Flags")]
        public GlobalFlags GlobalFlags = new();
    }

    private void ReadCollectionConfig()
    {
        try
        {
            string collectionConfigString = File.ReadAllText($"{Location}/collectionConfig.json");
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
            if (Directory.Exists($@"{Location}\{box}"))
                newBoxList.Add(box);
        }

        // add new boxes (i.e. folders with pkus)
        string[] folders = Directory.GetDirectories(Location);
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
                DialogResult dr = MessageBox.Show($"{msg}\n Would you like to add them to the collection?", "New boxes detected", MessageBoxButtons.YesNo);
                if (dr == DialogResult.No)
                    addBoxes = false;
            }
            else
                MessageBox.Show($"{msg}\n Adding them to the collection.", "New boxes detected");

            if (addBoxes)
                newBoxList.AddRange(newContainsPKU);
        }

        //If box list is empty on load, add default box...
        if (newBoxList.Count < 1)
            newBoxList.Add("Default");

        // save new box list
        config.Boxes = newBoxList;

        //if currentBoxID not in range, set to 0
        if (config.CurrentBoxID < 0 || config.CurrentBoxID >= config.Boxes.Count)
            CurrentBoxID.Value = 0;

        WriteCollectionConfig();
    }

    private void WriteCollectionConfig()
    {
        string configPath = $"{Location}/collectionConfig.json";
        string newConfigText = JsonConvert.SerializeObject(config, Formatting.Indented);
        try
        {
            File.WriteAllText(configPath, newConfigText);
        }
        catch
        {
            Debug.WriteLine($"There was a problem writing the collectionConfig.json file to {configPath}");
        }
    }

    public static bool CollectionConfigExistsIn(string path)
        => File.Exists(@$"{path}\collectionConfig.json");

    public static bool CreateCollectionConfig(string path)
    {
        if (CollectionConfigExistsIn(path))
            return false; //config already exists

        try
        {
            new PKUCollectionConfig().ToString().WriteToFile(@$"{path}\collectionConfig.json");
        }
        catch
        {
            return false; // failed to write config
        }
        return true;
    }


    /* ------------------------------------
     * Override Collection Methods
     * ------------------------------------
    */
    public override string[] GetBoxNames() => config.Boxes.ToArray();

    protected override pkuBox CreateBox(int boxID) => new(Location, config.Boxes[boxID]);


    /* ------------------------------------
     * pku Specific Collection Methods
     * ------------------------------------
    */
    public bool AddBox(string boxName)
    {
        //ignore attmepts to create invalid or duplicate boxes
        if (boxName is null or "" || GetBoxNames().Contains(boxName, StringComparer.InvariantCultureIgnoreCase))
            return false;

        config.Boxes.Add(boxName);
        WriteCollectionConfig();
        return true;
    }

    public void RemoveCurrentBox()
    {
        int id = CurrentBoxID.GetAs<int>();
        SwitchBox(0); //switch to new box 0
        config.Boxes.RemoveAt(id);
        WriteCollectionConfig(); //write new box list
    }

    //public GetExportedList()


    /* ------------------------------------
     * Global Flag Methods
     * ------------------------------------
    */
    public GlobalFlags GetGlobalFlags() => config.GlobalFlags;

    public void SetBattleStatOverrideFlag(bool val)
    {
        config.GlobalFlags.Battle_Stat_Override = val;
        WriteCollectionConfig();
    }

    public void SetDefaultFormOverrideFlag(bool val)
    {
        config.GlobalFlags.Default_Form_Override = val;
        WriteCollectionConfig();
    }
}

public class pkuBox : Box
{
    // Box vars
    public override string FormatName => "pku";
    public override int Width => GetDims(BoxType).width;
    public override int Height => GetDims(BoxType).height;

    // pkuBox vars
    private readonly string Path;
    public string Name { get; }
    private pkuBoxConfig BoxConfig;
    public BoxConfigType BoxType => BoxConfig.BoxType;
    public SortedDictionary<int, pkuObject> Data { get; protected set; } = new();

    public pkuBox(string path, string name)
    {
        Path = path;
        Name = name;
        LoadBoxConfig();
        LoadBackground();
        LoadPKUFiles();
    }


    /* ------------------------------------
     * Box Config Methods
     * ------------------------------------
    */
    public class pkuBoxConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum BoxConfigType
        {
            LIST = int.MaxValue,
            THIRTY = 30,
            SIXTY = 60,
            NINTYSIX = 96
        }

        public static (int width, int height) GetDims(BoxConfigType bc) => bc switch
        {
            BoxConfigType.LIST => (0, 0),
            BoxConfigType.THIRTY => (6, 5),
            BoxConfigType.SIXTY => (12, 5),
            BoxConfigType.NINTYSIX => (12, 8),
            _ => throw new NotImplementedException()
        };

        public pkuBoxConfig()
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

    // Trys to read the boxconfig from the box path
    // If it doesn't exist or is malformed, generates a new one in the directory.
    public void LoadBoxConfig()
    {
        pkuBoxConfig boxConfig;
        string configPath = @$"{Path}\{Name}\boxConfig.json";
        try
        {
            string configText = File.ReadAllText(configPath);
            boxConfig = JsonConvert.DeserializeObject<pkuBoxConfig>(configText);
        }
        catch
        {
            Debug.WriteLine($"Box config for {Name} does not exist or is invalid. Generating a new one...");
            boxConfig = new pkuBoxConfig();
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

        BoxConfig = boxConfig;
    }

    // Write the boxConfig object to the box folder, replacing the old one if it existed.
    public void WriteBoxConfig()
    {
        // Update pkuFiles
        SortedDictionary<int, string> pkfn = new();
        foreach (var kp in Data)
            pkfn.Add(kp.Key, kp.Value.SourceFilename);
        BoxConfig.pkuFileNames = pkfn;

        // Update exported list
        List<string> toRemove = new();
        foreach (string filename in BoxConfig.ExportedPku)
        {
            if (!Data.Any(x => x.Value.SourceFilename.EqualsCaseInsensitive(filename)))
                toRemove.Add(filename);
        }
        BoxConfig.ExportedPku.RemoveAll(x => toRemove.Contains(x));

        string configPath = $"{Path}/{Name}/boxConfig.json";
        string newConfigText = JsonConvert.SerializeObject(BoxConfig, Formatting.Indented);
        try
        {
            File.WriteAllText(configPath, newConfigText);
        }
        catch
        {
            Debug.WriteLine($"There was a problem writing the boxConfig.json file to {configPath}");
        }
    }

    // Reads the pku files from the folder, and rectifies them with those in the config
    public void LoadPKUFiles()
    {
        List<pkuObject> validPKUs = new();

        /* ------------------------------------
         * Read all .pku files from box
         * ------------------------------------
        */
        string boxPathString = $@"{Path}\{Name}";
        DataUtil.CreateDirectory(boxPathString); //create folder if it doesn't exist
        DirectoryInfo boxPath = new(boxPathString);
        List<FileInfo> allPkuFiles = new(boxPath.GetFiles("*.pku"));


        /* ------------------------------------
         * Notify and remove invalid
         * .pku's from consideration
         * ------------------------------------
        */
        //list of invalid pku file names and their reasons for being invalid
        Dictionary<string, string> invalidPKUs = new();

        // remove invalid pku's from consideration, and add them to invalid message
        foreach (FileInfo fi in allPkuFiles)
        {
            //checks if pku file is valid, if so adds it to box list, else adds it to error dict
            string pkuText = null;
            try {
                pkuText = File.ReadAllText(fi.FullName);
            }
            catch { }
            (pkuObject pku, string erorrMsg) = pkuObject.Deserialize(pkuText);

            if (erorrMsg is null)
            {
                pku.SourceFilename = fi.Name;
                validPKUs.Add(pku);
            }
            else
                invalidPKUs.Add(fi.Name, erorrMsg);
        }

        // alert user of invalid pkus
        if (invalidPKUs.Count > 0)
        {
            string msg = $"Some of the .pku files in the \"{Name}\" box are invalid. These will be ignored. Please fix or delete them:";
            int tempNum = 1;
            foreach (var kvp in invalidPKUs)
                msg += $"\n          {tempNum++}) {kvp.Key}: {kvp.Value}";
            MessageBox.Show(msg);
        }
        // -------------------

        // Get list of new .pku files
        List<pkuObject> newValidPKUs = validPKUs.Where(x => !BoxConfig.pkuFileNames.Values
                                                .Contains(x.SourceFilename, StringComparer.OrdinalIgnoreCase))
                                                .ToList();

        // (optionally) ask user if they want to add the new files
        DialogResult dr = DialogResult.Yes;
        if (newValidPKUs.Count > 0 && Properties.Settings.Default.Ask_Auto_Add)
            dr = MessageBox.Show($"Some new .pku files were added to the {Name} folder since this box was last opened, would you like to add them to the boxconfig?", "New .pku files found", MessageBoxButtons.YesNo);
        bool addNewFiles = dr is DialogResult.Yes;


        /* ------------------------------------
         * Create final pkuFiles dictionary
         * ------------------------------------
        */
        //read all .pku files in listed in the (new) config first
        //only reads until box is full, then ignores the rest
        int numInConfig = 0;
        foreach (var kvp in BoxConfig.pkuFileNames)
        {
            if (numInConfig >= (int)BoxConfig.BoxType)
                break;

            pkuObject pku = validPKUs.Find(x => x.SourceFilename.EqualsCaseInsensitive(kvp.Value));
            if (pku is not null)
            {
                Data.Add(kvp.Key, pku);
                numInConfig++;
            }
        }

        //add new pku files if there is space leftover
        if (addNewFiles)
        {
            foreach (pkuObject pku in newValidPKUs)
            {
                if (numInConfig >= (int)BoxConfig.BoxType)
                {
                    MessageBox.Show("There isn't enough space to add all the new .pku files, so only some were added. Either make the box larger, or move these files to another box.");
                    break;
                }

                int nextAvailableIndex = Enumerable.Range(1, int.MaxValue).Except(Data.Keys).FirstOrDefault(); // gets first available slot
                Data.Add(nextAvailableIndex, pku); // Adds it
            }
        }
        WriteBoxConfig(); //Data is updated upon loading
    }

    // Loads the boxBG.png as an Image. Returns default null on failure
    public void LoadBackground()
    {
        DirectoryInfo boxPath = new($"{Path}/{Name}");

        Image boxBG = null;
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
                MessageBox.Show($"Failed to read the box background for: {Name}.");
                Debug.WriteLine(ex.Message);
            }
        }

        Background = boxBG ?? Properties.Resources.grassbox;
    }


    /* ------------------------------------
     * Override Box Methods
     * ------------------------------------
    */
    public override IEnumerable<(int, FormatObject)> ReadBox()
    {
        foreach((int slotID, pkuObject pku) in Data)
            yield return (slotID, pku);
    }

    public override Slot CreateSlotInfo(FormatObject pkmn)
    {
        pkuObject pku = pkmn as pkuObject;
        string defaultName = TagUtil.GetDefaultName(pku, pku.IsEgg(), pku.Game_Info.Language.Value);
        var sprites = ImageUtil.GetSprites(pku);
        Slot s = new(
            pku,
            sprites[0],
            sprites[1],
            sprites[2],
            pku.Nickname.Value ?? defaultName,
            pku.Species.Value,
            pku.Game_Info.Origin_Game.Value,
            pku.Game_Info.OT.Value,
            pku.Forms.Value.JoinLexical() ?? DexUtil.GetDefaultForm(pku.Species.Value),
            pku.Appearance.Value?.Take(10).ToArray().JoinLexical(),
            pku.Catch_Info.Ball.Value,
            pku.IsShadow()
        );
        s.AddPKUData(ContainsExportedName(pku.SourceFilename), pku.SourceFilename);
        return s;
    }

    public override int NextAvailableSlot()
    {
        if (BoxConfig.BoxType is BoxConfigType.LIST)
        {
            int maxKey = Data.Keys.Count > 0 ? Data.Keys.Max() : 0;
            return maxKey + 1; //list type, infinite space
        }
        else
        {
            for (int i = 1; i <= Capacity; i++)
                if (!Data.ContainsKey(i))
                    return i; //matrix type, return first available slot
        }
        return -1; //matrix type, full
    }

    public override bool SetSlot(FormatObject pkmn, int slotID)
    {
        UpdatePKUFilename(pkmn as pkuObject); //make sure filename doesn't coincide with another pku
        if (TryWritePKU(pkmn as pkuObject))
        {
            Data[slotID] = pkmn as pkuObject;
            WriteBoxConfig();
            return true;
        }
        return false; //failed to write file
    }

    public override bool SwapSlots(int slotIDA, int slotIDB)
    {
        // switch desired pokemon
        bool aSuccess = Data.TryGetValue(slotIDA, out pkuObject pkuA);
        bool bSuccess = Data.TryGetValue(slotIDB, out pkuObject pkuB);

        if (aSuccess && bSuccess) //Both slots are not empty
        {
            Data[slotIDA] = pkuB;
            Data[slotIDB] = pkuA;
        }
        else if (aSuccess) //slot b is empty
        {
            Data[slotIDB] = pkuA;
            Data.Remove(slotIDA);
        }
        else if (bSuccess) //slot a is empty
        {
            Data[slotIDA] = pkuB;
            Data.Remove(slotIDB);
        }
        //else both slots are empty, do nothing

        WriteBoxConfig();  // write out new boxConfig
        return true; //This can't fail... right?
    }

    public override bool ClearSlot(int slotID)
    {
        if (Data.ContainsKey(slotID))
        {
            try
            {
                if (Properties.Settings.Default.Send_to_Recycle)
                    FileSystem.DeleteFile($"{Path}/{Name}/{Data[slotID].SourceFilename}", UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); //send pku file to recycle bin
                else
                    File.Delete($"{Path}/{Name}/{Data[slotID].SourceFilename}"); //delete pku file from box folder
            }
            catch
            {
                Debug.WriteLine($"Failed to delete/recycle {Path}/{Name}/{Data[slotID].SourceFilename}");
                return false; //failed to delete file... already in use? no permission?
            }
        }

        // will only run if deletion is successful
        Data.Remove(slotID); //remove pku from boxConfig
        WriteBoxConfig();  // write out new boxConfig
        return true;
    }


    /* ------------------------------------
     * pku Specific Box Methods
     * ------------------------------------
    */
    //returns whether the filename was changed
    public bool UpdatePKUFilename(pkuObject pku)
    {
        string oldName = pku.SourceFilename;
        string newName = DataUtil.GetNextFilePath($"{Path}/{Name}/{oldName}");
        pku.SourceFilename = System.IO.Path.GetFileName(newName);
        return oldName != newName;
    }

    public bool TryWritePKU(pkuObject pku)
    {
        try
        {
            File.WriteAllBytes($"{Path}/{Name}/{pku.SourceFilename}", pku.ToFile());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool ContainsExportedName(string filename)
        => BoxConfig.ExportedPku.Any(s => s.EqualsCaseInsensitive(filename));

    public void CheckOut(Slot slot)
    {
        if (!ContainsExportedName(slot.Filename))
            BoxConfig.ExportedPku.Add(slot.Filename);

        //Mark SlotInfo2 as checked out, so UI can be updated.
        slot.CheckedOut = true;

        WriteBoxConfig();  // write out new boxConfig
    }

    public void CheckIn(Slot slot)
    {
        if (ContainsExportedName(slot.Filename))
            BoxConfig.ExportedPku.Remove(slot.Filename);

        //TODO: the pku isn't actualy modified, which it should be

        //Mark SlotInfo2 as checked out, so UI can be updated.
        slot.CheckedOut = false;

        WriteBoxConfig();  // write out new boxConfig
    }

    public bool ChangeBoxType(BoxConfigType type)
    {
        int max = (int)type;
        if (Data.Count > max)
            return false; //too small

        BoxConfig.BoxType = type;

        //squeeze box if neccesary
        if (type is not BoxConfigType.LIST)
        {
            Dictionary<int, pkuObject> displacedPku = new();
            foreach (var kp in Data)
            {
                if (kp.Key > max)
                    displacedPku.Add(kp.Key, kp.Value);
            }
            foreach ((int id, pkuObject pku) in displacedPku)
            {
                Data.Remove(id);
                int newID = Enumerable.Range(1, max).Except(Data.Keys).FirstOrDefault();
                Data.Add(newID, pku);
            }
        }
        WriteBoxConfig(); // write out new boxConfig
        return true;
    }

    public bool CanChangeBoxType(BoxConfigType type)
        => type is BoxConfigType.LIST || Data.Keys.Count <= (int)type;
}