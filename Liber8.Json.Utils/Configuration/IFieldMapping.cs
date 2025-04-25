using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace Liber8.Json.Utils.Configuration
{
    /// <summary>
    /// Defines the configuration for mapping a field from source JSON to output JSON.
    /// </summary>
    public interface IFieldMapping
    {
        /// <summary>
        /// Gets the name of the field in the output JSON.
        /// </summary>
        string OutputFieldName { get; }

        /// <summary>
        /// Gets the list of possible source paths to extract the value from.
        /// The first non-null result will be used.
        /// </summary>
        IReadOnlyList<string> SourcePaths { get; }

        /// <summary>
        /// Gets the target type to convert the extracted value to.
        /// </summary>
        Type? TargetType { get; }

        /// <summary>
        /// Gets the default value to use when no value is found in any of the source paths.
        /// </summary>
        JToken? DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether to omit the field when no value is found and no default is specified.
        /// </summary>
        bool OmitIfNotFound { get; }

        /// <summary>
        /// Gets a value indicating whether this field should trigger array explosion.
        /// </summary>
        bool IsArrayExplosionTrigger { get; }

        /// <summary>
        /// Gets the list of fields from parent context to include with each array item when array explosion is triggered.
        /// </summary>
        IReadOnlyList<string>? ParentContextFields { get; }

        /// <summary>
        /// Gets the filter expression to determine which array items should be included.
        /// </summary>
        string? ArrayItemFilter { get; }
    }
}
