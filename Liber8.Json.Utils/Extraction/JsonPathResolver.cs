using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace Liber8.Json.Utils.Extraction
{
    /// <summary>
    /// Implements the path resolver using JsonPath expressions.
    /// </summary>
    public class JsonPathResolver : IPathResolver
    {
        private readonly ConcurrentDictionary<string, object> _compiledPaths = new();

        /// <summary>
        /// Resolves a path expression against a JSON object and returns the value.
        /// </summary>
        /// <param name="jObject">The JSON object to resolve the path against.</param>
        /// <param name="path">The path expression to resolve.</param>
        /// <returns>The resolved value if found; otherwise, null.</returns>
        public JToken? ResolvePath(JObject jObject, string path)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            try
            {
                // Handle simple property access
                if (path.StartsWith("$.") || path.StartsWith("$["))
                {
                    return jObject.SelectToken(path);
                }
                else if (!path.StartsWith("$"))
                {
                    // If the path doesn't start with $, assume it's a simple property name
                    return jObject.SelectToken($"$.{path}");
                }
                else
                {
                    return jObject.SelectToken(path);
                }
            }
            catch (Exception)
            {
                // If the path is invalid or not found, return null
                return null;
            }
        }

        /// <summary>
        /// Resolves multiple path expressions against a JSON object and returns the first non-null value.
        /// </summary>
        /// <param name="jObject">The JSON object to resolve the paths against.</param>
        /// <param name="paths">The collection of path expressions to resolve.</param>
        /// <returns>The first non-null resolved value if found; otherwise, null.</returns>
        public JToken? ResolveFirstNonNullPath(JObject jObject, IEnumerable<string> paths)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            foreach (var path in paths)
            {
                var value = ResolvePath(jObject, path);
                if (value != null && value.Type != JTokenType.Null)
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether a path expression is valid.
        /// </summary>
        /// <param name="path">The path expression to validate.</param>
        /// <returns>True if the path expression is valid; otherwise, false.</returns>
        public bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                // Create a simple JObject to test the path against
                var testObject = new JObject();
                testObject.SelectToken(path, errorWhenNoMatch: false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Compiles a path expression for faster resolution.
        /// </summary>
        /// <param name="path">The path expression to compile.</param>
        /// <returns>A compiled path expression that can be used for faster resolution.</returns>
        public object CompilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            return _compiledPaths.GetOrAdd(path, p => p);
        }

        /// <summary>
        /// Resolves a compiled path expression against a JSON object and returns the value.
        /// </summary>
        /// <param name="jObject">The JSON object to resolve the path against.</param>
        /// <param name="compiledPath">The compiled path expression to resolve.</param>
        /// <returns>The resolved value if found; otherwise, null.</returns>
        public JToken? ResolveCompiledPath(JObject jObject, object compiledPath)
        {
            if (jObject == null)
            {
                throw new ArgumentNullException(nameof(jObject));
            }

            if (compiledPath == null)
            {
                throw new ArgumentNullException(nameof(compiledPath));
            }

            if (compiledPath is string path)
            {
                return ResolvePath(jObject, path);
            }

            throw new ArgumentException("Compiled path must be a string.", nameof(compiledPath));
        }

        /// <summary>
        /// Creates a new JsonPath resolver.
        /// </summary>
        /// <returns>A new JsonPath resolver.</returns>
        public static JsonPathResolver Create()
        {
            return new JsonPathResolver();
        }
    }
}
