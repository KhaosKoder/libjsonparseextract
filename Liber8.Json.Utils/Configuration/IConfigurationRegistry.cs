namespace Liber8.Json.Utils.Configuration
{
    /// <summary>
    /// Defines the registry for action configurations.
    /// </summary>
    public interface IConfigurationRegistry
    {
        /// <summary>
        /// Registers an action configuration.
        /// </summary>
        /// <param name="configuration">The action configuration to register.</param>
        void RegisterConfiguration(IActionConfiguration configuration);

        /// <summary>
        /// Gets the action configuration for the specified action type.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        /// <returns>The action configuration if found; otherwise, null.</returns>
        IActionConfiguration? GetConfiguration(string actionType);

        /// <summary>
        /// Determines whether a configuration exists for the specified action type.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        /// <returns>True if a configuration exists; otherwise, false.</returns>
        bool HasConfiguration(string actionType);

        /// <summary>
        /// Gets the default configuration to use when no specific configuration is found.
        /// </summary>
        /// <returns>The default configuration if set; otherwise, null.</returns>
        IActionConfiguration? GetDefaultConfiguration();

        /// <summary>
        /// Sets the default configuration to use when no specific configuration is found.
        /// </summary>
        /// <param name="configuration">The default configuration.</param>
        void SetDefaultConfiguration(IActionConfiguration configuration);

        /// <summary>
        /// Gets all registered action configurations.
        /// </summary>
        /// <returns>A collection of all registered action configurations.</returns>
        IReadOnlyCollection<IActionConfiguration> GetAllConfigurations();

        /// <summary>
        /// Clears all registered configurations.
        /// </summary>
        void ClearConfigurations();
    }
}
