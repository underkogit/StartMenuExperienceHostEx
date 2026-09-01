using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ExperienceHost.DataAccess.SQL.Structures;
using StartMenuExperienceHostEx.Services;

namespace StartMenuExperienceHostEx.Views.Controls;

public sealed class DraggableCanvas : Canvas
{
    private readonly WindowInputService _inputService;

    private Control? _draggedElement;
    private IPointer? _pointer;
    private Point _offset;

    public delegate void ElementReleasedHandler(
        ApplicationControl element,
        Point2D position);


    public event ElementReleasedHandler? ElementReleased;

    public DraggableCanvas()
    {
        _inputService = ServiceLocator.GetService<WindowInputService>();
    }

    protected override void OnAttachedToVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _inputService.Attach(this);
        _inputService.MousePositionChanged += OnMousePositionChanged;

        AddHandler(
            PointerPressedEvent,
            OnPointerPressed,
            RoutingStrategies.Bubble);

        AddHandler(
            PointerReleasedEvent,
            OnPointerReleased,
            RoutingStrategies.Bubble);

        AddHandler(
            PointerCaptureLostEvent,
            OnPointerCaptureLost,
            RoutingStrategies.Bubble);
    }

    protected override void OnDetachedFromVisualTree(
        VisualTreeAttachmentEventArgs e)
    {
        _inputService.MousePositionChanged -= OnMousePositionChanged;
        _inputService.Detach();

        base.OnDetachedFromVisualTree(e);
    }

    private void OnPointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);

        if (point.Properties.PointerUpdateKind !=
            PointerUpdateKind.LeftButtonPressed)
        {
            return;
        }

        if (e.Source is not Visual source)
            return;

        Visual element = source;

        while (element.GetVisualParent() is Visual parent &&
               parent != this)
        {
            element = parent;
        }

        if (element is not Control control ||
            control.GetVisualParent() != this)
        {
            return;
        }

        _draggedElement = control;
        _pointer = e.Pointer;

        var mousePosition = _inputService.MousePositionGrid;

        var left = Canvas.GetLeft(control);
        var top = Canvas.GetTop(control);

        if (double.IsNaN(left))
            left = 0;

        if (double.IsNaN(top))
            top = 0;

        _offset = new Point(
            mousePosition.X - left,
            mousePosition.Y - top);

        _pointer.Capture(this);

        e.Handled = true;
    }

    private void OnMousePositionChanged(
        Point2D mousePosition,
        Point2D mousePositionGrid)
    {
        if (_draggedElement is null ||
            _pointer is null)
        {
            return;
        }

        var x = mousePositionGrid.X - _offset.X;
        var y = mousePositionGrid.Y - _offset.Y;

        x = Math.Max(0, x);
        y = Math.Max(0, y);

        if (!double.IsNaN(Width))
        {
            x = Math.Min(
                x,
                Math.Max(0, Width - _draggedElement.Bounds.Width));
        }

        if (!double.IsNaN(Height))
        {
            y = Math.Min(
                y,
                Math.Max(0, Height - _draggedElement.Bounds.Height));
        }

        Canvas.SetLeft(_draggedElement, x);
        Canvas.SetTop(_draggedElement, y);
    }

    private void OnPointerReleased(
        object? sender,
        PointerReleasedEventArgs e)
    {
        if (_pointer is null ||
            e.Pointer != _pointer)
        {
            return;
        }

        if (_draggedElement is not null)
        {
            var position = new Point2D(
                (int)Canvas.GetLeft(_draggedElement),
                (int)Canvas.GetTop(_draggedElement));
            if (_draggedElement is ApplicationControl application)
                ElementReleased?.Invoke(
                    application,
                    position);
        }


        StopDragging();
        e.Handled = true;
    }

    private void OnPointerCaptureLost(
        object? sender,
        PointerCaptureLostEventArgs e)
    {
        StopDragging();
    }

    private void StopDragging()
    {
        _pointer?.Capture(null);

        _pointer = null;
        _draggedElement = null;
    }
}