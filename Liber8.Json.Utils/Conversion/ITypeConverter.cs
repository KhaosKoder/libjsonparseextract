using Newtonsoft.Json.Linq;

namespace Liber8.Json.Utils.Conversion
{
    /// <summary>
    /// Defines the contract for converting JToken values to specific types.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Converts a JToken value to the specified type.
        /// </summary>
        /// <param name="value">The JToken value to convert.</param>
        /// <param name="targetType">The target type to convert to.</param>
        /// <returns>The converted value.</returns>
        object? Convert(JToken value, Type targetType);

        /// <summary>
        /// Converts a JToken value to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <param name="value">The JToken value to convert.</param>
        /// <returns>The converted value.</returns>
        T? Convert<T>(JToken value);

        /// <summary>
        /// Determines whether this converter can convert to the specified type.
        /// </summary>
        /// <param name="targetType">The target type to check.</param>
        /// <returns>True if this converter can convert to the specified type; otherwise, false.</returns>
        bool CanConvert(Type targetType);

        /// <summary>
        /// Registers a custom converter for a specific type.
        /// </summary>
        /// <param name="targetType">The target type for the converter.</param>
        /// <param name="converter">The function that performs the conversion.</param>
        void RegisterConverter(Type targetType, Func<JToken, object?> converter);

        /// <summary>
        /// Registers a custom converter for a specific type.
        /// </summary>
        /// <typeparam name="T">The target type for the converter.</typeparam>
        /// <param name="converter">The function that performs the conversion.</param>
        void RegisterConverter<T>(Func<JToken, T?> converter);
    }
}
