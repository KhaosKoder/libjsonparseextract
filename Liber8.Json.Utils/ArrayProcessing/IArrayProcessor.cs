using Newtonsoft.Json.Linq;
using Liber8.Json.Utils.Configuration;

namespace Liber8.Json.Utils.ArrayProcessing
{
    /// <summary>
    /// Defines the contract for processing arrays in JSON objects.
    /// </summary>
    public interface IArrayProcessor
    {
        /// <summary>
        /// Processes an array field in a JSON object according to the action configuration.
        /// </summary>
        /// <param name="jObject">The JSON object containing the array.</param>
        /// <param name="configuration">The action configuration.</param>
        /// <returns>A collection of processed JSON objects.</returns>
        IEnumerable<JObject> ProcessArray(JObject jObject, IActionConfiguration configuration);

        /// <summary>
        /// Explodes an array field in a JSON object into multiple JSON objects.
        /// </summary>
        /// <param name="jObject">The JSON object containing the array.</param>
        /// <param name="arrayFieldName">The name of the array field to explode.</param>
        /// <param name="parentContextFields">The collection of field names from the parent context to include with each array item.</param>
        /// <param name="arrayItemFilter">The filter expression to determine which array items should be included.</param>
        /// <returns>A collection of exploded JSON objects.</returns>
        IEnumerable<JObject> ExplodeArray(JObject jObject, string arrayFieldName, IEnumerable<string>? parentContextFields = null, string? arrayItemFilter = null);

        /// <summary>
        /// Preserves an array structure while simplifying its elements.
        /// </summary>
        /// <param name="jArray">The array to process.</param>
        /// <param name="configuration">The action configuration to apply to each array element.</param>
        /// <returns>A processed array with simplified elements.</returns>
        JArray PreserveArray(JArray jArray, IActionConfiguration configuration);

        /// <summary>
        /// Determines whether a JSON object contains an array field that should be processed.
        /// </summary>
        /// <param name="jObject">The JSON object to check.</param>
        /// <param name="configuration">The action configuration.</param>
        /// <returns>True if the JSON object contains an array field that should be processed; otherwise, false.</returns>
        bool ShouldProcessArray(JObject jObject, IActionConfiguration configuration);

        /// <summary>
        /// Evaluates an array item filter expression against a JSON object.
        /// </summary>
        /// <param name="jObject">The JSON object to evaluate the filter against.</param>
        /// <param name="filterExpression">The filter expression to evaluate.</param>
        /// <returns>True if the JSON object passes the filter; otherwise, false.</returns>
        bool EvaluateFilter(JObject jObject, string filterExpression);
    }
}
