using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace Liber8.Json.Utils.ErrorHandling
{
    /// <summary>
    /// Represents a handler for errors that occur during JSON processing.
    /// </summary>
    public class ErrorHandler : IErrorHandler
    {
        private readonly ConcurrentBag<ProcessingError> _errors = new();
        private readonly HashSet<string> _ignoreErrorsForFields = new();
        private bool _failFast;

        /// <summary>
        /// Gets a value indicating whether any errors have been recorded.
        /// </summary>
        public bool HasErrors => _errors.Any();

        /// <summary>
        /// Gets the collection of recorded errors.
        /// </summary>
        public IReadOnlyCollection<ProcessingError> Errors => _errors.ToList();

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandler"/> class.
        /// </summary>
        /// <param name="failFast">Whether to fail fast on the first error.</param>
        /// <param name="ignoreErrorsForFields">The collection of field names for which errors should be ignored.</param>
        public ErrorHandler(bool failFast = false, IEnumerable<string>? ignoreErrorsForFields = null)
        {
            _failFast = failFast;
            if (ignoreErrorsForFields != null)
            {
                foreach (var fieldName in ignoreErrorsForFields)
                {
                    _ignoreErrorsForFields.Add(fieldName);
                }
            }
        }

        /// <summary>
        /// Records a parsing error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="jsonText">The JSON text that caused the error.</param>
        /// <param name="lineNumber">The line number where the error occurred.</param>
        /// <param name="position">The position within the line where the error occurred.</param>
        public void RecordParsingError(string message, string jsonText, int lineNumber, int position)
        {
            var error = new ProcessingError(message, jsonText, lineNumber, position);
            _errors.Add(error);

            if (_failFast)
            {
                ThrowIfErrors();
            }
        }

        /// <summary>
        /// Records a path resolution error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="path">The path that could not be resolved.</param>
        /// <param name="jObject">The JSON object being processed.</param>
        public void RecordPathResolutionError(string message, string fieldName, string path, JObject jObject)
        {
            if (ShouldIgnoreErrorsForField(fieldName))
            {
                return;
            }

            var error = new ProcessingError(message, fieldName, path, jObject);
            _errors.Add(error);

            if (_failFast)
            {
                ThrowIfErrors();
            }
        }

        /// <summary>
        /// Records a type conversion error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="value">The value that could not be converted.</param>
        /// <param name="targetType">The target type for the conversion.</param>
        public void RecordTypeConversionError(string message, string fieldName, JToken value, Type targetType)
        {
            if (ShouldIgnoreErrorsForField(fieldName))
            {
                return;
            }

            var error = new ProcessingError(message, fieldName, value, targetType);
            _errors.Add(error);

            if (_failFast)
            {
                ThrowIfErrors();
            }
        }

        /// <summary>
        /// Records a general processing error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="fieldName">The name of the field being processed.</param>
        /// <param name="exception">The exception that caused the error, if any.</param>
        public void RecordProcessingError(string message, string? fieldName = null, Exception? exception = null)
        {
            if (fieldName != null && ShouldIgnoreErrorsForField(fieldName))
            {
                return;
            }

            var error = new ProcessingError(message, fieldName, exception);
            _errors.Add(error);

            if (_failFast)
            {
                ThrowIfErrors();
            }
        }

        /// <summary>
        /// Determines whether to ignore errors for a specific field.
        /// </summary>
        /// <param name="fieldName">The name of the field to check.</param>
        /// <returns>True if errors for the field should be ignored; otherwise, false.</returns>
        public bool ShouldIgnoreErrorsForField(string fieldName)
        {
            return _ignoreErrorsForFields.Contains(fieldName);
        }

        /// <summary>
        /// Sets the collection of field names for which errors should be ignored.
        /// </summary>
        /// <param name="fieldNames">The collection of field names.</param>
        public void SetIgnoreErrorsForFields(IEnumerable<string> fieldNames)
        {
            _ignoreErrorsForFields.Clear();
            foreach (var fieldName in fieldNames)
            {
                _ignoreErrorsForFields.Add(fieldName);
            }
        }

        /// <summary>
        /// Sets whether to fail fast on the first error.
        /// </summary>
        /// <param name="failFast">True to fail fast; otherwise, false.</param>
        public void SetFailFast(bool failFast)
        {
            _failFast = failFast;
        }

        /// <summary>
        /// Clears all recorded errors.
        /// </summary>
        public void ClearErrors()
        {
            while (!_errors.IsEmpty)
            {
                _errors.TryTake(out _);
            }
        }

        /// <summary>
        /// Throws an exception if any errors have been recorded.
        /// </summary>
        /// <exception cref="AggregateException">Thrown when errors have been recorded.</exception>
        public void ThrowIfErrors()
        {
            if (HasErrors)
            {
                var exceptions = _errors.Select(e => new Exception(e.ToString())).ToList();
                throw new AggregateException("One or more errors occurred during JSON processing.", exceptions);
            }
        }

        /// <summary>
        /// Creates a new error handler.
        /// </summary>
        /// <param name="failFast">Whether to fail fast on the first error.</param>
        /// <param name="ignoreErrorsForFields">The collection of field names for which errors should be ignored.</param>
        /// <returns>A new error handler.</returns>
        public static ErrorHandler Create(bool failFast = false, IEnumerable<string>? ignoreErrorsForFields = null)
        {
            return new ErrorHandler(failFast, ignoreErrorsForFields);
        }
    }
}
