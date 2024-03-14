
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
// Created: October 29, 2018 3:49:26 PM
// 


using Gorgon.Editor.Content;

namespace Gorgon.Editor.UI;

/// <summary>
/// The flags used to identify the current content state
/// </summary>
public enum ContentState
{
    /// <summary>
    /// Content is unmodified.
    /// </summary>
    Unmodified = 0,
    /// <summary>
    /// Content is new.
    /// </summary>
    New = 1,
    /// <summary>
    /// Content is modified.
    /// </summary>
    Modified = 2
}

/// <summary>
/// The reason why the application is saving the content
/// </summary>
public enum SaveReason
{
    /// <summary>
    /// The user hit the save button.
    /// </summary>
    UserSave = 0,
    /// <summary>
    /// The application or project is shutting down.
    /// </summary>
    AppProjectShutdown = 1,
    /// <summary>
    /// The content is closing. Plug in developers should use this state to bypass error handling in a command to allow exceptions to bubble up.
    /// </summary>
    ContentShutdown = 2
}

/// <summary>
/// Common interface for editor content view models
/// </summary>
public interface IEditorContent
    : IViewModel
{

    /// <summary>
    /// Property to return the clipboard handler for this view model.
    /// </summary>
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
    IClipboardHandler Clipboard
    {
        get;
    }

    /// <summary>
    /// Property to set or return the current content state.
    /// </summary>
    ContentState ContentState
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the content file.
    /// </summary>
    /// <remarks>
    /// This is the file that contains the content data. Useful in cases where the path for the file is required, or other file properties are needed.
    /// </remarks>
    IContentFile File
    {
        get;
    }

    /// <summary>
    /// Property to return the type of content.
    /// </summary>
    /// <remarks>
    /// The content type is a user defined string that indicates what the data for the content represents. For example, for an image editor, this could return "Image".
    /// </remarks>
    string ContentType
    {
        get;
    }

    /// <summary>
    /// Property to return the command used when the content is about to be closed.
    /// </summary>
    /// <remarks>
    /// This command will be executed when the user shuts down the content via some UI element (e.g. the 'X' in a window). 
    /// </remarks>
    IEditorAsyncCommand<CloseContentArgs> CloseContentCommand
    {
        get;
    }

    /// <summary>
    /// Property to set or return the command used to save the content.
    /// </summary>
    /// <remarks>
    /// This command will be executed when the user wants to persist the content data back to the project file system. Developers must assign a command here in order to persist the data back to the 
    /// file system.
    /// </remarks>
    IEditorAsyncCommand<SaveReason> SaveContentCommand
    {
        get;
        set;
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
    IEditorContext CommandContext
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return whether there are files selected by the user in project file system.
    /// </summary>
    /// <remarks>
    /// This property is useful for enabling or disabling UI elements based on file selection.
    /// </remarks>
    bool FilesAreSelected
    {
        get;
    }

}
