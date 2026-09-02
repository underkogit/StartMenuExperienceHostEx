using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StartMenuExperienceHostEx.Helper;
using StartMenuExperienceHostEx.Services;
using StartMenuExperienceHostEx.ViewModels;
using StartMenuExperienceHostEx.Views;

namespace StartMenuExperienceHostEx;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceLocator.Initialize();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = ServiceLocator.GetService<MainWindow>();
            mainWindow.Hide();
                //desktop.MainWindow = mainWindow;
            
        }

        base.OnFrameworkInitializationCompleted();
    }
}