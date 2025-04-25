using Newtonsoft.Json.Linq;
using Liber8.Json.Utils.Configuration;
using Liber8.Json.Utils.Conversion;
using Liber8.Json.Utils.ErrorHandling;
using Liber8.Json.Utils.Extraction;
using Liber8.Json.Utils.ArrayProcessing;

namespace Liber8.Json.Utils.Parsing
{
    /// <summary>
    /// Provides default parsing functionality for JSON objects.
    /// </summary>
    public class DefaultParserEngine : IParserEngine
    {
        private readonly IPathResolver _pathResolver;
        private readonly ITypeConverter _typeConverter;
        private readonly IArrayProcessor _arrayProcessor;
        private readonly IErrorHandler _errorHandler;

        /// <summary>
        /// Gets the error handler used by this parser engine.
        /// </summary>
        public IErrorHandler ErrorHandler => _errorHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParserEngine"/> class.
        /// </summary>
        /// <param name="pathResolver">The path resolver to use for resolving paths.</param>
        /// <param name="typeConverter">The type converter to use for converting values.</param>
        /// <param name="errorHandler">The error handler to use for handling errors.</param>
        public DefaultParserEngine(
            IPathResolver pathResolver,
            ITypeConverter typeConverter,
            IErrorHandler errorHandler)
        {
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
            _typeConverter = typeConverter ?? throw new ArgumentNullException(nameof(typeConverter));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _arrayProcessor = new DefaultArrayProcessor(pathResolver, this);
        }

        /// <summary>
        /// Parses a JSON string using the specified action configuration.
        /// </summary>
        /// <param name="jsonString">The JSON string to parse.</param>
        /// <param name="configuration">The action configuration to use.</param>
        /// <returns>A collection of parsed JObjects.</returns>
        public IEnumerable<JObject> Parse(string jsonString, IActionConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new ArgumentException("JSON string cannot be null or whitespace.", nameof(jsonString));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            try
            {
                var jObject = JObject.Parse(jsonString);
                return Parse(jObject, configuration);
            }
            catch (Exception ex)
            {
                _errorHandler.RecordParsingError(
                    $"Failed to parse JSON string: {ex.Message}",
                    jsonString,
                    0,
                    0);

                if (_errorHandler.HasErrors && configuration.FailFast)
                {
                    _errorHandler.ThrowIfErrors();
                }

                return Enumerable.Empty<JObject>();
            }
        }

        /// <summary>
        /// Parses a JObject using the specified action configuration.
        /// </summary>
        /// <param name="jObject">The JObject to parse.</param>
        /// <param name="configuration">The action configuration to use.</param>
        /// <returns>A collection of parsed JObjects.</returns>
        public IEnumerable<JObject> Parse(JObject jObject, IActionConfiguration configuration)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            List<JObject> results = new List<JObject>();
            
            try
            {
                // Check if the JObject contains an array that should be processed
                if (_arrayProcessor.ShouldProcessArray(jObject, configuration))
                {
                    // Process the array and return the results
                    foreach (var processedObject in _arrayProcessor.ProcessArray(jObject, configuration))
                    {
                        // Create the output object for each processed object
                        results.Add(CreateOutputObject(processedObject, configuration));
                    }
                }
                else
                {
                    // Create the output object for the input JObject
                    results.Add(CreateOutputObject(jObject, configuration));
                }
            }
            catch (Exception ex)
            {
                _errorHandler.RecordProcessingError(
                    $"Failed to parse JObject: {ex.Message}",
                    null,
                    ex);

                if (_errorHandler.HasErrors && configuration.FailFast)
                {
                    _errorHandler.ThrowIfErrors();
                }
            }
            
            return results;
        }

        /// <summary>
        /// Extracts a field value from a JObject using the specified field mapping.
        /// </summary>
        /// <param name="jObject">The JObject to extract the value from.</param>
        /// <param name="fieldMapping">The field mapping to use.</param>
        /// <returns>The extracted value if found; otherwise, null.</returns>
        public JToken? ExtractFieldValue(JObject jObject, IFieldMapping fieldMapping)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (fieldMapping == null)
            {
                throw new ArgumentNullException(nameof(fieldMapping));
            }

