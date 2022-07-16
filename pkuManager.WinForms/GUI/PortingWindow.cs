using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats;
using pkuManager.WinForms.Formats.pku;
using pkuManager.WinForms.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace pkuManager.WinForms.GUI;

public partial class PortingWindow : Form
{
    private static readonly SaveFileDialog sfd = new();
    private static readonly OpenFileDialog ofd = new();

    private PortingWindow(bool import, string format, string ext)
    {
        InitializeComponent();
        UpdateDisplayText(import, format, ext);
        
        AcceptButton = acceptButton; //makes 'ENTER' act like accept button.
        CancelButton = cancelButton; //makes 'ESC' act like cancel button.
        cancelButton.Click += (s, e) => Close();
    }

    // Clears the past warning/error/note panels and populates them with new ones.
    private void PopulateAlerts(List<Alert> warnings, List<Alert> errors, List<Alert> notes)
    {
        var panelPairs = new[] {
            (warnings, warningPanel),
            (errors, errorPanel),
            (notes, notesPanel)
        };
        foreach (var (list, panel) in panelPairs)
        {
            panel.Controls.Clear();
            foreach (Alert a in list)
            {
                if (a is not null)
                {
                    AlertBox ab;
                    if (a is ChoiceAlert ca)
                    {
                        ab = ca.UsesRadioButtons ? new RadioAlertBox(ca, panel.Width)
                                                 : new ComboAlertBox(ca, panel.Width);
                    }
                    else
                        ab = new AlertBox(a, panel.Width);
                    panel.Controls.Add(ab);
                }
            }
        }
    }

    // Updates the text on the warning window to the given context
    private void UpdateDisplayText(bool import, string format, string ext)
    {
        string type = import ? "Import" : "Export";
        descLabel.Text = $"The following warnings and errors must be acknowledged before {type}ing to the " +
            $"{format} {(format != ext ? $"(.{ext}) " : "")}format.";
        Text = $"Export Warning ({format})";
    }

    //Whether there is at least one alert in any of the alert panels.
    private bool SomeAlertsPresent()
        => warningPanel.Controls.Count > 0 || errorPanel.Controls.Count > 0 || notesPanel.Controls.Count > 0;


    /* ------------------------------------
     * Importing stuff
     * ------------------------------------
    */
    public enum ImportStatus
    {
        Success,
        Invalid_File,
        Canceled
    }

    // Sets up and opens a importer window, or just auto imports if there are no questions or notes
    public static (pkuObject, ImportStatus, string) RunImportWindow(string format, Registry.FormatInfo fi, GlobalFlags flags, bool checkInMode)
    {
        Debug.WriteLine($"Attempting to import {format} (.{fi.Ext}) to .pku");

        PortingWindow importerWindow = new(true, format, fi.Ext);

        (byte[] file, string filename) = GetFile(ofd, fi.Ext);
        if (file is null) //file read was a failure
            return (null, ImportStatus.Canceled, "No file chosen.");

        Importer importer = (Importer)Activator.CreateInstance(fi.Importer, file, flags, checkInMode);

        if (!importer.CanPort) //file is invalid for this format
            return (null, ImportStatus.Invalid_File, importer.Reason);

        importer.FirstHalf(); //importer calculates what needs to be added to alert lists
        importerWindow.PopulateAlerts(importer.Warnings, importer.Errors, importer.Notes); //add these to the importerWindow

        pkuObject importedPKU = null;
        bool acceptButtonHit = false;

        // update accept button with proper behavior
        importerWindow.acceptButton.Click += (s, e) => {
            importedPKU = importer.ToPKU();
            acceptButtonHit = true;
            importerWindow.Close();
        };

        // if there are alerts
        if (importerWindow.SomeAlertsPresent())
        {
            importerWindow.ShowDialog();
            if (acceptButtonHit)
            {
                if (importedPKU is null)
                    throw new Exception("A null pkuObject was returned by the importer, despite canImport() passing... fix this");

                importedPKU.SourceFilename = filename; //mark with source file
                return (importedPKU, ImportStatus.Success, null);
            }
            else
                return (null, ImportStatus.Canceled, "Import canceled.");
        }
        else // bypass window if no alerts
        {
            importedPKU = importer.ToPKU();
            importedPKU.SourceFilename = filename; //mark with source file
            return (importedPKU, ImportStatus.Success, null);
        }
    }

