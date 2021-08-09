using pkuManager.Alerts;
using System;
using System.Collections.Generic;

namespace pkuManager.Common
{
    // Base class for all exporters. All formats must have a corresponding exporter class that implements this.
    // Usage: an implemented exporter is instantiated with the desired pku. It can then be exported via toFileChecked(), if canExport() is true.
    public abstract class Exporter
    {
        // The particular pku to be (hopefully) exported by this exporter
        public PKUObject pku;

        // The global flags to (optionally) be acted upon by the exporter.
        public GlobalFlags globalFlags;

        // Lists of the warning and error panels to be displayed on the warning window.
        public List<Alert> warnings, errors, notes;

        // Whether or not the alerts have been processed or not.
        private bool alertsProcessed = false;

        // Instantiates the exporter with the given pku and GlobalFlags
        public Exporter(PKUObject pku, GlobalFlags globalFlags)
        {
            this.pku = pku ?? throw new ArgumentException("Can't make an exporter with a null .pku!");
            this.globalFlags = globalFlags;
            warnings = new List<Alert>();
            errors = new List<Alert>();
            notes = new List<Alert>();
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

        /// <summary>
        /// Whether or not the given pku can be exported to this format at all (e.g. Giratina cannot be exported to pk3).
        /// </summary>
        /// <returns></returns>
        public abstract bool canExport();

        /// <summary>
        /// Adds all the relevant <see cref="Alert"/>s to the <see cref="warnings"/>, <see cref="errors"/>, and <see cref="notes"/> lists
        /// pertaining to the pku given at instantiation. Note that since the implemented exporter is creating these Alerts,
        /// they can keep references to them for use in error resolution.
        /// </summary>
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
            if (!alertsProcessed)
                throw new Exception("The alerts for this .pku have not been processed yet! This should not have happened.");

            return toFile();
        }
    }
}
