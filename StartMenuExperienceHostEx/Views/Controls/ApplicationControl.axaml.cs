using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;

namespace StartMenuExperienceHostEx.Views.Controls;

public partial class ApplicationControl : UserControl
{
    public static readonly StyledProperty<string?> ApplicationNameProperty =
        AvaloniaProperty.Register<ApplicationControl, string?>(
            nameof(ApplicationName));

    public static readonly StyledProperty<string?> DiskProperty =
        AvaloniaProperty.Register<ApplicationControl, string?>(
            nameof(Disk));

    public static readonly StyledProperty<string?> FilePathProperty =
        AvaloniaProperty.Register<ApplicationControl, string?>(
            nameof(FilePath));

    public static readonly StyledProperty<string?> FullFilePathProperty =
        AvaloniaProperty.Register<ApplicationControl, string?>(
            nameof(FullFilePath));

    public static readonly StyledProperty<Bitmap?> ImageProperty =
        AvaloniaProperty.Register<ApplicationControl, Bitmap?>(
            nameof(Image));

    public static readonly RoutedEvent<DragEventArgs> DragOverEvent =
        RoutedEvent.Register<ApplicationControl, DragEventArgs>(
            nameof(DragOver), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DragEventArgs> DropEvent =
        RoutedEvent.Register<ApplicationControl, DragEventArgs>(
            nameof(Drop), RoutingStrategies.Bubble);

    public string? ApplicationName
    {
        get => GetValue(ApplicationNameProperty);
        set => SetValue(ApplicationNameProperty, value);
    }

    public string? Disk
    {
        get => GetValue(DiskProperty);
        set => SetValue(DiskProperty, value);
    }

    public string? FilePath
    {
        get => GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    public string? FullFilePath
    {
        get => GetValue(FullFilePathProperty);
        set => SetValue(FullFilePathProperty, value);
    }

    public Bitmap? Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    public event EventHandler<DragEventArgs> DragOver
    {
        add => AddHandler(DragOverEvent, value);
        remove => RemoveHandler(DragOverEvent, value);
    }

    public event EventHandler<DragEventArgs> Drop
    {
        add => AddHandler(DropEvent, value);
        remove => RemoveHandler(DropEvent, value);
    }

    public ApplicationControl()
    {
        InitializeComponent();
    }
}