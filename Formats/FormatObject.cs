namespace pkuManager.Formats
{
    /// <summary>
    /// The base class for all Pokemon data structure implementations.
    /// </summary>
    public abstract class FormatObject
    {
        /// <summary>
        /// Exports this object as a file in the implemented format.
        /// </summary>
        /// <returns>A <see cref="byte"/> array representation of the
        ///          file this object represents.</returns>
        public abstract byte[] ToFile();

        /// <summary>
        /// Determines whether the given file is a valid instance of this format.
        /// </summary>
        /// <param name="file">A file.</param>
        /// <returns>Whether or not this file is valid, and a reason if it's not.</returns>
        public abstract (bool isValid, string reason) IsFile(byte[] file);

        /// <summary>
        /// Fills in the details using a file version of this format,
        /// overwriting any data that was there previously.
        /// </summary>
        /// <param name="file">A file that satisfies <see cref="IsFile(byte[])"/>.</param>
        public abstract void FromFile(byte[] file);
    }
}
