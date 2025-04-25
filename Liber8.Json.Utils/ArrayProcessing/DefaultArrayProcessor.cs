using Newtonsoft.Json.Linq;
using Liber8.Json.Utils.Configuration;
using Liber8.Json.Utils.Extraction;
using Liber8.Json.Utils.Parsing;

namespace Liber8.Json.Utils.ArrayProcessing
{
    /// <summary>
    /// Provides default array processing functionality.
    /// </summary>
    public class DefaultArrayProcessor : IArrayProcessor
    {
        private readonly IPathResolver _pathResolver;
        private readonly IParserEngine? _parserEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultArrayProcessor"/> class.
        /// </summary>
        /// <param name="pathResolver">The path resolver to use for resolving paths.</param>
        /// <param name="parserEngine">The parser engine to use for processing array elements.</param>
        public DefaultArrayProcessor(IPathResolver pathResolver, IParserEngine? parserEngine = null)
        {
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
            _parserEngine = parserEngine;
        }

        /// <summary>
        /// Processes an array field in a JSON object according to the action configuration.
        /// </summary>
        /// <param name="jObject">The JSON object containing the array.</param>
        /// <param name="configuration">The action configuration.</param>
        /// <returns>A collection of processed JSON objects.</returns>
        public IEnumerable<JObject> ProcessArray(JObject jObject, IActionConfiguration configuration)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (!ShouldProcessArray(jObject, configuration))
            {
                // If there's no array to process or array processing is not configured, return the original object
                yield return jObject;
                yield break;
            }

            if (configuration.PreserveArrays)
            {
                // Preserve the array structure but process its elements
                var processedObject = new JObject();
                foreach (var property in jObject.Properties())
                {
                    if (property.Name == configuration.ArrayExplosionField)
                    {
                        if (property.Value is JArray jArray)
                        {
                            processedObject[property.Name] = PreserveArray(jArray, configuration);
                        }
                        else
                        {
                            processedObject[property.Name] = property.Value;
                        }
                    }
                    else
                    {
                        processedObject[property.Name] = property.Value;
                    }
                }
                yield return processedObject;
            }
            else
            {
                // Explode the array into multiple objects
                var fieldMapping = configuration.GetFieldMapping(configuration.ArrayExplosionField!);
                if (fieldMapping == null)
                {
                    throw new InvalidOperationException($"Field mapping for array explosion field '{configuration.ArrayExplosionField}' not found.");
                }

                foreach (var explodedObject in ExplodeArray(
                    jObject,
                    configuration.ArrayExplosionField!,
                    fieldMapping.ParentContextFields,
                    fieldMapping.ArrayItemFilter))
                {
                    yield return explodedObject;
                }
            }
        }

        /// <summary>
        /// Explodes an array field in a JSON object into multiple JSON objects.
        /// </summary>
        /// <param name="jObject">The JSON object containing the array.</param>
        /// <param name="arrayFieldName">The name of the array field to explode.</param>
        /// <param name="parentContextFields">The collection of field names from the parent context to include with each array item.</param>
        /// <param name="arrayItemFilter">The filter expression to determine which array items should be included.</param>
        /// <returns>A collection of exploded JSON objects.</returns>
        public IEnumerable<JObject> ExplodeArray(JObject jObject, string arrayFieldName, IEnumerable<string>? parentContextFields = null, string? arrayItemFilter = null)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (string.IsNullOrWhiteSpace(arrayFieldName))
            {
                throw new ArgumentException("Array field name cannot be null or whitespace.", nameof(arrayFieldName));
            }

            // Get the array field value
            var arrayToken = _pathResolver.ResolvePath(jObject, arrayFieldName);
            if (arrayToken == null || arrayToken.Type != JTokenType.Array)
            {
                // If the array field is not found or is not an array, return the original object
                yield return jObject;
                yield break;
            }

            var jArray = (JArray)arrayToken;
            if (jArray.Count == 0)
            {
                // If the array is empty, return the original object without the array field
                var resultObject = new JObject();
                foreach (var property in jObject.Properties())
                {
                    if (property.Name != arrayFieldName)
                    {
                        resultObject[property.Name] = property.Value;
                    }
                }
                yield return resultObject;
                yield break;
            }

            // Get the parent context fields to include with each array item
            var parentContext = new JObject();
            if (parentContextFields != null)
            {
                foreach (var fieldName in parentContextFields)
                {
                    var fieldValue = _pathResolver.ResolvePath(jObject, fieldName);
                    if (fieldValue != null)
                    {
                        parentContext[fieldName] = fieldValue;
                    }
                }
            }

