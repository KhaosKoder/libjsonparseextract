namespace Liber8.Json.Utils.Configuration
{
    /// <summary>
    /// Provides a fluent API for building action configurations.
    /// </summary>
    public class ActionConfigurationBuilder
    {
        private readonly string _actionType;
        private readonly List<IFieldMapping> _fieldMappings = new();
        private bool _failFast;
        private List<string>? _ignoreErrorsForFields;
        private bool _preserveArrays;
        private string? _arrayExplosionField;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionConfigurationBuilder"/> class.
        /// </summary>
        /// <param name="actionType">The action type this configuration applies to.</param>
        public ActionConfigurationBuilder(string actionType)
        {
            if (string.IsNullOrWhiteSpace(actionType))
            {
                throw new ArgumentException("Action type cannot be null or whitespace.", nameof(actionType));
            }

            _actionType = actionType;
        }

        /// <summary>
        /// Adds a field mapping to the configuration.
        /// </summary>
        /// <param name="fieldMapping">The field mapping to add.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder AddFieldMapping(IFieldMapping fieldMapping)
        {
            _fieldMappings.Add(fieldMapping);
            return this;
        }

        /// <summary>
        /// Adds multiple field mappings to the configuration.
        /// </summary>
        /// <param name="fieldMappings">The field mappings to add.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder AddFieldMappings(IEnumerable<IFieldMapping> fieldMappings)
        {
            _fieldMappings.AddRange(fieldMappings);
            return this;
        }

        /// <summary>
        /// Adds a field mapping with the specified output field name and a single source path.
        /// </summary>
        /// <param name="outputFieldName">The name of the field in the output JSON.</param>
        /// <param name="sourcePath">The source path to extract the value from.</param>
        /// <param name="targetType">The target type to convert the extracted value to.</param>
        /// <param name="defaultValue">The default value to use when no value is found in the source path.</param>
        /// <param name="omitIfNotFound">Whether to omit the field when no value is found and no default is specified.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder AddField(
            string outputFieldName,
            string sourcePath,
            Type? targetType = null,
            object? defaultValue = null,
            bool omitIfNotFound = false)
        {
            _fieldMappings.Add(FieldMapping.Create(
                outputFieldName,
                sourcePath,
                targetType,
                defaultValue != null ? Newtonsoft.Json.Linq.JToken.FromObject(defaultValue) : null,
                omitIfNotFound));
            return this;
        }

        /// <summary>
        /// Adds a field mapping with the specified output field name and multiple source paths.
        /// </summary>
        /// <param name="outputFieldName">The name of the field in the output JSON.</param>
        /// <param name="sourcePaths">The list of possible source paths to extract the value from.</param>
        /// <param name="targetType">The target type to convert the extracted value to.</param>
        /// <param name="defaultValue">The default value to use when no value is found in any of the source paths.</param>
        /// <param name="omitIfNotFound">Whether to omit the field when no value is found and no default is specified.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder AddField(
            string outputFieldName,
            IEnumerable<string> sourcePaths,
            Type? targetType = null,
            object? defaultValue = null,
            bool omitIfNotFound = false)
        {
            _fieldMappings.Add(FieldMapping.Create(
                outputFieldName,
                sourcePaths,
                targetType,
                defaultValue != null ? Newtonsoft.Json.Linq.JToken.FromObject(defaultValue) : null,
                omitIfNotFound));
            return this;
        }

        /// <summary>
        /// Adds a field mapping for array explosion.
        /// </summary>
        /// <param name="outputFieldName">The name of the field in the output JSON.</param>
        /// <param name="sourcePath">The source path to the array field.</param>
        /// <param name="parentContextFields">The list of fields from parent context to include with each array item.</param>
        /// <param name="arrayItemFilter">The filter expression to determine which array items should be included.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder AddArrayExplosionField(
            string outputFieldName,
            string sourcePath,
            IEnumerable<string>? parentContextFields = null,
            string? arrayItemFilter = null)
        {
            _fieldMappings.Add(FieldMapping.CreateForArrayExplosion(
                outputFieldName,
                sourcePath,
                parentContextFields,
                arrayItemFilter));
            _arrayExplosionField = outputFieldName;
            return this;
        }

        /// <summary>
        /// Sets whether to fail fast on the first error.
        /// </summary>
        /// <param name="failFast">Whether to fail fast on the first error.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder WithFailFast(bool failFast = true)
        {
            _failFast = failFast;
            return this;
        }

        /// <summary>
        /// Sets the collection of field names for which errors should be ignored.
        /// </summary>
        /// <param name="fieldNames">The collection of field names.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder WithIgnoreErrorsForFields(IEnumerable<string> fieldNames)
        {
            _ignoreErrorsForFields = fieldNames.ToList();
            return this;
        }

        /// <summary>
        /// Sets whether to preserve arrays in the output.
        /// </summary>
        /// <param name="preserveArrays">Whether to preserve arrays in the output.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder WithPreserveArrays(bool preserveArrays = true)
        {
            _preserveArrays = preserveArrays;
            return this;
        }

        /// <summary>
        /// Sets the name of the field that should trigger array explosion.
        /// </summary>
        /// <param name="arrayExplosionField">The name of the field that should trigger array explosion.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ActionConfigurationBuilder WithArrayExplosionField(string arrayExplosionField)
        {
            _arrayExplosionField = arrayExplosionField;
            return this;
        }

        /// <summary>
        /// Builds the action configuration.
        /// </summary>
        /// <returns>The built action configuration.</returns>
        public ActionConfiguration Build()
        {
            return new ActionConfiguration(
                _actionType,
                _fieldMappings,
                _failFast,
                _ignoreErrorsForFields,
                _preserveArrays,
                _arrayExplosionField);
        }
    }
}
