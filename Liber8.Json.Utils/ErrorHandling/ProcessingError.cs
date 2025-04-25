using Newtonsoft.Json.Linq;

namespace Liber8.Json.Utils.ErrorHandling
{
    /// <summary>
    /// Represents an error that occurred during JSON processing.
    /// </summary>
    public class ProcessingError
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the type of the error.
        /// </summary>
        public ProcessingErrorType ErrorType { get; }

        /// <summary>
        /// Gets the name of the field being processed when the error occurred.
        /// </summary>
        public string? FieldName { get; }

        /// <summary>
        /// Gets the path that could not be resolved.
        /// </summary>
        public string? Path { get; }

        /// <summary>
        /// Gets the JSON object being processed when the error occurred.
        /// </summary>
        public JObject? JsonObject { get; }

        /// <summary>
        /// Gets the JSON text that caused the error.
        /// </summary>
        public string? JsonText { get; }

        /// <summary>
        /// Gets the line number where the error occurred.
        /// </summary>
        public int? LineNumber { get; }

        /// <summary>
        /// Gets the position within the line where the error occurred.
        /// </summary>
        public int? Position { get; }

        /// <summary>
        /// Gets the value that could not be converted.
        /// </summary>
        public JToken? Value { get; }

        /// <summary>
        /// Gets the target type for the conversion.
        /// </summary>
        public Type? TargetType { get; }

        /// <summary>
        /// Gets the exception that caused the error, if any.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingError"/> class for a parsing error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="jsonText">The JSON text that caused the error.</param>
        /// <param name="lineNumber">The line number where the error occurred.</param>
        /// <param name="position">The position within the line where the error occurred.</param>
        public ProcessingError(string message, string jsonText, int lineNumber, int position)
        {
            Message = message;
            ErrorType = ProcessingErrorType.Parsing;
            JsonText = jsonText;
            LineNumber = lineNumber;
            Position = position;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingError"/> class for a path resolution error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="path">The path that could not be resolved.</param>
        /// <param name="jObject">The JSON object being processed.</param>
        public ProcessingError(string message, string fieldName, string path, JObject jObject)
        {
            Message = message;
            ErrorType = ProcessingErrorType.PathResolution;
            FieldName = fieldName;
            Path = path;
            JsonObject = jObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingError"/> class for a type conversion error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="value">The value that could not be converted.</param>
        /// <param name="targetType">The target type for the conversion.</param>
        public ProcessingError(string message, string fieldName, JToken value, Type targetType)
        {
            Message = message;
            ErrorType = ProcessingErrorType.TypeConversion;
            FieldName = fieldName;
            Value = value;
            TargetType = targetType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingError"/> class for a general processing error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="exception">The exception that caused the error, if any.</param>
        public ProcessingError(string message, string? fieldName = null, Exception? exception = null)
        {
            Message = message;
            ErrorType = ProcessingErrorType.General;
            FieldName = fieldName;
            Exception = exception;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{ErrorType} error: {Message}" + (FieldName != null ? $" (Field: {FieldName})" : string.Empty);
        }
    }
}
