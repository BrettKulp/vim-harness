using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Editor;

namespace VimHarness.Tui.Input;

public sealed class VimKeyHandler : IDisposable
{
    private readonly View _root;
    private readonly VimPane [] _panes;
    private bool _ctrlWPending;
    private bool _attached;
    private int _activePaneIndex;
    private Mode mode = Mode.NORMAL;

    public Mode Mode => mode;

    public VimKeyHandler (View root, params VimPane [] panes)
    {
        _root = root;
        _panes = panes;
        _activePaneIndex = 0;
    }

    public void Attach()
    {
        if (_attached) return;

        _root.KeyDown += OnKeyDown;

        foreach(VimPane pane in _panes)
        {
            pane.KeyDown += OnKeyDown;
            pane.KeyBindings.Add(Key.H, Command.Left);
            pane.KeyBindings.Add(Key.J, Command.Down);
            pane.KeyBindings.Add(Key.K, Command.Up);
            pane.KeyBindings.Add(Key.L, Command.Right);
            pane.KeyBindings.Remove(Key.C.WithCtrl);
            //pane.KeyBindings.Add(Key.W, MoveFocus(-1));
            //pane.KeyBindings.Add(Key.Q, MoveFocus(-1));
        }
        _attached = true;
    }

    private void OnKeyDown (object? sender, Key key)
    {

        if (key == Key.Esc)
        {
            _ctrlWPending = false;
            mode = Mode.NORMAL;
            key.Handled = true;
            if ( _panes[GetActivePaneIndex()].Pane == Pane.Prompt)
            {
                VimPane pane = _panes[_activePaneIndex];
                pane.Cursor = pane.Cursor with { Style = CursorStyle.SteadyBlock };
            }
            return;
        }

        if (key == Key.I)
        {
            _ctrlWPending = false;
            mode = Mode.INSERT;
            key.Handled = true;
            if ( _panes[GetActivePaneIndex()].Pane == Pane.Prompt)
            {
                VimPane pane = _panes[_activePaneIndex];
                pane.Cursor = pane.Cursor with { Style = CursorStyle.BlinkingBar };
            }
            return;
        }

        if(_ctrlWPending)
        {
            _ctrlWPending = false;
            Key plain = key.NoCtrl;

            if (plain == Key.K)
            {
                MoveFocus (-1);
                key.Handled = true;
                return;
            }
            if (plain == Key.J)
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

    public void Focus (Pane pane)
    {
        for (var i = 0; i < _panes.Length; i++)
        {
            if (_panes [i].Pane == pane)
            {
                _activePaneIndex = i;
                _panes [i].SetFocus ();
                return;
            }
        }

        throw new ArgumentOutOfRangeException (nameof (pane), pane, "Pane is not registered.");
    }

    private void MoveFocus (int delta)
    {
        _activePaneIndex = GetActivePaneIndex ();

        int next = (_activePaneIndex + delta + _panes.Length) % _panes.Length;
        _activePaneIndex = next;
        _panes [next].SetFocus ();
    }

    private int GetActivePaneIndex ()
    {
        for (var i = 0; i < _panes.Length; i++)
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

        foreach (VimPane pane in _panes)
        {
            pane.KeyDown -= OnKeyDown;
        }

        _attached = false;
    }
}
