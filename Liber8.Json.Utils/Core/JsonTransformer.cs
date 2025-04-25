using Newtonsoft.Json.Linq;
using Liber8.Json.Utils.Configuration;
using Liber8.Json.Utils.Parsing;
using Liber8.Json.Utils.ErrorHandling;

namespace Liber8.Json.Utils.Core
{
    /// <summary>
    /// Provides functionality for transforming complex JSON objects into simpler ones
    /// based on configurable field mappings.
    /// </summary>
    public class JsonTransformer : IJsonTransformer
    {
        private readonly IConfigurationRegistry _configurationRegistry;
        private readonly IParserEngine _parserEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTransformer"/> class.
        /// </summary>
        /// <param name="configurationRegistry">The configuration registry to use for retrieving action configurations.</param>
        /// <param name="parserEngine">The parser engine to use for parsing JSON objects.</param>
        public JsonTransformer(IConfigurationRegistry configurationRegistry, IParserEngine parserEngine)
        {
            _configurationRegistry = configurationRegistry ?? throw new ArgumentNullException(nameof(configurationRegistry));
            _parserEngine = parserEngine ?? throw new ArgumentNullException(nameof(parserEngine));
        }

        /// <summary>
        /// Transforms a JSON string into one or more simplified JObjects based on the action type.
        /// </summary>
        /// <param name="jsonString">The JSON string to transform.</param>
        /// <param name="actionTypeFieldName">The name of the field that contains the action type.</param>
        /// <returns>A collection of transformed JObjects.</returns>
        public IEnumerable<JObject> Transform(string jsonString, string actionTypeFieldName = "Action")
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new ArgumentException("JSON string cannot be null or whitespace.", nameof(jsonString));
            }

            if (string.IsNullOrWhiteSpace(actionTypeFieldName))
            {
                throw new ArgumentException("Action type field name cannot be null or whitespace.", nameof(actionTypeFieldName));
            }

