using pkuManager.Alerts;
using pkuManager.pku;
using System;
using System.Collections.Generic;

namespace pkuManager.Common
{
    // Base class for all importers.
    // Usage: an implemented importer is instantiated with the desired pku. It can then return a pkuObject via toPKU(), if canImport() is true.
    public abstract class Importer
    {
        // The file to be (hopefully) imported by this exporter.
        public byte[] file;

        // Whether this importer instance is part of a check-in.
        public bool checkInMode;

        // The global flags to (optionally) be acted upon by the importer.
        public GlobalFlags globalFlags;

        // Lists of the warning and error panels to be displayed on the warning window.
        public List<Alert> questions, notes;

        // Whether or not the alerts have been processed or not.
        private bool alertsProcessed = false;

        // Instantiates the importer with the given file, GlobalFlags, and whether this is a check-in or not.
        public Importer(byte[] file, GlobalFlags globalFlags, bool checkInMode)
        {
            this.file = file;
            this.globalFlags = globalFlags;
            this.checkInMode = checkInMode;
            questions = new List<Alert>();
            notes = new List<Alert>();
        }

        /// <summary>
        /// Whether or not the given file can be imported as the assumed format (i.e. it's valid).
        /// </summary>
        /// <returns></returns>
        public abstract bool canImport();

        /// <summary>
        /// Adds all the relevant <see cref="Alert"/>s to the <see cref="questions"/>, and <see cref="notes"/> lists
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

        // A pkuObject of the imported "formatName" format file.
        // processAlerts will have been run before this.
        protected abstract pkuObject toPKU();

        // Same as above but checks if this file can be imported before trying it. Throws error if it cannot, or alerts have not been processed.
        public pkuObject toPKUChecked()
        {
            if (!canImport())
                throw new Exception("This .pku cannot be exported to the targeted format, yet toPKUChecked() was called anyway.");
            if (!alertsProcessed)
                throw new Exception("The alerts for this .pku have not been processed yet! This should not have happened.");

            return toPKU();
        }
    }
}
