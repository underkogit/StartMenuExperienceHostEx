using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ExperienceHost.DataAccess.SQL.Structures;

namespace StartMenuExperienceHostEx.Services;

public sealed class WindowInputService : IDisposable
{
    private Canvas? _userControl;
    private Point _mousePosition;

    public Point2D MousePosition { get; private set; } = new();

    public Point2D MousePositionGrid { get; private set; } = new();

    public int GridSize { get; set; } = 10;

    public event Action<Point2D, Point2D>? MousePositionChanged;

    public void Attach(Canvas window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (ReferenceEquals(_userControl, window))
            return;

        Detach();

        _userControl = window;
        _userControl.PointerMoved += OnPointerMoved;
    }

    public void Detach()
    {
        if (_userControl is null)
            return;

        _userControl.PointerMoved -= OnPointerMoved;
        _userControl = null;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_userControl is null)
            return;

        if (GridSize <= 0)
            throw new InvalidOperationException(
                "GridSize > 0");

        var position = e.GetPosition(_userControl);
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