using Newtonsoft.Json.Linq;

namespace Liber8.Json.Utils.ErrorHandling
{
    /// <summary>
    /// Defines the contract for handling errors during JSON processing.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Gets a value indicating whether any errors have been recorded.
        /// </summary>
        bool HasErrors { get; }

        /// <summary>
        /// Gets the collection of recorded errors.
        /// </summary>
        IReadOnlyCollection<ProcessingError> Errors { get; }

        /// <summary>
        /// Records a parsing error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="jsonText">The JSON text that caused the error.</param>
        /// <param name="lineNumber">The line number where the error occurred.</param>
        /// <param name="position">The position within the line where the error occurred.</param>
        void RecordParsingError(string message, string jsonText, int lineNumber, int position);

        /// <summary>
        /// Records a path resolution error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="path">The path that could not be resolved.</param>
        /// <param name="jObject">The JSON object being processed.</param>
        void RecordPathResolutionError(string message, string fieldName, string path, JObject jObject);

        /// <summary>
        /// Records a type conversion error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="value">The value that could not be converted.</param>
        /// <param name="targetType">The target type for the conversion.</param>
        void RecordTypeConversionError(string message, string fieldName, JToken value, Type targetType);

        /// <summary>
        /// Records a general processing error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="exception">The exception that caused the error, if any.</param>
        void RecordProcessingError(string message, string? fieldName = null, Exception? exception = null);

        /// <summary>
        /// Determines whether to ignore errors for a specific field.
        /// </summary>
        /// <param name="fieldName">The name of the field to check.</param>
        /// <returns>True if errors for the field should be ignored; otherwise, false.</returns>
        bool ShouldIgnoreErrorsForField(string fieldName);

        /// <summary>
        /// Sets the collection of field names for which errors should be ignored.
        /// </summary>
        /// <param name="fieldNames">The collection of field names.</param>
        void SetIgnoreErrorsForFields(IEnumerable<string> fieldNames);

        /// <summary>
        /// Sets whether to fail fast on the first error.
        /// </summary>
        /// <param name="failFast">True to fail fast; otherwise, false.</param>
        void SetFailFast(bool failFast);

        /// <summary>
        /// Clears all recorded errors.
        /// </summary>
        void ClearErrors();

        /// <summary>
        /// Throws an exception if any errors have been recorded.
        /// </summary>
        /// <exception cref="AggregateException">Thrown when errors have been recorded.</exception>
        void ThrowIfErrors();
    }
}
