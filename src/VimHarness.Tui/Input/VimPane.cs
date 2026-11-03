using Terminal.Gui.Editor;

namespace VimHarness.Tui.Input;

public sealed class VimPane : Editor
{
    public required Pane Pane { get; init; }
}
