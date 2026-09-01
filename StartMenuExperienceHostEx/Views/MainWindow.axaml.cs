using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using StartMenuExperienceHostEx.Helper;
using StartMenuExperienceHostEx.Services;
using StartMenuExperienceHostEx.ViewModels;

namespace StartMenuExperienceHostEx.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ServiceLocator.GetService<MainWindowViewModel>().SetMainWindow(this);
    }

    private void Window_OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Formats.Contains(DataFormat.File)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private async void Window_OnDrop(
        object? sender,
        DragEventArgs e)
    {
        if (!e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            e.Handled = true;
            return;
        }

        var paths =
            from file in e.DataTransfer.TryGetFiles()
                         ?? Enumerable.Empty<IStorageItem>()
            let path = file.Path.LocalPath
            where File.Exists(path) || Directory.Exists(path)
            select path;

        if (DataContext is MainWindowViewModel viewModel)
        {
            Topmost = true;

            foreach (var path in paths)
            {
                await viewModel.AddApplicationAsync(path);
            }
        }

        e.Handled = true;
    }


    private void WinOnClosing(object? sender, WindowClosingEventArgs e)
    {
        this.Dispose();
    }

    private void WinOnClosed(object? sender, EventArgs e)
    {
        this.Dispose();
    }

    private void Dispose()
    {
        if (DataContext is IDisposable disposable)

        {
            disposable.Dispose();
        }
    }
}