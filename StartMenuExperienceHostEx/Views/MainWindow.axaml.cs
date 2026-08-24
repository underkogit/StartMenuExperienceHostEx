using Avalonia.Controls;
using Avalonia.Interactivity;
using StartMenuExperienceHostEx.ViewModels;

namespace StartMenuExperienceHostEx.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
    }

    private void WinOnLoaded(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is MainWindowViewModel vm)
        {
            
        }
    }
}