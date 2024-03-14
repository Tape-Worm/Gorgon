
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: August 3, 2020 3:51:47 PM
// 


using System.Text;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Examples;

/// <summary>
/// Undo/redo data for undoing and redoing text change operations
/// </summary>
internal class TextChangeUndoRedo
{
    public string Text;
}

/// <summary>
/// The view model used to communicate with the text viewer control
/// </summary>
/// <remarks>
/// <para>
/// The editor makes heavy use of MVVM (or, more accurately, a bastardized version of MVVM) to communicate data to and from the view. We use this 
/// view model to facilitate that
/// 
/// In this example, our view model will present our content data back to the view via the properties (presentation), and handle content modification 
/// via the commands (controller)
/// </para>
/// </remarks>
internal class TextContent
    : ContentEditorViewModelBase<TextContentParameters>, ITextContent
{

    // The text content.
    private string _text;
    // The font used to draw the text.
    private FontFace _font = FontFace.Arial;
    // The color of the text.
    private GorgonColor _color = GorgonColor.Black;
    // The currently active panel.
    private IHostedPanelViewModel _currentPanel;
    // The editor used to modify text.
    private TextEditorService _textEditor;
    // The service used for providing undo/redo functionality.
    private IUndoService _undoService;



    /// <summary>
    /// Property to return the view model for the text color editor.
    /// </summary>
    public ITextColor TextColor
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the currently active hosted panel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property holds the view model for the currently active hosted panel which can be used for parameters for an operation on the content. Setting this value will bring the panel up on the UI and 
    /// setting it to <b>null</b> will remove it.
    /// </para>
    /// </remarks>
    public IHostedPanelViewModel CurrentPanel
    {
        get => _currentPanel;
        set
        {
            if (_currentPanel == value)
            {
                return;
            }

            if (_currentPanel is not null)
            {
                _currentPanel.PropertyChanged -= TextColor_PropertyChanged;
                _currentPanel.IsActive = false;
            }

            OnPropertyChanging();
            _currentPanel = value;
            OnPropertyChanged();

            if (_currentPanel is not null)
            {
                _currentPanel.IsActive = true;
                _currentPanel.PropertyChanged += TextColor_PropertyChanged;
            }
        }
    }

    /// <summary>Property to return the command to undo an operation.</summary>
    public IEditorCommand<object> UndoCommand
    {
        get;
    }

    /// <summary>Property to return the command to redo an operation.</summary>
    public IEditorCommand<object> RedoCommand
    {
        get;
    }

    /// <summary>Property to return the type of content.</summary>
    /// <remarks>The content type is a user defined string that indicates what the data for the content represents. For example, for an image editor, this could return "Image".</remarks>
    public override string ContentType => TextViewerPlugin.ContentTypeValue;

    /// <summary>
    /// Property to return the text to display.
    /// </summary>
    public string Text
    {
        get => _text ?? string.Empty;
        private set
        {
            // When setting a property on the view model, we should always check it against the current 
            // value against the incoming value. 
            //
            // This keeps the view model from becoming "chatty" and potentially causing an infinite loop
            // or other more difficult to handle scenarios.
            if (string.Equals(_text, value, StringComparison.CurrentCulture))
            {
                return;
            }

            // The property setter is marked as private because we only need to assign the text from 
            // within the view model, in response to a change text command.

            // Using the OnPropertyChanging and OnPropertyChanged methods we can fire the events that 
            // external objects can hook to react to the changes.
            //
            // The OnProperty* are special methods and should only be called within a property setter. 
            // To notify a property is changing/ has changed within a method, use the 
            // NotifyPropertyChanging/NotifyPropertyChanged methods and pass in the name of the 
            // property.
            OnPropertyChanging();
            _text = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the font face to use.
    /// </summary>
    public FontFace FontFace
    {
        get => _font;
        set
        {
            if (_font == value)
            {
                return;
            }

            OnPropertyChanging();
            _font = value;
            OnPropertyChanged();

            // Because this specific property has a public setter, we need to ensure that if we've made changes, 
            // then the content should be marked the content as changed.
            //
            // The ContentState property lets the editor know that we've modified the property and will update 
            // the UI accordingly.
            ContentState = ContentState.Modified;
        }
    }

    /// <summary>
    /// Property to return the color of the text.
    /// </summary>
    public GorgonColor Color
    {
        get => _color;
        private set
        {
            if (_color == value)
            {
                return;
            }

            OnPropertyChanging();
            _color = value;
            OnPropertyChanged();
        }
    }

    // The following properties are command objects used to execute functionality against our content data.
    //
    // A command object is similar to an event, but it will contain two callback methods: 
    // 1. An optional Func{bool} method that is called to determine whether the command can execute or not.
    // 2. A required Action{T} method that is called to execute the operation against the content data.
    //
    // This gives us an advantage in that we can update our UI to disallow execution of a function if the 
    // state of the view model won't allow it. We can also use this functionality to record our operations 
    // so that we can have a Undo/Redo functionality (which we implement in our change text command).

    /// <summary>
    /// Property to return the command used to activate the text color editor.
    /// </summary>
    public IEditorCommand<object> ActivateTextColorCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to alter the text of the content.
    /// </summary>
    public IEditorCommand<object> ChangeTextCommand
    {
        get;
    }



    /// <summary>Handles the PropertyChanged event of the TextColor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void TextColor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // We monitor the text color viewmodel for changes to its state using this event handler.
        // In this case, when the panel is no longer active, we call the method for activation 
        // which will disable the panel by setting the CurrentPanel to NULL.
        switch (e.PropertyName)
        {
            case nameof(ITextColor.IsActive):
                DoActivateTextColorEditor();
                break;
        }
    }

    /// <summary>
    /// Function to save the text content to the content file.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task SaveContentAsync()
    {
        Stream stream = null;
        StreamWriter writer = null;

        try
        {
            // Because we can't embed the font and color in the actual content, we can actually bind it to the file via 
            // the file attributes. This allows us to extend the file data without storing the data in the file itself
            // and keep the file format as it should be.
            File.Metadata.Attributes["TextFont"] = FontFace.ToString();
            File.Metadata.Attributes["TextColor"] = Color.ToARGB().ToString();

            // Before we write our data back to the file system we need to mark the content file as closed. If we don't 
            // do this an exception will be thrown. 
            //
            // This flag is set by the developer to give the most control over our files and is used to notify the editor 
            // that a file should be considered "Open" so it can't be modified/deleted while it's being worked on, or 
            // is a dependency for open content. Basically, it's a means to keep your data safe while it's being worked 
            // on. 
            File.IsOpen = false;

            stream = ContentFileManager.OpenStream(File.Path, FileMode.Create);
            writer = new StreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(Text);
            await writer.FlushAsync();

            // When we've saved our data, we must mark the content as unmodified so that the editor won't nag you about 
            // unsaved changes.
            ContentState = ContentState.Unmodified;
        }
        finally
        {
            writer.Dispose();
            stream.Dispose();
            File.IsOpen = true;
        }
    }


    /// <summary>
    /// Function to determine if the text can be changed or not.
    /// </summary>
    /// <returns><b>true</b> if the text can be changed, <b>false</b> if not.</returns>
    private bool CanChangeText() => CurrentPanel is null;

    /// <summary>
    /// Function to alter the text content.
    /// </summary>
    private void DoChangeText()
    {
        TextChangeUndoRedo undoArgs;
        TextChangeUndoRedo redoArgs;

        // Function to change the text in the content.
        // This method will update the text property on our behalf. Our undo/redo functionality will call this method 
        // when undoing or redoing the text.
        //
        // We return the updated text here (or null if we cancel or it fails for some reason), and use that to determine if we 
        // should put the operation on the undo/redo stack.
        string ChangeText(string text)
        {
            try
            {
                // If we don't provide text to the parameter, then ask for our text from the user.
                // When we redo/undo, text will come in through the parameter instead of being 
                // prompted for the text (which, obviously is what we want).
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = _textEditor.GetText(Text);
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                Text = text;

                // if we've made changes, then we should mark the content as changed.
                ContentState = ContentState.Modified;

                return text;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, "There was an error changing the text.");
                return null;
            }
        }

        undoArgs = new TextChangeUndoRedo
        {
            Text = Text
        };
        redoArgs = new TextChangeUndoRedo
        {
            Text = ChangeText(null)
        };

        Task UndoRedoAction(TextChangeUndoRedo args, CancellationToken cancelToken)
        {
            ChangeText(args.Text);
            return Task.CompletedTask;
        }

        if (redoArgs.Text is null)
        {
            return;
        }

        _undoService.Record("Text changed.", UndoRedoAction, UndoRedoAction, undoArgs, redoArgs);
        NotifyPropertyChanged(nameof(UndoCommand));
    }

    /// <summary>
    /// Function to determine if the color can be changed on the text.
    /// </summary>
    /// <returns><b>true</b> if the editor can assign the text color, <b>false</b> if not.</returns>
    private bool CanSetColor() => (CurrentPanel == TextColor) && (TextColor.OriginalColor != TextColor.SelectedColor);

    /// <summary>
    /// Function to assign the text color.
    /// </summary>
    private void DoSetColor()
    {
        try
        {
            // Here we'll finalize setting the color of the text once the user dismisses the color panel.
            // You'll note that the renderer will pick up this change and update the text sprite accordingly.
            Color = TextColor.SelectedColor;
            CurrentPanel = null;

            // if we've made changes, then we should mark the content as changed.
            ContentState = ContentState.Modified;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, "There was an error setting the text color.");
        }
    }

    /// <summary>
    /// Function to determine if the text color editor can be activated.
    /// </summary>
    /// <returns><b>true</b> if the editor can activate, <b>false</b> if not.</returns>
    private bool CanActivateTextColorEditor() => CurrentPanel is null;

    /// <summary>
    /// Function to activate the text color editor.
    /// </summary>
    private void DoActivateTextColorEditor()
    {
        // This is where we activate the editor panel for the text color.
        // You might be wondering why we'd need to wrap a simple property set within a try-catch block. 
        // This is because setting the property will set off a series of side effects on the view. If 
        // we don't capture any exceptions, the application will crash outright. Obviously, we don't 
        // want that.
        // 
        // So, best practices are that we should always wrap our command callbacks in a try-catch at
        // minimum. 
        try
        {
            if (CurrentPanel is not null)
            {
                CurrentPanel = null;
                return;
            }

            TextColor.SelectedColor = TextColor.OriginalColor = Color;
            CurrentPanel = TextColor;

            // if we've made changes, then we should mark the content as changed.
            ContentState = ContentState.Modified;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, "There was an error activating the text color editor.");
        }
    }

    /// <summary>
    /// Function to determine if the content can be saved.
    /// </summary>
    /// <param name="saveReason">The reason that the content is being saved.</param>
    /// <returns><b>true</b> if the content can be saved, <b>false</b> if not.</returns>
    private bool CanSave(SaveReason saveReason) => (ContentState != ContentState.Unmodified)
                                                        && (((CommandContext is null) && (CurrentPanel is null)) || (saveReason != SaveReason.UserSave));

    /// <summary>
    /// Function to save the content.
    /// </summary>
    /// <param name="saveReason">The reason that the content is being saved.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoSaveAsync(SaveReason saveReason)
    {
        try
        {
            ShowWaitPanel("Saving...");

            // Turn off any active panel.
            CurrentPanel = null;

            await SaveContentAsync();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, "There was an error saving the content.");
        }
        finally
        {
            HideWaitPanel();
        }
    }


    /// <summary>
    /// Function to determine if an undo operation is possible.
    /// </summary>
    /// <returns><b>true</b> if the last action can be undone, <b>false</b> if not.</returns>
    private bool CanUndo() => (_undoService.CanUndo) && (CurrentPanel is null);

    /// <summary>
    /// Function to determine if a redo operation is possible.
    /// </summary>
    /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
    private bool CanRedo() => (_undoService.CanRedo) && (CurrentPanel is null);

    /// <summary>
    /// Function called when a redo operation is requested.
    /// </summary>
    private async void DoRedoAsync()
    {
        try
        {
            await _undoService.Redo();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, "Error while performing redo operation.");
        }
    }

    /// <summary>
    /// Function called when an undo operation is requested.
    /// </summary>
    private async void DoUndoAsync()
    {
        try
        {
            await _undoService.Undo();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, "Error while performing undo operation.");
        }
    }

    /// <summary>Function to determine the action to take when this content is closing.</summary>
    /// <returns>
    ///   <b>true</b> to continue with closing, <b>false</b> to cancel the close request.</returns>
    /// <remarks>
    ///   <para>
    /// Content plug in developers should override this method so that users are given a chance to save their content data (if it has changed) prior to closing the content.
    /// </para>
    ///   <para>
    /// This is set up as an asynchronous method so that users may save their data asynchronously to keep the UI usable.
    /// </para>
    /// </remarks>
    protected override async Task<bool> OnCloseContentTaskAsync()
    {
        // When closing, we should check whether the content data is modified, this way we can prompt the user to save 
        // the data prior to closing it down.

        if (ContentState == ContentState.Unmodified)
        {
            return true;
        }

        MessageResponse response = HostServices.MessageDisplay.ShowConfirmation($"The file '{File.Name}' has unsaved changes.\n\nWould you like to save now?", allowCancel: true);

        switch (response)
        {
            case MessageResponse.Yes:
                await DoSaveAsync(SaveReason.ContentShutdown);
                return true;
            case MessageResponse.Cancel:
                return false;
            default:
                return true;
        }
    }

    /// <summary>Function to initialize the content.</summary>
    /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
    /// <remarks>
    ///   <para>
    /// This is required in order to inject initialization data for the view model from an external source (i.e. the editor host application). Developers will use this area to perform setup of the content
    /// view model, and validate any information being injected.
    /// </para>
    ///   <para>
    /// Ideally this can be used to create resources, child view models, and other objects that need to be initialized when the view model is created.
    /// </para>
    ///   <para>
    /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
    /// </para>
    /// </remarks>
    protected override void OnInitialize(TextContentParameters injectionParameters)
    {
        base.OnInitialize(injectionParameters);

        TextColor = injectionParameters.TextColor;
        _text = injectionParameters.Text;
        _textEditor = injectionParameters.TextEditor;
        _undoService = injectionParameters.UndoService;

        // Since we store the color and font in the file attributes, we can retrieve that data here and update the view model.
        if ((!File.Metadata.Attributes.TryGetValue("TextFont", out string font))
            || (!Enum.TryParse(font, out _font)))
        {
            _font = injectionParameters.Settings.DefaultFont;
        }

        if ((!File.Metadata.Attributes.TryGetValue("TextColor", out string color))
            || (!int.TryParse(color, out int colorValue)))
        {
            colorValue = unchecked((int)0xFF000000);
        }

        _color = new GorgonColor(colorValue);
    }


    /// <summary>Function called when the associated view is loaded.</summary>
    /// <remarks>
    ///   <para>
    /// This method should be overridden when there is a need to set up functionality (e.g. events) when the UI first loads with the view model attached. Unlike the <see cref="ViewModelBase{T, THs}.Initialize"/> method, this
    /// method may be called multiple times during the lifetime of the application.
    /// </para>
    ///   <para>
    /// Anything that requires tear down should have their tear down functionality in the accompanying <see cref="ViewModelBase{T, THs}.Unload"/> method.
    /// </para>
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        // Our panel for changing the text color (and other panel types) allow us to assign the command for 
        // accepting the change provided by the panel editor. This allows us to update the local property 
        // without providing the sub panel access to the parent view model.
        TextColor.OkCommand = new EditorCommand<object>(DoSetColor, CanSetColor);
    }


    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    /// <seealso cref="ViewModelBase{T, THs}.Load" />
    protected override void OnUnload()
    {
        // Whenever we set and event, or assign a command on a child view model, we should reset it so we 
        // avoid potential event leakage.
        TextColor.OkCommand = null;

        base.OnUnload();
    }



    /// <summary>Initializes a new instance of the <see cref="TextContent"/> class.</summary>
    public TextContent()
    {
        ActivateTextColorCommand = new EditorCommand<object>(DoActivateTextColorEditor, CanActivateTextColorEditor);
        ChangeTextCommand = new EditorCommand<object>(DoChangeText, CanChangeText);
        SaveContentCommand = new EditorAsyncCommand<SaveReason>(DoSaveAsync, CanSave);
        UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
        RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
    }

}
