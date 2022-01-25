using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using pkuManager.Formats.Modules;
using pkuManager.Formats.pkx;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static pkuManager.Formats.pku.pkuBox.pkuBoxConfig;

namespace pkuManager.Formats.pku;

// the use of message boxes here is not really following the model.
public class pkuCollection : Collection
{
    public override string FormatName => "pku";

    public override string Name => Path.GetFileName(Location);
    public override int BoxCount => config.Boxes.Count;
    public override int CurrentBoxID { get; protected set; }
    public pkuBox CurrentPKUBox => CurrentBox as pkuBox;

    private PKUCollectionConfig config;

    public pkuCollection(string path) : base(path) { }

    protected override bool DetermineValidity()
        => CollectionConfigExistsIn(Location);

    protected override void Init()
        => ReadCollectionConfig(); // Load collection config (collectionConfig.json)


    /* ------------------------------------
     * Collection Config Methods
     * ------------------------------------
    */
    public class PKUCollectionConfig
    {
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
        int id = CurrentBoxID;
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
    private readonly string Path;
    private pkuBoxConfig BoxConfig;
    public BoxConfigType BoxType => BoxConfig.BoxType;

    public pkuBox(string path, string name)
    {
        Path = path;
        Name = name;
        LoadBoxConfig();
        LoadBackground();
        LoadPKUFiles();

        //get dimensions
        (Width, Height) = BoxConfig.BoxType switch
        {
            BoxConfigType.LIST => (0, 0),
            BoxConfigType.THIRTY => (6, 5),
            BoxConfigType.SIXTY => (12, 5),
            BoxConfigType.NINTYSIX => (12, 8),
            _ => throw new NotImplementedException()
        };
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
            pkfn.Add(kp.Key, kp.Value.Location);
        BoxConfig.pkuFileNames = pkfn;

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

    private Slot CreateSlot(pkuObject pku, string filename)
    {
        int? dex = pkxUtil.GetNationalDex(pku.Species);
        Language? lang = pku.Game_Info.Language.ToEnum<Language>();
        string defaultName = dex.HasValue && lang.HasValue ? PokeAPIUtil.GetSpeciesNameTranslated(dex.Value, lang.Value) : pku.Species;
        return new(
            pku,
            ImageUtil.GetSprite(pku, ImageUtil.Sprite_Type.Box),
            ImageUtil.GetSprite(pku, ImageUtil.Sprite_Type.Front),
            ImageUtil.GetSprite(pku, ImageUtil.Sprite_Type.Back),
            pku.Nickname ?? defaultName,
            pku.Species,
            pku.Game_Info.Origin_Game ?? pku.Game_Info.Official_Origin_Game,
            pku.True_OT.Get() ?? pku.Game_Info.OT,
            pku.Forms,
            pku.Appearance,
            pku.Catch_Info.Ball,
            pku.IsShadow(),
            !pku.True_OT.IsNull,
            filename,
            "Filename",
            ContainsExportedName(filename)
        );
    }

    // Reads the pku files from the folder, and rectifies them with those in the config
    public void LoadPKUFiles()
    {
        //maps all valid .pku files (filenames) to loaded in pkus
        Dictionary<string, pkuObject> validPKUs = new(StringComparer.OrdinalIgnoreCase);

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
                validPKUs.Add(fi.Name, pku);
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
        List<string> newBoxConfigNames = validPKUs.Keys.Where(x => !BoxConfig.pkuFileNames.Values
                                            .Contains(x, StringComparer.OrdinalIgnoreCase)).ToList();

        // (optionally) ask user if they want to add the new files
        DialogResult dr = DialogResult.Yes;
        if (validPKUs.Count - newBoxConfigNames.Count > 0 && Properties.Settings.Default.Ask_Auto_Add)
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

            if(validPKUs.TryGetValue(kvp.Value, out pkuObject pku))
            {
                Data.Add(kvp.Key, CreateSlot(pku, kvp.Value));
                numInConfig++;
            }
        }

        //add new pku files if there is space leftover
        if (addNewFiles)
        {
            foreach (string filename in newBoxConfigNames)
            {
                if (numInConfig >= (int)BoxConfig.BoxType)
                {
                    MessageBox.Show("There isn't enough space to add all the new .pku files, so only some were added. Either make the box larger, or move these files to another box.");
                    break;
                }

                int nextAvailableIndex = Enumerable.Range(1, int.MaxValue).Except(Data.Keys).FirstOrDefault(); // gets first available slot
                Data.Add(nextAvailableIndex, CreateSlot(validPKUs[filename], filename)); // Adds it
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
    public override bool SwapSlots(int slotIDA, int slotIDB)
    {
        // switch desired pokemon
        bool aSuccess = Data.TryGetValue(slotIDA, out Slot slotA);
        bool bSuccess = Data.TryGetValue(slotIDB, out Slot slotB);

        if (aSuccess && bSuccess) //Both slots are not empty
        {
            Data[slotIDA] = slotB;
            Data[slotIDB] = slotA;
        }
        else if (aSuccess) //slot b is empty
        {
            Data[slotIDB] = slotA;
            Data.Remove(slotIDA);
        }
        else if (bSuccess) //slot a is empty
        {
            Data[slotIDA] = slotB;
            Data.Remove(slotIDB);
        }
        //else both slots are empty, do nothing

        WriteBoxConfig();  // write out new boxConfig
        return true; //This can't fail... right?
    }

    public override bool ReleaseSlot(int slotID)
    {
        if (Data.ContainsKey(slotID))
        {
            try
            {
                if (Properties.Settings.Default.Send_to_Recycle)
                    FileSystem.DeleteFile($"{Path}/{Name}/{Data[slotID].Location}", UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin); //send pku file to recycle bin
                else
                    File.Delete($"{Path}/{Name}/{Data[slotID].Location}"); //delete pku file from box folder
            }
            catch
            {
                Debug.WriteLine($"Failed to delete/recycle {Path}/{Name}/{Data[slotID].Location}");
                return false; //failed to delete file... already in use? no permission?
            }
        }

        // will only run if deletion is successful
        Data.Remove(slotID); //remove pku from boxConfig
        WriteBoxConfig();  // write out new boxConfig
        return true;
    }

    public override bool InjectPokemon(FormatObject fo)
    {
        int firstAvailableSlot = Enumerable.Range(1, (int)BoxConfig.BoxType).Except(Data.Keys).First();
        if (firstAvailableSlot > (int)BoxConfig.BoxType)
            return false; //not enough space

        pkuObject pku = fo as pkuObject;
        string filename = DataUtil.GetNextFilePath(@$"{Path}\{Name}\{pku.Nickname ?? pku.Species ?? "PKMN"}.pku");
        try {
            File.WriteAllBytes(filename, pku.ToFile());
        }
        catch {
            return false; //couldn't write file
        }

        FileInfo nf = new(filename);
        Debug.WriteLine($"Adding \"{nf.Name}\" to box.");
        Data.Add(firstAvailableSlot, CreateSlot(pku, filename));
        WriteBoxConfig(); // write out new boxConfig

        return true;
    }


    /* ------------------------------------
     * pku Specific Box Methods
     * ------------------------------------
    */
    private bool ContainsExportedName(string filename)
        => BoxConfig.ExportedPku.Any(s => s.EqualsCaseInsensitive(filename));

    public void CheckOut(Slot slot)
    {
        if (!ContainsExportedName(slot.Location))
            BoxConfig.ExportedPku.Add(slot.Location);

        //Mark SlotInfo2 as checked out, so UI can be updated.
        slot.CheckedOut = true;

        WriteBoxConfig();  // write out new boxConfig
    }

    public void CheckIn(Slot slot)
    {
        if (ContainsExportedName(slot.Location))
            BoxConfig.ExportedPku.Remove(slot.Location);

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
            foreach (var kp in Data)
            {
                int tempIndex;
                int key = kp.Key;
                Slot val = kp.Value;
                if (key > max)
                {
                    tempIndex = Enumerable.Range(1, max).Except(Data.Keys).FirstOrDefault();
                    Data.Remove(key);
                    Data.Add(tempIndex, val);
                }
            }
        }
        WriteBoxConfig(); // write out new boxConfig
        return true;
    }

    public bool CanChangeBoxType(BoxConfigType type)
        => type is BoxConfigType.LIST || Data.Keys.Count <= (int)type;
}