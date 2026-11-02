using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Editor;

namespace VimHarness.Tui.Input;

public sealed class VimKeyHandler : IDisposable
{
    private readonly View _root;
    private readonly IReadOnlyList<Editor> _panes;
    private bool _ctrlWPending;
    private bool _attached;
    private int _activePaneIndex;

    public VimKeyHandler (View root, params Editor [] panes)
    {
        _root = root;
        _panes = panes;
        _activePaneIndex = 0;
    }

    public void Attach()
    {
        if (_attached) return;

        _root.KeyDown += OnKeyDown;

        foreach(Editor pane in _panes)
        {
            pane.KeyDown += OnKeyDown;
            pane.KeyBindings.Add(Key.H, Command.Left);
            pane.KeyBindings.Add(Key.J, Command.Down);
            pane.KeyBindings.Add(Key.K, Command.Up);
            pane.KeyBindings.Add(Key.L, Command.Right);
        }
        _attached = true;
    }

    private void OnKeyDown (object? sender, Key key)
    {
        if(_ctrlWPending)
        {
            _ctrlWPending = false;
            if (key == Key.K) 
            {
                MoveFocus (-1); 
                key.Handled = true; 
                return;
            }
            if (key == Key.J)
            {
                MoveFocus (+1);
                key.Handled = true;
                return;
            }

            return;
        }

        if (key == Key.W.WithCtrl)
        {
            _ctrlWPending = true;
            key.Handled = true;
        }
    }

    private void MoveFocus (int delta)
    {
        _activePaneIndex = GetActivePaneIndex ();

        int next = (_activePaneIndex + delta + _panes.Count) % _panes.Count;
        _activePaneIndex = next;
        _panes [next].SetFocus ();
    }

    private int GetActivePaneIndex ()
    {
        for (var i = 0; i < _panes.Count; i++)
        {
            if (_panes [i].HasFocus)
            {
                return i;
            }
        }

        return _activePaneIndex;
    }

    public void Dispose()
    {
        if (!_attached) return;
        _root.KeyDown -= OnKeyDown;

        foreach (Editor pane in _panes)
        {
            pane.KeyDown -= OnKeyDown;
        }

        _attached = false;
    }
}
