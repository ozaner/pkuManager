using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.GUI;
using pkuManager.pku;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace pkuManager
{
    public partial class ImportingWindow : Form
    {
        private static OpenFileDialog ofd = new OpenFileDialog();

        private ImportingWindow(string format, string ext)
        {
            InitializeComponent();
            UpdateDisplayText(format, ext);
        }

        // Clears the past warning/error/note panels and populates them with the new ones.
        private void PopulateAlerts(List<Alert> questions, List<Alert> notes)
        {
            questionsPanel.Controls.Clear();
            notesPanel.Controls.Clear();

            if (questions != null)
            {
                foreach (Alert a in questions)
                {
                    if (a != null)
                    {
                        if (a is RadioButtonAlert alert)
                            questionsPanel.Controls.Add(new RadioAlertBox(alert));
                        else
                            questionsPanel.Controls.Add(new AlertBox(a));
                    }
                }
            }

            if (notes != null)
            {
                foreach (Alert n in notes)
                {
                    if (n is RadioButtonAlert alert)
                        notesPanel.Controls.Add(new RadioAlertBox(alert));
                    else
                        notesPanel.Controls.Add(new AlertBox(n));
                }
            }
        }

        // Changes the text on the warning window to match the given format and file extension.
        private void UpdateDisplayText(string format, string ext)
        {
            descLabel.Text = $"The following questions must be answered before importing this {format} ({ext}) file.";
            Text = "Import Questions (" + format + ")";
        }

        // gets a byte array to be imported
        private static byte[] GetFile(OpenFileDialog ofd, string ext)
        {
            ofd.DefaultExt = ext;
            ofd.Filter = ext + " files (*." + ext + ")|*." + ext + "|All files (*.*)|*.*";
            ofd.Title = $"Import a .{ext}";
            //ofd.FilterIndex = 1; //redundant
            DialogResult result = ofd.ShowDialog(); // Show the dialog box.

            if (result == DialogResult.OK) //Successfully found a file
            {
                Debug.WriteLine("File found for importing!");
                return File.ReadAllBytes(ofd.FileName);
            }
            else
            {
                Debug.WriteLine("Failed to get file to import...");
                return null;
            }
        }

        public enum ImportStatus
        {
            Success,
            Invalid_File,
            File_Not_Chosen
        }

        // Sets up and opens a importer window, or just auto imports if there are no questions or notes
        public static (pkuObject, ImportStatus) RunImportWindow(Registry.FormatInfo fi, GlobalFlags flags, bool checkInMode)
        {
            string format = fi.name;
            string ext = fi.ext;

            Debug.WriteLine($"Attempting to import {format} (.{ext}) to .pku");

            ImportingWindow importerWindow = new ImportingWindow(format, ext);

            byte[] file = GetFile(ofd, ext);
            if (file == null) //file read was a failure
            {
                Debug.WriteLine($"Import failed, file not found");
                return (null, ImportStatus.File_Not_Chosen);
            }

            Importer importer = (Importer)Activator.CreateInstance(fi.importer, file, flags, checkInMode);

            if(!importer.canImport()) //file is invalid for this format
            {
                Debug.WriteLine($"Import failed, file invalid");
                return (null, ImportStatus.Invalid_File);
            }

            importer.processAlertsChecked(); //importer calculates what needs to be added to alert lists
            importerWindow.PopulateAlerts(importer.questions, importer.notes); //add these to the importerWindow

            pkuObject importedPKU = null;

            // update accept button with proper behavior
            importerWindow.acceptButton.Click += (s, e) => {
                importedPKU = importer.toPKUChecked();
                importerWindow.Hide();
            };

            // bypass window if nothing to ask/report
            if (importerWindow.questionsPanel.Controls.Count > 0 || importerWindow.notesPanel.Controls.Count > 0)
                importerWindow.ShowDialog();
            else
                importedPKU = importer.toPKUChecked();

            if (importedPKU == null)
                throw new Exception("A null pkuObject was returned by the importer, despite canImport() passing... fix this");

            return (importedPKU, ImportStatus.Success);
        }
    }
}
