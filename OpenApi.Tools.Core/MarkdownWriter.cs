using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System;

namespace OpenApi.Tools.Core
{
    /// <summary>
    /// Creates a markdown file for Open API endpoint represented by <see cref="OpenApiPathItem"/>
    /// </summary>
    internal class MarkdownWriter
    {
        private readonly TextWriter _writer;
        private readonly MarkdownWriterOptions _markdownOptions;
        private readonly MarkmapOptions _markmapOptions;
        private CancellationToken _cancellationToken;

        internal MarkdownWriter(TextWriter writer) : this(writer, MarkdownWriterOptions.Default, MarkmapOptions.Default)
        {
        }

        internal MarkdownWriter(TextWriter writer, MarkdownWriterOptions markdownOptions, MarkmapOptions markmapOptions)
        {
            _writer = writer;
            _markdownOptions = markdownOptions;
            _markmapOptions = markmapOptions;
        }

        /// <summary>
        /// Writes info about the selected endpoint into Markdown file
        /// </summary>
        /// <param name="endpoint">The endpoint</param>
        /// <param name="item">Open API definition of the endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        internal async Task WriteEndpointAsync(string endpoint, OpenApiPathItem item, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            await WriteHeaderAsync();
            await WriteTitleAsync(endpoint);
            await WriteDescriptionAsync(item.Description);
            await WriteSummaryAsync(item.Summary);
            await WriteOperationsAsync(item.Operations, 0);
            await WriteParametersAsync(item.Parameters, 0);
            await WriteExtensionsAsync(item.Extensions, 0);
        }

        internal async Task WriteSchemaAsync(string name, OpenApiSchema schema, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            await WriteHeaderAsync();
            await WriteTitleAsync(name);
            _currentSchemaDepth++;
            //await WriteReferenceAsync(schema.Reference, indent);
            await WriteSchemaEnumAsync(schema.Enum, 0);
            await WriteSchemaAsync(schema.Properties, 0);
            await WriteSchemaItemsAsync(schema.Items, 0);
            await WriteSchemaAsync(schema.AllOf, 0);
            await WriteSchemaAsync(schema.AnyOf, 0);
            await WriteExtensionsAsync(schema.Extensions, 0);
            _currentSchemaDepth--;
        }

        private async Task WriteHeaderAsync()
        {
            await WriteTextAsync("---");
            await WriteTextAsync("markmap:");
            if (_markmapOptions.ColorFreezeLevel > 0)
            {
                await WriteTextAsync($"  colorFreezeLevel: {_markmapOptions.ColorFreezeLevel}");
            }
            if (_markmapOptions.InitialExpandLevel >= 0)
            {
                await WriteTextAsync($"  initialExpandLevel: {_markmapOptions.InitialExpandLevel}");
            }
            if (_markmapOptions.Color != null && _markmapOptions.Color.Length > 0)
            {
                await WriteTextAsync("  color:");
                foreach (var color in _markmapOptions.Color)
                {
                    await WriteTextAsync($"\"{color}\"");
                }
            }
            await WriteTextAsync($"  duration: {_markmapOptions.Duration}");
            if (_markmapOptions.MaxWidth > 0)
            {
                await WriteTextAsync($"  maxWidth: {_markmapOptions.MaxWidth}");
            }
            if (!_markmapOptions.Zoom)
            {
                await WriteTextAsync("  zoom: false");
            }
            if (!_markmapOptions.Pan)
            {
                await WriteTextAsync("  pan: false");
            }
            await WriteTextAsync("---");
            await WriteTextAsync(string.Empty);
        }

        private async Task WriteTitleAsync(string title)
        {
            if (_markdownOptions.ShowNameInRoot)
            {
                await WriteTextAsync($"# {title}");
            }
            else
            {
                await WriteTextAsync("#");
                await WriteListItemAsync($"**{title}**", 0);
            }
        }

