using Terminal.Gui.App;
using Terminal.Gui.Input;
using VimHarness.Tui;

Application.SetDefaultKeyBinding(Command.Quit, Bind.All(Key.C.WithCtrl));

var app = new TerminalApp();
app.Run();
