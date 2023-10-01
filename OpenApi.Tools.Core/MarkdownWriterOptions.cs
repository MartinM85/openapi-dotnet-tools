using Microsoft.OpenApi.Models;

namespace OpenApi.Tools.Core
{
    /// <summary>
    /// Markdown settings
    /// </summary>
    public readonly struct MarkdownWriterOptions
    {
        /// <summary>
        /// Default MaxSchemaDepth
        /// </summary>
        public const int DefaultMaxSchemaDepth = 3;

        /// <summary>
        /// Default MaxSchemaDepth when standalone schema is generated
        /// </summary>
        public const int DefaultMaxSchemaDepthForSchema = 2;

        /// <summary>
        /// Default MaxSingleItems
        /// </summary>
        public const int DefaultMaxSingleItems = 10;

        /// <summary>
        /// Default MaxSingleItems when standalone schema is generated
        /// </summary>
        public const int DefaultMaxSingleItemsForSchema = 0;

        /// <summary>
        /// Default GroupItemsSize
        /// </summary>
        public const int DefaultGroupItemsSize = 6;

        /// <summary>
        /// Default GroupItemsSize when standalone schema is generated
        /// </summary>
        public const int DefaultGroupItemsSizeForSchema = 0;

        /// <summary>
        /// Default ShowNameInRoot
        /// </summary>
        public const bool DefaultShowNameInRoot = true;

        /// <summary>
        /// Affects how deep the writer goes in case of <see cref="OpenApiSchema"/> and its properties.
        /// </summary>
        public readonly int MaxSchemaDepth;

        /// <summary>
        /// The size of <see cref="OpenApiSchema.Enum"/>/<see cref="OpenApiSchema.Properties"/> collection when items are printed one per line.
        /// </summary>
        public readonly int MaxSingleItems;

        /// <summary>
        /// Number of items from <see cref="OpenApiSchema.Enum"/>/<see cref="OpenApiSchema.Properties"/> that printed on one line if the size of <see cref="OpenApiSchema.Enum"/>/<see cref="OpenApiSchema.Properties"/> exceeds <see cref="MaxSingleItems"/>
        /// </summary>
        public readonly int GroupItemsSize;

        /// <summary>
        /// If true then name of the endpoint or the schema is displayed in the root, otherwise as the first subitem.
        /// </summary>
        public readonly bool ShowNameInRoot;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="maxSchemaDepth">Affects how deep the writer goes in case of <see cref="OpenApiSchema"/> and its properties</param>
        /// <param name="maxSingleItems">The size of <see cref="OpenApiSchema.Enum"/>/<see cref="OpenApiSchema.Properties"/> collection when items are printed one per line</param>
        /// <param name="groupItemsSize">Number of items from <see cref="OpenApiSchema.Enum"/>/<see cref="OpenApiSchema.Properties"/> that printed on one line if the size of <see cref="OpenApiSchema.Enum"/>/<see cref="OpenApiSchema.Properties"/> exceeds <see cref="MaxSingleItems"/></param>
        /// <param name="showNameInRoot">If true then name of the endpoint or the schema is displayed in the root, otherwise as the first subitem</param>
        public MarkdownWriterOptions(int maxSchemaDepth, int maxSingleItems, int groupItemsSize, bool showNameInRoot)
        {
            MaxSchemaDepth = maxSchemaDepth;
            MaxSingleItems = maxSingleItems;
            GroupItemsSize = groupItemsSize;
            ShowNameInRoot = showNameInRoot;
        }

        /// <summary>
        /// Default options
        /// </summary>
        public static MarkdownWriterOptions Default => new(DefaultMaxSchemaDepth, DefaultMaxSingleItems, DefaultGroupItemsSize, DefaultShowNameInRoot);

        /// <summary>
        /// Default options for schema
        /// </summary>
        public static MarkdownWriterOptions DefaultForSchema => new(DefaultMaxSchemaDepthForSchema, DefaultMaxSingleItemsForSchema, DefaultGroupItemsSizeForSchema, DefaultShowNameInRoot);
    }
}