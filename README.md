# VimHarness

VimHarness is a terminal UI wrapper that brings Vim motions and keybindings to the terminal. It is also intended to wrap AI coding harnesses, giving each of them Vim-style controls inside the same application.

The idea is to have one terminal application where you can drive multiple AI harnesses with familiar Vim behavior instead of learning a different set of keybindings for every tool.

## Inspiration

- [opencode-vim](https://github.com/leohenon/opencode-vim)
- [Vim](https://github.com/vim/vim)
- [Neovim](https://github.com/neovim/neovim)
- [hash](https://github.com/tfcace/hash)

## Roadmap

- Build out a solid set of Vim motions and keybindings
- Add Copilot support via the Agent Client Protocol (ACP)
- Add Codex support
- Add any other AI harness that speaks ACP

## Project layout

- `src/VimHarness.App` — application entry point (`vh`)
- `src/VimHarness.Tui` — Terminal.Gui based UI and Vim key handling
- `src/VimHarness.Acp` — ACP integration

## Building and running

Requires the .NET 10 SDK.

```sh
dotnet build
dotnet run --project src/VimHarness.App
```

## Status

Early development. The UI and a small set of Vim bindings are in place; AI harness integration is still being built out.

## License

MIT. See [LICENSE](LICENSE).

