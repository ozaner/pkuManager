using pkuManager.Alerts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace pkuManager
{
    public partial class WarningWindow : Form
    {
        private SaveFileDialog sfd;
        private EventHandler oldEventHandler;

        public WarningWindow(SaveFileDialog sfd)
        {
            InitializeComponent();
            this.sfd = sfd;
        }

        // Clears the past warnings/error panels and populates them with the new ones.
        // If both warnings & errors are empty/null, noWarnings is set to true.
        private void populateWarnings(List<AlertBox> warnings, List<AlertBox> errors)
        {
            warningPanel.Controls.Clear();
            errorPanel.Controls.Clear();

            if(warnings != null)
            {
                foreach (Panel w in warnings)
                {
                    if (w != null)
                        warningPanel.Controls.Add(w);
                }
            }

            if (errors != null)
            {
                foreach (Panel e in errors)
                {
                    if (e != null)
                        errorPanel.Controls.Add(e);               
                }
            }
        }

        // Changes the text on the warning window to match the given format and file extension.
        private void changeFormatText(string format, string extension)
        {
            descLabel.Text = "The following warnings and errors must be acknowledged before exporting to the " + format + " (." + extension + ") format.";
            this.Text  = "Export Warning (" + format + ")";
        }

        // Changes which exporter the acceptButton uses.
        private void resetAcceptButtonEvent(Exporter exporter)
        {
            acceptButton.Click -= oldEventHandler;
            oldEventHandler = new EventHandler(delegate (object sender, EventArgs e)
            {
                WarningWindow.exportFormat(exporter, sfd);
                this.Hide(); //"Closes" (same window each time) the warning window when done exporting (failure or not)
            });
            acceptButton.Click += oldEventHandler;
        }

        // Generic behavior for the accept button in the Warning Window
        private static void exportFormat(Exporter exporter, SaveFileDialog sfd)
        {
            string pkuToFormat = ".pku to " + exporter.formatName + " (." + exporter.formatExtension + ")"; //For console logging

            Console.WriteLine("Exporting " + pkuToFormat  + "...");

            sfd.DefaultExt = exporter.formatExtension;
            sfd.Filter = exporter.formatExtension + " files (*." + exporter.formatExtension + ")|*." + exporter.formatExtension + "|All files (*.*)|*.*";
            //saveFileDialog.FilterIndex = 1; //redundant
            DialogResult result = sfd.ShowDialog(); // Show the dialog box.

            if (result == DialogResult.OK) //Successful choice of file name + location
            {
                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(sfd.FileName), sfd.FileName), exporter.toFileChecked());
                Console.WriteLine("Exported " + pkuToFormat + "!");
                //Console.WriteLine(JsonConvert.SerializeObject(exporter.pku, Formatting.Indented)); //TODO maybe one day don't need to print out the entire pku...
            }
            else
                Console.WriteLine("Failed to export " + pkuToFormat + "!");
        }

        // Sets up and opens the warning window, or just auto accepts if there are no warnings/errors
        public void runWarningWindow(Exporter exporter)
        {
            exporter.processAlertsChecked();
            populateWarnings(exporter.warnings, exporter.errors);
            changeFormatText(exporter.formatName, exporter.formatExtension);
            resetAcceptButtonEvent(exporter);
            if (warningPanel.Controls.Count > 0 || errorPanel.Controls.Count > 0)
                ShowDialog();
            else
                oldEventHandler.Invoke(null, null);
        }
    }
}