        private async Task WriteDescriptionAsync(string description, int indent = 0)
        {
            if (!string.IsNullOrEmpty(description))
            {
                await WriteListItemAsync($"**Description**: {description}", indent);
            }
        }

        private async Task WriteSummaryAsync(string summary, int indent = 0)
        {
            if (!string.IsNullOrEmpty(summary))
            {
                await WriteListItemAsync($"**Summary**: {summary}", indent);
            }
        }

        private async Task WriteOperationsAsync(IDictionary<OperationType, OpenApiOperation> operations, int indent)
        {
            if (operations.Any())
            {
                await WriteListItemAsync("Operations", indent);
                foreach (var operation in operations)
                {
                    await WriteListItemAsync($"**{operation.Key}**{(operation.Value.Deprecated ? " (deprecated)" : string.Empty)}", indent + 2);
                    await WriteDescriptionAsync(operation.Value.Description, indent + 4);
                    await WriteSummaryAsync(operation.Value.Summary, indent + 4);
                    await WriteListItemAsync($"**Id**: {operation.Value.OperationId}", indent + 4);
                    await WriteParametersAsync(operation.Value.Parameters, indent + 4);
                    await WriteRequestBodyAsync(operation.Value.RequestBody, indent + 4);
                    await WriteResponsesAsync(operation.Value.Responses, indent + 4);
                    await WriteExtensionsAsync(operation.Value.Extensions, indent + 4);
                }
            }
        }

        private int _currentSchemaDepth;
        private async Task WriteParametersAsync(IList<OpenApiParameter> parameters, int indent)
        {
            if (parameters.Any())
            {
                await WriteListItemAsync("Parameters", indent);
                foreach (var parameter in parameters)
                {
                    var paramInfo = $"{parameter.Name} ({parameter.In} | {(parameter.Required ? "required" : "optional")} | {parameter.Schema?.Type} | {parameter.Style}{(parameter.Deprecated ? " | deprecated" : string.Empty)})";
                    await WriteListItemAsync(paramInfo, indent + 2);
                    await WriteDescriptionAsync(parameter.Description, indent + 4);
                    await WriteContentAsync(parameter.Content, indent + 4);
                    await WriteSchemaAsync(parameter.Schema, indent + 2);
                    _currentSchemaDepth = 0;
                    await WriteExtensionsAsync(parameter.Extensions, indent + 2);
                }
            }
        }

        private async Task WriteExtensionsAsync(IDictionary<string, IOpenApiExtension> extensions, int indent)
        {
            foreach (var extension in extensions)
            {
                if (extension.Key == "x-ms-enum" && extension.Value is OpenApiObject enums)
                {
                    await WriteListItemAsync(extension.Key, indent);
                    await WriteEnumExtensionAsync(enums, indent + 2);
                }
                else if (extension.Value is OpenApiObject oaObject)
                {
                    await WriteListItemAsync(extension.Key, indent);
                    await WriteOpenApiObjectAsync(oaObject, indent + 2);
                }
                else if (extension.Value is OpenApiArray oaArray)
                {
                    await WriteListItemAsync($"**{extension.Key}**", indent);
                    await WriteOpenApiArrayAsync(oaArray, indent + 2);
                }
                else if (extension.Value is OpenApiString oaString)
                {
                    await WriteKeyValueAsync(extension.Key, oaString.Value, indent);
                }
                else if (extension.Value is OpenApiBoolean oaBool)
                {
                    await WriteKeyValueAsync(extension.Key, oaBool.Value ? "true" : "false", indent);
                }
                else
                {

                }
            }
        }

