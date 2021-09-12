using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.pku;
using System.Collections.Generic;

namespace pkuManager.Formats
{
    /// <summary>
    /// The base class for all Exporters. All formats that can be<br/>
    /// exported must have a corresponding class implements this.
    /// </summary>
    public abstract class Exporter : Porter
    {
        /// <summary>
        /// A list of warnings to be displayed on the exporter window.<br/>
        /// A warning is an alert about a value that, generally, requires no input from the user.
        /// </summary>
        public List<Alert> Warnings { get; } = new();

        /// <summary>
        /// A list of errors to be displayed on the exporter window.<br/>
        /// An error is an alert about a value that, generally, requires input from the user.
        /// </summary>
        public List<Alert> Errors { get; } = new();

        /// <summary>
        /// The base exporter constructor.
        /// </summary>
        /// <param name="data">A data structure representing the non-pku format.<br/>
        ///                    This parameter should be hidden by any implementation of a porter.</param>
        /// <inheritdoc cref="Porter(pkuObject, GlobalFlags, FormatObject)"/>
        public Exporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

        /// <summary>
        /// Returns the exported file generated from the given <see cref="pku"/>.<br/>
        /// Should only be run after <see cref="Porter.FirstHalf"/>.
        /// </summary>
        /// <returns>A <see cref="byte"/> array representation of the exported file.</returns>
        public byte[] ToFile()
        {
            SecondHalf();
            return Data.ToFile();
        }
    }
}
