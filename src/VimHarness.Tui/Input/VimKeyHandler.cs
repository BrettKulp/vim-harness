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

    public VimKeyHandler (View root, params VimPane [] panes)
    {
        _root = root;
        _panes = panes;
        _activePaneIndex = 0;
    }

    public void Attach()
    {
        if (_attached) return;


        foreach(VimPane pane in _panes)
        {
            pane.KeyDown += OnKeyDown;
            pane.KeyBindings.Remove(Key.C.WithCtrl);
        }
        _attached = true;
    }


    private void OnKeyDown (object? sender, Key key)
    {
        if (sender is not VimPane pane) return;
        //VimPane pane = _panes[GetActivePaneIndex()];

        if (key == Key.H)
        {
            if (pane.Mode == Mode.NORMAL)
            {
                pane.InvokeCommand(Command.Left);
                key.Handled = true;
                return;

            }
        }

        if (key == Key.J)
        {
            if (pane.Mode == Mode.NORMAL)
            {
                // if it is normal we move instead of write
                pane.InvokeCommand(Command.Down);
                key.Handled = true;
                return;

            }
        }

        if (key == Key.K)
        {
            if (pane.Mode == Mode.NORMAL)
            {
                // if it is normal we move instead of write
                pane.InvokeCommand(Command.Up);
                key.Handled = true;
                return;

            }
        }



        if (key == Key.L)
        {
            if (pane.Mode == Mode.NORMAL)
            {
                // if it is normal we move instead of write
                pane.InvokeCommand(Command.Right);
                key.Handled = true;
                return;

            }
            if (pane.Mode == Mode.INSERT)
            {
                // we do not do the movment
            }
        }

        if (key == Key.Esc)
        {
            _ctrlWPending = false;
            pane.Mode = Mode.NORMAL;
            key.Handled = true;
            if ( pane.Pane == Pane.Prompt)
            {
                pane.Cursor = pane.Cursor with { Style = CursorStyle.SteadyBlock };
            }
            return;
        }

        if (key == Key.I)
        {
            _ctrlWPending = false;
            pane.Mode = Mode.INSERT;
            key.Handled = true;
            if ( pane.Pane == Pane.Prompt)
            {
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

        _attached = false;
    }
}
