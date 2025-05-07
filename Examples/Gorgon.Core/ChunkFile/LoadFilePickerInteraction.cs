// Gorgon.
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 28, 2025 10:35:04 PM
//

using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Gorgon.IO;
using ReactiveUI;

namespace Gorgon.Examples;

/// <summary>
/// An interaction that allows creationg of a native file picker from a view/view model.
/// </summary>
internal static class LoadFilePickerInteraction
{
    // The interaction for the view and view model.
    private static readonly Interaction<Unit, string> _interaction = new();

    /// <summary>
    /// Function to retrieve the file.
    /// </summary>
    /// <param name="topLevel">The top level control.</param>
    /// <returns>The file name if one is selected, or an empty string if canceled.</returns>
    private static async Task<string> GetFileAsync(TopLevel topLevel)
    {
        string defaultPath = Path.GetFullPath(Path.Combine(ExampleConfig.Default.ResourceLocation, "Chunk Files")).FormatDirectory(Path.DirectorySeparatorChar);

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open a Gorgon Chunk file...",
            AllowMultiple = false,
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(defaultPath)
        });

        if (files.Count == 0)
        {
            return string.Empty;
        }

        return files[0].Path.LocalPath;
    }

    /// <summary>
    /// Function to trigger the load file picker dialog.
    /// </summary>
    /// <returns>The file path of the file selected, or an empty string if cancelled.</returns>
    public static async Task<string> GetFilePathAsync() => await _interaction.Handle(Unit.Default);

    /// <summary>
    /// Function to register the native file picker.
    /// </summary>
    /// <param name="view">The view to register.</param>
    public static void RegisterView(Control view) =>
        _interaction.RegisterHandler(async interaction =>
        {
            TopLevel topLevel = TopLevel.GetTopLevel(view) ?? throw new ArgumentException("The top level control could not be found for the view.", nameof(view));
            string result = await GetFileAsync(topLevel);
            interaction.SetOutput(result);
        });
}
