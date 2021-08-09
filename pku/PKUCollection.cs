using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using pkuManager.Common;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static pkuManager.pku.PKUCollection.PKUBoxConfig;

namespace pkuManager.pku
{
    public class PKUCollection : Collection
    {
        private readonly string path;
        PKUCollectionConfig config;

        public PKUCollection(string path)
        {
            this.path = path;
            collectionName = Path.GetFileName(path); //set collection name to folder name
            RefreshConfig(); // Load collection config (collectionConfig.json)
        }

        private void RefreshConfig()
        {
            string collectionConfigString = File.ReadAllText(path + "/collectionConfig.json");
            config = JsonConvert.DeserializeObject<PKUCollectionConfig>(collectionConfigString);
        }

        //public getExportedList()

        public override string[] GetBoxList()
        {
            return config.Boxes;
        }

        public override BoxInfo getBoxInfo(int boxID)
        {
            //read box
            string name = config.Boxes[boxID];
            PKUBoxConfig boxConfig = BoxHelperMethods.ReadBoxConfig(path, name);
            SortedDictionary<int, FileInfo> pkuFiles = BoxHelperMethods.ReadPKUFiles(path, name, boxConfig);

            //create box object
            BoxInfo box = new BoxInfo
            {
                background = BoxHelperMethods.LoadBackground(path, name) //box bg
            };

            //box dimensions
            switch (boxConfig.boxType)
            {
                case BoxConfigType.LIST:
                    box.width = box.height = 0;
                    break;
                case BoxConfigType.THIRTY:
                    box.width = 6;
                    box.height = 5;
                    break;
                case BoxConfigType.SIXTY:
                    box.width = 12;
                    box.height = 5;
                    break;
                case BoxConfigType.NINTYSIX:
                    box.width = 12;
                    box.height = 8;
                    break;
                default:
                    break;
            }

            //box slots
            SortedDictionary<int, SlotInfo> slots = new SortedDictionary<int, SlotInfo>();
            PKUObject pku;
            foreach (var kvp in pkuFiles)
            {
                pku = pkuUtil.ImportPKU(kvp.Value);
                int? dex = pkxUtil.GetNationalDex(pku.Species);
                Language? lang = pkxUtil.GetLanguage(pku.Game_Info?.Language);
                string defaultName = dex.HasValue && lang.HasValue ? PokeAPIUtil.GetSpeciesNameTranslated(dex.Value, lang.Value) : pku.Species;

                slots[kvp.Key] = new SlotInfo
                {
                    game = pku.Game_Info?.Origin_Game ?? pku.Game_Info?.Official_Origin_Game,
                    location = kvp.Value.Name,
                    locationIdentifier = "Filename",
                    nickname = pku.Nickname ?? defaultName,
                    OT = pku.True_OT ?? pku.Game_Info?.OT,
                    trueOT = pku.True_OT != null,
                    species = pku.Species,
                    frontSprite = ImageUtil.GetSpriteURL(pku, ImageUtil.SpriteTypes.Front),
                    backSprite = ImageUtil.GetSpriteURL(pku, ImageUtil.SpriteTypes.Back),
                    iconURL = ImageUtil.GetSpriteURL(pku, ImageUtil.SpriteTypes.Box).url,
                    format = "pku",
                    checkedOut = BoxConfigContainsExportedName(boxConfig, kvp.Value.Name),
                    ball = pku.Catch_Info?.Pokeball,
                    hasShadowHaze = pku.Shadow_Info?.Shadow == true
                };
            }
            box.slots = slots;

            return box;
        }

        public override byte[] getPKMN(int boxID, int slot)
        {
            // read in from PKUCollection
            string name = config.Boxes[boxID];
            PKUBoxConfig boxConfig = BoxHelperMethods.ReadBoxConfig(path, name);
            SortedDictionary<int, FileInfo> pkuFiles = BoxHelperMethods.ReadPKUFiles(path, name, boxConfig);

            bool valid = pkuFiles.TryGetValue(slot, out FileInfo fi);
            return valid ? File.ReadAllBytes(fi.FullName) : null;
        }

