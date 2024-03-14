#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 29, 2018 4:15:09 PM
// 
#endregion

using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI.Views;
using Gorgon.IO;

namespace Gorgon.Editor.UI;

/// <summary>
/// Common functionality for a content view model.
/// </summary>
/// <typeparam name="T">The type of dependency injection object. Must be a class, and implement <see cref="IContentViewModelInjection"/>.</typeparam>
/// <remarks>
/// <para>
/// This base class provides functionality needed to communicate the state of the content (i.e. if it's been updated), a handler for manipulating the internal application clipboard, and commands to 
/// persist the content data and close the content UI. 
/// </para>
/// <para>
/// Content is what the editor defines as the items that users will create/edit with the editor, this could be images, sprites, music, sound, scripts, etc... Developers of content plug ins must inherit 
/// from this type so that their content model data is updated from the UI, and returns feedback to the UI.
/// </para>
/// <para>
/// As an example, if you wanted to create a very simple text editor, you would define a view with a multi-line textbox control on it, and then create a TextEditorContent view model based on this type 
/// and add a command to recieve updates from the UI (e.g. when text is entered), and a property to return the current text stored in the data.
/// </para>
/// <para>
/// Views that are associated with view models derived from this type must inherit from the <see cref="ContentBaseControl"/>. And developers must register their content views using the 
/// <see cref="ViewFactory"/>.<see cref="ViewFactory.Register{T}(Func{System.Windows.Forms.Control})"/> method so that the host editor application can present the UI to the end user. 
/// </para>
/// </remarks>
/// <seealso cref="ViewFactory"/>
/// <seealso cref="ContentBaseControl"/>
public abstract class ContentEditorViewModelBase<T>
    : ViewModelBase<T, IHostContentServices>, IEditorContent
    where T : class, IContentViewModelInjection
{
    #region Variables.
    // The command used to close the content.
    private IEditorAsyncCommand<CloseContentArgs> _closeCommand;

    // The file for the content.
    private IContentFile _file;

    // The current content state.
    private ContentState _state = ContentState.New;

    // The command used to save the content.
    private IEditorAsyncCommand<SaveReason> _saveContentCommand;

    // The context for commands from the content.
    private IEditorContext _commandContext;

    // The clipboard interface for the view model.
    private IClipboardHandler _clipboardHandler;

    // The number of files selected in the project file explorer.
    private bool _filesSelected;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the file manager used to manage content files.
    /// </summary>
    protected IContentFileManager ContentFileManager
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return whether there are files selected by the user in project file system.
    /// </summary>
    /// <remarks>
    /// This property is useful for enabling or disabling UI elements based on file selection.
    /// </remarks>
    public bool FilesAreSelected
    {
        get => _filesSelected;
        private set
        {
            if (_filesSelected == value)
            {
                return;
            }

            OnPropertyChanging();
            _filesSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the current content state.
    /// </summary>
    /// <remarks>
    /// Changing this value will update the <see cref="File"/> and mark it as changed if the value is <see cref="ContentState.New"/>, or <see cref="ContentState.Modified"/>.
    /// </remarks>
    public ContentState ContentState
    {
        get => _state;
        set
        {
            if ((_state == value) || ((_state == ContentState.New) && (value != ContentState.Unmodified)))
            {
                return;
            }

            OnPropertyChanging();
            _state = value;
            OnPropertyChanged();

            if (File is null)
            {
                return;
            }

            File.IsChanged = _state != ContentState.Unmodified;
        }
    }

    /// <summary>
    /// Property to return the file for the content.
    /// </summary>
    /// <remarks>
    /// This is the file that contains the content data. Useful in cases where the path for the file is required, or other file properties are needed.
    /// </remarks>
    public IContentFile File
    {
        get => _file;
        protected set
        {
            if (_file == value)
            {
                return;
            }

            if (_file is not null)
            {
                _file.Renamed -= File_Renamed;
                _file.IsOpen = false;
            }

            OnPropertyChanging();
            _file = value;
            OnPropertyChanged();

            if (_file is not null)
            {
                _file.Renamed += File_Renamed;
                _file.IsOpen = true;
            }
        }
    }

    /// <summary>
    /// Property to return the type of content.
    /// </summary>
    /// <remarks>
    /// The content type is a user defined string that indicates what the data for the content represents. For example, for an image editor, this could return "Image".
    /// </remarks>
    public abstract string ContentType
    {
        get;
    }

    /// <summary>Property to return the command used to close the content.</summary>
    /// <remarks>
    /// <para>
    /// This command will be executed when the user shuts down the content via some UI element (e.g. the 'X' in a window). 
    /// </para>
    /// <para>
    /// Plug in developers may override this command by setting the property in their own view model. If it is not assigned by the developer, then an internal command is executed. This internal command 
    /// should cover most use cases, so overriding the command should only be done in unique circumstances.
    /// </para>
    /// </remarks>
    public IEditorAsyncCommand<CloseContentArgs> CloseContentCommand
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
    /// Property to set or return the command used to save the content.
    /// </summary>
    /// <remarks>
    /// This command will be executed when the user wants to persist the content data back to the project file system. Developers must assign a command here in order to persist the data back to the 
    /// file system.
    /// </remarks>
    public IEditorAsyncCommand<SaveReason> SaveContentCommand
    {
        get => _saveContentCommand;
        set
        {
            if (_saveContentCommand == value)
            {
                return;
            }

            OnPropertyChanging();
            _saveContentCommand = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the current context for commands from this content.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used in UI interactions where the context of the operations may change depending on state. An example would be a context for an MS Office ribbon control, where activating a tool would 
    /// then show a new set of commands that were previously hidden. These commands would appear under a context heading on the ribbon.
    /// </para>
    /// <para>
    /// If the UI does not support contexts, then this property would be ignored.
    /// </para>
    /// </remarks>
    public IEditorContext CommandContext
    {
        get => _commandContext;
        set
        {
            if (_commandContext == value)
            {
                return;
            }

            OnPropertyChanging();
            _commandContext = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the clipboard handler for this view model.</summary>
    /// <remarks>
    /// <para>
    /// This gives access to the application clipboard functionality to copy, cut and paste data. If this is assigned, then your view should have some means to execute the required commands for the copy, 
    /// cut and paste operations.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The application clipboard is <b>not</b> the same as the Windows clipboard. Data copied to the application clipboard is not accessible by other applications.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public IClipboardHandler Clipboard
    {
        get => _clipboardHandler;
        protected set
        {
            if (_clipboardHandler == value)
            {
                return;
            }

            OnPropertyChanging();
            _clipboardHandler = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Methods.
    /// <summary>Handles the Renamed event of the File control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ContentFileRenamedEventArgs"/> instance containing the event data.</param>
    private void File_Renamed(object sender, ContentFileRenamedEventArgs e) => NotifyPropertyChanged(nameof(File));

    /// <summary>Handles the SelectedFilesChanged event of the ContentFileManager control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ContentFileManager_SelectedFilesChanged(object sender, EventArgs e)
    {
        FilesAreSelected = false;
        FilesAreSelected = ContentFileManager.GetSelectedFiles().Count > 0;
    }

    /// <summary>
    /// Function called to close the content.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private async Task DoCloseContentAsync(CloseContentArgs args)
    {
        try
        {
            if (args.CheckChanges)
            {
                bool continueClose = await OnCloseContentTaskAsync();

                if (!continueClose)
                {
                    args.Cancel = true;
                    return;
                }

                ContentState = ContentState.Unmodified;
            }

            if (File is not null)
            {
                File.IsChanged = false;
            }
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CLOSE_CONTENT);
        }
    }

    /// <summary>
    /// Function to save the content from a working file to the actual file on the file system.
    /// </summary>
    /// <param name="workFile">The working file for the content.</param>
    /// <returns>A task for asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="workFile"/> is a copy of the original file in the project file system. This working file is managed by a scratch file system passed to the content plug in. By handling 
    /// files in this manner, we can ensure that the original file can remain intact should something go wrong, or an undesirable change is included in the content by reverting to the original file.
    /// </para>
    /// </remarks>
    protected async Task SaveContentFileAsync(IGorgonVirtualFile workFile)
    {
        if (File is null)
        {
            return;
        }

        Stream inStream = null;
        Stream outStream = null;

        try
        {
            inStream = workFile.OpenStream();
            File.IsOpen = false;
            outStream = ContentFileManager.OpenStream(File.Path, FileMode.Create);
            File.IsOpen = true;

            await inStream.CopyToAsync(outStream);
        }
        finally
        {
            File.IsOpen = true;
            File.Refresh();
            outStream?.Dispose();
            inStream?.Dispose();
        }
    }

    /// <summary>
    /// Function to determine the action to take when this content is closing.
    /// </summary>
    /// <returns><b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
    /// <remarks>
    /// <para>
    /// Content plug in developers should override this method so that users are given a chance to save their content data (if it has changed) prior to closing the content. 
    /// </para>
    /// <para>
    /// This is set up as an asynchronous method so that users may save their data asynchronously to keep the UI usable.
    /// </para>
    /// </remarks>
    protected virtual Task<bool> OnCloseContentTaskAsync() => Task.FromResult(true);

    /// <summary>
    /// Function to initialize the content.
    /// </summary>
    /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="injectionParameters"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <see cref="IContentViewModelInjection.File"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This is required in order to inject initialization data for the view model from an external source (i.e. the editor host application). Developers will use this area to perform setup of the content 
    /// view model, and validate any information being injected. 
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
        _filesSelected = ContentFileManager.GetSelectedFiles().Count > 0;
        _file = injectionParameters.File;

        if (_file is null)
        {
            throw new ArgumentException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, string.Empty), nameof(injectionParameters));
        }

        if (!string.IsNullOrWhiteSpace(ContentType))
        {
            _file.Metadata.Attributes[CommonEditorConstants.ContentTypeAttr] = ContentType;
        }

        _file.Renamed += File_Renamed;
        ContentFileManager.SelectedFilesChanged += ContentFileManager_SelectedFilesChanged;

    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    /// <seealso cref="ViewModelBase{T, Ths}.Load" />
    protected override void OnUnload()
    {
        CommandContext = null;

        // Unassign events and reset state, but do not set to NULL, that'll be handled later on.
        ContentFileManager.SelectedFilesChanged -= ContentFileManager_SelectedFilesChanged;
        _file.Renamed -= File_Renamed;
        _file.IsOpen = false;
        _file.IsChanged = false;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the EditorContentCommon class.</summary>
    protected ContentEditorViewModelBase() => CloseContentCommand = new EditorAsyncCommand<CloseContentArgs>(DoCloseContentAsync);
    #endregion
}
