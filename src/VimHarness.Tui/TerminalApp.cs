namespace VimHarness.Tui;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Editor;

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
            Height = Dim.Fill(7)
        };

        Editor chatHistory = new()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            Text = "Welcom to VimHarness!"
        };

        chatFrame.Add(chatHistory);

        FrameView promptFrame = new()
        {
            Title = "Prompt",
            X = 0,
            Y = Pos.Bottom(chatFrame),
            Width = Dim.Fill(),
            Height = 7
        };

        Editor prompt = new()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        promptFrame.Add(prompt);

        window.Add(chatFrame, promptFrame);
        app.Run(window);
    }
}

