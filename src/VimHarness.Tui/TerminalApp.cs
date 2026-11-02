namespace VimHarness.Tui;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Editor;
using VimHarness.Tui.Input;

// notes
// may want Code for the chat history or just code files

// Dialog for approvales

// file dialog for switching directories where the agent is running

// list view for messages


// 


public class TerminalApp
{
    public void Run()
    {
        using IApplication app = Application.Create();
        app.Init();

        using Window window = new()
        {
            Title = "VimHarness a Harness for AI Harnesses that adds Vim Keybinds"
        };

        // Chat history 
        FrameView chatFrame = new()
        {
            Title = "Chat History",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(7),
            CanFocus = true 
        };

        Editor chatHistory = new()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            Text = "Welcom to VimHarness!\nThis is a termnal Wrapper intended for AI harnesses that adds VIM"
        };

        chatFrame.Add(chatHistory);

        FrameView promptFrame = new()
        {
            Title = "Prompt",
            X = 0,
            Y = Pos.Bottom(chatFrame),
            Width = Dim.Fill(),
            Height = 7,
            CanFocus = true 
        };

        Editor prompt = new()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        promptFrame.Add(prompt);

        window.Add(chatFrame, promptFrame);

        using VimKeyHandler vim = new (window, chatHistory, prompt);
        vim.Attach();
        prompt.SetFocus();

        app.Run(window);
    }
}

