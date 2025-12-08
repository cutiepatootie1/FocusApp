using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace FocusApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialSymbolsOutlined-Regular.ttf", "MaterialSymbols");
                });
            // Services
            builder.Services.AddSingleton<Services.JsonDataService>();
            // ViewModels
            builder.Services.AddTransient<ViewModels.DashboardViewModel>();
            // Views
            builder.Services.AddTransient<Views.DashboardPage>();
            builder.Services.AddTransient<Views.Components.FocusToggleView>();

#if WINDOWS
            builder.Services.AddSingleton<Services.Interface.IFocusService, Platforms.Windows.FocusService>();
            builder.Services.AddSingleton<Services.Interface.IAppListService, Platforms.Windows.AppListService>();
#elif ANDROID
            builder.Services.AddSingleton<Services.Interface.IFocusService, Platforms.Android.FocusService>();
            #endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
