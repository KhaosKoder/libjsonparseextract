using Newtonsoft.Json.Linq;

namespace Liber8.Json.Utils.Extraction
{
    /// <summary>
    /// Defines the contract for resolving paths in JSON objects.
    /// </summary>
    public interface IPathResolver
    {
        /// <summary>
        /// Resolves a path expression against a JSON object and returns the value.
        /// </summary>
        /// <param name="jObject">The JSON object to resolve the path against.</param>
        /// <param name="path">The path expression to resolve.</param>
        /// <returns>The resolved value if found; otherwise, null.</returns>
        JToken? ResolvePath(JObject jObject, string path);

        /// <summary>
        /// Resolves multiple path expressions against a JSON object and returns the first non-null value.
        /// </summary>
        /// <param name="jObject">The JSON object to resolve the paths against.</param>
        /// <param name="paths">The collection of path expressions to resolve.</param>
        /// <returns>The first non-null resolved value if found; otherwise, null.</returns>
        JToken? ResolveFirstNonNullPath(JObject jObject, IEnumerable<string> paths);

        /// <summary>
        /// Determines whether a path expression is valid.
        /// </summary>
        /// <param name="path">The path expression to validate.</param>
        /// <returns>True if the path expression is valid; otherwise, false.</returns>
        bool IsValidPath(string path);

        /// <summary>
        /// Compiles a path expression for faster resolution.
        /// </summary>
        /// <param name="path">The path expression to compile.</param>
        /// <returns>A compiled path expression that can be used for faster resolution.</returns>
        object CompilePath(string path);

        /// <summary>
        /// Resolves a compiled path expression against a JSON object and returns the value.
        /// </summary>
        /// <param name="jObject">The JSON object to resolve the path against.</param>
        /// <param name="compiledPath">The compiled path expression to resolve.</param>
        /// <returns>The resolved value if found; otherwise, null.</returns>
        JToken? ResolveCompiledPath(JObject jObject, object compiledPath);
    }
}
