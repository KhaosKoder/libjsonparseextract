using Newtonsoft.Json.Linq;
using Liber8.Json.Utils.Configuration;
using Liber8.Json.Utils.ErrorHandling;

namespace Liber8.Json.Utils.Parsing
{
    /// <summary>
    /// Defines the contract for the parser engine that extracts and transforms data.
    /// </summary>
    public interface IParserEngine
    {
        /// <summary>
        /// Gets the error handler used by this parser engine.
        /// </summary>
        IErrorHandler ErrorHandler { get; }

        /// <summary>
        /// Parses a JSON string using the specified action configuration.
        /// </summary>
        /// <param name="jsonString">The JSON string to parse.</param>
        /// <param name="configuration">The action configuration to use.</param>
        /// <returns>A collection of parsed JObjects.</returns>
        IEnumerable<JObject> Parse(string jsonString, IActionConfiguration configuration);

        /// <summary>
        /// Parses a JObject using the specified action configuration.
        /// </summary>
        /// <param name="jObject">The JObject to parse.</param>
        /// <param name="configuration">The action configuration to use.</param>
        /// <returns>A collection of parsed JObjects.</returns>
        IEnumerable<JObject> Parse(JObject jObject, IActionConfiguration configuration);

        /// <summary>
        /// Extracts a field value from a JObject using the specified field mapping.
        /// </summary>
        /// <param name="jObject">The JObject to extract the value from.</param>
        /// <param name="fieldMapping">The field mapping to use.</param>
        /// <returns>The extracted value if found; otherwise, null.</returns>
        JToken? ExtractFieldValue(JObject jObject, IFieldMapping fieldMapping);

        /// <summary>
        /// Creates a new JObject with the extracted field values.
        /// </summary>
        /// <param name="jObject">The source JObject.</param>
        /// <param name="configuration">The action configuration to use.</param>
        /// <returns>A new JObject with the extracted field values.</returns>
        JObject CreateOutputObject(JObject jObject, IActionConfiguration configuration);

        /// <summary>
        /// Determines the action type from a JObject.
        /// </summary>
        /// <param name="jObject">The JObject to determine the action type from.</param>
        /// <param name="actionTypeFieldName">The name of the field that contains the action type.</param>
        /// <returns>The action type if found; otherwise, null.</returns>
        string? DetermineActionType(JObject jObject, string actionTypeFieldName);
    }
}
