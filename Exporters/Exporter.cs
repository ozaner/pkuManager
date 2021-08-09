using pkuManager.Alerts;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace pkuManager
{
    // Base class for all exporters. All formats must have a corresponding exporter class that implements this.
    // Usage: an implemented exporter is instantiated with the desired pku. It can then be exported via toFileChecked(), if canExport() is true.
    public abstract class Exporter
    {
        // The particular pku to be (hopefully) exported by this exporter
        public PKUObject pku;

        // Lists of the warning and error panels to be displayed on the warning window.
        public List<AlertBox> warnings, errors;

        // Whether or not the alerts have been processed or not.
        private bool alertsProcessed = false;

        // Instantiates the exporter with the given pku
        public Exporter(PKUObject pku)
        {
            if (pku == null)
                throw new Exception("Can't make an exporter with a null .pku!");
            this.pku = pku;
            warnings = new List<AlertBox>();
            errors = new List<AlertBox>();
        }

        /// <summary>
        /// The <i>unique</i> name of format this exporter converts to
        /// (e.g. Gen 4 Pokemon, Showdown!, etc.).
        /// </summary>
        public abstract string formatName { get; }

        /// <summary>
        /// The file extension of the format this exporter converts to (e.g. pk4, txt, etc.).
        /// </summary>
        public abstract string formatExtension { get; }

        // Whether or not the given pku can be exported at all (e.g. Giratina cannot be exported to pk3)
        public abstract bool canExport();

        // Creates a list of warning and error panels relating to the pku given at instantiation.
        // Since the exporter creates them, it has a reference to them and can use them to export the pokemon
        // (e.g. can select an option from an error panel with radio buttons and exporter uses that choice)
        protected abstract void processAlerts();

        // Processes the alerts and also sets the alertsProcessed flag to true.
        public void processAlertsChecked()
        {
            alertsProcessed = true;
            processAlerts();
        }

        // A byte array representing the exported file of the given pku in the "formatName" format.
        // processAlerts will have been run before this.
        protected abstract byte[] toFile();

        // Same as below but checks if this pku can be exported before trying it. Throws error if it cannot, or alerts have not been processed.
        public byte[] toFileChecked()
        {
            if (!canExport())
                throw new Exception("This .pku cannot be exported to " + formatName + "(." + formatExtension + ")!");
            if(!alertsProcessed)
                throw new Exception("The alerts for this .pku have not been processed yet! This should not have happened.");

            return toFile();
        }
    }
}
