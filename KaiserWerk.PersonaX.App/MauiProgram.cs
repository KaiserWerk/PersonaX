using CommunityToolkit.Maui;
using KaiserWerk.PersonaX.App.ViewModels;
using KaiserWerk.PersonaX.App.Views;
using KaiserWerk.PersonaX.Core.Services;
using KaiserWerk.PersonaX.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using System.IO;

namespace KaiserWerk.PersonaX.App;

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
            })
            .RegisterServices()
            .RegisterViewModels()
            .RegisterViews()
        ;



#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        var dbFilename = "persona_x.db";
        var folder = Path.Join(FileSystem.Current.AppDataDirectory, "PersonaX");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        
        builder.Services.AddDbContext<AppDbContext>(opts =>
        {
            opts.UseSqlite($"Data Source={Path.Join(folder, dbFilename)}");
        });
        builder.Services.AddSingleton<IDataService, DummyDataService>();

        return builder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AllPersonsViewModel>();
        builder.Services.AddSingleton<AddPersonViewModel>();
        builder.Services.AddSingleton<EditPersonViewModel>();
        builder.Services.AddSingleton<CaptureMediaViewModel>();
        builder.Services.AddSingleton<UserSettingsViewModel>();

        return builder;
    }

    public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AllPersonsPage>();
        builder.Services.AddSingleton<CaptureMediaPage>();
        builder.Services.AddSingleton<UserSettingsPage>();

        return builder;
    }
}