            try
            {
                // Try to resolve the value using the source paths
                var value = _pathResolver.ResolveFirstNonNullPath(jObject, fieldMapping.SourcePaths);

                // If no value is found, use the default value
                if (value == null || value.Type == JTokenType.Null)
                {
                    return fieldMapping.DefaultValue;
                }

                // If a target type is specified, convert the value
                if (fieldMapping.TargetType != null)
                {
                    try
                    {
                        var convertedValue = _typeConverter.Convert(value, fieldMapping.TargetType);
                        return convertedValue != null ? JToken.FromObject(convertedValue) : null;
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.RecordTypeConversionError(
                            $"Failed to convert value '{value}' to type '{fieldMapping.TargetType.Name}': {ex.Message}",
                            fieldMapping.OutputFieldName,
                            value,
                            fieldMapping.TargetType);

                        if (_errorHandler.ShouldIgnoreErrorsForField(fieldMapping.OutputFieldName))
                        {
                            return fieldMapping.DefaultValue;
                        }

                        throw;
                    }
                }

                return value;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException && _errorHandler.HasErrors))
            {
                _errorHandler.RecordProcessingError(
                    $"Failed to extract field value for '{fieldMapping.OutputFieldName}': {ex.Message}",
                    fieldMapping.OutputFieldName,
                    ex);

                if (_errorHandler.ShouldIgnoreErrorsForField(fieldMapping.OutputFieldName))
                {
                    return fieldMapping.DefaultValue;
                }

                throw;
            }
        }

        /// <summary>
        /// Creates a new JObject with the extracted field values.
        /// </summary>
        /// <param name="jObject">The source JObject.</param>
        /// <param name="configuration">The action configuration to use.</param>
        /// <returns>A new JObject with the extracted field values.</returns>
        public JObject CreateOutputObject(JObject jObject, IActionConfiguration configuration)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var outputObject = new JObject();

            foreach (var fieldMapping in configuration.FieldMappings)
            {
                try
                {
                    var value = ExtractFieldValue(jObject, fieldMapping);

                    // If the value is null and omitIfNotFound is true, skip this field
                    if ((value == null || value.Type == JTokenType.Null) && fieldMapping.OmitIfNotFound)
                    {
                        continue;
                    }

                    // Add the field to the output object
                    outputObject[fieldMapping.OutputFieldName] = value;
                }
                catch (Exception ex) when (!(ex is InvalidOperationException && _errorHandler.HasErrors))
                {
                    _errorHandler.RecordProcessingError(
                        $"Failed to process field '{fieldMapping.OutputFieldName}': {ex.Message}",
                        fieldMapping.OutputFieldName,
                        ex);

                    if (_errorHandler.HasErrors && configuration.FailFast && !_errorHandler.ShouldIgnoreErrorsForField(fieldMapping.OutputFieldName))
                    {
                        _errorHandler.ThrowIfErrors();
                    }
                }
            }

            return outputObject;
        }

        /// <summary>
        /// Determines the action type from a JObject.
        /// </summary>
        /// <param name="jObject">The JObject to determine the action type from.</param>
        /// <param name="actionTypeFieldName">The name of the field that contains the action type.</param>
        /// <returns>The action type if found; otherwise, null.</returns>
        public string? DetermineActionType(JObject jObject, string actionTypeFieldName)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (string.IsNullOrWhiteSpace(actionTypeFieldName))
            {
                throw new ArgumentException("Action type field name cannot be null or whitespace.", nameof(actionTypeFieldName));
            }

            var actionTypeToken = _pathResolver.ResolvePath(jObject, actionTypeFieldName);
            return actionTypeToken?.ToString();
        }

        /// <summary>
        /// Creates a new default parser engine.
        /// </summary>
        /// <param name="pathResolver">The path resolver to use for resolving paths.</param>
        /// <param name="typeConverter">The type converter to use for converting values.</param>
        /// <param name="errorHandler">The error handler to use for handling errors.</param>
        /// <returns>A new default parser engine.</returns>
        public static DefaultParserEngine Create(
            IPathResolver? pathResolver = null,
            ITypeConverter? typeConverter = null,
            IErrorHandler? errorHandler = null)
        {
            pathResolver ??= new JsonPathResolver();
            typeConverter ??= new DefaultTypeConverter();
            errorHandler ??= new ErrorHandler();

            return new DefaultParserEngine(pathResolver, typeConverter, errorHandler);
        }
    }
}
