using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ExperienceHost.DataAccess.SQL.Structures;

namespace StartMenuExperienceHostEx.Services;

public sealed class WindowInputService : IDisposable
{
    private Window? _window;
    private Point _mousePosition;

    public Point2D MousePosition { get; private set; } = new();

    public Point2D MousePositionGrid { get; private set; } = new();

    public int GridSize { get; set; } = 10;

    public event Action<Point2D, Point2D>? MousePositionChanged;

    public void Attach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (ReferenceEquals(_window, window))
            return;

        Detach();

        _window = window;
        _window.PointerMoved += OnPointerMoved;
    }

    public void Detach()
    {
        if (_window is null)
            return;

        _window.PointerMoved -= OnPointerMoved;
        _window = null;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_window is null)
            return;

        if (GridSize <= 0)
            throw new InvalidOperationException(
                "GridSize > 0");

        var position = e.GetPosition(_window);
        if (position == _mousePosition)
            return;
        _mousePosition = position;
        MousePosition = new Point2D(
            (int)position.X,
            (int)position.Y);
        MousePositionGrid = new Point2D(
            (int)(position.X / GridSize) * GridSize,
            (int)(position.Y / GridSize) * GridSize);
        MousePositionChanged?.Invoke(
            MousePosition,
            MousePositionGrid);
    }

    public void Dispose()
    {
        Detach();
    }
}