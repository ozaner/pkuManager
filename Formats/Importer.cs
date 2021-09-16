using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using System;
using System.Collections.Generic;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats
{
    /// <summary>
    /// The base class for all Importers. All formats that can be<br/>
    /// imported must have a corresponding class implements this.
    /// </summary>
    public abstract class Importer : Porter
    {
        /// <summary>
        /// <see langword="true"/> importer instance is meant to check-in the given file.<br/>
        /// <see langword="false"/> if it is meant to just import it.
        /// </summary>
        public bool CheckInMode { get; }

        /// <summary>
        /// A list of questions to be displayed on the exporter window.<br/>
        /// A question is an alert about a value that, generally, requires input from the user.
        /// </summary>
        public List<Alert> Questions { get; } = new();

        /// <summary>
        /// Reference to passed file for <see cref="CanPort"/> to check.
        /// </summary>
        private byte[] file;

        /// <summary>
        /// The base importer constructor.
        /// </summary>
        /// <inheritdoc cref="Porter(pkuObject, GlobalFlags, FormatObject)"/>
        public Importer(byte[] file, GlobalFlags globalFlags, bool checkInMode) : base(new(), globalFlags)
        {
            CheckInMode = checkInMode;

            // if file isn't valid, importer won't ever reach ToPKU.
            this.file = file;
            if (Data.IsFile(file).isValid)
                DataType.GetMethod(nameof(FormatObject.FromFile)).Invoke(Data, new[] { file });
        }

        public sealed override (bool canPort, string reason) CanPort()
        {
            (bool isValid, string reason) = Data.IsFile(file);
            if (!isValid)
                return (false, reason);
            else
                return CanImport();
        }

        /// <summary>
        /// Determines whether or not the given file can be imported to the desired format and a
        /// reason why if it cannot, assuming it passed <see cref="FormatObject.IsFile(byte[])"/>.
        /// </summary>
        /// <inheritdoc cref="CanPort"/>
        public abstract (bool isValid, string reason) CanImport();

        /// <summary>
        /// Returns the imported <see cref="pkuObject"/> generated from the given file.<br/>
        /// Should only be run after <see cref="Porter.FirstHalf"/>.
        /// </summary>
        /// <returns>A <see cref="byte"/> array representation of the exported file.</returns>
        public pkuObject ToPKU()
        {
            SecondHalf();
            return pku;
        }


        /* ------------------------------------
         * Universal Import Questions
         * ------------------------------------
        */
        // TrueOT
        [PorterDirective(ProcessingPhase.FirstPass)]
        protected virtual void AskTrueOT()
        {
            RadioButtonAlert rba = new("True OT", "Do you want to include a True OT on this pku?", new RadioButtonAlert.RBAChoice[]
            {
                new("Include True OT:", null, true),
                new("Don't Include", null)
            });
            TrueOTResolver = new(rba, new Func<string>[] { () => rba.Choices[0].TextEntry, () => null }, s => pku.True_OT = s);
            Questions.Add(rba);
        }

        // TrueOT ErrorResolver
        [PorterDirective(ProcessingPhase.SecondPass)]
        protected virtual ErrorResolver<string> TrueOTResolver { get; set; }
    }
}
