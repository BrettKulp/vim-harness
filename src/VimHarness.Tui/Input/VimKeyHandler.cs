using Terminal.Gui.Drivers;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Editor;
using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace VimHarness.Tui.Input;

public class BashRunner {
    public static async Task<(string Output, string Error, int ExitCode)> RunCommandAsync(string command)
    {

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo};
        processStartInfo.ArgumentList.Add("-c");
        processStartInfo.ArgumentList.Add(command);
        process.Start();

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (await outputTask, await errorTask, process.ExitCode);
    }
}

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

    private void InvokeCommand(VimPane pane, Command command)
    {
        pane.InvokeCommand(command);
    }


    private async void OnKeyDown (object? sender, Key key)
    {
        if (sender is not VimPane pane) return;

        if (key == Key.H)
        {
            if (pane.Mode == Mode.NORMAL)
            {
                InvokeCommand(pane, Command.Left);
                SetKeyHandled();
                return;

            }
        }

        if (key == Key.J)
        {
            if (pane.Mode == Mode.NORMAL)
            {
                // if it is normal we move instead of write
                InvokeCommand(pane,Command.Down);
                SetKeyHandled();
                return;

            }
        }

        if (key == Key.K)
        {
            if (pane.Mode == Mode.NORMAL)
            {
                // if it is normal we move instead of write
                InvokeCommand(pane,Command.Up);
                SetKeyHandled();
                return;

            }
        }

        if (key == Key.L)
        {
            if (pane.Mode == Mode.NORMAL)
            {
                // if it is normal we move instead of write
                InvokeCommand(pane,Command.Right);
                SetKeyHandled();
                return;

            }
            if (pane.Mode == Mode.INSERT)
            {
                // we do not do the movment
            }
        }

        if (key == Key.X) 
        {
            if (pane.Mode != Mode.INSERT)
            {
                _ctrlWPending = false;
                key.Handled = true;
                InvokeCommand(pane,Command.DeleteCharRight);
                SetKeyHandled();
            }
        }

        if (key == Key.Esc)
        {
            _ctrlWPending = false;
            pane.Mode = Mode.NORMAL;
            pane.ClearSelection();
                SetKeyHandled();
            if ( pane.Pane == Pane.Prompt)
            {
                pane.Cursor = pane.Cursor with { Style = CursorStyle.SteadyBlock };
            }
            return;
        }

        if (key == Key.I)
        {
            if (pane.Mode != Mode.INSERT)
            {
                _ctrlWPending = false;
                if ( pane.Pane == Pane.Prompt )
                {
                pane.ClearSelection();
                pane.Mode = Mode.INSERT;
                SetKeyHandled();
                pane.Cursor = pane.Cursor with { Style = CursorStyle.BlinkingBar };
                return;
                }
            }
        }

        if(_ctrlWPending && pane.Mode != Mode.INSERT)
        {
            _ctrlWPending = false;
            Key plain = key.NoCtrl;

            if (plain == Key.K)
            {
                MoveFocus (-1);
                SetKeyHandled();
                return;
            }
            if (plain == Key.J)
            {
                MoveFocus (+1);
                SetKeyHandled();
                return;
            }

            return;
        }

        if (key == Key.Enter && pane.Mode == Mode.NORMAL && pane.Pane == Pane.Prompt)
        {
            string text = pane.Document.Text;
            var (output, error, exitCode) = await BashRunner.RunCommandAsync(text);

            if (exitCode == 0)
            {
                Console.WriteLine($"Output:\n{output}");
            }
            else
            {
            Console.WriteLine($"Error (Code {exitCode}):\n{error}");
            }
            
        }


        if (key == Key.W.WithCtrl)
        {
            _ctrlWPending = true;
            SetKeyHandled();
        }


        if (key == Key.V && pane.Mode != Mode.INSERT)
        {
            // enter visual mode
            if (pane.Mode == Mode.INSERT) 
            {
                pane.Mode = Mode.VISUAL;
                //Console.WriteLine(pane.CaretOffset);
                _ctrlWPending = false;
                SetKeyHandled();
            }
        }

        if (key == Key.V.WithShift)
        {
            pane.SelectAll();
            pane.Mode = Mode.VISUAL_LINE;
            SetKeyHandled();
        }

        // did not hit any
        if (pane.Mode != Mode.INSERT && !_ctrlWPending && (key != Key.C.WithCtrl))
        {
            _ctrlWPending = false;
            SetKeyHandled();
            return;
        }

        void SetKeyHandled()
        {
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
