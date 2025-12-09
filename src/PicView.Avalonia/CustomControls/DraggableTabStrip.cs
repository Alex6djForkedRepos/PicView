using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.CustomControls;

[PseudoClasses(DraggingPseudoClass, DetachingPseudoClass)]
public class DraggableTabControl : TabControl
{
    private const double DragThreshold = 4.0;
    private const double DetachThreshold = 50.0; // Vertical distance to trigger detachment
    private const string DraggingPseudoClass = ":dragging";
    private const string DetachingPseudoClass = ":detaching";

    private readonly Dictionary<TabItem, double> _originalX = new();
    private int _currentTargetIndex = -1;

    private double _draggedTabStartX;
    private double _draggedTabWidth;
    private bool _isDetaching;
    private bool _isDragging;

    private double _pointerOffsetWithinTab;
    private TabItem? _pressedContainer;
    private Point _pressedPoint;
    private int _sourceIndex = -1;

    protected override Type StyleKeyOverride => typeof(DraggableTabControl);

    public event EventHandler<TabDetachEventArgs>? TabDetached;
    public event EventHandler<TabCreatedEventArgs>? TabCreated;

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is not TabItem tsi)
        {
            return;
        }
        
        // Check if this is a newly added item (not just being recycled)
        if (item is TabViewModel { IsClosing: false })
        {
            // Raise the TabCreated event
            TabCreated?.Invoke(tsi, new TabCreatedEventArgs(item, index));
        }

        tsi.AddHandler(PointerPressedEvent, OnItemPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tsi.AddHandler(PointerMovedEvent, OnItemPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tsi.AddHandler(PointerReleasedEvent, OnItemPointerReleased,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        tsi.PointerCaptureLost += OnPointerCaptureLost;
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is TabItem tsi)
        {
            tsi.RemoveHandler(PointerPressedEvent, OnItemPointerPressed);
            tsi.RemoveHandler(PointerMovedEvent, OnItemPointerMoved);
            tsi.RemoveHandler(PointerReleasedEvent, OnItemPointerReleased);
            tsi.PointerCaptureLost -= OnPointerCaptureLost;

            if (tsi.DataContext is TabViewModel { IsClosing: true })
            {
                tsi.RenderTransform = null;

                if (_pressedContainer == tsi)
                {
                    EndDrag();
                }
            }
        }

        base.ClearContainerForItemOverride(container);
    }

    private void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        if (!props.IsLeftButtonPressed)
        {
            return;
        }

        if (sender is not TabItem tsi)
        {
            return;
        }

        _pressedContainer = tsi;
        _pressedPoint = e.GetPosition(this);
        _isDragging = false;
        _isDetaching = false;
        ItemFromContainer(tsi);
        _sourceIndex = IndexFromContainer(tsi);
        _currentTargetIndex = _sourceIndex;

        // Cache positions
        _originalX.Clear();
        foreach (var c in GetRealizedContainers())
        {
            if (c is TabItem t)
            {
                _originalX[t] = t.Bounds.X;
            }
        }

        _draggedTabStartX = tsi.Bounds.X;
        _draggedTabWidth = tsi.Bounds.Width;
        _pointerOffsetWithinTab = _pressedPoint.X - _draggedTabStartX;
    }

