using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace Liber8.Json.Utils.Configuration
{
    /// <summary>
    /// Represents a field mapping configuration.
    /// </summary>
    public class FieldMapping : IFieldMapping
    {
        /// <summary>
        /// Gets the name of the field in the output JSON.
        /// </summary>
        public string OutputFieldName { get; }

        /// <summary>
        /// Gets the list of possible source paths to extract the value from.
        /// The first non-null result will be used.
        /// </summary>
        public IReadOnlyList<string> SourcePaths { get; }

        /// <summary>
        /// Gets the target type to convert the extracted value to.
        /// </summary>
        public Type? TargetType { get; }

        /// <summary>
        /// Gets the default value to use when no value is found in any of the source paths.
        /// </summary>
        public JToken? DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether to omit the field when no value is found and no default is specified.
        /// </summary>
        public bool OmitIfNotFound { get; }

        /// <summary>
        /// Gets a value indicating whether this field should trigger array explosion.
        /// </summary>
        public bool IsArrayExplosionTrigger { get; }

        /// <summary>
        /// Gets the list of fields from parent context to include with each array item when array explosion is triggered.
        /// </summary>
        public IReadOnlyList<string>? ParentContextFields { get; }

        /// <summary>
        /// Gets the filter expression to determine which array items should be included.
        /// </summary>
        public string? ArrayItemFilter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldMapping"/> class.
        /// </summary>
        /// <param name="outputFieldName">The name of the field in the output JSON.</param>
        /// <param name="sourcePaths">The list of possible source paths to extract the value from.</param>
        /// <param name="targetType">The target type to convert the extracted value to.</param>
        /// <param name="defaultValue">The default value to use when no value is found in any of the source paths.</param>
        /// <param name="omitIfNotFound">Whether to omit the field when no value is found and no default is specified.</param>
        /// <param name="isArrayExplosionTrigger">Whether this field should trigger array explosion.</param>
        /// <param name="parentContextFields">The list of fields from parent context to include with each array item when array explosion is triggered.</param>
        /// <param name="arrayItemFilter">The filter expression to determine which array items should be included.</param>
        public FieldMapping(
            string outputFieldName,
            IEnumerable<string> sourcePaths,
            Type? targetType = null,
            JToken? defaultValue = null,
            bool omitIfNotFound = false,
            bool isArrayExplosionTrigger = false,
            IEnumerable<string>? parentContextFields = null,
            string? arrayItemFilter = null)
        {
            if (string.IsNullOrWhiteSpace(outputFieldName))
            {
                throw new ArgumentException("Output field name cannot be null or whitespace.", nameof(outputFieldName));
            }

            if (sourcePaths == null || !sourcePaths.Any())
            {
                throw new ArgumentException("Source paths cannot be null or empty.", nameof(sourcePaths));
            }

            OutputFieldName = outputFieldName;
            SourcePaths = sourcePaths.ToList();
            TargetType = targetType;
            DefaultValue = defaultValue;
            OmitIfNotFound = omitIfNotFound;
            IsArrayExplosionTrigger = isArrayExplosionTrigger;
            ParentContextFields = parentContextFields?.ToList();
            ArrayItemFilter = arrayItemFilter;
        }

        /// <summary>
        /// Creates a new field mapping with the specified output field name and a single source path.
        /// </summary>
        /// <param name="outputFieldName">The name of the field in the output JSON.</param>
        /// <param name="sourcePath">The source path to extract the value from.</param>
        /// <param name="targetType">The target type to convert the extracted value to.</param>
        /// <param name="defaultValue">The default value to use when no value is found in the source path.</param>
        /// <param name="omitIfNotFound">Whether to omit the field when no value is found and no default is specified.</param>
        /// <returns>A new field mapping.</returns>
        public static FieldMapping Create(
            string outputFieldName,
            string sourcePath,
            Type? targetType = null,
            JToken? defaultValue = null,
            bool omitIfNotFound = false)
        {
            return new FieldMapping(
                outputFieldName,
                new[] { sourcePath },
                targetType,
                defaultValue,
                omitIfNotFound);
        }

        /// <summary>
        /// Creates a new field mapping with the specified output field name and multiple source paths.
        /// </summary>
        /// <param name="outputFieldName">The name of the field in the output JSON.</param>
        /// <param name="sourcePaths">The list of possible source paths to extract the value from.</param>
        /// <param name="targetType">The target type to convert the extracted value to.</param>
        /// <param name="defaultValue">The default value to use when no value is found in any of the source paths.</param>
        /// <param name="omitIfNotFound">Whether to omit the field when no value is found and no default is specified.</param>
        /// <returns>A new field mapping.</returns>
        public static FieldMapping Create(
            string outputFieldName,
            IEnumerable<string> sourcePaths,
            Type? targetType = null,
            JToken? defaultValue = null,
            bool omitIfNotFound = false)
        {
            return new FieldMapping(
                outputFieldName,
                sourcePaths,
                targetType,
                defaultValue,
                omitIfNotFound);
        }

        /// <summary>
        /// Creates a new field mapping for array explosion.
        /// </summary>
        /// <param name="outputFieldName">The name of the field in the output JSON.</param>
        /// <param name="sourcePath">The source path to the array field.</param>
        /// <param name="parentContextFields">The list of fields from parent context to include with each array item.</param>
        /// <param name="arrayItemFilter">The filter expression to determine which array items should be included.</param>
        /// <returns>A new field mapping configured for array explosion.</returns>
        public static FieldMapping CreateForArrayExplosion(
            string outputFieldName,
            string sourcePath,
            IEnumerable<string>? parentContextFields = null,
            string? arrayItemFilter = null)
        {
            return new FieldMapping(
                outputFieldName,
                new[] { sourcePath },
                isArrayExplosionTrigger: true,
                parentContextFields: parentContextFields,
                arrayItemFilter: arrayItemFilter);
        }
    }
}
