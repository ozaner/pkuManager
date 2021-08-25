namespace pkuManager.Common
{
    /// <summary>
    /// The base class for all Pokemon data structure implementations.
    /// </summary>
    public abstract class FormatObject
    {
        /// <summary>
        /// Exports this object as a file in the implemented format.
        /// </summary>
        /// <returns>A <see cref="byte"/> array representation of the file this object represents.</returns>
        public abstract byte[] ToFile();
    }
}
