namespace Liber8.Json.Utils.ErrorHandling
{
    /// <summary>
    /// Defines the types of errors that can occur during JSON processing.
    /// </summary>
    public enum ProcessingErrorType
    {
        /// <summary>
        /// A general processing error.
        /// </summary>
        General,

        /// <summary>
        /// An error that occurred during JSON parsing.
        /// </summary>
        Parsing,

        /// <summary>
        /// An error that occurred during path resolution.
        /// </summary>
        PathResolution,

        /// <summary>
        /// An error that occurred during type conversion.
        /// </summary>
        TypeConversion,

        /// <summary>
        /// An error that occurred during array processing.
        /// </summary>
        ArrayProcessing,

        /// <summary>
        /// An error that occurred during configuration validation.
        /// </summary>
        ConfigurationValidation
    }
}