private void OnItemPointerMoved(object? sender, PointerEventArgs e)
{
    if (_pressedContainer == null)
    {
        return;
    }

    var pos = e.GetPosition(this);
    var deltaX = pos.X - _pressedPoint.X;
    var deltaY = pos.Y - _pressedPoint.Y;
    var absVerticalDelta = Math.Abs(deltaY);

    // 1. Start dragging if threshold crossed
    if (!_isDragging)
    {
        if (Math.Abs(deltaX) > DragThreshold || absVerticalDelta > DragThreshold)
        {
            _isDragging = true;
            e.Pointer.Capture(_pressedContainer);
            PseudoClasses.Set(DraggingPseudoClass, true);
            _pressedContainer.ZIndex = 1000;
        }
        else
        {
            return;
        }
    }

    // 2. State Switching Logic
    
    // Case A: Transition from Attached -> Detaching
    if (!_isDetaching && absVerticalDelta > DetachThreshold)
    {
        _isDetaching = true;

        PseudoClasses.Set(DetachingPseudoClass, true);
        PseudoClasses.Set(DraggingPseudoClass, false);

        // Visual feedback for detachment
        _pressedContainer.Opacity = 0.7;
    }
    // Case B: Transition from Detaching -> Attached (THE FIX)
    else if (_isDetaching && absVerticalDelta <= DetachThreshold)
    {
        _isDetaching = false;

        PseudoClasses.Set(DetachingPseudoClass, false);
        PseudoClasses.Set(DraggingPseudoClass, true);

        // Restore visual state
        _pressedContainer.Opacity = 1.0;
        
        // We do NOT return here. We let the code fall through 
        // to the "Normal horizontal drag logic" below, 
        // which will snap the Y transform back to 0.
    }

    // 3. Movement Logic

    // If we are definitely detaching, move freely in X and Y
    if (_isDetaching)
    {
        var dragLeft = pos.X - _pointerOffsetWithinTab;
        _pressedContainer.RenderTransform = new TranslateTransform(
            dragLeft - _draggedTabStartX,
            deltaY
        );
        e.Handled = true;
        return;
    }

    // Normal horizontal drag logic (Reordering)
    var dragLeftPos = pos.X - _pointerOffsetWithinTab;
    var dragCenter = dragLeftPos + _draggedTabWidth / 2;

    var realized = GetRealizedContainers().OfType<TabItem>().ToArray();
    if (realized.Length == 0)
    {
        return;
    }

    var newTargetIndex = _sourceIndex;

    for (var i = 0; i < realized.Length; i++)
    {
        var tab = realized[i];
        var tabX = _originalX.TryGetValue(tab, out var val) ? val : tab.Bounds.X;
        var center = tabX + tab.Bounds.Width / 2;

        if (dragCenter > center)
        {
            newTargetIndex = i;
        }
    }

    _currentTargetIndex = newTargetIndex;

    // Visual updates for reordering
    for (var i = 0; i < realized.Length; i++)
    {
        var tab = realized[i];
        var startX = _originalX.TryGetValue(tab, out var val) ? val : tab.Bounds.X;
        double offset = 0;

        if (tab == _pressedContainer)
        {
            offset = dragLeftPos - startX;
        }
        else
        {
            // Logic: If the item needs to move left or right to fill the gap
            if (_currentTargetIndex > _sourceIndex) // Dragging Right
            {
                if (i > _sourceIndex && i <= _currentTargetIndex)
                {
                    offset = -_draggedTabWidth;
                }
            }
            else if (_currentTargetIndex < _sourceIndex) // Dragging Left
            {
                if (i >= _currentTargetIndex && i < _sourceIndex)
                {
                    offset = _draggedTabWidth;
                }
            }
        }

        // IMPORTANT: The Y value is 0 here, which ensures that if we 
        // just came back from being detached, the tab snaps back to the strip row.
        tab.RenderTransform = new TranslateTransform(offset, 0);
    }

    e.Handled = true;
}

    private void OnItemPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_pressedContainer == null)
        {
            return;
        }

        // Handle detachment
        if (_isDetaching)
        {
            var item = ItemFromContainer(_pressedContainer);
            if (item != null)
            {
                var screenPos = _pressedContainer.PointToScreen(e.GetPosition(_pressedContainer));

                // Remove item from current collection
                var list = ItemsSource as IList ?? Items;
                if (list != null && _sourceIndex >= 0 && _sourceIndex < list.Count)
                {
                    list.RemoveAt(_sourceIndex);
                }

                // Raise event for creating new window
                TabDetached?.Invoke(sender, new TabDetachEventArgs(item, screenPos));
            }

            EndDrag();
            e.Handled = true;
            return;
        }

        // Normal reorder logic
        if (_isDragging && _currentTargetIndex >= 0)
        {
            TryMoveItem(_sourceIndex, _currentTargetIndex);
            SelectedIndex = _currentTargetIndex;
        }

        EndDrag();
        e.Handled = true;
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        EndDrag();
    }

    private void EndDrag()
    {
        if (_pressedContainer != null)
        {
            PseudoClasses.Remove(DraggingPseudoClass);
            PseudoClasses.Remove(DetachingPseudoClass);

            _pressedContainer.Opacity = 1.0;
            _pressedContainer.ZIndex = 0;
        }

        // Iterate ALL containers and reset their transforms.
        // After the data move, layout will arrange them correctly. 
        // If we leave transforms, they will be visually offset from their new correct positions.
        foreach (var c in GetRealizedContainers())
        {
            c.RenderTransform = null;
        }

        _pressedContainer = null;
        _isDragging = false;
        _isDetaching = false;
        _sourceIndex = -1;
        _currentTargetIndex = -1;
        _originalX.Clear();
    }

    private bool TryMoveItem(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex)
        {
            return true;
        }

        var list = ItemsSource as IList ?? Items;
        if (list == null || oldIndex < 0 || oldIndex >= list.Count)
        {
            return false;
        }

        if (newIndex < 0)
        {
            newIndex = 0;
        }

        if (newIndex >= list.Count)
        {
            newIndex = list.Count - 1;
        }

        try
        {
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

// Event args for tab detachment
public class TabDetachEventArgs(object detachedItem, PixelPoint screenPosition) : EventArgs
{
    public object DetachedItem { get; } = detachedItem;
    public PixelPoint ScreenPosition { get; } = screenPosition;
}

public class TabCreatedEventArgs(object createdItem, int index) : EventArgs
{
    public object CreatedItem { get; } = createdItem;
    public int Index { get; } = index;
}