        public override void SwapSlots(int boxID, int slotA, int slotB)
        {
            // read in from PKUCollection
            string name = config.Boxes[boxID];
            PKUBoxConfig boxConfig = BoxHelperMethods.ReadBoxConfig(path, name);
            SortedDictionary<int, FileInfo> pkuFiles = BoxHelperMethods.ReadPKUFiles(path, name, boxConfig);

            // switch desired pokemon
            bool aSuccess = pkuFiles.TryGetValue(slotA, out FileInfo pkuA);
            bool bSuccess = pkuFiles.TryGetValue(slotB, out FileInfo pkuB);

            if (aSuccess && bSuccess) //Both slots are not empty
            {
                pkuFiles[slotA] = pkuB;
                pkuFiles[slotB] = pkuA;
            }
            else if (aSuccess) //slot b is empty
            {
                pkuFiles[slotB] = pkuA;
                pkuFiles.Remove(slotA);
            }
            else if (bSuccess) //slot a is empty
            {
                pkuFiles[slotA] = pkuB;
                pkuFiles.Remove(slotB);
            }
            //else both slots are empty, do nothing

            // write out new boxConfig (no files need to be changed)
            BoxHelperMethods.WriteBoxConfig(path, name, boxConfig, pkuFiles);
        }

        public override void Delete(int boxID, int slotA)
        {
            // read in from PKUCollection
            string name = config.Boxes[boxID];
            PKUBoxConfig boxConfig = BoxHelperMethods.ReadBoxConfig(path, name);
            SortedDictionary<int, FileInfo> pkuFiles = BoxHelperMethods.ReadPKUFiles(path, name, boxConfig);

            if (pkuFiles.ContainsKey(slotA))
            {
                try
                {
                    File.Delete(pkuFiles[slotA].FullName); //delete pku file from box folder
                }
                catch
                {
                    Console.WriteLine($"Failed to delete {pkuFiles[slotA].FullName}");
                }
            }

            pkuFiles.Remove(slotA); //remove pku from boxConfig

            // write out new boxConfig (no files need to be changed)
            BoxHelperMethods.WriteBoxConfig(path, name, boxConfig, pkuFiles);
        }

        public override void Lock()
        {
            //TODO: Deal with this

            //string[] boxList = GetBoxList();
            //for (int i = 0; i < boxList.Length; i++)
            //{
            //    var allPKUs = BoxHelperMethods.ReadPKUFiles(path, boxList[i], BoxHelperMethods.ReadBoxConfig(path, boxList[i]));
            //    foreach (var kvp in allPKUs)
            //    {
            //        kvp.Value.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            //    }
            //}
        }

        public override void Unlock()
        {

        }

        public void ChangeBoxType(int boxID, BoxConfigType type)
        {
            // read in from PKUCollection
            string name = config.Boxes[boxID];
            PKUBoxConfig boxConfig = BoxHelperMethods.ReadBoxConfig(path, name);
            SortedDictionary<int, FileInfo> pkuFiles = BoxHelperMethods.ReadPKUFiles(path, name, boxConfig);

            boxConfig.boxType = type; //change boxConfig's boxType

            //squeeze box if it's too large
            if (boxConfig.boxType != BoxConfigType.LIST)
            {
                int max = boxConfig.boxType switch
                {
                    BoxConfigType.THIRTY => 30,
                    BoxConfigType.SIXTY => 60,
                    BoxConfigType.NINTYSIX => 96,
                    _ => 96 // no other case
                };

                foreach (var kp in pkuFiles)
                {
                    int tempIndex;
                    int key = kp.Key;
                    FileInfo val = kp.Value;
                    if (key > max)
                    {
                        tempIndex = Enumerable.Range(1, max).Except(pkuFiles.Keys).FirstOrDefault();
                        pkuFiles.Remove(key);
                        pkuFiles.Add(tempIndex, val);
                    }
                }
            }

            // write out new boxConfig (no files need to be changed)
            BoxHelperMethods.WriteBoxConfig(path, name, boxConfig, pkuFiles);
        }

