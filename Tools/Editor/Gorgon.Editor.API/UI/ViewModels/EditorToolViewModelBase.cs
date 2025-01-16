
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: October 29, 2018 4:15:09 PM
// 

using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Tools;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.UI;

/// <summary>
/// Common functionality for an editor tool plug-in view model
/// </summary>
/// <typeparam name="T">The type of dependency injection object. Must be a class, and implement <see cref="IEditorToolViewModelInjection"/>.</typeparam>
/// <remarks>
/// <para>
/// This base class provides functionality needed to communicate the state of the tool with its UI
/// </para>
/// <para>
/// An editor tool plug-in is a special type of plug-in that adds functionality to the editor via the main application ribbon. Tools can be anything from a special bit of functionality to manage 
/// data, to batch processing, etc... To begin implementing an editor tool, developers must inherit from this type so that their tool model data is updated from the UI, and feedback is returned 
/// to the tool UI
/// </para>
/// <para>
/// As an example, if you wanted to create a tool that converted all images in the file system to grayscale (why? why not?), you would define a view with a file list containing the images to convert,     
/// and then create a BatchGrayScale view model based on this type and add an observable list to receive updates from the UI (e.g. when images are selected), and a command to process the image data 
/// and write back to the file system
/// </para>
/// <para>
/// Views that are associated with view models derived from this type must inherit from the <see cref="EditorToolBaseForm"/>. 
/// </para>
/// </remarks>
/// <seealso cref="ViewFactory"/>
/// <seealso cref="EditorToolBaseForm"/>
public abstract class EditorToolViewModelBase<T>
    : ViewModelBase<T, IHostContentServices>, IEditorTool
    where T : class, IEditorToolViewModelInjection
{

    // The command used to close the content.
    private IEditorAsyncCommand<CloseToolArgs> _closeCommand;

    /// <summary>
    /// Property to return the file manager used to manage content files.
    /// </summary>
    protected IContentFileManager ContentFileManager
    {
        get;
        private set;
    }

    /// <summary>Property to return the command used to close the tool.</summary>
    /// <remarks>
    /// <para>
    /// This command will be executed when the user shuts down the tool via a close UI element. 
    /// </para>
    /// <para>
    /// Plug in developers may override this command by setting the property in their own view model. If it is not assigned by the developer, then an internal command is executed. This internal 
    /// command should cover most use cases, so overriding the command should only be done in unique circumstances.
    /// </para>
    /// </remarks>
    public IEditorAsyncCommand<CloseToolArgs> CloseToolCommand
    {
        get => _closeCommand;
        protected set
        {
            if (_closeCommand == value)
            {
                return;
            }

            OnPropertyChanging();
            _closeCommand = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Function called to close the tool.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private async Task DoCloseToolAsync(CloseToolArgs args)
    {
        try
        {
            if (!args.CheckChanges)
            {
                return;
            }
            bool continueClose = await OnCloseToolTaskAsync();

            if (!continueClose)
            {
                args.Cancel = true;
            }
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CLOSE_TOOL);
        }
    }

    /// <summary>
    /// Function to determine the action to take when this tool is closing.
    /// </summary>
    /// <returns><b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
    /// <remarks>
    /// <para>
    /// Tool plug-in developers can override this method to verify changes, or perform last minute updates as needed.
    /// </para>
    /// <para>
    /// This is set up as an asynchronous method so that users may save their data asynchronously to keep the UI usable.
    /// </para>
    /// </remarks>
    protected virtual Task<bool> OnCloseToolTaskAsync() => Task.FromResult(true);

    /// <summary>
    /// Function to initialize the tool.
    /// </summary>
    /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="injectionParameters"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This is required in order to inject initialization data for the view model from an external source (i.e. the editor host application). Developers will use this area to perform setup of 
    /// the editor view model, and validate any information being injected. 
    /// </para>
    /// <para>
    /// Ideally this can be used to create resources, child view models, and other objects that need to be initialized when the view model is created.
    /// </para>
    /// <para>
    /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
    /// </para>
    /// </remarks>
    protected override void OnInitialize(T injectionParameters)
    {
        if (injectionParameters is null)
        {
            throw new ArgumentNullException(nameof(injectionParameters));
        }

        ContentFileManager = injectionParameters.ContentFileManager;
    }

    /// <summary>Initializes a new instance of the EditorContentCommon class.</summary>
    protected EditorToolViewModelBase() => CloseToolCommand = new EditorAsyncCommand<CloseToolArgs>(DoCloseToolAsync);

}
