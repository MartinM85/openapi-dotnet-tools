using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OpenApi.Tools.Core
{
    /// <summary>
    /// Open API tools
    /// </summary>
    public class OpenApiTools : IOpenApiTools
    {
        /// <summary>
        /// Maximum number of concurrent tasks when executing markmap-cli command.
        /// For large Open API, it can take more than one hour
        /// </summary>
        private const int MarkmapGenMaxConcurrentTasks = -1;
        private const int ProgressCheckMillisecondsDelay = 4000;
        private OpenApiDocument _openApiDocument = new();

        /// <summary>
        /// Loads Open API specification from the yaml/json file
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task LoadDocumentFromFileAsync(string path, CancellationToken cancellationToken = default)
        {
            _openApiDocument = await OpenApiDocumentFactory.ReadFromFileAsync(path, cancellationToken);
        }

        /// <summary>
        /// Loads Open API specification from URL
        /// </summary>
        /// <param name="absoluteUrl">Absolute URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task LoadDocumentFromUrlAsync(string absoluteUrl, CancellationToken cancellationToken = default)
        {
            _openApiDocument = await OpenApiDocumentFactory.ReadFromUrlAsync(absoluteUrl, cancellationToken);
        }

        /// <summary>
        /// Loads Open API specification from URL
        /// </summary>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="relativeUrl">Relative URL</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task LoadDocumentFromUrlAsync(string baseUrl, string relativeUrl, CancellationToken cancellationToken = default)
        {
            _openApiDocument = await OpenApiDocumentFactory.ReadFromUrlAsync(baseUrl, relativeUrl, cancellationToken);
        }

        /// <summary>
        /// Loads Open API specification from stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task LoadDocumentFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            _openApiDocument = await OpenApiDocumentFactory.ReadFromStreamAsync(stream, cancellationToken);
        }

        /// <summary>
        /// Returns all paths (endpoints)
        /// </summary>
        /// <returns>List of paths (endpoints)</returns>
        public List<string> GetPaths()
        {
            return _openApiDocument.Paths.Select(x => x.Key).ToList();
        }

        /// <summary>
        /// Finds paths that match the specified pattern
        /// </summary>
        /// <param name="pattern">The part of the path, or regular expression
        /// </param>
        /// <returns>List of paths (endpoints)</returns>
        public List<string> FindPath(string pattern)
        {
            return _openApiDocument.Paths.Where(x => Regex.IsMatch(x.Key, pattern)).Select(x => x.Key).ToList();
        }

        /// <summary>
        /// Returns all schemas
        /// </summary>
        /// <returns>List of schemas</returns>
        public List<string> GetSchemas()
        {
            return _openApiDocument.Components.Schemas.Select(x => x.Key).ToList();
        }

        /// <summary>
        /// Finds schemas that match the specific pattern
        /// </summary>
        /// <param name="pattern">The part of the schema, or regular expression</param>
        /// <returns>List of schemas</returns>
        public List<string> FindSchema(string pattern)
        {
            return _openApiDocument.Components.Schemas.Where(x => Regex.IsMatch(x.Key, pattern)).Select(x => x.Key).ToList();
        }

        /// <summary>
        /// Generates Markdown file for the specific path (endpoint)
        /// </summary>
        /// <param name="path">The source path (endpoint) like /users/{user_id}/messages/{message_id}</param>
        /// <param name="filePath">The output path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateEndpointMarkdownFileAsync(string path, string filePath, CancellationToken cancellationToken = default)
        {
            await GenerateEndpointMarkdownFileAsync(path, filePath, MarkdownWriterOptions.Default, MarkmapOptions.Default, cancellationToken);
        }

        /// <summary>
        /// Generates Markdown file for the specific path (endpoint)
        /// </summary>
        /// <param name="path">The source path (endpoint) like /users/{user_id}/messages/{message_id}</param>
        /// <param name="filePath">The output path</param>
        /// <param name="markdownOptions">Markdown options</param>
        /// <param name="markmapOptions">Markmap options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateEndpointMarkdownFileAsync(string path, string filePath, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions, CancellationToken cancellationToken = default)
        {
            var endpoint = _openApiDocument.Paths[path];
            new FileInfo(filePath).Directory?.Create();
            using var streamWriter = new StreamWriter(filePath);
            var writer = new MarkdownWriter(streamWriter, markdownOptions, markmapOptions);
            await writer.WriteEndpointAsync(path, endpoint, cancellationToken);
        }

        /// <summary>
        /// Generates Markdown files for the specific paths (endpoints)
        /// </summary>
        /// <param name="paths">The source paths (endpoints) like /users/{user_id}/messages/{message_id}</param>
        /// <param name="folderPath">The output path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateEndpointMarkdownFilesAsync(List<string> paths, string folderPath, CancellationToken cancellationToken = default)
        {
            await GenerateEndpointMarkdownFilesAsync(paths, folderPath, MarkdownWriterOptions.Default, MarkmapOptions.Default, cancellationToken);
        }

        /// <summary>
        /// Generates Markdown files for the specific paths (endpoints)
        /// </summary>
        /// <param name="paths">The source paths (endpoints) like /users/{user_id}/messages/{message_id}</param>
        /// <param name="folderPath">The output path</param>
        /// <param name="markdownOptions">Markdown options</param>
        /// <param name="markmapOptions">Markmap options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateEndpointMarkdownFilesAsync(List<string> paths, string folderPath, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions, CancellationToken cancellationToken = default)
        {
            foreach (var path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var subFolders = Path.Combine(path.Split('/').Where(x => !string.IsNullOrEmpty(x)).ToArray());
                var filePath = Path.Combine(folderPath, subFolders, "endpoint.md");
                await GenerateEndpointMarkdownFileAsync(path, filePath, markdownOptions, markmapOptions, cancellationToken);
            }
        }

        /// <summary>
        /// Generates Markdown file for the specific schema
        /// </summary>
        /// <param name="name">The name of the schema</param>
        /// <param name="filePath">The output path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateSchemaMarkdownFileAsync(string name, string filePath, CancellationToken cancellationToken = default)
        {
            await GenerateSchemaMarkdownFileAsync(name, filePath, MarkdownWriterOptions.DefaultForSchema, MarkmapOptions.Default, cancellationToken);
        }

        /// <summary>
        /// Generates Markdown file for the specific schema
        /// </summary>
        /// <param name="name">The name of the schema</param>
        /// <param name="filePath">The output path</param>
        /// <param name="markdownOptions">Markdown options</param>
        /// <param name="markmapOptions">Markmap options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateSchemaMarkdownFileAsync(string name, string filePath, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions, CancellationToken cancellationToken = default)
        {
            var schema = _openApiDocument.Components.Schemas[name];
            new FileInfo(filePath).Directory?.Create();
            using var streamWriter = new StreamWriter(filePath);
            var writer = new MarkdownWriter(streamWriter, markdownOptions, markmapOptions);
            await writer.WriteSchemaAsync(name, schema, cancellationToken);
        }

        /// <summary>
        /// Generates Markdown file for the specific schemas
        /// </summary>
        /// <param name="schemas">The list of schemas</param>
        /// <param name="folderPath">The output path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task GenerateSchemaMarkdownFilesAsync(List<string> schemas, string folderPath, CancellationToken cancellationToken = default)
        {
            await GenerateSchemaMarkdownFilesAsync(schemas, folderPath, MarkdownWriterOptions.DefaultForSchema, MarkmapOptions.Default, cancellationToken);
        }

        /// <summary>
        /// Generates Markdown file for the specific schemas
        /// </summary>
        /// <param name="schemas">The list of schemas</param>
        /// <param name="folderPath">The output path</param>
        /// <param name="markdownOptions">Markdown options</param>
        /// <param name="markmapOptions">Markmap options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task GenerateSchemaMarkdownFilesAsync(List<string> schemas, string folderPath, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions, CancellationToken cancellationToken = default)
        {
            foreach (var schema in schemas)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var filePath = Path.Combine(folderPath, $"{schema}.md");
                await GenerateSchemaMarkdownFileAsync(schema, filePath, cancellationToken);
            }
        }

        /// <summary>
        /// Generates a markmap html file to explore generated content.
        /// It requires 'nmp' package manager to be installed on a machine
        /// </summary>
        /// <param name="input">Path to the input .md file</param>
        /// <param name="output">Path to the output .html file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateMarkmapFileAsync(string input, string output, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(input))
            {
                throw new FileNotFoundException($"The specified file '{input}' does not exist.", input);
            }
            var cmd = $"npx markmap-cli {input} -o {output} --no-open";
            string psiFileName;
            string psiArgs;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                psiFileName = "cmd";
                psiArgs = $"/c {cmd}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                psiFileName = "/bin/bash";
                psiArgs = $"{cmd}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                psiFileName = "zsh";
                psiArgs = $"{cmd}";
            }
            else
            {
                throw new NotSupportedException("Unsupported OS platform.");
            }
            Process proc = null;
            try
            {
                var startInfo = new ProcessStartInfo { FileName = psiFileName, Arguments = psiArgs, RedirectStandardInput = true, CreateNoWindow = true };
                proc = new Process { StartInfo = startInfo };
                proc.Start();
                proc.StandardInput.BaseStream.Close();
                await proc.WaitForExitAsync(cancellationToken);
            }
            finally
            {
                proc?.Dispose();
            }
        }

        /// <summary>
        /// Generates markmap html files to explore generated content.
        /// It requires 'nmp' package manager to be installed on a machine
        /// </summary>
        /// <param name="inputFolder">Input folder with .md files</param>
        /// <param name="outputFolder">Output folder where .html files will be generated</param>
        /// <param name="progress">Progress updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateMarkmapFilesAsync(string inputFolder, string outputFolder, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(inputFolder))
            {
                throw new DirectoryNotFoundException($"The directory {inputFolder} does not exist.");
            }
            var files = FindMarkmapFiles(inputFolder, "*.md");
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MarkmapGenMaxConcurrentTasks,
                CancellationToken = cancellationToken
            };

            long currentCount = 0;

            Task progressTask = null;
            using var resetEvent = new AutoResetEvent(false);
            if (progress != null)
            {
                var filesCount = (double)files.Count;
                progressTask = Task.Run(async () =>
                {
                    try
                    {
                        int lastProgress = -1;
                        while (true)
                        {
                            var filesRead = Interlocked.Read(ref currentCount);
                            var currentProgress = (int)(filesRead / filesCount * 100);
                            if (currentProgress != lastProgress)
                            {
                                progress.Report(currentProgress);
                            }
                            lastProgress = currentProgress;

                            if (filesRead == filesCount)
                            {
                                break;
                            }
                            await Task.Delay(ProgressCheckMillisecondsDelay, cancellationToken);
                        }
                    }
                    catch(TaskCanceledException) { }
                });
            }
            
            await Parallel.ForEachAsync(files, options, async (file, ct) =>
            {
                var relativePath = Path.GetRelativePath(inputFolder, Path.GetDirectoryName(file));
                var fileOutputFolder = Path.Combine(outputFolder, relativePath);
                Directory.CreateDirectory(fileOutputFolder);
                var outputFilePath = Path.Combine(outputFolder, relativePath, $"{Path.GetFileNameWithoutExtension(file)}.html");
                await GenerateMarkmapFileAsync(file, outputFilePath, cancellationToken);
                Interlocked.Increment(ref currentCount);
            });

            if (progress != null)
            {
                await progressTask;
                progressTask.Dispose();
            } 
        }

        /// <summary>
        /// Finds markmap files in the specific folder and all its subfolders
        /// </summary>
        /// <param name="inputFolder">Input folder</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <returns>List of markmap files</returns>
        public IList<string> FindMarkmapFiles(string inputFolder, string searchPattern)
        {
            return Directory.GetFiles(inputFolder, searchPattern, SearchOption.AllDirectories).OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Takes the input folder and list of full markmap files paths and creates an item for each file path
        /// </summary>
        /// <param name="inputFolder">Input folder</param>
        /// <param name="paths">Markmap files</param>
        /// <param name="includeFileName">Whether the name of the file is included in the item</param>
        /// <returns>Returns a collection of files paths and related items</returns>
        public IDictionary<string, string> ParseItemsFromMarkmapFilePath(string inputFolder, IList<string> paths, bool includeFileName)
        {
            return paths.Select(x => new
            {
                Path = PathHelper.GetShortPath(x),
                Item = includeFileName ? $"/{Path.GetRelativePath(inputFolder, Path.GetDirectoryName(x))}/{Path.GetFileNameWithoutExtension(x)}".Replace("\\", "/").Replace("/.",string.Empty) :
                $"/{Path.GetRelativePath(inputFolder, Path.GetDirectoryName(x))}".Replace("\\", "/")
            }).ToDictionary(x => x.Item, y => y.Path);
        }

        /// <summary>
        /// Close opened Open API specification
        /// </summary>
        public void CloseDocument()
        {
            // attempt to release OpenApiDocument from the memory
            if (_openApiDocument != null)
            {
                ClearComponents(_openApiDocument.Components);
                ClearPaths(_openApiDocument.Paths);
                ClearTags(_openApiDocument.Tags);
                ClearServers(_openApiDocument.Servers);
                _openApiDocument = new();
            }
            GC.Collect();

            void ClearComponents(OpenApiComponents components)
            {
                if (components != null)
                {
                    ClearExtensions(components.Extensions);
                    ClearCallbacks(components.Callbacks);
                    ClearParametersDict(components.Parameters);
                    ClearResponsesDict(components.Responses);
                    ClearRequestBodies(components.RequestBodies);
                    ClearExamples(components.Examples);
                    ClearSchemaDict(components.Schemas);
                }                
            }

            void ClearRequestBodies(IDictionary<string, OpenApiRequestBody> requestBodies)
            {
                foreach (var item in requestBodies)
                {
                    ClearRequestBody(item.Value);
                }
                requestBodies.Clear();
            }

            void ClearResponsesDict(IDictionary<string, OpenApiResponse> responses)
            {
                foreach (var item in responses)
                {
                    ClearContent(item.Value.Content);
                    ClearExtensions(item.Value.Extensions);
                    ClearHeaders(item.Value.Headers);
                    ClearLinks(item.Value.Links);
                }
                responses.Clear();
            }

            void ClearParametersDict(IDictionary<string, OpenApiParameter> parameters)
            {
                foreach (var item in parameters)
                {
                    ClearExtensions(item.Value.Extensions);
                    ClearContent(item.Value.Content);
                    ClearSchema(item.Value.Schema);
                    ClearExamples(item.Value.Examples);
                }
                parameters.Clear();
            }

            void ClearPaths(OpenApiPaths paths)
            {
                if (paths != null)
                {
                    foreach (var path in paths)
                    {
                        ClearOperations(path.Value.Operations);
                        ClearExtensions(path.Value.Extensions);
                        ClearServers(path.Value.Servers);
                        ClearParameters(path.Value.Parameters);
                    }
                    paths.Clear();
                }
            }

            void ClearOperations(IDictionary<OperationType, OpenApiOperation> operations)
            {
                foreach (var operation in operations)
                {
                    ClearCallbacks(operation.Value.Callbacks);
                    ClearExtensions(operation.Value.Extensions);
                    ClearServers(operation.Value.Servers);
                    ClearTags(operation.Value.Tags);
                    ClearExternalDocs(operation.Value.ExternalDocs);
                    ClearParameters(operation.Value.Parameters);
                    ClearRequestBody(operation.Value.RequestBody);
                    ClearResponses(operation.Value.Responses);
                    ClearSecurity(operation.Value.Security);
                }
                operations.Clear();
            }

            void ClearSecurity(IList<OpenApiSecurityRequirement> security)
            {
                foreach(var securityRequirement in security)
                {
                    securityRequirement.Clear();
                }
            }

            void ClearResponses(OpenApiResponses responses)
            {
                foreach (var response in responses)
                {
                    ClearContent(response.Value.Content);
                    ClearExtensions(response.Value.Extensions);
                    ClearHeaders(response.Value.Headers);
                    ClearLinks(response.Value.Links);
                }
            }

            void ClearLinks(IDictionary<string, OpenApiLink> links)
            {
                foreach (var link in links)
                {
                    ClearExtensions(link.Value.Extensions);
                    //ClearParameters(link.Value.Parameters);
                    //ClearRequestBody(link.Value.RequestBody);
                }
                links.Clear();
            }

            void ClearRequestBody(OpenApiRequestBody body)
            {
                if (body != null)
                {
                    ClearContent(body.Content);
                    ClearExtensions(body.Extensions);
                }
            }

            void ClearExternalDocs(OpenApiExternalDocs externalDocs)
            {
                if (externalDocs != null)
                {
                    ClearExtensions(externalDocs.Extensions);
                }
            }

            void ClearCallbacks(IDictionary<string, OpenApiCallback> callbacks)
            {
                foreach (var callback in callbacks)
                {
                    ClearPathItems(callback.Value.PathItems);
                    ClearExtensions(callback.Value.Extensions);
                }
                callbacks.Clear();
            }

            void ClearPathItems(Dictionary<RuntimeExpression, OpenApiPathItem> pathItems)
            {
                foreach (var item in pathItems)
                {
                    ClearParameters(item.Value.Parameters);
                    ClearExtensions(item.Value.Extensions);
                    ClearOperations(item.Value.Operations);
                    ClearServers(item.Value.Servers);
                }
                pathItems.Clear();
            }

            void ClearParameters(IList<OpenApiParameter> parameters)
            {
                foreach (var parameter in parameters)
                {
                    ClearExtensions(parameter.Extensions);
                    ClearContent(parameter.Content);
                    ClearSchema(parameter.Schema);
                    ClearExamples(parameter.Examples);
                }
                parameters.Clear();
            }

            void ClearContent(IDictionary<string, OpenApiMediaType> content)
            {
                foreach (var item in content)
                {
                    ClearExtensions(item.Value.Extensions);
                    ClearEncoding(item.Value.Encoding);
                    ClearExamples(item.Value.Examples);
                    ClearSchema(item.Value.Schema);
                }
                content.Clear();
            }

            void ClearEncoding(IDictionary<string, OpenApiEncoding> encoding)
            {
                foreach (var item in encoding)
                {
                    ClearExtensions(item.Value.Extensions);
                    ClearHeaders(item.Value.Headers);
                }
                encoding.Clear();
            }

            void ClearHeaders(IDictionary<string, OpenApiHeader> headers)
            {
                foreach (var item in headers)
                {
                    ClearExtensions(item.Value.Extensions);
                    ClearContent(item.Value.Content);
                    ClearExamples(item.Value.Examples);
                    ClearSchema(item.Value.Schema);
                }
                headers.Clear();
            }

            void ClearSchema(OpenApiSchema schema)
            {
                if (schema != null)
                {
                    ClearExtensions(schema.Extensions);
                    ClearAdditionalProperties(schema.AdditionalProperties);
                    ClearAllOf(schema.AllOf);
                    ClearAnyOf(schema.AnyOf);
                    ClearItems(schema.Items);
                    ClearNot(schema.Not);
                    ClearOneOf(schema.OneOf);
                    ClearSchemaDict(schema.Properties);
                }
            }

            void ClearAdditionalProperties(OpenApiSchema schema)
            {
                ClearSchema(schema);
            }

            void ClearItems(OpenApiSchema schema)
            {
                ClearSchema(schema);
            }

            void ClearNot(OpenApiSchema schema)
            {
                ClearSchema(schema);
            }

            void ClearSchemaDict(IDictionary<string, OpenApiSchema> schemaDict)
            {
                //foreach (var item in schemaDict)
                //{
                //    ClearSchema(item.Value);
                //}
                schemaDict.Clear();
            }

            void ClearAllOf(IList<OpenApiSchema> allOf)
            {
                foreach(var item in allOf)
                {
                    ClearSchema(item);
                }
            }

            void ClearAnyOf(IList<OpenApiSchema> anyOf)
            {
                foreach (var item in anyOf)
                {
                    ClearSchema(item);
                }
            }

            void ClearOneOf(IList<OpenApiSchema> oneOf)
            {
                foreach (var item in oneOf)
                {
                    ClearSchema(item);
                }
            }

            void ClearExamples(IDictionary<string, OpenApiExample> examples)
            {
                foreach (var item in examples)
                {
                    ClearExtensions(item.Value.Extensions);
                }
                examples.Clear();
            }

            void ClearServers(IList<OpenApiServer> servers)
            {
                foreach (var server in servers)
                {
                    ClearExtensions(server.Extensions);
                    ClearVariables(server.Variables);
                }
                servers.Clear();
            }

            void ClearVariables(IDictionary<string, OpenApiServerVariable> variables)
            {
                foreach (var variable in variables)
                {
                    ClearExtensions(variable.Value.Extensions);
                }
                variables.Clear();
            }

            void ClearTags(IList<OpenApiTag> tags)
            {
                foreach (var tag in tags) 
                {
                    ClearExtensions(tag.Extensions);
                }
                tags.Clear();
            }

            void ClearExtensions(IDictionary<string, IOpenApiExtension> extensions)
            {
                extensions.Clear();
            }
        }
    }
}