        public bool CanChangeBoxType(int boxID, BoxConfigType type)
        {
            // read in from PKUCollection
            string name = config.Boxes[boxID];
            PKUBoxConfig boxConfig = BoxHelperMethods.ReadBoxConfig(path, name);
            SortedDictionary<int, FileInfo> pkuFiles = BoxHelperMethods.ReadPKUFiles(path, name, boxConfig);

            if (type == BoxConfigType.LIST)
                return true;
            else if (type == BoxConfigType.THIRTY)
                return pkuFiles.Keys.Count <= 30;
            else if (type == BoxConfigType.SIXTY)
                return pkuFiles.Keys.Count <= 60;
            else if (type == BoxConfigType.NINTYSIX)
                return pkuFiles.Keys.Count <= 96;
            throw new ArgumentException("This BoxConfigType hasn't been accounted for...)"); //shouldn't happen
        }

        private bool BoxConfigContainsExportedName(PKUBoxConfig boxConfig, string filename)
        {
            return boxConfig.exportedPku.Any(s => DataUtil.stringEqualsCaseInsensitive(s, filename));
        }

        public void CheckOut(int boxID, SlotInfo slotInfo)
        {
            // read in from PKUCollection
            string name = config.Boxes[boxID];
            PKUBoxConfig boxConfig = BoxHelperMethods.ReadBoxConfig(path, name);
            SortedDictionary<int, FileInfo> pkuFiles = BoxHelperMethods.ReadPKUFiles(path, name, boxConfig);

            if (!BoxConfigContainsExportedName(boxConfig, slotInfo.location))
                boxConfig.exportedPku.Add(slotInfo.location);

            //Mark slotInfo as checked out, so UI can be updated.
            slotInfo.checkedOut = true;

            // write out new boxConfig (no files need to be changed)
            BoxHelperMethods.WriteBoxConfig(path, name, boxConfig, pkuFiles);
        }

        public void SetBattleStatOverrideFlag(bool val)
        {
            config.globalFlags.Battle_Stat_Override = val;
            CollectionHelperMethods.WriteCollectionConfig(path, config);
        }

        public GlobalFlags GetGlobalFlags()
        {
            return config.globalFlags;
        }

        private static class CollectionHelperMethods
        {
            // Write the boxConfig object to the box folder, replacing the old one if it existed.
            public static void WriteCollectionConfig(string path, PKUCollectionConfig config)
            {
                string configPath = path + "/collectionConfig.json";
                string newConfigText = JsonConvert.SerializeObject(config, Formatting.Indented);
                try
                {
                    File.WriteAllText(configPath, newConfigText);
                }
                catch
                {
                    Console.WriteLine("There was a problem writing the collectionConfig.json file to " + configPath);
                }
            }
        }

