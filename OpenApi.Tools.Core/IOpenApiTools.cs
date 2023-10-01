namespace OpenApi.Tools.Core
{
    /// <summary>
    /// API tools
    /// </summary>
    public interface IOpenApiTools
    {
        /// <summary>
        /// Closes opened Open API specification
        /// </summary>
        void CloseDocument();

        /// <summary>
        /// Finds paths that match the specified pattern
        /// </summary>
        /// <param name="pattern">The part of the path, or regular expression
        /// </param>
        /// <returns>List of paths (endpoints)</returns>
        List<string> FindPath(string pattern);

        /// <summary>
        /// Generates Markdown file for the specific path (endpoint)
        /// </summary>
        /// <param name="path">The source path (endpoint) like /users/{user_id}/messages/{message_id}</param>
        /// <param name="filePath">The output path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GenerateEndpointMarkdownFileAsync(string path, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates Markdown file for the specific path (endpoint)
        /// </summary>
        /// <param name="path">The source path (endpoint) like /users/{user_id}/messages/{message_id}</param>
        /// <param name="filePath">The output path</param>
        /// <param name="markdownOptions">Markdown options</param>
        /// <param name="markmapOptions">Markmap options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GenerateEndpointMarkdownFileAsync(string path, string filePath, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates Markdown files for the specific paths (endpoints)
        /// </summary>
        /// <param name="paths">The source paths (endpoints) like /users/{user_id}/messages/{message_id}</param>
        /// <param name="folderPath">The output path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GenerateEndpointMarkdownFilesAsync(List<string> paths, string folderPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates Markdown files for the specific paths (endpoints)
        /// </summary>
        /// <param name="paths">The source paths (endpoints) like /users/{user_id}/messages/{message_id}</param>
        /// <param name="folderPath">The output path</param>
        /// <param name="markdownOptions">Markdown options</param>
        /// <param name="markmapOptions">Markmap options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GenerateEndpointMarkdownFilesAsync(List<string> paths, string folderPath, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates markmap html file to explore generated content.
        /// It requires 'nmp' package manager to be installed on a machine
        /// </summary>
        /// <param name="input">Path to the input .md file</param>
        /// <param name="output">Path to the output .html file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GenerateMarkmapFileAsync(string input, string output, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates markmap html files to explore generated content.
        /// It requires 'nmp' package manager to be installed on a machine
        /// </summary>
        /// <param name="inputFolder">Input folder with .md files</param>
        /// <param name="outputFolder">Output folder where .html files will be generated</param>
        /// <param name="progress">Progress updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GenerateMarkmapFilesAsync(string inputFolder, string outputFolder, IProgress<int> progress = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Returns all paths (endpoints)
        /// </summary>
        /// <returns>List of paths (endpoints)</returns>
        List<string> GetPaths();

        /// <summary>
        /// Loads Open API specification from the yaml/json file
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task LoadDocumentFromFileAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads Open API specification from URL
        /// </summary>
        /// <param name="absoluteUrl">Absolute URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task LoadDocumentFromUrlAsync(string absoluteUrl, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads Open API specification from URL
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="relativeUrl">Relative URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task LoadDocumentFromUrlAsync(string baseUrl, string relativeUrl, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads Open API specification from stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task LoadDocumentFromStreamAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds markmap files in the specific folder and all its subfolders
        /// </summary>
        /// <param name="inputFolder">Input folder</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <returns>List of markmap files</returns>
        IList<string> FindMarkmapFiles(string inputFolder, string searchPattern);

        /// <summary>
        /// Takes the input folder and list of full markmap files paths and creates an item for each file path
        /// </summary>
        /// <param name="inputFolder">Input folder</param>
        /// <param name="paths">Markmap files</param>
        /// <param name="includeFileName">Whether the name of the file is included in the item</param>
        /// <returns>Returns a collection of files paths and related items</returns>
        IDictionary<string, string> ParseItemsFromMarkmapFilePath(string inputFolder, IList<string> paths, bool includeFileName);

        /// <summary>
        /// Generates Markdown file for the specific schema
        /// </summary>
        /// <param name="name">The name of the schema</param>
        /// <param name="filePath">The output path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GenerateSchemaMarkdownFileAsync(string name, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates Markdown file for the specific schema
        /// </summary>
        /// <param name="name">The name of the schema</param>
        /// <param name="filePath">The output path</param>
        /// <param name="markdownOptions">Markdown options</param>
        /// <param name="markmapOptions">Markmap options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GenerateSchemaMarkdownFileAsync(string name, string filePath, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates Markdown file for the specific schemas
        /// </summary>
        /// <param name="schemas">The list of schemas</param>
        /// <param name="folderPath">The output path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task GenerateSchemaMarkdownFilesAsync(List<string> schemas, string folderPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates Markdown file for the specific schemas
        /// </summary>
        /// <param name="schemas">The list of schemas</param>
        /// <param name="folderPath">The output path</param>
        /// <param name="markdownOptions">Markdown options</param>
        /// <param name="markmapOptions">Markmap options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task GenerateSchemaMarkdownFilesAsync(List<string> schemas, string folderPath, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all schemas
        /// </summary>
        /// <returns>List of schemas</returns>
        List<string> GetSchemas();

        /// <summary>
        /// Finds schemas that match the specific pattern
        /// </summary>
        /// <param name="pattern">The part of the schema, or regular expression</param>
        /// <returns>List of schemas</returns>
        List<string> FindSchema(string pattern);
    }
}