using pkuManager.Alerts;
using pkuManager.Formats;
using pkuManager.Formats.pku;
using pkuManager.GUI;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace pkuManager;

public partial class ImportingWindow : Form
{
    private static readonly OpenFileDialog ofd = new();

    private ImportingWindow(string format, string ext)
    {
        InitializeComponent();
        UpdateDisplayText(format, ext);
        AcceptButton = acceptButton;
        CancelButton = null;
    }

    // Clears the past warning/error/note panels and populates them with the new ones.
    private void PopulateAlerts(List<Alert> questions, List<Alert> notes)
    {
        questionsPanel.Controls.Clear();
        notesPanel.Controls.Clear();

        if (questions is not null)
        {
            foreach (Alert a in questions)
            {
                if (a is not null)
                {
                    if (a is RadioButtonAlert rba)
                        questionsPanel.Controls.Add(new RadioAlertBox(rba));
                    else
                        questionsPanel.Controls.Add(new AlertBox(a));
                }
            }
        }

        if (notes is not null)
        {
            foreach (Alert n in notes)
            {
                if (n is RadioButtonAlert rba)
                    notesPanel.Controls.Add(new RadioAlertBox(rba));
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
        ofd.SetExtension(ext);
        ofd.Title = $"Import a .{ext}";
        //ofd.FilterIndex = 1; //redundant
        DialogResult result = ofd.ShowDialog(); // Show the dialog box.

        if (result is DialogResult.OK) //Successfully found a file
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
        Canceled
    }

    // Sets up and opens a importer window, or just auto imports if there are no questions or notes
    public static (pkuObject, ImportStatus, string) RunImportWindow(string format, Registry.FormatInfo fi, GlobalFlags flags, bool checkInMode)
    {
        string ext = fi.Ext;

        Debug.WriteLine($"Attempting to import {format} (.{ext}) to .pku");

        ImportingWindow importerWindow = new(format, ext);

        byte[] file = GetFile(ofd, ext);
        if (file is null) //file read was a failure
            return (null, ImportStatus.Canceled, "No file chosen.");

        Importer importer = (Importer)Activator.CreateInstance(fi.Importer, file, flags, checkInMode);

        (bool canPort, string reason) = importer.CanPort();
        if (!canPort) //file is invalid for this format
            return (null, ImportStatus.Invalid_File, reason);

        importer.FirstHalf(); //importer calculates what needs to be added to alert lists
        importerWindow.PopulateAlerts(importer.Questions, importer.Notes); //add these to the importerWindow

        pkuObject importedPKU = null;
        bool acceptButtonHit = false;

        // update accept button with proper behavior
        importerWindow.acceptButton.Click += (s, e) => {
            importedPKU = importer.ToPKU();
            acceptButtonHit = true;
            importerWindow.Close();
        };

        // if there are alerts
        if (importerWindow.questionsPanel.Controls.Count > 0 || importerWindow.notesPanel.Controls.Count > 0)
        {
            importerWindow.ShowDialog();
            if (acceptButtonHit)
            {
                if (importedPKU is null)
                    throw new Exception("A null pkuObject was returned by the importer, despite canImport() passing... fix this");

                return (importedPKU, ImportStatus.Success, null);
            }
            else
                return (null, ImportStatus.Canceled, "Import canceled.");
        }
        else // bypass window if no alerts
        {
            importedPKU = importer.ToPKU();
            return (importedPKU, ImportStatus.Success, null);
        }
    }
}