        private static class BoxHelperMethods
        {
            // Reads the pku files from the folder, and rectifies them with those in the config
            public static SortedDictionary<int, FileInfo> ReadPKUFiles(string path, string name, PKUBoxConfig boxConfig)
            {
                SortedDictionary<int, FileInfo> pkuFiles = new SortedDictionary<int, FileInfo>();
                string boxPathString = $@"{path}\{name}";
                DataUtil.CreateDirectory(boxPathString); //create folder if it doesn't exist

                DirectoryInfo boxPath = new DirectoryInfo(boxPathString);
                FileInfo[] allPkus = boxPath.GetFiles("*.pku"); //box must exist at ths point

                // read pku files from config, and remove ones that dont exist
                SortedDictionary<int, string> newPkuFileNames = new SortedDictionary<int, string>();
                foreach (var kp in boxConfig.pkuFileNames)
                {
                    if (allPkus.Any(f => DataUtil.stringEqualsCaseInsensitive(f.Name, kp.Value)))
                        newPkuFileNames.Add(kp.Key, kp.Value);
                }
                boxConfig.pkuFileNames = newPkuFileNames;

                int totalNumOfPKUs = allPkus.Length;
                int numOfPKUsConfig = boxConfig.pkuFileNames.Values.Count();

                bool useAllPKUs = false;

                // figures out whether to use all pkus or just the listed ones.
                if (totalNumOfPKUs == 0 || numOfPKUsConfig == 0) //if there are no pokemon listed in the box, just put them all in.
                    useAllPKUs = true;
                else if (totalNumOfPKUs - numOfPKUsConfig > 0) //if there is mismatch, ask what to do
                {
                    if (boxConfig.askAutoAdd)
                    {
                        DialogResult dr = MessageBox.Show("There are " + totalNumOfPKUs + " .pku files in the " + name + " folder, but only "
                                + numOfPKUsConfig + " are listed in the boxConfig.json. Would you like to add the remaining "
                                + (totalNumOfPKUs - numOfPKUsConfig) + " to the box?", "Box Config Mismatch", MessageBoxButtons.YesNo);

                        if (dr == DialogResult.Yes)
                            useAllPKUs = true;
                        else if (dr == DialogResult.No)
                            useAllPKUs = false;
                    }
                    else
                        useAllPKUs = true;
                }

                //read all .pku files in config first
                FileInfo[] temp;
                foreach (int i in boxConfig.pkuFileNames.Keys)
                {
                    temp = boxPath.GetFiles(boxConfig.pkuFileNames[i]);
                    if (temp.Length > 0)
                        pkuFiles.Add(i, temp[0]);
                }

                //if other pku files are to be considered, read them too.
                if (useAllPKUs)
                {
                    IEnumerable<string> leftoverPkus = allPkus.Select(o => o.Name.ToLowerInvariant()) //only selects from valid .pkus
                        .Except(pkuFiles.Values.Select(s => s.Name.ToLowerInvariant()));
                    int tempIndex;
                    foreach (string fileName in leftoverPkus)
                    {
                        tempIndex = Enumerable.Range(1, int.MaxValue).Except(pkuFiles.Keys).FirstOrDefault(); // gets first available slot
                        pkuFiles.Add(tempIndex, new FileInfo(boxPathString + "/" + fileName)); // Adds it
                    }
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
                    Console.WriteLine($"Box Config for {name} box doesnt't exist. Generating a new one...");
                    boxConfig = new PKUBoxConfig();
                    string newConfigText = JsonConvert.SerializeObject(boxConfig, Formatting.Indented);

                    DataUtil.WriteStringToFileChecked(configPath, newConfigText); //Write file
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
                SortedDictionary<int, string> pkfn = new SortedDictionary<int, string>();
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
                    Console.WriteLine("There was a problem writing the boxConfig.json file to " + configPath);
                }
            }

            // Loads the boxBG.png as an Image. Returns default grassbox if this fails.
            public static Image LoadBackground(string path, string name)
            {
                DirectoryInfo boxPath = new DirectoryInfo(path + "/" + name);

                Image boxBG = Properties.Resources.grassbox;
                FileInfo[] boxBGFiles = new FileInfo[] { };

                //Get the box.png file if it exists
                boxBGFiles = boxPath.GetFiles("box.png"); //box should exist at this point. 

                // Try reading the box.png to an Image object if it exists.
                if (boxBGFiles.Count() != 0)
                {
                    try
                    {
                        boxBG = Image.FromFile(boxBGFiles[0].FullName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to read the box background for \"" + name + "\" !");
                        Console.WriteLine(ex.Message);
                    }
                }

                return boxBG;
            }
        }

        public partial class PKUCollectionConfig
        {
            private string[] boxes = new string[] { "Default" };

            [JsonProperty("Boxes")]
            public string[] Boxes { get => boxes; set => boxes = value.Distinct().ToArray(); }

            [JsonProperty("Global Flags")]
            public GlobalFlags globalFlags = new GlobalFlags();
        }

        public partial class PKUBoxConfig
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum BoxConfigType
            {
                LIST,
                THIRTY,
                SIXTY,
                NINTYSIX
            }

            public PKUBoxConfig()
            {
                pkuFileNames = new SortedDictionary<int, string>();
                exportedPku = new List<string>();
            }

            [JsonProperty("Ask Auto-Add")]
            public bool askAutoAdd;

            [JsonProperty("Box Type")]
            public BoxConfigType boxType = BoxConfigType.LIST;

            [JsonProperty("Exported")]
            public List<string> exportedPku;

            [JsonProperty("pku")]
            public SortedDictionary<int, string> pkuFileNames;
        }
    }
}
