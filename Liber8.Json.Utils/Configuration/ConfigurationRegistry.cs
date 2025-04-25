using System.Collections.Concurrent;

namespace Liber8.Json.Utils.Configuration
{
    /// <summary>
    /// Represents a registry for action configurations.
    /// </summary>
    public class ConfigurationRegistry : IConfigurationRegistry
    {
        private readonly ConcurrentDictionary<string, IActionConfiguration> _configurations = new();
        private IActionConfiguration? _defaultConfiguration;

        /// <summary>
        /// Registers an action configuration.
        /// </summary>
        /// <param name="configuration">The action configuration to register.</param>
        public void RegisterConfiguration(IActionConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(configuration.ActionType))
            {
                throw new ArgumentException("Action type cannot be null or whitespace.", nameof(configuration));
            }

            _configurations[configuration.ActionType] = configuration;
        }

        /// <summary>
        /// Gets the action configuration for the specified action type.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        /// <returns>The action configuration if found; otherwise, null.</returns>
        public IActionConfiguration? GetConfiguration(string actionType)
        {
            if (string.IsNullOrWhiteSpace(actionType))
            {
                throw new ArgumentException("Action type cannot be null or whitespace.", nameof(actionType));
            }

            _configurations.TryGetValue(actionType, out var configuration);
            return configuration;
        }

        /// <summary>
        /// Determines whether a configuration exists for the specified action type.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        /// <returns>True if a configuration exists; otherwise, false.</returns>
        public bool HasConfiguration(string actionType)
        {
            if (string.IsNullOrWhiteSpace(actionType))
            {
                throw new ArgumentException("Action type cannot be null or whitespace.", nameof(actionType));
            }

            return _configurations.ContainsKey(actionType);
        }

        /// <summary>
        /// Gets the default configuration to use when no specific configuration is found.
        /// </summary>
        /// <returns>The default configuration if set; otherwise, null.</returns>
        public IActionConfiguration? GetDefaultConfiguration()
        {
            return _defaultConfiguration;
        }

        /// <summary>
        /// Sets the default configuration to use when no specific configuration is found.
        /// </summary>
        /// <param name="configuration">The default configuration.</param>
        public void SetDefaultConfiguration(IActionConfiguration configuration)
        {
            _defaultConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets all registered action configurations.
        /// </summary>
        /// <returns>A collection of all registered action configurations.</returns>
        public IReadOnlyCollection<IActionConfiguration> GetAllConfigurations()
        {
            return _configurations.Values.ToList();
        }

        /// <summary>
        /// Clears all registered configurations.
        /// </summary>
        public void ClearConfigurations()
        {
            _configurations.Clear();
            _defaultConfiguration = null;
        }

        /// <summary>
        /// Creates a new configuration registry.
        /// </summary>
        /// <returns>A new configuration registry.</returns>
        public static ConfigurationRegistry Create()
        {
            return new ConfigurationRegistry();
        }

        /// <summary>
        /// Creates a new configuration registry with the specified configurations.
        /// </summary>
        /// <param name="configurations">The configurations to register.</param>
        /// <returns>A new configuration registry.</returns>
        public static ConfigurationRegistry Create(IEnumerable<IActionConfiguration> configurations)
        {
            var registry = new ConfigurationRegistry();
            foreach (var configuration in configurations)
            {
                registry.RegisterConfiguration(configuration);
            }
            return registry;
        }

        /// <summary>
        /// Creates a new configuration registry with the specified configurations and default configuration.
        /// </summary>
        /// <param name="configurations">The configurations to register.</param>
        /// <param name="defaultConfiguration">The default configuration to use when no specific configuration is found.</param>
        /// <returns>A new configuration registry.</returns>
        public static ConfigurationRegistry Create(IEnumerable<IActionConfiguration> configurations, IActionConfiguration defaultConfiguration)
        {
            var registry = Create(configurations);
            registry.SetDefaultConfiguration(defaultConfiguration);
            return registry;
        }
    }
}
