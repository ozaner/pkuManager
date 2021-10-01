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
        /// Fills in the details using a file version of this format,
        /// overwriting any data that was there previously.
        /// </summary>
        /// <param name="file">A file that satisfies <see cref="IsFile(byte[])"/>.</param>
        public abstract void FromFile(byte[] file);
    }
}
