using Microsoft.Extensions.Logging;
using OpenApi.Tools.Core;
using OpenApi.Tools.MarkmapViewer.Services;
using OpenApi.Tools.MarkmapViewer.ViewModels;
using OpenApi.Tools.MarkmapViewer.Views;

namespace OpenApi.Tools.MarkmapViewer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<IOpenApiTools, OpenApiTools>();
            builder.Services.AddSingleton<MarkmapViewerView>();
            builder.Services.AddSingleton<MarkmapViewerViewModel>();
            builder.Services.AddSingleton<IAlertService, AlertService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}