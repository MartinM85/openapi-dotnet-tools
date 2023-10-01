using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace OpenApi.Tools.Core
{
    /// <summary>
    /// Reads Open API specification from different sources.
    /// </summary>
    public static class OpenApiDocumentFactory
    {
        /// <summary>
        /// Reads Open API from existing file
        /// </summary>
        /// <param name="path">The file to be read</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OpenApiDocument"/></returns>
        public static async Task<OpenApiDocument> ReadFromFileAsync(string path, CancellationToken cancellationToken)
        {
            var reader = new OpenApiStreamReader();
            using var fileReader = File.OpenRead(path);
            var readResult = await reader.ReadAsync(fileReader, cancellationToken);
            return readResult.OpenApiDocument;
        }

        /// <summary>
        /// Reads Open API from existing url
        /// </summary>
        /// <param name="baseUrl">Base address</param>
        /// <param name="relativeUrl">Relative path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OpenApiDocument"/></returns>
        /// <example>
        /// <paramref name="baseUrl">https://raw.githubusercontent.com/OAI/OpenAPI-Specification/</paramref>
        /// <paramref name="relativeUrl">master/examples/v3.0/petstore.yaml</paramref>
        /// </example>
        public static async Task<OpenApiDocument> ReadFromUrlAsync(string baseUrl, string relativeUrl, CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            using var stream = await httpClient.GetStreamAsync(relativeUrl, cancellationToken);
            var reader = new OpenApiStreamReader();
            var readResult = await reader.ReadAsync(stream, cancellationToken);
            return readResult.OpenApiDocument;
        }

        /// <summary>
        /// Reads Open API from existing url
        /// </summary>
        /// <param name="absoluteUrl">Absolute path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OpenApiDocument"/></returns>
        /// <example>
        /// <paramref name="absoluteUrl">https://raw.githubusercontent.com/OAI/OpenAPI-Specification/master/examples/v3.0/petstore.yaml</paramref>
        /// </example>
        public static async Task<OpenApiDocument> ReadFromUrlAsync(string absoluteUrl, CancellationToken cancellationToken)
        {
            using var stream = await new HttpClient().GetStreamAsync(absoluteUrl, cancellationToken);
            var reader = new OpenApiStreamReader();
            var readResult = await reader.ReadAsync(stream, cancellationToken);
            return readResult.OpenApiDocument;
        }

        /// <summary>
        /// Reads Open API from stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OpenApiDocument"/></returns>
        public static async Task<OpenApiDocument> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            var reader = new OpenApiStreamReader();
            var readResult = await reader.ReadAsync(stream, cancellationToken);
            return readResult.OpenApiDocument;
        }
    }
}