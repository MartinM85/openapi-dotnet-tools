namespace OpenApi.Tools.Core
{
    /// <summary>
    /// Markmap options
    /// </summary>
    public readonly struct MarkmapOptions
    {
        /// <summary>
        /// Default ColorFreezeLevel
        /// </summary>
        public const int DefaultColorFreezeLevel = 6;
        /// <summary>
        /// Default InitialExpandLevel
        /// </summary>
        public const int DefaultInitialExpandLevel = 5;
        /// <summary>
        /// Default Color
        /// </summary>
        public const string[] DefaultColor = null;
        /// <summary>
        /// Default Duration
        /// </summary>
        public const int DefaultDuration = 500;
        /// <summary>
        /// Default MaxWidth
        /// </summary>
        public const int DefaultMaxWidth = 0;
        /// <summary>
        /// Default Zoom
        /// </summary>
        public const bool DefaultZoom = true;
        /// <summary>
        /// Default Pan
        /// </summary>
        public const bool DefaultPan = true;

        /// <summary>
        /// Freeze color at the specified level of branches.
        /// 0 for no freezing at all.
        /// </summary>
        public readonly int ColorFreezeLevel;

        /// <summary>
        /// The maximum level of nodes to expand on initial render.
        /// -1 for expanding all levels.
        /// </summary>
        public readonly int InitialExpandLevel;

        /// <summary>
        /// A list of colors to use as the branch and circle colors for each node.
        /// If none is provided, d3.schemeCategory10 will be used.
        /// </summary>
        public readonly string[] Color;

        /// <summary>
        /// The animation duration when folding/unfolding a node. 
        /// </summary>
        public readonly int Duration;

        /// <summary>
        /// The max width of each node content. 0 for no limit.
        /// </summary>
        public readonly int MaxWidth;

        /// <summary>
        /// Whether to allow zooming the markmap.
        /// </summary>
        public readonly bool Zoom;

        /// <summary>
        /// Whether to allow panning the markmap.
        /// </summary>
        public readonly bool Pan;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="colorFreezeLevel">Freeze color at the specified level of branches. 0 for no freezing at all.</param>
        /// <param name="initialExpandLevel">The maximum level of nodes to expand on initial render. -1 for expanding all levels.</param>
        /// <param name="color">A list of colors to use as the branch and circle colors for each node. If none is provided, d3.schemeCategory10 will be used.</param>
        /// <param name="duration">The animation duration when folding/unfolding a node.</param>
        /// <param name="maxWidth">The max width of each node content. 0 for no limit.</param>
        /// <param name="zoom">Whether to allow zooming the markmap.</param>
        /// <param name="pan">Whether to allow panning the markmap.</param>
        public MarkmapOptions(int colorFreezeLevel, int initialExpandLevel, string[] color, int duration, int maxWidth, bool zoom, bool pan)
        {
            ColorFreezeLevel = colorFreezeLevel;
            InitialExpandLevel = initialExpandLevel;
            Color = color;
            Duration = duration;
            MaxWidth = maxWidth;
            Zoom = zoom;
            Pan = pan;
        }

        /// <summary>
        /// Default options
        /// </summary>
        public static MarkmapOptions Default => new (DefaultColorFreezeLevel, DefaultInitialExpandLevel, DefaultColor, DefaultDuration, DefaultMaxWidth, DefaultZoom, DefaultPan);
    }
}