        private async Task WriteEnumExtensionAsync(OpenApiObject enums, int indent)
        {
            // name
            if (enums.TryGetValue("name", out var enumName) && enumName is OpenApiString oaString)
            {
                await WriteKeyValueAsync("name", oaString.Value, indent);
                if (enums.TryGetValue("modelAsString", out var model) && model is OpenApiBoolean oaBool)
                {
                    await WriteKeyValueAsync("modelAsString", oaBool.Value ? "true" : "false", indent + 2);
                    if (enums.TryGetValue("values", out var values) && values is OpenApiArray oaArray)
                    {
                        await WriteListItemAsync("values", indent + 2);
                        foreach (var item in oaArray)
                        {
                            if (item is OpenApiObject oaObject)
                            {
                                if (oaObject.TryGetValue("value", out var rawValue) && rawValue is OpenApiString value
                                    && oaObject.TryGetValue("description", out var rawDescription) && rawDescription is OpenApiString description
                                    && oaObject.TryGetValue("name", out var rawName) && rawName is OpenApiString name)
                                {
                                    await WriteListItemAsync(value.Value, indent + 4);
                                    await WriteKeyValueAsync("description", description.Value, indent + 6);
                                    await WriteKeyValueAsync("name", name.Value, indent + 6);
                                }
                            }
                        }
                    }
                }
            }

        }

        private async Task WriteOpenApiObjectAsync(OpenApiObject oaObject,int indent)
        {
            foreach (var oaExtension in oaObject)
            {
                if (oaExtension.Value is OpenApiString oaString)
                {
                    await WriteKeyValueAsync(oaExtension.Key, oaString.Value, indent);
                }
                else if (oaExtension.Value is OpenApiBoolean oaBool)
                {
                    await WriteKeyValueAsync(oaExtension.Key, oaBool.Value ? "true" : "false", indent);
                }
                else if (oaExtension.Value is OpenApiArray oaArray)
                {
                    await WriteOpenApiArrayAsync(oaArray, indent + 2);
                }
                else if (oaExtension.Value is OpenApiDateTime oaDate)
                {
                    await WriteKeyValueAsync(oaExtension.Key, oaDate.Value.ToString(), indent);
                }
                else
                {

                }
            }
        }

        private async Task WriteOpenApiArrayAsync(OpenApiArray oaArray, int indent)
        {
            foreach (var oaExtension in oaArray)
            {
                if (oaExtension is OpenApiString oaString)
                {
                    await WriteListItemAsync($"{oaString.Value}", indent);
                }
                else if (oaExtension is OpenApiBoolean oaBool)
                {
                    await WriteListItemAsync($"{(oaBool.Value ? "true" : "false")}", indent);
                }
                else if (oaExtension is OpenApiObject oaObject)
                {
                    await WriteOpenApiObjectAsync(oaObject, indent);
                }
                else if (oaExtension is OpenApiDateTime oaDate)
                {
                    await WriteListItemAsync(oaDate.Value.ToString(), indent);
                }
                else
                {

                }
            }
        }

        private async Task WriteKeyValueAsync(string key, string value, int indent)
        {
            await WriteListItemAsync($"**{key}**: {value}", indent);
        }

        private async Task WriteSchemaAsync(OpenApiSchema schema, int indent)
        {
            _currentSchemaDepth++;
            if (schema != null && _currentSchemaDepth <= _markdownOptions.MaxSchemaDepth)
            {
                await WriteReferenceAsync(schema.Reference, indent);
                await WriteSchemaEnumAsync(schema.Enum, indent);
                var indentIncrement = schema.Reference == null && schema.Enum.Count == 0 ? 0 : 2;
                await WriteSchemaAsync(schema.Properties, indent + indentIncrement);
                await WriteSchemaItemsAsync(schema.Items, indent + indentIncrement);
                await WriteSchemaAsync(schema.AllOf, indent + indentIncrement);
                await WriteSchemaAsync(schema.AnyOf, indent + indentIncrement);
                await WriteExtensionsAsync(schema.Extensions, indent + indentIncrement);
            }
            _currentSchemaDepth--;
        }

