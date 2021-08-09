using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace pkuManager
{
    public partial class WarningWindow : Form
    {
        private SaveFileDialog sfd;
        private EventHandler currentEventHandler;

        /// <summary>
        /// Whether or not the most recent export attempt was successful.
        /// </summary>
        public bool successfulExport = false;

        public WarningWindow(SaveFileDialog sfd)
        {
            InitializeComponent();
            this.sfd = sfd;
        }

        // Clears the past warning/error/note panels and populates them with the new ones.
        private void populateAlerts(List<Alert> warnings, List<Alert> errors, List<Alert> notes)
        {
            warningPanel.Controls.Clear();
            errorPanel.Controls.Clear();
            notesPanel.Controls.Clear();

            if (warnings != null)
            {
                foreach (Alert a in warnings)
                {
                    if (a != null)
                    {
                        if (a is RadioButtonAlert)
                            warningPanel.Controls.Add(new RadioAlertBox((RadioButtonAlert)a));
                        else
                            warningPanel.Controls.Add(new AlertBox(a));
                    }
                }
            }

            if (errors != null)
            {
                foreach (Alert e in errors)
                {
                    if (e != null)
                    {
                        if (e is RadioButtonAlert)
                            errorPanel.Controls.Add(new RadioAlertBox((RadioButtonAlert)e));
                        else
                            errorPanel.Controls.Add(new AlertBox(e));
                    }
                }
            }

            if (notes != null)
            {
                foreach (Alert n in notes)
                {
                    if (n != null)
                        notesPanel.Controls.Add(new NoteBox(n));
                }
            }
        }

        // Changes the text on the warning window to match the given format and file extension.
        private void changeFormatText(string format, string extension)
        {
            descLabel.Text = "The following warnings and errors must be acknowledged before exporting to the " + format + " (." + extension + ") format.";
            this.Text = "Export Warning (" + format + ")";
        }

        // Changes which exporter the acceptButton uses.
        private void resetAcceptButtonEvent(Exporter exporter)
        {
            acceptButton.Click -= currentEventHandler;
            currentEventHandler = new EventHandler(delegate (object sender, EventArgs e)
            {
                exportFormat(exporter, sfd);
                this.Hide(); //"Closes" (same window each time) the warning window when done exporting (failure or not)
            });
            acceptButton.Click += currentEventHandler;
        }

        // Generic behavior for the accept button in the Warning Window
        private void exportFormat(Exporter exporter, SaveFileDialog sfd)
        {
            string pkuToFormat = ".pku to " + exporter.formatName + " (." + exporter.formatExtension + ")"; //For console logging

            Console.WriteLine("Exporting " + pkuToFormat + "...");

            sfd.DefaultExt = exporter.formatExtension;
            sfd.Filter = exporter.formatExtension + " files (*." + exporter.formatExtension + ")|*." + exporter.formatExtension + "|All files (*.*)|*.*";
            //saveFileDialog.FilterIndex = 1; //redundant
            DialogResult result = sfd.ShowDialog(); // Show the dialog box.

            if (result == DialogResult.OK) //Successful choice of file name + location
            {
                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(sfd.FileName), sfd.FileName), exporter.toFileChecked());
                Console.WriteLine("Exported " + pkuToFormat + "!");
                successfulExport = true;
                //Console.WriteLine(JsonConvert.SerializeObject(exporter.pku, Formatting.Indented)); //prints out the entire pku
            }
            else
            {
                Console.WriteLine("Failed to export " + pkuToFormat + "!");
                successfulExport = false;
            }
        }

        // Sets up and opens the warning window, or just auto accepts if there are no warnings, errors, or notes
        public void runWarningWindow(Exporter exporter)
        {
            exporter.processAlertsChecked();
            populateAlerts(exporter.warnings, exporter.errors, exporter.notes);
            changeFormatText(exporter.formatName, exporter.formatExtension);
            resetAcceptButtonEvent(exporter);

            successfulExport = false;

            if (warningPanel.Controls.Count > 0 || errorPanel.Controls.Count > 0 || notesPanel.Controls.Count > 0)
                ShowDialog();
            else
                currentEventHandler.Invoke(null, null);
        }
    }
}
