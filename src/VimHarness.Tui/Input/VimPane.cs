using Terminal.Gui.Editor;

namespace VimHarness.Tui.Input;

public sealed class VimPane : Editor
{
    public required Pane Pane { get; init; }
    public VimHarness.Tui.Input.Mode Mode { get; set; } = Mode.NORMAL;
}
