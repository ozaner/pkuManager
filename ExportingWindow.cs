using pkuManager.Alerts;
using pkuManager.Formats;
using pkuManager.Formats.pku;
using pkuManager.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace pkuManager;

public partial class ExportingWindow : Form
{
    private static readonly SaveFileDialog sfd = new();

    private ExportingWindow(string format, string ext)
    {
        InitializeComponent();
        UpdateDisplayText(format, ext);
    }

    public enum ExportStatus
    {
        Success,
        Invalid_Format,
        Failed
    }

    // Clears the past warning/error/note panels and populates them with the new ones.
    private void PopulateAlerts(List<Alert> warnings, List<Alert> errors, List<Alert> notes)
    {
        warningPanel.Controls.Clear();
        errorPanel.Controls.Clear();
        notesPanel.Controls.Clear();

        if (warnings is not null)
        {
            foreach (Alert a in warnings)
            {
                if (a is not null)
                {
                    if (a is RadioButtonAlert rba)
                        warningPanel.Controls.Add(new RadioAlertBox(rba));
                    else
                        warningPanel.Controls.Add(new AlertBox(a));
                }
            }
        }

        if (errors is not null)
        {
            foreach (Alert e in errors)
            {
                if (e is not null)
                {
                    if (e is RadioButtonAlert rba)
                        errorPanel.Controls.Add(new RadioAlertBox(rba));
                    else
                        errorPanel.Controls.Add(new AlertBox(e));
                }
            }
        }

        if (notes is not null)
        {
            foreach (Alert n in notes)
            {
                if (n is not null)
                    notesPanel.Controls.Add(new NoteBox(n));
            }
        }
    }

    // Changes the text on the warning window to match the given format and file extension.
    private void UpdateDisplayText(string format, string ext)
    {
        descLabel.Text = "The following warnings and errors must be acknowledged before exporting to the " + format + " (." + ext + ") format.";
        Text = "Export Warning (" + format + ")";
    }

    // Generic behavior for the accept button in the Warning Window
    private static ExportStatus ExportFormat(Exporter exporter, Registry.FormatInfo fi)
    {
        string pkuToFormat = ".pku to (." + fi.Ext + ")"; //For console logging

        Debug.WriteLine("Exporting " + pkuToFormat + "...");

        sfd.DefaultExt = fi.Ext;
        sfd.Filter = fi.Ext + " files (*." + fi.Ext + ")|*." + fi.Ext + "|All files (*.*)|*.*";
        //saveFileDialog.FilterIndex = 1; //redundant
        DialogResult result = sfd.ShowDialog(); // Show the dialog box.

        if (result is DialogResult.OK) //Successful choice of file name + location
        {
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(sfd.FileName), sfd.FileName), exporter.ToFile());
            Debug.WriteLine("Exported " + pkuToFormat + "!");
            return ExportStatus.Success;
            //Debug.WriteLine(JsonConvert.SerializeObject(exporter.pku, Formatting.Indented)); //prints out the entire pku
        }
        else
        {
            Debug.WriteLine("Failed to export " + pkuToFormat + "!");
            return ExportStatus.Failed;
        }
    }

    public static bool CanExport(Registry.FormatInfo fi, pkuObject pku, GlobalFlags flags)
    {
        Exporter exporter = (Exporter)Activator.CreateInstance(fi.Exporter, new object[] { pku.DeepCopy(), flags });
        return exporter.CanPort().canPort;
    }

    // Sets up and opens the warning window, or just auto accepts if there are no warnings, errors, or notes
    public static ExportStatus RunWarningWindow(string format, Registry.FormatInfo fi, pkuObject pku, GlobalFlags flags)
    {
        string ext = fi.Ext;

        Debug.WriteLine($"Attempting to export .pku to  {format} (.{ext})");

        ExportingWindow exporterWindow = new(format, ext);

        Exporter exporter = (Exporter)Activator.CreateInstance(fi.Exporter, new object[] { pku.DeepCopy(), flags });

        if (!exporter.CanPort().canPort) //pku is invalid for this format
        {
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
        if (exporterWindow.warningPanel.Controls.Count > 0 || exporterWindow.errorPanel.Controls.Count > 0 || exporterWindow.notesPanel.Controls.Count > 0)
            exporterWindow.ShowDialog();
        else
            statusCode = ExportFormat(exporter, fi);

        return statusCode;
    }
}