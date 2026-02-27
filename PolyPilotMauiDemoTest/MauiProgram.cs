using Microsoft.Extensions.Logging;
#if DEBUG
using MauiDevFlow.Agent;
#endif

namespace PolyPilotMauiDemoTest
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

            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
        builder.AddMauiDevFlowAgent(options =>
        {
            options.Port = 9223;
        });
#endif

            return builder.Build();
        }
    }
}
