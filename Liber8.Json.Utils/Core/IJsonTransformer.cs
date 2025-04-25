using Newtonsoft.Json.Linq;

namespace Liber8.Json.Utils.Core
{
    /// <summary>
    /// Defines the contract for transforming complex JSON objects into simpler ones
    /// based on configurable field mappings.
    /// </summary>
    public interface IJsonTransformer
    {
        /// <summary>
        /// Transforms a JSON string into one or more simplified JObjects based on the action type.
        /// </summary>
        /// <param name="jsonString">The JSON string to transform.</param>
        /// <param name="actionTypeFieldName">The name of the field that contains the action type.</param>
        /// <returns>A collection of transformed JObjects.</returns>
        IEnumerable<JObject> Transform(string jsonString, string actionTypeFieldName = "Action");

        /// <summary>
        /// Transforms a JObject into one or more simplified JObjects based on the action type.
        /// </summary>
        /// <param name="jObject">The JObject to transform.</param>
        /// <param name="actionTypeFieldName">The name of the field that contains the action type.</param>
        /// <returns>A collection of transformed JObjects.</returns>
        IEnumerable<JObject> Transform(JObject jObject, string actionTypeFieldName = "Action");

        /// <summary>
        /// Transforms a JSON string into one or more simplified JObjects using the specified action type.
        /// </summary>
        /// <param name="jsonString">The JSON string to transform.</param>
        /// <param name="actionType">The explicit action type to use for transformation.</param>
        /// <returns>A collection of transformed JObjects.</returns>
        IEnumerable<JObject> TransformWithExplicitAction(string jsonString, string actionType);

        /// <summary>
        /// Transforms a JObject into one or more simplified JObjects using the specified action type.
        /// </summary>
        /// <param name="jObject">The JObject to transform.</param>
        /// <param name="actionType">The explicit action type to use for transformation.</param>
        /// <returns>A collection of transformed JObjects.</returns>
        IEnumerable<JObject> TransformWithExplicitAction(JObject jObject, string actionType);
    }
}
