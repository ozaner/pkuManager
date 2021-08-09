using Newtonsoft.Json;
using pkuManager.Exporters;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using APNGLib;
using APNGViewer;
using Newtonsoft.Json.Linq;
using static pkuManager.BoxDisplay;

namespace pkuManager
{
    public partial class ManagerWindow : Form
    {
        // Refrences to certain UI Elements
        SaveFileDialog saveFileDialog;
        WarningWindow warningWindow;
        FolderBrowserDialog pssSelectorDialog;
        APNGBox spriteAPNGBox; //New one is created whenever an APNG is loaded
        PictureBox spritePictureBox; //Same one is reused when an image is loaded.
        BoxDisplay boxDisplay;

        // Constants
        static string EXPORT_BUTTON_INTRO = "Export to \n";

        // Relevant variables
        string pssPath = "C:\\Users\\PSS";
        string currentBox;
        PKUSlot currentPKUSlot;

        // Pay no attention to this variable
        bool _selectorSwitch = false; // Used to stop an infinite loop when box is refreshed and the 0th box is chosen.

        // Constructor initializes UI elements
        public ManagerWindow()
        {
            InitializeComponent();
            initializeExportButtons();
            pssSelectorDialog = new FolderBrowserDialog();
            saveFileDialog = new SaveFileDialog();
            warningWindow = new WarningWindow(saveFileDialog);
            //saveFileDialog.RestoreDirectory = true;
            refreshBoxList();
            loadBox(currentBox); //should have a currentBox from above refresh.

            spritePictureBox = new PictureBox();
            Summary.Controls.Add(spritePictureBox);

            //JObject output = new JObject();
            //for (int i = 1; i < 898 + 1; i++)
            //{
            //    JObject o = new JObject();
            //    o.Add("National Dex", i);
            //    output.Add(ReadingUtil.getStringFromFile("Species", i), o);
            //}
            //output = Utilities.getCombinedJson(new JObject[] { output, pkCommons.NATIONALDEX_DATA });
            //Console.WriteLine(output);
        }

        // Displays the given image (Either an Image or APNG object) in the correct location
        private void drawSprite(object image)
        {
            if (image is APNG)
                drawAPNG((APNG)image);
            else if (image is Image)
                drawPicture((Image)image);
            else
                throw new Exception("Only Image and APNG objets should have been passed here.");
        }

        // Removes the currently displayed pokemon sprite
        private void clearSprite()
        {
            spritePictureBox.Image = null;
            Summary.Controls.Remove(spriteAPNGBox);
        }

        // Helper method for drawSprite(), deals with setting up APNGBox for APNG objects.
        private void drawAPNG(APNG apng)
        {
            spriteAPNGBox = new APNGBox(apng);
            spriteAPNGBox.Start();
            int xOffset = Summary.Width / 2;
            int yOffset = 175;
            spriteAPNGBox.Location = new Point(xOffset - spriteAPNGBox.Size.Width/2, yOffset- spriteAPNGBox.Size.Height/2);
            Summary.Controls.Add(spriteAPNGBox);
            spriteAPNGBox.BringToFront();
        }

        // Helper method for drawSprite(), deals with setting up PictureBox for Image objects.
        private void drawPicture(Image image)
        {
            int xOffset = Summary.Width / 2;
            int yOffset = 175;
            spritePictureBox.Image = image;
            spritePictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            spritePictureBox.Location = new Point(xOffset - spritePictureBox.Size.Width / 2, yOffset - spritePictureBox.Size.Height / 2);
        }

