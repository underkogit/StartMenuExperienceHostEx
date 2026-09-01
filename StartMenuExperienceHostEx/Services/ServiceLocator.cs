using System;
using System.IO;
using ExperienceHost.DataAccess.SQL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StartMenuExperienceHostEx.ViewModels;
using StartMenuExperienceHostEx.Views;

namespace StartMenuExperienceHostEx.Services;

public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    public static void Initialize()
    {
        var services = new ServiceCollection();

        services.AddDbContext<SqliteDbContext>(options =>
            options.UseSqlite($"Data Source={GetDbPath()}"));


        services.AddSingleton<WindowInputService>();
        services.AddSingleton<KeyboardShortcutService>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public static T GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>()
               ?? throw new InvalidOperationException($"Service {typeof(T)} not registered");
    }

    private static string GetDbPath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "ExperienceHost.db");
    }
}