        private async Task WritePropertyOrRelationshipAsync(string property, OpenApiSchema schema, int indent)
        {
            await WriteListItemAsync($"{property} {(!string.IsNullOrEmpty(schema.Type) ? string.Format("({0})", schema.Type) : string.Empty)}", indent);
            _currentSchemaDepth++;
            if (schema != null && _currentSchemaDepth <= _markdownOptions.MaxSchemaDepth)
            {
                await WriteReferenceAsync(schema.Reference, indent + 2);
                await WriteSchemaEnumAsync(schema.Enum, indent + 2);
                await WriteSchemaAsync(schema.Properties, indent + 2);
                await WriteSchemaItemsAsync(schema.Items, indent + 2);
                await WriteSchemaAsync(schema.AllOf, indent + 2);
                await WriteSchemaAsync(schema.AnyOf, indent + 2);
                await WriteExtensionsAsync(schema.Extensions, indent + 2);
            }
            _currentSchemaDepth--;
        }

        private async Task WriteSchemaEnumAsync(IList<IOpenApiAny> items, int indent)
        {
            if (items.Any())
            {
                await WriteListItemAsync("Items", indent);
                if (_markdownOptions.MaxSingleItems > 0 && items.Count > _markdownOptions.MaxSingleItems)
                {
                    foreach (var itemEnums in items.Chunk(_markdownOptions.GroupItemsSize))
                    {
                        var oaStrings = string.Join(", ", itemEnums.OfType<OpenApiString>().Select(x => x.Value.Replace("*", "'*'")));
                        await WriteListItemAsync(oaStrings, indent + 2);
                    }
                }
                else
                {
                    foreach (var item in items)
                    {
                        if (item is OpenApiString oaString)
                        {
                            await WriteListItemAsync(oaString.Value.Replace("*", "'*'"), indent + 2);
                        }
                    }
                }
            }
        }

        private async Task WriteSchemaItemsAsync(OpenApiSchema schema, int indent)
        {
            if (schema != null && _currentSchemaDepth <= _markdownOptions.MaxSchemaDepth)
            {
                await WriteReferenceAsync(schema.Reference, indent);
                await WriteSchemaEnumAsync(schema.Enum, indent);
                await WriteSchemaAsync(schema.Properties, indent);
                await WriteSchemaAsync(schema.Items, indent);
                await WriteSchemaAsync(schema.AllOf, indent);
                await WriteSchemaAsync(schema.AnyOf, indent);
                await WriteExtensionsAsync(schema.Extensions, indent);
            }
        }

        private async Task WriteSchemaAsync(IDictionary<string, OpenApiSchema> schemas, int indent)
        {
            if (schemas.Any() && _currentSchemaDepth <= _markdownOptions.MaxSchemaDepth)
            {
                var properties = schemas.Where(x => x.Value.Extensions.All(y => y.Key != "x-ms-navigationProperty")).ToList();
                await WritePropertiesAsync(properties, indent);

                var relationships = schemas.Where(x => x.Value.Extensions.Any(y => y.Key == "x-ms-navigationProperty")).ToList();
                await WriteRelationshipsAsync(relationships, indent);
            }
        }

        private async Task WritePropertiesAsync(List<KeyValuePair<string, OpenApiSchema>> properties, int indent)
        {
            if (properties.Any())
            {
                await WriteListItemAsync("Properties", indent);
                if (_markdownOptions.MaxSingleItems > 0 && properties.Count > _markdownOptions.MaxSingleItems)
                {
                    foreach (var props in properties.Chunk(_markdownOptions.GroupItemsSize))
                    {
                        var oaStrings = string.Join(", ", props.Select(x => x.Key));
                        await WriteListItemAsync(oaStrings, indent + 2);
                    }
                }
                else
                {
                    foreach (var property in properties)
                    {
                        await WritePropertyOrRelationshipAsync(property.Key, property.Value, indent + 2);
                    }
                }
            }
        }

