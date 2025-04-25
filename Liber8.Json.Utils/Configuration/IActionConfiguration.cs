using Newtonsoft.Json.Linq;

namespace Liber8.Json.Utils.Configuration
{
    /// <summary>
    /// Defines the configuration for a specific action type.
    /// </summary>
    public interface IActionConfiguration
    {
        /// <summary>
        /// Gets the action type this configuration applies to.
        /// </summary>
        string ActionType { get; }

        /// <summary>
        /// Gets the collection of field mappings for this action type.
        /// </summary>
        IReadOnlyCollection<IFieldMapping> FieldMappings { get; }

        /// <summary>
        /// Gets a value indicating whether to fail fast on the first error.
        /// </summary>
        bool FailFast { get; }

        /// <summary>
        /// Gets the collection of field names for which errors should be ignored.
        /// </summary>
        IReadOnlyCollection<string>? IgnoreErrorsForFields { get; }

        /// <summary>
        /// Gets a value indicating whether to preserve arrays in the output.
        /// </summary>
        bool PreserveArrays { get; }

        /// <summary>
        /// Gets the name of the field that should trigger array explosion.
        /// </summary>
        string? ArrayExplosionField { get; }

        /// <summary>
        /// Validates the configuration for consistency and completeness.
        /// </summary>
        /// <returns>True if the configuration is valid; otherwise, false.</returns>
        bool Validate();

        /// <summary>
        /// Gets the field mapping for the specified output field name.
        /// </summary>
        /// <param name="outputFieldName">The name of the output field.</param>
        /// <returns>The field mapping if found; otherwise, null.</returns>
        IFieldMapping? GetFieldMapping(string outputFieldName);
    }
}
