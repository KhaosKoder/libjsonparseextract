using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Globalization;

namespace Liber8.Json.Utils.Conversion
{
    /// <summary>
    /// Provides default type conversion functionality for JToken values.
    /// </summary>
    public class DefaultTypeConverter : ITypeConverter
    {
        private readonly ConcurrentDictionary<Type, Func<JToken, object?>> _converters = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTypeConverter"/> class.
        /// </summary>
        public DefaultTypeConverter()
        {
            RegisterDefaultConverters();
        }

        /// <summary>
        /// Converts a JToken value to the specified type.
        /// </summary>
        /// <param name="value">The JToken value to convert.</param>
        /// <param name="targetType">The target type to convert to.</param>
        /// <returns>The converted value.</returns>
        public object? Convert(JToken value, Type targetType)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return GetDefaultValue(targetType);
            }

            if (targetType == typeof(JToken) || targetType == value.GetType())
            {
                return value;
            }

            if (_converters.TryGetValue(targetType, out var converter))
            {
                return converter(value);
            }

            // Try to use Newtonsoft.Json's built-in conversion
            try
            {
                return value.ToObject(targetType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert value '{value}' to type '{targetType.Name}'.", ex);
            }
        }

        /// <summary>
        /// Converts a JToken value to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <param name="value">The JToken value to convert.</param>
        /// <returns>The converted value.</returns>
        public T? Convert<T>(JToken value)
        {
            var result = Convert(value, typeof(T));
            return result == null ? default : (T)result;
        }

        /// <summary>
        /// Determines whether this converter can convert to the specified type.
        /// </summary>
        /// <param name="targetType">The target type to check.</param>
        /// <returns>True if this converter can convert to the specified type; otherwise, false.</returns>
        public bool CanConvert(Type targetType)
        {
            return _converters.ContainsKey(targetType) || IsBuiltInConvertible(targetType);
        }

        /// <summary>
        /// Registers a custom converter for a specific type.
        /// </summary>
        /// <param name="targetType">The target type for the converter.</param>
        /// <param name="converter">The function that performs the conversion.</param>
        public void RegisterConverter(Type targetType, Func<JToken, object?> converter)
        {
            _converters[targetType] = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        /// <summary>
        /// Registers a custom converter for a specific type.
        /// </summary>
        /// <typeparam name="T">The target type for the converter.</typeparam>
        /// <param name="converter">The function that performs the conversion.</param>
        public void RegisterConverter<T>(Func<JToken, T?> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            _converters[typeof(T)] = token => converter(token);
        }

        private void RegisterDefaultConverters()
        {
            // String conversion
            RegisterConverter<string>(token =>
            {
                if (token.Type == JTokenType.Null)
                {
                    return null;
                }

                return token.ToString();
            });

            // Boolean conversion
            RegisterConverter<bool>(token =>
            {
                if (token.Type == JTokenType.Boolean)
                {
                    return token.Value<bool>();
                }

                if (token.Type == JTokenType.String)
                {
                    var stringValue = token.Value<string>();
                    if (bool.TryParse(stringValue, out var boolValue))
                    {
                        return boolValue;
                    }

                    // Handle common string representations
                    stringValue = stringValue?.ToLowerInvariant();
                    return stringValue switch
                    {
                        "1" or "yes" or "y" or "true" or "t" => true,
                        "0" or "no" or "n" or "false" or "f" => false,
                        _ => throw new InvalidOperationException($"Cannot convert '{token}' to Boolean.")
                    };
                }

                if (token.Type == JTokenType.Integer)
                {
                    var intValue = token.Value<int>();
                    return intValue != 0;
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to Boolean.");
            });

            // Integer conversions
            RegisterConverter<int>(token =>
            {
                if (token.Type == JTokenType.Integer)
                {
                    return token.Value<int>();
                }

                if (token.Type == JTokenType.String)
                {
                    if (int.TryParse(token.Value<string>(), out var intValue))
                    {
                        return intValue;
                    }
                }

                if (token.Type == JTokenType.Float)
                {
                    return (int)token.Value<double>();
                }

                if (token.Type == JTokenType.Boolean)
                {
                    return token.Value<bool>() ? 1 : 0;
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to Int32.");
            });

            RegisterConverter<long>(token =>
            {
                if (token.Type == JTokenType.Integer)
                {
                    return token.Value<long>();
                }

                if (token.Type == JTokenType.String)
                {
                    if (long.TryParse(token.Value<string>(), out var longValue))
                    {
                        return longValue;
                    }
                }

                if (token.Type == JTokenType.Float)
                {
                    return (long)token.Value<double>();
                }

                if (token.Type == JTokenType.Boolean)
                {
                    return token.Value<bool>() ? 1L : 0L;
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to Int64.");
            });

            // Floating-point conversions
            RegisterConverter<float>(token =>
            {
                if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                {
                    return token.Value<float>();
                }

                if (token.Type == JTokenType.String)
                {
                    if (float.TryParse(token.Value<string>(), NumberStyles.Any, CultureInfo.InvariantCulture, out var floatValue))
                    {
                        return floatValue;
                    }
                }

                if (token.Type == JTokenType.Boolean)
                {
                    return token.Value<bool>() ? 1.0f : 0.0f;
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to Single.");
            });

            RegisterConverter<double>(token =>
            {
                if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                {
                    return token.Value<double>();
                }

                if (token.Type == JTokenType.String)
                {
                    if (double.TryParse(token.Value<string>(), NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
                    {
                        return doubleValue;
                    }
                }

                if (token.Type == JTokenType.Boolean)
                {
                    return token.Value<bool>() ? 1.0 : 0.0;
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to Double.");
            });

            // DateTime conversion
            RegisterConverter<DateTime>(token =>
            {
                if (token.Type == JTokenType.Date)
                {
                    return token.Value<DateTime>();
                }

                if (token.Type == JTokenType.String)
                {
                    var stringValue = token.Value<string>();
                    if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
                    {
                        return dateTimeValue;
                    }
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to DateTime.");
            });

            // Guid conversion
            RegisterConverter<Guid>(token =>
            {
                if (token.Type == JTokenType.String)
                {
                    var stringValue = token.Value<string>();
                    if (Guid.TryParse(stringValue, out var guidValue))
                    {
                        return guidValue;
                    }
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to Guid.");
            });

            // Enum conversion
            RegisterConverter(typeof(Enum), token =>
            {
                throw new InvalidOperationException("Cannot convert to Enum directly. Use a specific enum type.");
            });

            // JObject conversion
            RegisterConverter<JObject>(token =>
            {
                if (token.Type == JTokenType.Object)
                {
                    return (JObject)token;
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to JObject.");
            });

            // JArray conversion
            RegisterConverter<JArray>(token =>
            {
                if (token.Type == JTokenType.Array)
                {
                    return (JArray)token;
                }

                throw new InvalidOperationException($"Cannot convert '{token}' to JArray.");
            });
        }

        private bool IsBuiltInConvertible(Type targetType)
        {
            return targetType.IsPrimitive ||
                   targetType == typeof(string) ||
                   targetType == typeof(DateTime) ||
                   targetType == typeof(Guid) ||
                   targetType == typeof(decimal) ||
                   targetType.IsEnum ||
                   targetType == typeof(JObject) ||
                   targetType == typeof(JArray) ||
                   targetType == typeof(JToken);
        }

        private object? GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        /// <summary>
        /// Creates a new default type converter.
        /// </summary>
        /// <returns>A new default type converter.</returns>
        public static DefaultTypeConverter Create()
        {
            return new DefaultTypeConverter();
        }
    }
}
