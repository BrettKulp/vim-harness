namespace VimHarness.Tui;
using System.Drawing;
using Terminal.Gui.App;
using Terminal.Gui.Drivers;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Editor;
using VimHarness.Tui.Input;
using System.Text;

public class TerminalApp
{
    public void Run()
    {

        TextReader? pipedReader = Console.IsInputRedirected ? Console.In : null;

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
            CanFocus = true,
        };

        VimPane chatHistory = new()
        {
            Pane = Pane.ChatHistory,
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
            CanFocus = true,
        };

        VimPane prompt = new()
        {
            Pane = Pane.Prompt,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        Point screenPosition = prompt.ViewportToScreen(new Point(0, 0));
        promptFrame.Add(prompt);

        window.Add(chatFrame, promptFrame);

        using VimKeyHandler vim = new (window, prompt, chatHistory);
        vim.Attach();
        vim.Focus(Pane.Prompt);

        if (pipedReader is not null)
        {
            window.Initialized += (_, _) => 
            {
                _ = Task.Run(async () => 
                        {
                            var buffer = new StringBuilder();
                            string? line;

                            while ((line = await pipedReader.ReadLineAsync()) is not null)
                            {
                                buffer.AppendLine(line);

                                // batch updates
                                if (pipedReader.Peek() == -1 || buffer.Length > 4096)
                                {
                                    string chunk = buffer.ToString();
                                    buffer.Clear();

                                    app.Invoke(() =>
                                            {
                                                chatHistory.Text += chunk;
                                                chatHistory.CaretOffset = chatHistory.Text.Length;
                                            });
                                }
                            }
                        }
                        );
            };
        }

        app.Run(window);
    }
}