        private async Task WriteRelationshipsAsync(List<KeyValuePair<string, OpenApiSchema>> relationships, int indent)
        {
            if (relationships.Any())
            {
                await WriteListItemAsync("Relationships", indent);
                if (_markdownOptions.MaxSingleItems > 0 && relationships.Count > _markdownOptions.MaxSingleItems)
                {
                    foreach (var props in relationships.Chunk(_markdownOptions.GroupItemsSize))
                    {
                        var oaStrings = string.Join(", ", props.Select(x => x.Key));
                        await WriteListItemAsync(oaStrings, indent + 2);
                    }
                }
                else
                {
                    foreach (var relationship in relationships)
                    {
                        await WritePropertyOrRelationshipAsync(relationship.Key, relationship.Value, indent + 2);
                    }
                }
            }
        }

        private async Task WriteSchemaAsync(IList<OpenApiSchema> schemas, int indent)
        {
            if (schemas.Any() && _currentSchemaDepth <= _markdownOptions.MaxSchemaDepth)
            {
                foreach (var schema in schemas.OrderBy(x => x.Reference?.Id))
                {
                    await WriteReferenceAsync(schema.Reference, indent);
                    await WriteSchemaAsync(schema.Properties, indent + 2);
                    await WriteSchemaAsync(schema.Items, indent + 2);
                    await WriteSchemaAsync(schema.AllOf, indent + 2);
                    await WriteSchemaAsync(schema.AnyOf, indent + 2);
                    await WriteExtensionsAsync(schema.Extensions, indent + 2);
                }
            }
        }

        private async Task WriteRequestBodyAsync(OpenApiRequestBody body, int indent)
        {
            if (body != null)
            {
                await WriteListItemAsync($"Request body ({(body.Required ? "required" : "optional")})", indent);
                await WriteDescriptionAsync(body.Description, indent + 2);
                await WriteContentAsync(body.Content, indent + 2);
                await WriteExtensionsAsync(body.Extensions, indent + 2);
            }
        }

        private async Task WriteContentAsync(IDictionary<string, OpenApiMediaType> content, int indent)
        {
            if (content.Any())
            {
                foreach (var item in content)
                {
                    await WriteListItemAsync($"**Content type**: {item.Key}", indent);
                    await WriteSchemaAsync(item.Value.Schema, indent + 2);
                    await WriteEncodingsAsync(item.Value.Encoding, indent + 2);
                    await WriteExtensionsAsync(item.Value.Extensions, indent + 2);
                }
            }
        }

        private async Task WriteReferenceAsync(OpenApiReference reference, int indent)
        {
            if (reference != null)
            {
                await WriteListItemAsync($"**schema $ref**: {reference.Id}", indent);
            }
        }

        private async Task WriteEncodingsAsync(IDictionary<string, OpenApiEncoding> encodings, int indent)
        {
            foreach(var encoding in encodings)
            {
                var encodingInfo = $"{encoding.Key} ({encoding.Value.ContentType} | {encoding.Value.Style})";
                await WriteListItemAsync(encodingInfo, indent);
            }
        }

        private async Task WriteResponsesAsync(OpenApiResponses responses, int indent)
        {
            if (responses.Any())
            {
                await WriteListItemAsync("Responses", indent);
                foreach (var response in responses)
                {
                    await WriteListItemAsync($"**{response.Key}**", indent + 2);
                    await WriteDescriptionAsync(response.Value.Description, indent + 4);
                    await WriteContentAsync(response.Value.Content, indent + 4);
                    await WriteHeadersAsync(response.Value.Headers, indent + 4);
                }
            }
        }

        private async Task WriteHeadersAsync(IDictionary<string, OpenApiHeader> headers, int indent)
        {
            foreach (var header in headers)
            {
                await WriteListItemAsync($"{header.Key}{(header.Value.Deprecated ? "(deprecated)" : string.Empty)}", indent);
                await WriteDescriptionAsync(header.Value.Description, indent + 2);
            }
        }

        private async Task WriteListItemAsync(string text, int indent)
        {
            var spaces = new string(' ', indent);
            await WriteTextAsync($"{spaces}- {text}");
        }

        private async Task WriteTextAsync(string text)
        {
            // using LF because markmap-cli generates a node for the settings when using CRLF
            await _writer.WriteAsync($"{text}\n".AsMemory(), _cancellationToken);
        }
    }
}