            try
            {
                var jObject = JObject.Parse(jsonString);
                return Transform(jObject, actionTypeFieldName);
            }
            catch (Exception ex)
            {
                _parserEngine.ErrorHandler.RecordParsingError(
                    $"Failed to parse JSON string: {ex.Message}",
                    jsonString,
                    0,
                    0);

                _parserEngine.ErrorHandler.ThrowIfErrors();
                return Enumerable.Empty<JObject>();
            }
        }

        /// <summary>
        /// Transforms a JObject into one or more simplified JObjects based on the action type.
        /// </summary>
        /// <param name="jObject">The JObject to transform.</param>
        /// <param name="actionTypeFieldName">The name of the field that contains the action type.</param>
        /// <returns>A collection of transformed JObjects.</returns>
        public IEnumerable<JObject> Transform(JObject jObject, string actionTypeFieldName = "Action")
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (string.IsNullOrWhiteSpace(actionTypeFieldName))
            {
                throw new ArgumentException("Action type field name cannot be null or whitespace.", nameof(actionTypeFieldName));
            }

            // Determine the action type
            var actionType = _parserEngine.DetermineActionType(jObject, actionTypeFieldName);
            if (string.IsNullOrWhiteSpace(actionType))
            {
                _parserEngine.ErrorHandler.RecordProcessingError(
                    $"Action type not found in field '{actionTypeFieldName}'.",
                    actionTypeFieldName);

                // Try to use the default configuration if available
                var defaultConfiguration = _configurationRegistry.GetDefaultConfiguration();
                if (defaultConfiguration != null)
                {
                    return _parserEngine.Parse(jObject, defaultConfiguration);
                }

                _parserEngine.ErrorHandler.ThrowIfErrors();
                return Enumerable.Empty<JObject>();
            }

            // Get the configuration for the action type
            var configuration = _configurationRegistry.GetConfiguration(actionType);
            if (configuration == null)
            {
                _parserEngine.ErrorHandler.RecordProcessingError(
                    $"Configuration not found for action type '{actionType}'.",
                    actionTypeFieldName);

                // Try to use the default configuration if available
                var defaultConfiguration = _configurationRegistry.GetDefaultConfiguration();
                if (defaultConfiguration != null)
                {
                    return _parserEngine.Parse(jObject, defaultConfiguration);
                }

                _parserEngine.ErrorHandler.ThrowIfErrors();
                return Enumerable.Empty<JObject>();
            }

            // Parse the JObject using the configuration
            return _parserEngine.Parse(jObject, configuration);
        }

        /// <summary>
        /// Transforms a JSON string into one or more simplified JObjects using the specified action type.
        /// </summary>
        /// <param name="jsonString">The JSON string to transform.</param>
        /// <param name="actionType">The explicit action type to use for transformation.</param>
        /// <returns>A collection of transformed JObjects.</returns>
        public IEnumerable<JObject> TransformWithExplicitAction(string jsonString, string actionType)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new ArgumentException("JSON string cannot be null or whitespace.", nameof(jsonString));
            }

            if (string.IsNullOrWhiteSpace(actionType))
            {
                throw new ArgumentException("Action type cannot be null or whitespace.", nameof(actionType));
            }

            try
            {
                var jObject = JObject.Parse(jsonString);
                return TransformWithExplicitAction(jObject, actionType);
            }
            catch (Exception ex)
            {
                _parserEngine.ErrorHandler.RecordParsingError(
                    $"Failed to parse JSON string: {ex.Message}",
                    jsonString,
                    0,
                    0);

                _parserEngine.ErrorHandler.ThrowIfErrors();
                return Enumerable.Empty<JObject>();
            }
        }

        /// <summary>
        /// Transforms a JObject into one or more simplified JObjects using the specified action type.
        /// </summary>
        /// <param name="jObject">The JObject to transform.</param>
        /// <param name="actionType">The explicit action type to use for transformation.</param>
        /// <returns>A collection of transformed JObjects.</returns>
        public IEnumerable<JObject> TransformWithExplicitAction(JObject jObject, string actionType)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (string.IsNullOrWhiteSpace(actionType))
            {
                throw new ArgumentException("Action type cannot be null or whitespace.", nameof(actionType));
            }

            // Get the configuration for the action type
            var configuration = _configurationRegistry.GetConfiguration(actionType);
            if (configuration == null)
            {
                _parserEngine.ErrorHandler.RecordProcessingError(
                    $"Configuration not found for action type '{actionType}'.",
                    null);

                // Try to use the default configuration if available
                var defaultConfiguration = _configurationRegistry.GetDefaultConfiguration();
                if (defaultConfiguration != null)
                {
                    return _parserEngine.Parse(jObject, defaultConfiguration);
                }

                _parserEngine.ErrorHandler.ThrowIfErrors();
                return Enumerable.Empty<JObject>();
            }

            // Parse the JObject using the configuration
            return _parserEngine.Parse(jObject, configuration);
        }

        /// <summary>
        /// Creates a new JSON transformer.
        /// </summary>
        /// <param name="configurationRegistry">The configuration registry to use for retrieving action configurations.</param>
        /// <param name="parserEngine">The parser engine to use for parsing JSON objects.</param>
        /// <returns>A new JSON transformer.</returns>
        public static JsonTransformer Create(IConfigurationRegistry configurationRegistry, IParserEngine? parserEngine = null)
        {
            parserEngine ??= DefaultParserEngine.Create();
            return new JsonTransformer(configurationRegistry, parserEngine);
        }

        /// <summary>
        /// Creates a new JSON transformer with a default configuration registry and parser engine.
        /// </summary>
        /// <returns>A new JSON transformer.</returns>
        public static JsonTransformer Create()
        {
            var configurationRegistry = ConfigurationRegistry.Create();
            var parserEngine = DefaultParserEngine.Create();
            return new JsonTransformer(configurationRegistry, parserEngine);
        }
    }
}
