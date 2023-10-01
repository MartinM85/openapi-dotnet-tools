using OpenApi.Tools.Core;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace OpenApi.Tools
{
    internal class ToolsCommandManager
    {
        private enum OpenApiType { Path, Schema }
        private readonly IOpenApiTools _tools;
        private readonly CancellationToken _cancellationToken;

        public ToolsCommandManager(IOpenApiTools tools, CancellationToken cancellationToken)
        {
            _tools = tools ?? throw new ArgumentNullException(nameof(tools));
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Creates root command
        /// </summary>
        /// <returns>Main action</returns>
        public RootCommand CreateRootCommand()
        {
            var rootCommand = new RootCommand
            {
                Description = "Markdown and markmap files generator for Open API documents."
            };
            rootCommand.AddCommand(CreateGenerateEndpointMarkdownFileCommand());
            rootCommand.AddCommand(CreateGenerateEndpointMarkdownFilesCommand());
            rootCommand.AddCommand(CreateGenerateSchemaMarkdownFileCommand());
            rootCommand.AddCommand(CreateGenerateSchemaMarkdownFilesCommand());
            rootCommand.AddCommand(CreateGenerateMarkmapFileCommand());
            rootCommand.AddCommand(CreateGenerateMarkmapFilesCommand());
            rootCommand.AddCommand(CreateListEndpointsCommand());
            rootCommand.AddCommand(CreateListSchemasCommand());

            return rootCommand;
        }

        private Command CreateGenerateEndpointMarkdownFileCommand()
        {
            var genMarkdownCmd = new Command("gen-endpoint-markdown-file", "Generates Markdown file for the specific endpoint.");
            // options
            var endpointOpt = new Option<string>(new[] { "--endpoint", "-e" }, "The source endpoint like /users/{user_id}/messages/{message_id}.") { IsRequired = true };
            genMarkdownCmd.AddGlobalOption(endpointOpt);
            var outputPathOpt = new Option<string>(new[] { "--output-file", "-o" }, "The output file path.") { IsRequired = true };
            genMarkdownCmd.AddGlobalOption(outputPathOpt);
            // markmap options
            var (czl, iel, color, dur, mwo, zoom, pan) = GetMarkmapOptions();
            genMarkdownCmd.AddGlobalOption(czl);
            genMarkdownCmd.AddGlobalOption(iel);
            genMarkdownCmd.AddGlobalOption(color);
            genMarkdownCmd.AddGlobalOption(dur);
            genMarkdownCmd.AddGlobalOption(mwo);
            genMarkdownCmd.AddGlobalOption(zoom);
            genMarkdownCmd.AddGlobalOption(pan);
            // markdown options
            var (msd, msi, gis, seor) = GetMarkdownOptions(OpenApiType.Path);
            genMarkdownCmd.AddGlobalOption(msd);
            genMarkdownCmd.AddGlobalOption(msi);
            genMarkdownCmd.AddGlobalOption(gis);
            genMarkdownCmd.AddGlobalOption(seor);

            // subcommand
            var fileCommand = new Command("file", "Loads Open API document from the yaml/json file and generates Markdown file for the specific endpoint.");

            var filePathOpt = new Option<string>(new[] { "--input-file", "-i" }, "The path to yaml/json file with Open API definition.") { IsRequired = true };
            fileCommand.AddOption(filePathOpt);
            genMarkdownCmd.AddCommand(fileCommand);

            var absUrlCommand = new Command("abs-url", "Loads Open API document from absolute URL.");
            var absoluteUrlPathOpt = new Option<string>(new[] { "--absolute-url", "-a" }, "Absolute URL.") { IsRequired = true };
            absUrlCommand.AddOption(absoluteUrlPathOpt);
            genMarkdownCmd.AddCommand(absUrlCommand);

            var relUrlCommand = new Command("rel-url", "Loads Open API document from base and relative URL.");
            var baseUrlPathOpt = new Option<string>(new[] { "--base-url", "-b" }, "Base URL.") { IsRequired = true };
            relUrlCommand.AddOption(baseUrlPathOpt);

            var relativeUrlPathOpt = new Option<string>(new[] { "--relative-url", "-r" }, "Relative URL.") { IsRequired = true };
            relUrlCommand.AddOption(relativeUrlPathOpt);
            genMarkdownCmd.AddCommand(relUrlCommand);

            fileCommand.SetHandler(async (context) =>
            {
                try
                {
                    var endpoint = context.ParseResult.GetValueForOption(endpointOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown file for '{endpoint}' endpoint.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var filePath = context.ParseResult.GetValueForOption(filePathOpt);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await _tools.LoadDocumentFromFileAsync(filePath, token);
                    }

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Path);

                    await _tools.GenerateEndpointMarkdownFileAsync(endpoint, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown file for '{endpoint}' endpoint.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            absUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    var endpoint = context.ParseResult.GetValueForOption(endpointOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown file for '{endpoint}' endpoint.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var absUrl = context.ParseResult.GetValueForOption(absoluteUrlPathOpt);
                    if (!string.IsNullOrEmpty(absUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(absUrl, token);
                    }

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Path);

                    await _tools.GenerateEndpointMarkdownFileAsync(endpoint, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown file for '{endpoint}' endpoint.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            relUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    var endpoint = context.ParseResult.GetValueForOption(endpointOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown file for '{endpoint}' endpoint.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;

                    var baseUrl = context.ParseResult.GetValueForOption(baseUrlPathOpt);
                    var relativeUrl = context.ParseResult.GetValueForOption(relativeUrlPathOpt);
                    if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(relativeUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(baseUrl, relativeUrl, token);
                    }

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Path);

                    await _tools.GenerateEndpointMarkdownFileAsync(endpoint, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown file for '{endpoint}' endpoint.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            return genMarkdownCmd;
        }

        private Command CreateGenerateEndpointMarkdownFilesCommand()
        {
            var genMarkdownCmd = new Command("gen-endpoint-markdown-files", "Generates Markdown files for the selected endpoints.");
            // options
            var searchPatternOpt = new Option<string>(new[] { "--search-pattern", "-s" }, "The part of the path, or regular expression") { IsRequired = true };
            genMarkdownCmd.AddGlobalOption(searchPatternOpt);
            var outputPathOpt = new Option<string>(new[] { "--output-folder", "-o" }, "The output folder where .md files will be generated.") { IsRequired = true };
            genMarkdownCmd.AddGlobalOption(outputPathOpt);
            // markdown options
            var (czl, iel, color, dur, mwo, zoom, pan) = GetMarkmapOptions();
            genMarkdownCmd.AddGlobalOption(czl);
            genMarkdownCmd.AddGlobalOption(iel);
            genMarkdownCmd.AddGlobalOption(color);
            genMarkdownCmd.AddGlobalOption(dur);
            genMarkdownCmd.AddGlobalOption(mwo);
            genMarkdownCmd.AddGlobalOption(zoom);
            genMarkdownCmd.AddGlobalOption(pan);
            // markdown options
            var (msd, msi, gis, seor) = GetMarkdownOptions(OpenApiType.Path);
            genMarkdownCmd.AddGlobalOption(msd);
            genMarkdownCmd.AddGlobalOption(msi);
            genMarkdownCmd.AddGlobalOption(gis);
            genMarkdownCmd.AddGlobalOption(seor);

            // subcommand
            var fileCommand = new Command("file", "Loads Open API document from the yaml/json file and generates Markdown file for the specific endpoint.");

            var filePathOpt = new Option<string>(new[] { "--input-file", "-i" }, "The path to yaml/json file with Open API definition.") { IsRequired = true };
            fileCommand.AddOption(filePathOpt);
            genMarkdownCmd.AddCommand(fileCommand);

            var absUrlCommand = new Command("abs-url", "Loads Open API document from absolute URL.");
            var absoluteUrlPathOpt = new Option<string>(new[] { "--absolute-url", "-a" }, "Absolute URL.");
            absUrlCommand.AddOption(absoluteUrlPathOpt);
            genMarkdownCmd.AddCommand(absUrlCommand);

            var relUrlCommand = new Command("rel-url", "Loads Open API document from  base and relative URL.");
            var baseUrlPathOpt = new Option<string>(new[] { "--base-url", "-b" }, "Base URL.");
            relUrlCommand.AddOption(baseUrlPathOpt);

            var relativeUrlPathOpt = new Option<string>(new[] { "--relative-url", "-r" }, "Relative URL.") { IsRequired = true };
            relUrlCommand.AddOption(relativeUrlPathOpt);
            genMarkdownCmd.AddCommand(relUrlCommand);

            fileCommand.SetHandler(async (context) =>
            {
                try
                {
                    var searchPattern = context.ParseResult.GetValueForOption(searchPatternOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown files for endpoints that matches '{searchPattern}' search pattern.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var filePath = context.ParseResult.GetValueForOption(filePathOpt);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await _tools.LoadDocumentFromFileAsync(filePath, token);
                    }

                    var endpoints = _tools.FindPath(searchPattern);

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Path);

                    await _tools.GenerateEndpointMarkdownFilesAsync(endpoints, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown files for the endpoints that match search pattern '{searchPattern}'.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown files. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            absUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    var searchPattern = context.ParseResult.GetValueForOption(searchPatternOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown files for endpoints that matches '{searchPattern}' search pattern.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var absUrl = context.ParseResult.GetValueForOption(absoluteUrlPathOpt);
                    if (!string.IsNullOrEmpty(absUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(absUrl, token);
                    }

                    var endpoints = _tools.FindPath(searchPattern);

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Path);

                    await _tools.GenerateEndpointMarkdownFilesAsync(endpoints, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown files for the endpoints that match search pattern '{searchPattern}'.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown files. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            relUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    var searchPattern = context.ParseResult.GetValueForOption(searchPatternOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown files for endpoints that matches '{searchPattern}' search pattern.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;

                    var baseUrl = context.ParseResult.GetValueForOption(baseUrlPathOpt);
                    var relativeUrl = context.ParseResult.GetValueForOption(relativeUrlPathOpt);
                    if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(relativeUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(baseUrl, relativeUrl, token);
                    }

                    var endpoints = _tools.FindPath(searchPattern);

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Path);

                    await _tools.GenerateEndpointMarkdownFilesAsync(endpoints, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown files for the endpoints that match search pattern '{searchPattern}'.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown files. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            return genMarkdownCmd;
        }

        private Command CreateGenerateSchemaMarkdownFileCommand()
        {
            var genMarkdownCmd = new Command("gen-schema-markdown-file", "Generates Markdown file for the specific schema.");
            // options
            var schematOpt = new Option<string>(new[] { "--schema", "-e" }, "The schema like microsoft.graph.user.") { IsRequired = true };
            genMarkdownCmd.AddGlobalOption(schematOpt);
            var outputPathOpt = new Option<string>(new[] { "--output-file", "-o" }, "The output file path.") { IsRequired = true };
            genMarkdownCmd.AddGlobalOption(outputPathOpt);
            // markmap options
            var (czl, iel, color, dur, mwo, zoom, pan) = GetMarkmapOptions();
            genMarkdownCmd.AddGlobalOption(czl);
            genMarkdownCmd.AddGlobalOption(iel);
            genMarkdownCmd.AddGlobalOption(color);
            genMarkdownCmd.AddGlobalOption(dur);
            genMarkdownCmd.AddGlobalOption(mwo);
            genMarkdownCmd.AddGlobalOption(zoom);
            genMarkdownCmd.AddGlobalOption(pan);
            // markdown options
            var (msd, msi, gis, seor) = GetMarkdownOptions(OpenApiType.Schema);
            genMarkdownCmd.AddGlobalOption(msd);
            genMarkdownCmd.AddGlobalOption(msi);
            genMarkdownCmd.AddGlobalOption(gis);
            genMarkdownCmd.AddGlobalOption(seor);

            // subcommand
            var fileCommand = new Command("file", "Loads Open API document from the yaml/json file and generates Markdown file for the specific schema.");

            var filePathOpt = new Option<string>(new[] { "--input-file", "-i" }, "The path to yaml/json file with Open API definition.") { IsRequired = true };
            fileCommand.AddOption(filePathOpt);
            genMarkdownCmd.AddCommand(fileCommand);

            var absUrlCommand = new Command("abs-url", "Loads Open API document from absolute URL.");
            var absoluteUrlPathOpt = new Option<string>(new[] { "--absolute-url", "-a" }, "Absolute URL.") { IsRequired = true };
            absUrlCommand.AddOption(absoluteUrlPathOpt);
            genMarkdownCmd.AddCommand(absUrlCommand);

            var relUrlCommand = new Command("rel-url", "Loads Open API document from base and relative URL.");
            var baseUrlPathOpt = new Option<string>(new[] { "--base-url", "-b" }, "Base URL.") { IsRequired = true };
            relUrlCommand.AddOption(baseUrlPathOpt);

            var relativeUrlPathOpt = new Option<string>(new[] { "--relative-url", "-r" }, "Relative URL.") { IsRequired = true };
            relUrlCommand.AddOption(relativeUrlPathOpt);
            genMarkdownCmd.AddCommand(relUrlCommand);

            fileCommand.SetHandler(async (context) =>
            {
                try
                {
                    var schema = context.ParseResult.GetValueForOption(schematOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown file for '{schema}' schema.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var filePath = context.ParseResult.GetValueForOption(filePathOpt);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await _tools.LoadDocumentFromFileAsync(filePath, token);
                    }

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Schema);

                    await _tools.GenerateSchemaMarkdownFileAsync(schema, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown file for '{schema}' schema.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            absUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    var schema = context.ParseResult.GetValueForOption(schematOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown file for '{schema}' schema.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var absUrl = context.ParseResult.GetValueForOption(absoluteUrlPathOpt);
                    if (!string.IsNullOrEmpty(absUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(absUrl, token);
                    }

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Schema);

                    await _tools.GenerateSchemaMarkdownFileAsync(schema, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown file for '{schema}' schema.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            relUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    var schema = context.ParseResult.GetValueForOption(schematOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown file for '{schema}' schema.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;

                    var baseUrl = context.ParseResult.GetValueForOption(baseUrlPathOpt);
                    var relativeUrl = context.ParseResult.GetValueForOption(relativeUrlPathOpt);
                    if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(relativeUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(baseUrl, relativeUrl, token);
                    }

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Schema);

                    await _tools.GenerateSchemaMarkdownFileAsync(schema, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown file for '{schema}' schema.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            return genMarkdownCmd;
        }

        private Command CreateGenerateSchemaMarkdownFilesCommand()
        {
            var genMarkdownCmd = new Command("gen-schema-markdown-files", "Generates Markdown files for the selected schemas.");
            // options
            var searchPatternOpt = new Option<string>(new[] { "--search-pattern", "-s" }, "The part of the schema, or regular expression") { IsRequired = true };
            genMarkdownCmd.AddGlobalOption(searchPatternOpt);
            var outputPathOpt = new Option<string>(new[] { "--output-folder", "-o" }, "The output folder where .md files will be generated.") { IsRequired = true };
            genMarkdownCmd.AddGlobalOption(outputPathOpt);
            // markdown options
            var (czl, iel, color, dur, mwo, zoom, pan) = GetMarkmapOptions();
            genMarkdownCmd.AddGlobalOption(czl);
            genMarkdownCmd.AddGlobalOption(iel);
            genMarkdownCmd.AddGlobalOption(color);
            genMarkdownCmd.AddGlobalOption(dur);
            genMarkdownCmd.AddGlobalOption(mwo);
            genMarkdownCmd.AddGlobalOption(zoom);
            genMarkdownCmd.AddGlobalOption(pan);
            // markdown options
            var (msd, msi, gis, seor) = GetMarkdownOptions(OpenApiType.Schema);
            genMarkdownCmd.AddGlobalOption(msd);
            genMarkdownCmd.AddGlobalOption(msi);
            genMarkdownCmd.AddGlobalOption(gis);
            genMarkdownCmd.AddGlobalOption(seor);

            // subcommand
            var fileCommand = new Command("file", "Loads Open API document from the yaml/json file and generates Markdown files for the specific schemas.");

            var filePathOpt = new Option<string>(new[] { "--input-file", "-i" }, "The path to yaml/json file with Open API definition.") { IsRequired = true };
            fileCommand.AddOption(filePathOpt);
            genMarkdownCmd.AddCommand(fileCommand);

            var absUrlCommand = new Command("abs-url", "Loads Open API document from absolute URL.");
            var absoluteUrlPathOpt = new Option<string>(new[] { "--absolute-url", "-a" }, "Absolute URL.");
            absUrlCommand.AddOption(absoluteUrlPathOpt);
            genMarkdownCmd.AddCommand(absUrlCommand);

            var relUrlCommand = new Command("rel-url", "Loads Open API document from  base and relative URL.");
            var baseUrlPathOpt = new Option<string>(new[] { "--base-url", "-b" }, "Base URL.");
            relUrlCommand.AddOption(baseUrlPathOpt);

            var relativeUrlPathOpt = new Option<string>(new[] { "--relative-url", "-r" }, "Relative URL.") { IsRequired = true };
            relUrlCommand.AddOption(relativeUrlPathOpt);
            genMarkdownCmd.AddCommand(relUrlCommand);

            fileCommand.SetHandler(async (context) =>
            {
                try
                {
                    var searchPattern = context.ParseResult.GetValueForOption(searchPatternOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown files for schemas that matches '{searchPattern}' search pattern.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var filePath = context.ParseResult.GetValueForOption(filePathOpt);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await _tools.LoadDocumentFromFileAsync(filePath, token);
                    }

                    var schemas = _tools.FindSchema(searchPattern);

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Schema);

                    await _tools.GenerateSchemaMarkdownFilesAsync(schemas, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown files for the schemas that match search pattern '{searchPattern}'.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            absUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    var searchPattern = context.ParseResult.GetValueForOption(searchPatternOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown files for schemas that matches '{searchPattern}' search pattern.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var absUrl = context.ParseResult.GetValueForOption(absoluteUrlPathOpt);
                    if (!string.IsNullOrEmpty(absUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(absUrl, token);
                    }

                    var schemas = _tools.FindSchema(searchPattern);

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Schema);

                    await _tools.GenerateSchemaMarkdownFilesAsync(schemas, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finished generating markdown files for the schemas that match search pattern '{searchPattern}'.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            relUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    var searchPattern = context.ParseResult.GetValueForOption(searchPatternOpt);
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine($"Started generating markdown files for schemas that matches '{searchPattern}' search pattern.");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;

                    var baseUrl = context.ParseResult.GetValueForOption(baseUrlPathOpt);
                    var relativeUrl = context.ParseResult.GetValueForOption(relativeUrlPathOpt);
                    if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(relativeUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(baseUrl, relativeUrl, token);
                    }

                    var schemas = _tools.FindSchema(searchPattern);

                    var output = context.ParseResult.GetValueForOption(outputPathOpt);
                    var (markdownOptions, markmapOptions) = ParseOptions(context, OpenApiType.Schema);

                    await _tools.GenerateSchemaMarkdownFilesAsync(schemas, output, markdownOptions, markmapOptions, token);
                    context.Console.WriteLine($"Finishing generating markdown files for the schemas that match search pattern '{searchPattern}'.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markdown file. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            return genMarkdownCmd;
        }

        private Command CreateGenerateMarkmapFileCommand()
        {
            var genMarkmapFileCmd = new Command("gen-markmap-file", "Generates markmap html file to explore generated content.");
            var inputOpt = new Option<string>(new[] { "--input-file", "-i" }, "Path to the input .md file.") { IsRequired = true };
            genMarkmapFileCmd.AddOption(inputOpt);

            var outputOpt = new Option<string>(new[] { "--output-file", "-o" }, "Path to the output .html file.") { IsRequired = true };
            genMarkmapFileCmd.AddOption(outputOpt);

            genMarkmapFileCmd.SetHandler(async (context) =>
            {
                var input = context.ParseResult.GetValueForOption(inputOpt);
                context.Console.WriteLine(string.Empty);
                context.Console.WriteLine($"Started generating markmap file from '{input}' markdown file.");
                try
                {                   
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;

                    var output = context.ParseResult.GetValueForOption(outputOpt);

                    await _tools.GenerateMarkmapFileAsync(input, output, token);

                    context.Console.WriteLine($"Finished generating markmap file from '{input}' markdown file.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markmap file. {ex.Message}");
                }
                finally
                {
                    context.Console.WriteLine(string.Empty);
                }
            });

            return genMarkmapFileCmd;
        }

        private Command CreateGenerateMarkmapFilesCommand()
        {
            var genMarkmapFileCmd = new Command("gen-markmap-files", "Generates markmap html files to explore generated content.");
            var inputOpt = new Option<string>(new[] { "--input-folder", "-i" }, "Input folder with .md files.") { IsRequired = true };
            genMarkmapFileCmd.AddOption(inputOpt);

            var outputOpt = new Option<string>(new[] { "--output-folder", "-o" }, "Output folder where .html files will be generated.") { IsRequired = true };
            genMarkmapFileCmd.AddOption(outputOpt);

            genMarkmapFileCmd.SetHandler(async (context) =>
            {
                var input = context.ParseResult.GetValueForOption(inputOpt);
                context.Console.WriteLine(string.Empty);
                context.Console.WriteLine($"Started generating markmap files from markdown files inside '{input}' folder.");
                try
                {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;

                    var output = context.ParseResult.GetValueForOption(outputOpt);

                    var progress = new Progress<int>(x =>
                    {
                        context.Console.WriteLine($"Progress: {x} %");
                    });

                    await _tools.GenerateMarkmapFilesAsync(input, output, progress, token);

                    context.Console.WriteLine($"Finished generating markmap files from markdown files inside '{input}' folder.");
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed generating markmap files. {ex.Message}");
                }
                finally
                {
                    context.Console.WriteLine(string.Empty);
                }
            });

            return genMarkmapFileCmd;
        }

        private Command CreateListEndpointsCommand()
        {
            var listEndpointCmd = new Command("list-endpoints", "Returns endpoints defined in Open API definition.");
            // subcommand
            var fileCommand = new Command("file", "Loads Open API document from the yaml/json file and returns all endpoints.");

            var filePathOpt = new Option<string>(new[] { "--input-file", "-i" }, "The path to yaml/json file with Open API definition.") { IsRequired = true };
            fileCommand.AddOption(filePathOpt);
            listEndpointCmd.AddCommand(fileCommand);

            var absUrlCommand = new Command("abs-url", "Loads Open API document from absolute URL.");
            var absoluteUrlPathOpt = new Option<string>(new[] { "--absolute-url", "-a" }, "Absolute URL.") { IsRequired = true };
            absUrlCommand.AddOption(absoluteUrlPathOpt);
            listEndpointCmd.AddCommand(absUrlCommand);

            var relUrlCommand = new Command("rel-url", "Loads Open API document from base and relative URL.");
            var baseUrlPathOpt = new Option<string>(new[] { "--base-url", "-b" }, "Base URL.") { IsRequired = true };
            relUrlCommand.AddOption(baseUrlPathOpt);

            var relativeUrlPathOpt = new Option<string>(new[] { "--relative-url", "-r" }, "Relative URL.") { IsRequired = true };
            relUrlCommand.AddOption(relativeUrlPathOpt);
            listEndpointCmd.AddCommand(relUrlCommand);

            fileCommand.SetHandler(async (context) =>
            {
                try
                {
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine("Endpoints defined in Open API document:");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var filePath = context.ParseResult.GetValueForOption(filePathOpt);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await _tools.LoadDocumentFromFileAsync(filePath, token);
                    }

                    var paths = _tools.GetPaths();

                    foreach ( var path in paths )
                    {
                        context.Console.WriteLine(path);
                    }
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed to list endpoints. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            absUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine("Endpoints defined in Open API document:");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var absUrl = context.ParseResult.GetValueForOption(absoluteUrlPathOpt);
                    if (!string.IsNullOrEmpty(absUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(absUrl, token);
                    }

                    var paths = _tools.GetPaths();

                    foreach (var path in paths)
                    {
                        context.Console.WriteLine(path);
                    }
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed to list endpoints. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            relUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine("Endpoints defined in Open API document:");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var baseUrl = context.ParseResult.GetValueForOption(baseUrlPathOpt);
                    var relativeUrl = context.ParseResult.GetValueForOption(relativeUrlPathOpt);
                    if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(relativeUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(baseUrl, relativeUrl, token);
                    }

                    var paths = _tools.GetPaths();

                    foreach (var path in paths)
                    {
                        context.Console.WriteLine(path);
                    }
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed to list endpoints. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            return listEndpointCmd;
        }

        private Command CreateListSchemasCommand()
        {
            var listSchemaCmd = new Command("list-schemas", "Returns schemas defined in Open API definition.");
            // subcommand
            var fileCommand = new Command("file", "Loads Open API document from the yaml/json file and returns all schemas.");

            var filePathOpt = new Option<string>(new[] { "--input-file", "-i" }, "The path to yaml/json file with Open API definition.") { IsRequired = true };
            fileCommand.AddOption(filePathOpt);
            listSchemaCmd.AddCommand(fileCommand);

            var absUrlCommand = new Command("abs-url", "Loads Open API document from absolute URL.");
            var absoluteUrlPathOpt = new Option<string>(new[] { "--absolute-url", "-a" }, "Absolute URL.") { IsRequired = true };
            absUrlCommand.AddOption(absoluteUrlPathOpt);
            listSchemaCmd.AddCommand(absUrlCommand);

            var relUrlCommand = new Command("rel-url", "Loads Open API document from base and relative URL.");
            var baseUrlPathOpt = new Option<string>(new[] { "--base-url", "-b" }, "Base URL.") { IsRequired = true };
            relUrlCommand.AddOption(baseUrlPathOpt);

            var relativeUrlPathOpt = new Option<string>(new[] { "--relative-url", "-r" }, "Relative URL.") { IsRequired = true };
            relUrlCommand.AddOption(relativeUrlPathOpt);
            listSchemaCmd.AddCommand(relUrlCommand);

            fileCommand.SetHandler(async (context) =>
            {
                try
                {
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine("Schemas defined in Open API document:");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var filePath = context.ParseResult.GetValueForOption(filePathOpt);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        await _tools.LoadDocumentFromFileAsync(filePath, token);
                    }

                    var schemas = _tools.GetSchemas();

                    foreach (var schema in schemas)
                    {
                        context.Console.WriteLine(schema);
                    }
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed to list schemas. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            absUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine("Schemas defined in Open API document:");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var absUrl = context.ParseResult.GetValueForOption(absoluteUrlPathOpt);
                    if (!string.IsNullOrEmpty(absUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(absUrl, token);
                    }

                    var schemas = _tools.GetSchemas();

                    foreach (var schema in schemas)
                    {
                        context.Console.WriteLine(schema);
                    }
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed to list schemas. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            relUrlCommand.SetHandler(async (context) =>
            {
                try
                {
                    context.Console.WriteLine(string.Empty);
                    context.Console.WriteLine("Schemas defined in Open API document:");
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.GetCancellationToken(), _cancellationToken);
                    var token = cts.Token;
                    var baseUrl = context.ParseResult.GetValueForOption(baseUrlPathOpt);
                    var relativeUrl = context.ParseResult.GetValueForOption(relativeUrlPathOpt);
                    if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(relativeUrl))
                    {
                        await _tools.LoadDocumentFromUrlAsync(baseUrl, relativeUrl, token);
                    }

                    var schemas = _tools.GetSchemas();

                    foreach (var schema in schemas)
                    {
                        context.Console.WriteLine(schema);
                    }
                }
                catch (Exception ex)
                {
                    context.Console.WriteLine($"Failed to list schemas. {ex.Message}");
                }
                finally
                {
                    _tools.CloseDocument();
                    context.Console.WriteLine(string.Empty);
                }
            });

            return listSchemaCmd;
        }

        private (MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions) ParseOptions(InvocationContext context, OpenApiType type)
        {
            var (czl, iel, color, dur, mwo, zm, pn) = GetMarkmapOptions();
            var colorFreezeLevel = context.ParseResult.GetValueForOption(czl);
            var initialExpandLevel = context.ParseResult.GetValueForOption(iel);
            var colors = context.ParseResult.GetValueForOption(color);
            var duration = context.ParseResult.GetValueForOption(dur);
            var maxWidth = context.ParseResult.GetValueForOption(mwo);
            var zoom = context.ParseResult.GetValueForOption(zm);
            var pan = context.ParseResult.GetValueForOption(pn);
            var markmapOptions = new MarkmapOptions(colorFreezeLevel, initialExpandLevel, colors, duration, maxWidth, zoom, pan);

            var (msd, msi, gis, seor) = GetMarkdownOptions(type);
            var maxSchemaDepth = context.ParseResult.GetValueForOption(msd);
            var maxSingleItems = context.ParseResult.GetValueForOption(msi);
            var groupItemsSize = context.ParseResult.GetValueForOption(gis);
            var showEndpointOnRoot = context.ParseResult.GetValueForOption(seor);
            var markdownWriterOptions = new MarkdownWriterOptions(maxSchemaDepth, maxSingleItems, groupItemsSize, showEndpointOnRoot);

            return (markdownWriterOptions, markmapOptions);
        }

        private (Option<int> czl, Option<int> iel, Option<string[]> color, Option<int> dur, Option<int> mwo, Option<bool> zoom, Option<bool> pan) _markmapOpts;
        private Dictionary<OpenApiType, (Option<int> maxSchemaDepthOpt, Option<int> maxSingleItemsOpt, Option<int> groupItemsSizeOpt, Option<bool> showEndpointOnRootOpt)> _markdownOpts = new();

        private (Option<int> czl, Option<int> iel, Option<string[]> color, Option<int> dur, Option<int> mwo, Option<bool> zoom, Option<bool> pan) GetMarkmapOptions()
        {
            if (_markmapOpts.Equals(default))
            {
                _markmapOpts = CreateMarkmapOptions();
            }
            return _markmapOpts;
        }

        private static (Option<int> czl, Option<int> iel, Option<string[]> color, Option<int> dur, Option<int> mwo, Option<bool> zoom, Option<bool> pan) CreateMarkmapOptions()
        {
            var colorFreezeLevelOpt = new Option<int>(new[] { "--color-freeze-level", "-f" }, "Freeze color at the specified level of branches. 0 for no freezing at all.");
            colorFreezeLevelOpt.SetDefaultValue(MarkmapOptions.DefaultColorFreezeLevel);
            
            var initialExpandLevelOpt = new Option<int>(new[] { "--initial-expand-level", "-n" }, "The maximum level of nodes to expand on initial render. -1 for expanding all levels.");
            initialExpandLevelOpt.SetDefaultValue(MarkmapOptions.DefaultInitialExpandLevel);
            
            var colorOpt = new Option<string[]>(new[] { "--color", "-c" }, "A list of colors to use as the branch and circle colors for each node. If none is provided, d3.schemeCategory10 will be used.");
            colorOpt.SetDefaultValue(MarkmapOptions.DefaultColor);
            
            var durationOpt = new Option<int>(new[] { "--duration", "-d" }, "The animation duration when folding/unfolding a node.");
            durationOpt.SetDefaultValue(MarkmapOptions.DefaultDuration);
            
            var maxWidthOpt = new Option<int>(new[] { "--max-width", "-m" }, "The max width of each node content. 0 for no limit.");
            maxWidthOpt.SetDefaultValue(MarkmapOptions.DefaultMaxWidth);
            
            var zoomOpt = new Option<bool>(new[] { "--zoom", "-z" }, "Whether to allow zooming the markmap.");
            zoomOpt.SetDefaultValue(MarkmapOptions.DefaultZoom);
            
            var panOpt = new Option<bool>(new[] { "--pan", "-p" }, "Whether to allow panning the markmap.");
            panOpt.SetDefaultValue(MarkmapOptions.DefaultPan);

            return (colorFreezeLevelOpt, initialExpandLevelOpt, colorOpt, durationOpt, maxWidthOpt, zoomOpt, panOpt);
        }

        private (Option<int> maxSchemaDepthOpt, Option<int> maxSingleItemsOpt, Option<int> groupItemsSizeOpt, Option<bool> showEndpointOnRootOpt) GetMarkdownOptions(OpenApiType type)
        {
            if (!_markdownOpts.ContainsKey(type))
            {
                _markdownOpts[type] = CreateMarkdownOptions(type);
            }
            return _markdownOpts[type];
        }

        private static (Option<int> maxSchemaDepthOpt, Option<int> maxSingleItemsOpt, Option<int> groupItemsSizeOpt, Option<bool> showEndpointOnRootOpt) CreateMarkdownOptions(OpenApiType type)
        {
            var defaults = type switch
            {
                OpenApiType.Path => (MarkdownWriterOptions.DefaultMaxSchemaDepth, MarkdownWriterOptions.DefaultMaxSingleItems, MarkdownWriterOptions.DefaultGroupItemsSize),
                OpenApiType.Schema => (MarkdownWriterOptions.DefaultMaxSchemaDepthForSchema, MarkdownWriterOptions.DefaultMaxSingleItemsForSchema, MarkdownWriterOptions.DefaultGroupItemsSizeForSchema),
                _ => throw new NotImplementedException()
            };
            var maxSchemaDepthOpt = new Option<int>(new[] { "--max-schema-depth", "-x" }, "Affects how deep the writer goes in case of OpenApiSchema and its properties.");
            maxSchemaDepthOpt.SetDefaultValue(defaults.Item1);

            var maxSingleItemsOpt = new Option<int>(new[] { "--max-single-items", "-s" }, "The size of OpenApiSchema Enum/Propeties collection when items are printed one per line.");
            maxSingleItemsOpt.SetDefaultValue(defaults.Item2);

            var groupItemsSizeOpt = new Option<int>(new[] { "--group-items-size", "-g" }, "Number of items from OpenApiSchema Enum/Properties that printed on one line if the size of OpenApiSchema Enum/Properties exceeds MaxSingleItems.");
            groupItemsSizeOpt.SetDefaultValue(defaults.Item3);

            var showEndpointOnRootOpt = new Option<bool>(new[] { "--show-name-in-root", "-h" }, "If true then name of the endpoint or the schema is displayed in the root, otherwise as the first subitem.");
            showEndpointOnRootOpt.SetDefaultValue(MarkdownWriterOptions.DefaultShowNameInRoot);

            return (maxSchemaDepthOpt, maxSingleItemsOpt, groupItemsSizeOpt, showEndpointOnRootOpt);
        }
    }
}