        // Creates a button for each export format registered in the Registry class.
        private void initializeExportButtons()
        {
            foreach (string name in Registry.EXPORTER_DICT.Keys)
            {
                Button eb = new Button();
                eb.Text = EXPORT_BUTTON_INTRO + name;
                eb.AutoSize = true;
                eb.Enabled = false;
                exportButtons.Controls.Add(eb);

                eb.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    PKUObject pku = currentPKUSlot.pku;
                    Exporter exporter = (Exporter)Activator.CreateInstance(Registry.EXPORTER_DICT[name], new[] { pku });

                    warningWindow.runWarningWindow(exporter);
                });
            }
        }

        // Updates which export buttons are visible given a PKUObject.
        private void updateExportButtonVisibility(PKUObject pku)
        {
            if (pku == null)
            {
                foreach (Control c in exportButtons.Controls)
                    c.Enabled = false;
            }
            else
            {
                foreach (Control button in exportButtons.Controls)
                {
                    string name = button.Text.Substring(EXPORT_BUTTON_INTRO.Length);
                    Exporter exporter = (Exporter)Activator.CreateInstance(Registry.EXPORTER_DICT[name], new[] { pku });
                    if (exporter.canExport())
                        button.Enabled = true;
                }
            }
        }

        // Opens a dialog to choose a new PSS location, and refreshes the box display.
        private void setPSSDirectory_Click(object sender, EventArgs e)
        {
            DialogResult result = pssSelectorDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                pssPath = pssSelectorDialog.SelectedPath; // set new path
                refreshButton_Click(sender, e); // refresh boxDisplay
            }
        }

        //doesn't do anything... yet
        private void importPkuButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Importing .pku...");
            Console.WriteLine(".pku imported! (NOT REALLY!)");
        }

        // Behavior for clicking on a pku slot in the box display
        private void boxSlot_Click(object sender, EventArgs e)
        {
            PKUSlot pkuSlot = ((PKUSlot)sender);
            PKUObject pku = pkuSlot.pku;

            // if it's the same slot selected, don't update anything
            if (currentPKUSlot != null && currentPKUSlot.Equals(pkuSlot))
                return;

            resetSelection(); // deselect old slot
            pkuSlot.select(); // add selection overlay even on empty slots
            currentPKUSlot = pkuSlot; // now this slot is selected

            // summary text box fill in
            if (pkuSlot.isSet)
            {
                if(pku.Nickname == null || pkCommons.isAnEgg(pku))
                {
                    nicknameLabel.Text = "Name";
                    nicknameTextBox.Text = pkCommons.isAnEgg(pku) ? "Egg" : pku.Species;
                }
                else
                {
                    nicknameLabel.Text = "Nickname";
                    nicknameTextBox.Text = pku.Nickname;
                }

                if (pku.True_OT != null)
                {
                    otLabel.Text = "True OT:";
                    otTextBox.Text = pku.True_OT;
                }
                else if(pku.Game_Info?.OT != null)
                {
                    otLabel.Text = "OT:";
                    otTextBox.Text = pku.Game_Info.OT;
                }
                speciesTextBox.Text = pku.Species;
                gameTextBox.Text = pku.Game_Info?.Origin_Game; //use official if origin is null (actually maybe not)
                filenameTextBox.Text = pkuSlot.pkuFile.Name;

                drawSprite(Utilities.getPKUSprite(pku));

                //this.Icon = Icon.FromHandle(((Bitmap)pkuSlot.BackgroundImage).GetHicon());
            }

            // Enable export buttons (or not if pku is null)
            updateExportButtonVisibility(pku);
        }

        // Behavior for clicking the box display itself
        private void boxDisplay_Click(object sender, EventArgs e)
        {
            resetSelection();
        }

        // Resets the summary and deselects the currently selected pku slot.
        // Happens when clicking the boxdisplay or an unset pku slot
        private void resetSelection()
        {
            currentPKUSlot?.deselect(); //remove selection overlay
            currentPKUSlot = null;

            // summary text box display
            nicknameLabel.Text = "Nickname";
            nicknameTextBox.Text = "";
            otLabel.Text = "True OT:";
            otTextBox.Text = "";
            speciesTextBox.Text = "";
            gameTextBox.Text = "";
            filenameTextBox.Text = "";

            // remove pokemon sprite
            clearSprite();

            //change window icon
            //this.Icon = Properties.Resources.pc;

            // grey out all export buttons
            updateExportButtonVisibility(null);
        }

        // Behavior for clicking the Refresh button
        private void refreshButton_Click(object sender, EventArgs e)
        {
            refreshBoxList(); //Refresh the list of boxes in the PSS (boxes may have been renamed/added/removed)
            loadBox(currentBox); //Refresh the current box (.pku's may have been changed/added/removed)
            resetSelection(); // Deselect the currently selected pku (deals with the case of current pku being removed/changed by last statement)
        }

        // Refreshes the list of boxes based on the pssconfig.json, this also updates the currentBox
        private void refreshBoxList()
        {
            string pssConfigstring = File.ReadAllText(pssPath + "/pssconfig.json");
            PSSConfigObject pssConfig = JsonConvert.DeserializeObject<PSSConfigObject>(pssConfigstring);
            boxSelector.Items.Clear();

            //Adds each of the folder names listed in the config into the dropdown
            foreach (string boxName in pssConfig.Boxes)
                boxSelector.Items.Add(boxName);

            //Set the currentBox to the 0th one, or null if no folders are listed.
            if (boxSelector.Items.Count == 0) //If no boxes were found
            {
                boxSelector.Text = null;
                currentBox = null;
            }
            else
            {
                _selectorSwitch = true; //So it doesn't trigger the event
                boxSelector.SelectedIndex = 0; //Select 0th box
                currentBox = (string)boxSelector.SelectedItem;
                _selectorSwitch = false;
            }
        }

        // Reloads the pku in the current box (folder) into the boxDisplay
        private void loadBox(string currentBox)
        {
            if (currentBox == null)
                return;

            DirectoryInfo boxFolder = new DirectoryInfo(pssPath + "/" + currentBox);
            FileInfo[] pkuFiles = new FileInfo[] { };
            FileInfo[] boxBGFiles = new FileInfo[] { };
            Image boxBG = null;

            // Try to get .pku and box.png files
            try
            {
                pkuFiles = boxFolder.GetFiles("*.pku"); //Get a list of all .pku files
                boxBGFiles = boxFolder.GetFiles("box.png"); //Get the box.png file if it exists
            }
            catch (Exception ex) //Show warning window if folder doesn't exist
            {
                MessageBox.Show("The folder \"" + currentBox + "\" does not exist in the selected PSS (" +
                    pssPath + "). Please remove it from your pssconfig.json.", currentBox + " does not exist!");
                Console.WriteLine(ex.Message);
            }

            // Try reading the box.png to an Image if it exists.
            if (boxBGFiles.Count() != 0)
            {
                try
                {
                    boxBG = Image.FromFile(boxBGFiles[0].FullName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read the box background for \"" + currentBox + "\" !");
                    Console.WriteLine(ex.Message);
                }
            }

            boxDisplayDock.Controls.Remove(boxDisplay);
            boxDisplay = new BoxDisplay(pkuFiles, BoxDisplayConfig.LIST, boxSlot_Click, boxDisplayDock.Size, boxBG);
            boxDisplayDock.Controls.Add(boxDisplay);
        }

        // Behavior when a different box is selected.
        private void boxSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_selectorSwitch)
            {
                currentBox = (string)boxSelector.SelectedItem;
                resetSelection();
                loadBox(currentBox);
            }
        }

        // Behavior for opening the current box in explorer
        private void openExplorerButton_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", pssPath + "\\" + (string)boxSelector.SelectedItem);
            Console.WriteLine("Just opened the " + (string)boxSelector.SelectedItem + " box in file explorer");
        }

        // Behavior for the toggle checkout viewer button
        private void viewCheckedoutButton_Click(object sender, EventArgs e)
        {
            checkoutDock.Visible = !checkoutDock.Visible;
            viewCheckedoutButton.Text = checkoutDock.Visible ? "Hide Check-Out" : "View Check-Out";
        }
    }
}
