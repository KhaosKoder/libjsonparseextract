using Newtonsoft.Json.Linq;

namespace Liber8.Json.Utils.Configuration
{
    /// <summary>
    /// Represents a configuration for a specific action type.
    /// </summary>
    public class ActionConfiguration : IActionConfiguration
    {
        private readonly Dictionary<string, IFieldMapping> _fieldMappings;

        /// <summary>
        /// Gets the action type this configuration applies to.
        /// </summary>
        public string ActionType { get; }

        /// <summary>
        /// Gets the collection of field mappings for this action type.
        /// </summary>
        public IReadOnlyCollection<IFieldMapping> FieldMappings => _fieldMappings.Values;

        /// <summary>
        /// Gets a value indicating whether to fail fast on the first error.
        /// </summary>
        public bool FailFast { get; }

        /// <summary>
        /// Gets the collection of field names for which errors should be ignored.
        /// </summary>
        public IReadOnlyCollection<string>? IgnoreErrorsForFields { get; }

        /// <summary>
        /// Gets a value indicating whether to preserve arrays in the output.
        /// </summary>
        public bool PreserveArrays { get; }

        /// <summary>
        /// Gets the name of the field that should trigger array explosion.
        /// </summary>
        public string? ArrayExplosionField { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionConfiguration"/> class.
        /// </summary>
        /// <param name="actionType">The action type this configuration applies to.</param>
        /// <param name="fieldMappings">The collection of field mappings for this action type.</param>
        /// <param name="failFast">Whether to fail fast on the first error.</param>
        /// <param name="ignoreErrorsForFields">The collection of field names for which errors should be ignored.</param>
        /// <param name="preserveArrays">Whether to preserve arrays in the output.</param>
        /// <param name="arrayExplosionField">The name of the field that should trigger array explosion.</param>
        public ActionConfiguration(
            string actionType,
            IEnumerable<IFieldMapping> fieldMappings,
            bool failFast = false,
            IEnumerable<string>? ignoreErrorsForFields = null,
            bool preserveArrays = false,
            string? arrayExplosionField = null)
        {
            if (string.IsNullOrWhiteSpace(actionType))
            {
                throw new ArgumentException("Action type cannot be null or whitespace.", nameof(actionType));
            }

            if (fieldMappings == null || !fieldMappings.Any())
            {
                throw new ArgumentException("Field mappings cannot be null or empty.", nameof(fieldMappings));
            }

            ActionType = actionType;
            _fieldMappings = fieldMappings.ToDictionary(fm => fm.OutputFieldName);
            FailFast = failFast;
            IgnoreErrorsForFields = ignoreErrorsForFields?.ToList();
            PreserveArrays = preserveArrays;
            ArrayExplosionField = arrayExplosionField;

            // Validate that if arrayExplosionField is specified, it exists in the field mappings
            if (arrayExplosionField != null && !_fieldMappings.ContainsKey(arrayExplosionField))
            {
                throw new ArgumentException($"Array explosion field '{arrayExplosionField}' is not defined in the field mappings.", nameof(arrayExplosionField));
            }

            // Validate that if any field is marked as an array explosion trigger, the arrayExplosionField is set to that field
            var explosionTriggerField = _fieldMappings.Values.FirstOrDefault(fm => fm.IsArrayExplosionTrigger);
            if (explosionTriggerField != null && arrayExplosionField != explosionTriggerField.OutputFieldName)
            {
                throw new ArgumentException($"Field '{explosionTriggerField.OutputFieldName}' is marked as an array explosion trigger, but the arrayExplosionField is set to '{arrayExplosionField}'.", nameof(arrayExplosionField));
            }
        }

        /// <summary>
        /// Validates the configuration for consistency and completeness.
        /// </summary>
        /// <returns>True if the configuration is valid; otherwise, false.</returns>
        public bool Validate()
        {
            // Check for duplicate output field names
            var duplicateFields = _fieldMappings.Values
                .GroupBy(fm => fm.OutputFieldName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateFields.Any())
            {
                throw new InvalidOperationException($"Duplicate output field names found: {string.Join(", ", duplicateFields)}");
            }

            // Check for invalid source paths
            foreach (var fieldMapping in _fieldMappings.Values)
            {
                if (fieldMapping.SourcePaths == null || !fieldMapping.SourcePaths.Any())
                {
                    throw new InvalidOperationException($"Field '{fieldMapping.OutputFieldName}' has no source paths defined.");
                }

                foreach (var sourcePath in fieldMapping.SourcePaths)
                {
                    if (string.IsNullOrWhiteSpace(sourcePath))
                    {
                        throw new InvalidOperationException($"Field '{fieldMapping.OutputFieldName}' has a null or whitespace source path.");
                    }
                }
            }

            // Check for array explosion configuration
            if (ArrayExplosionField != null)
            {
                if (!_fieldMappings.TryGetValue(ArrayExplosionField, out var explosionField))
                {
                    throw new InvalidOperationException($"Array explosion field '{ArrayExplosionField}' is not defined in the field mappings.");
                }

                if (!explosionField.IsArrayExplosionTrigger)
                {
                    throw new InvalidOperationException($"Array explosion field '{ArrayExplosionField}' is not marked as an array explosion trigger.");
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the field mapping for the specified output field name.
        /// </summary>
        /// <param name="outputFieldName">The name of the output field.</param>
        /// <returns>The field mapping if found; otherwise, null.</returns>
        public IFieldMapping? GetFieldMapping(string outputFieldName)
        {
            _fieldMappings.TryGetValue(outputFieldName, out var fieldMapping);
            return fieldMapping;
        }

        /// <summary>
        /// Creates a new action configuration with the specified action type and field mappings.
        /// </summary>
        /// <param name="actionType">The action type this configuration applies to.</param>
        /// <param name="fieldMappings">The collection of field mappings for this action type.</param>
        /// <param name="failFast">Whether to fail fast on the first error.</param>
        /// <param name="ignoreErrorsForFields">The collection of field names for which errors should be ignored.</param>
        /// <param name="preserveArrays">Whether to preserve arrays in the output.</param>
        /// <param name="arrayExplosionField">The name of the field that should trigger array explosion.</param>
        /// <returns>A new action configuration.</returns>
        public static ActionConfiguration Create(
            string actionType,
            IEnumerable<IFieldMapping> fieldMappings,
            bool failFast = false,
            IEnumerable<string>? ignoreErrorsForFields = null,
            bool preserveArrays = false,
            string? arrayExplosionField = null)
        {
            return new ActionConfiguration(
                actionType,
                fieldMappings,
                failFast,
                ignoreErrorsForFields,
                preserveArrays,
                arrayExplosionField);
        }

        /// <summary>
        /// Creates a new action configuration with the specified action type and field mappings.
        /// </summary>
        /// <param name="actionType">The action type this configuration applies to.</param>
        /// <param name="fieldMappings">The collection of field mappings for this action type.</param>
        /// <returns>A new action configuration.</returns>
        public static ActionConfiguration Create(
            string actionType,
            params IFieldMapping[] fieldMappings)
        {
            return new ActionConfiguration(
                actionType,
                fieldMappings);
        }

        /// <summary>
        /// Creates a new action configuration builder for the specified action type.
        /// </summary>
        /// <param name="actionType">The action type this configuration applies to.</param>
        /// <returns>A new action configuration builder.</returns>
        public static ActionConfigurationBuilder Builder(string actionType)
        {
            return new ActionConfigurationBuilder(actionType);
        }
    }
}