    // gets a byte array to be imported
    private static (byte[], string filename) GetFile(OpenFileDialog ofd, string ext)
    {
        ofd.SetExtension(ext);
        ofd.Title = $"Import a .{ext}";
        //ofd.FilterIndex = 1; //redundant
        DialogResult result = ofd.ShowDialog(); // Show the dialog box.

        if (result is DialogResult.OK) //Successfully found a file
        {
            Debug.WriteLine("File found for importing!");
            return (File.ReadAllBytes(ofd.FileName), Path.GetFileNameWithoutExtension(ofd.SafeFileName) + ".pku");
        }
        else
        {
            Debug.WriteLine("Failed to get file to import...");
            return (null, null);
        }
    }


    /* ------------------------------------
     * Exporting stuff
     * ------------------------------------
    */
    public enum ExportStatus
    {
        Success,
        Invalid_Format,
        Failed
    }

    public static bool CanExport(Registry.FormatInfo fi, pkuObject pku, GlobalFlags flags, bool isCheckOut)
    {
        Exporter exporter = (Exporter)Activator.CreateInstance(fi.Exporter, new object[] { pku.DeepCopy(), flags, isCheckOut });
        return exporter.CanPort;
    }

    // Sets up and opens the warning window, or just auto accepts if there are no warnings, errors, or notes
    public static ExportStatus RunExportWindow(string format, Registry.FormatInfo fi, pkuObject pku, GlobalFlags flags, bool isCheckOut)
    {
        Debug.WriteLine($"Attempting to export .pku to {format} (.{fi.Ext})");

        PortingWindow exporterWindow = new(false, format, fi.Ext);
        Exporter exporter = (Exporter)Activator.CreateInstance(fi.Exporter, new object[] { pku.DeepCopy(), flags, isCheckOut });

        if (!exporter.CanPort) //pku is invalid for this format
        {
            //should never get here because only valid formats should have been called...
            Debug.WriteLine($"Export failed, pku is invalid for this format.");
            return ExportStatus.Invalid_Format;
        }

        exporter.FirstHalf(); //exporter calculates what needs to be added to alert lists
        exporterWindow.PopulateAlerts(exporter.Warnings, exporter.Errors, exporter.Notes); //add these to the exporterWindow

        ExportStatus statusCode = ExportStatus.Failed;

        // update accept button with proper behavior
        exporterWindow.acceptButton.Click += (s, e) => {
            statusCode = ExportFormat(exporter, fi);
            exporterWindow.Hide();
        };

        // bypass window if nothing to ask/report
        if (exporterWindow.SomeAlertsPresent())
            exporterWindow.ShowDialog();
        else
            statusCode = ExportFormat(exporter, fi);

        return statusCode;
    }

    // Generic behavior for the accept button in the Warning Window
    private static ExportStatus ExportFormat(Exporter exporter, Registry.FormatInfo fi)
    {
        string pkuToFormat = $".pku to (.{fi.Ext})"; //For console logging

        Debug.WriteLine($"Exporting {pkuToFormat}...");

        sfd.SetExtension(fi.Ext);
        //saveFileDialog.FilterIndex = 1; //redundant
        DialogResult result = sfd.ShowDialog(); // Show the dialog box.

        if (result is DialogResult.OK) //Successful choice of file name + location
        {
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(sfd.FileName), sfd.FileName), exporter.ToFile());
            Debug.WriteLine($"Exported {pkuToFormat}!");
            return ExportStatus.Success;
            //Debug.WriteLine(JsonConvert.SerializeObject(exporter.pku, Formatting.Indented)); //prints out the entire pku
        }
        else
        {
            Debug.WriteLine($"Failed to export {pkuToFormat}!");
            return ExportStatus.Failed;
        }
    }
}