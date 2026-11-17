
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Aura;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement(isAndroidForegroundServiceEnabled: false)
            .ConfigureFonts(fonts =>
            {
                // Fonte dos títulos
                fonts.AddFont(
                    "BodoniModa-VariableFont_opsz,wght.ttf",
                    "BodoniModa");

                fonts.AddFont(
                    "BodoniModa-Italic-VariableFont_opsz,wght.ttf",
                    "BodoniModaItalic");

                // Fonte dos textos
                fonts.AddFont(
                    "DMSans-VariableFont_opsz,wght.ttf",
                    "DMSans");

                fonts.AddFont(
                    "DMSans-Italic-VariableFont_opsz,wght.ttf",
                    "DMSansItalic");

                // Fonte dos detalhes e números
                fonts.AddFont("SpaceMono-Regular.ttf", "SpaceMono");
                fonts.AddFont("SpaceMono-Bold.ttf", "SpaceMonoBold");

                // Fontes padrão
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