            // Process each array item
            foreach (var arrayItem in jArray)
            {
                if (arrayItem.Type != JTokenType.Object)
                {
                    // Skip non-object array items
                    continue;
                }

                var arrayItemObject = (JObject)arrayItem;

                // Apply filter if specified
                if (!string.IsNullOrWhiteSpace(arrayItemFilter) && !EvaluateFilter(arrayItemObject, arrayItemFilter))
                {
                    continue;
                }

                // Create a new object with the parent context fields and the array item properties
                var resultObject = new JObject();

                // Add parent context fields
                foreach (var property in parentContext.Properties())
                {
                    resultObject[property.Name] = property.Value;
                }

                // Add array item properties
                foreach (var property in arrayItemObject.Properties())
                {
                    resultObject[property.Name] = property.Value;
                }

                yield return resultObject;
            }
        }

        /// <summary>
        /// Preserves an array structure while simplifying its elements.
        /// </summary>
        /// <param name="jArray">The array to process.</param>
        /// <param name="configuration">The action configuration to apply to each array element.</param>
        /// <returns>A processed array with simplified elements.</returns>
        public JArray PreserveArray(JArray jArray, IActionConfiguration configuration)
        {
            if (jArray == null)
            {
                throw new ArgumentNullException(nameof(jArray));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (_parserEngine == null)
            {
                // If no parser engine is provided, return the original array
                return jArray;
            }

            var resultArray = new JArray();
            foreach (var arrayItem in jArray)
            {
                if (arrayItem.Type == JTokenType.Object)
                {
                    var processedItems = _parserEngine.Parse((JObject)arrayItem, configuration);
                    foreach (var processedItem in processedItems)
                    {
                        resultArray.Add(processedItem);
                    }
                }
                else
                {
                    resultArray.Add(arrayItem);
                }
            }

            return resultArray;
        }

        /// <summary>
        /// Determines whether a JSON object contains an array field that should be processed.
        /// </summary>
        /// <param name="jObject">The JSON object to check.</param>
        /// <param name="configuration">The action configuration.</param>
        /// <returns>True if the JSON object contains an array field that should be processed; otherwise, false.</returns>
        public bool ShouldProcessArray(JObject jObject, IActionConfiguration configuration)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(configuration.ArrayExplosionField))
            {
                return false;
            }

            var arrayToken = _pathResolver.ResolvePath(jObject, configuration.ArrayExplosionField);
            return arrayToken != null && arrayToken.Type == JTokenType.Array;
        }

        /// <summary>
        /// Evaluates an array item filter expression against a JSON object.
        /// </summary>
        /// <param name="jObject">The JSON object to evaluate the filter against.</param>
        /// <param name="filterExpression">The filter expression to evaluate.</param>
        /// <returns>True if the JSON object passes the filter; otherwise, false.</returns>
        public bool EvaluateFilter(JObject jObject, string filterExpression)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (string.IsNullOrWhiteSpace(filterExpression))
            {
                return true;
            }

            // Simple filter expression evaluation
            // Format: "path=value" or "path!=value"
            if (filterExpression.Contains("!="))
            {
                var parts = filterExpression.Split("!=", 2);
                if (parts.Length == 2)
                {
                    var path = parts[0].Trim();
                    var expectedValue = parts[1].Trim();
                    var actualValue = _pathResolver.ResolvePath(jObject, path)?.ToString();
                    return actualValue != expectedValue;
                }
            }
            else if (filterExpression.Contains("="))
            {
                var parts = filterExpression.Split("=", 2);
                if (parts.Length == 2)
                {
                    var path = parts[0].Trim();
                    var expectedValue = parts[1].Trim();
                    var actualValue = _pathResolver.ResolvePath(jObject, path)?.ToString();
                    return actualValue == expectedValue;
                }
            }

            // If the filter expression is not recognized, return true
            return true;
        }

        /// <summary>
        /// Creates a new default array processor.
        /// </summary>
        /// <param name="pathResolver">The path resolver to use for resolving paths.</param>
        /// <param name="parserEngine">The parser engine to use for processing array elements.</param>
        /// <returns>A new default array processor.</returns>
        public static DefaultArrayProcessor Create(IPathResolver pathResolver, IParserEngine? parserEngine = null)
        {
            return new DefaultArrayProcessor(pathResolver, parserEngine);
        }
    }
}
