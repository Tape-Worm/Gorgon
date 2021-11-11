#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 3, 2020 1:41:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Examples.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.IO;
using Microsoft.IO;

namespace Gorgon.Examples
{
    /// <summary>
    /// An example content plug in for the Gorgon Editor.
    /// </summary>
    /// <remarks>
    /// 
    /// The Gorgon Editor is able to load in various plug ins for end user content creation. This allows for flexibility by keeping the content workflow localized to one central application.
    /// 
    /// Content plug ins produce/edit content files that contain the native data for the content (e.g. an image file will contain image data in a standard format like dds). But along with 
    /// these files metadata is stored so that extraneous data that can't be stored in the file itself can be accessed. The metadata can be attributes which is a key/value string pair that 
    /// can contain things like codec information, state information, etc...  Along with this attribute data, a list of categorized dependency file paths can be stored so that files that the 
    /// content requires to open correctly can be opened by the editor as required.
    /// 
    /// In this example, we'll show how to build a simple text viewer for the editor.  This class is the main entry point for the plug in. The editor calls the code in here to initialize and 
    /// prepare the content for editing, so this is where we need to start.
    /// 
    /// Some notes before we begin:
    /// #1 This is a DLL project, for it to work correctly, the resulting DLL must be copied to the Plugins content directory of the Gorgon.Editor install location. For example, 
    /// if the editor is installed in C:\Gorgon\Editor\, then the plug in should be copied into the C:\Gorgon\Editor\Plugins\Content directory.
    /// 
    /// #2 Running this plug in from Visual Studio should automatically copy the plug in and start up the editor.
    /// 
    /// #3 This is not meant for inexperienced developers. If you are a beginner, or just not comfortable with C#, it may be very confusing to understand as there are a lot of pieces to 
    /// building an editor plug in. Proceed at your own peril.
    /// 
    /// There are several parts to a content plug in:
    /// 1. The plug in (this file).
    /// 2. The views (UI for the content editor)
    /// 3. The view models (for MVVM)
    /// 4. The services (functionality to perform various operations using agnostic code)
    /// 5. The settings (global plug in settings)
    /// 
    /// To start, we need to set up the initial plug in code to communicate with the host application (our editor).  
    /// 
    /// We begin by creating a class, and inheriting from the ContentPlugIn base class, and implement the IContentPlugInMetadata interface. The base class provides common functionality and 
    /// abstract methods to override that will allow building a preview image, and return information about the plug in.
    /// </remarks>
    internal class TextViewerPlugin
        : ContentPlugIn, IContentPlugInMetadata
    {
        #region Constants.
        // All content plug ins should provide a type value providing an ID describing what content is produced by this plug in. This will be stored 
        // in the metadata for the content file and can be used to identify the type of content when it is used as a dependency.

        /// <summary>
        /// The type of content handled by this content plug in.
        /// </summary>
        public const string ContentTypeValue = "Text";
        #endregion

        #region Variables.
        // No thumbnail image.
        // This is the default image that will be displayed in the preview panel on the main window. 
        // Some plug ins will want to render small representation of the content so users can determine 
        // if the content file contains the data they're looking for. However, in this simple example, 
        // we have no need of it, and we'll define a default image to display (embedded in the 
        // resources of the plug in).
        private IGorgonImage _noThumbnail;

        // This contains the global settings for the plug in. These can contain defaults for the plug in 
        // and can be changed any time by going into the File tab, and clicking on the Settings button.
        private TextContentSettings _settings = new();

        // This the view model for the settings. It is used on the settings panel control supplied by 
        // our plug in and is used to persist our settings back to the disk.
        private Settings _settingsViewModel;

        // Some plugins will have their own specific settings that should be persisted and reloaded 
        // when the editor starts up. The typical convention used by editor plug ins is to use the 
        // fully qualified type name of the plug in as the base file name.
        //
        // These files are stored in the %AppData%\Tape_Worm\Gorgon.Editor\ContentPlugins\ directory 
        // as JSON files. 
        //
        // We'll leave this value here as a placeholder, but for now, this example doesn't have any 
        // settings to remember/restore, so we'll not be making use of this yet.
        /// <summary>
        /// The name of the settings file.
        /// </summary>
        public static readonly string SettingsName = typeof(TextViewerPlugin).FullName;
        #endregion

        #region Properties.
        // This name is used as an internal identifier for the plug in. This is typically the fully qualified type name of the plug in class.

        /// <summary>Property to return the name of the plug in.</summary>
        string IContentPlugInMetadata.PlugInName => Name;

        // This is a friendly description of the plug in used for display purposes.

        /// <summary>Property to return the description of the plugin.</summary>
        string IContentPlugInMetadata.Description => Description;

        // Some plug ins are only capable of editing existing content, and in those cases this value should return false. For plug ins that can 
        // create new content this property should return true.

        /// <summary>Property to return whether or not the plugin is capable of creating content.</summary>
        public override bool CanCreateContent => true;

        /// <summary>Property to return the ID of the small icon for this plug in.</summary>
        public Guid SmallIconID
        {
            // This is the GUID for the icon that will show in the main file explorer panel 
            // of the editor. This, and other icon IDs are used to index the icon image in 
            // a list of icon images. Because it's used as an ID, it must be unique.
            //
            // We define this GUID in the constructor of our plug in (this) class.
            get;
        }

        /// <summary>Property to return the ID of the new icon for this plug in.</summary>
        public Guid NewIconID
        {
            // For plug ins with the CanCreateContent property set to true, we should provide 
            // an icon for new content. This icon will be used in menu items for creating a 
            // new content file. 
            //
            // This property works on the same principle as the SmallIconID.
            //
            // We define this GUID in the constructor of our plug in (this) class.
            get;
        }

        /// <summary>Property to return the ID for the type of content produced by this plug in.</summary>
        public override string ContentTypeID => ContentTypeValue;

        /// <summary>Property to return the friendly (i.e shown on the UI) name for the type of content.</summary>
        public string ContentType => "Example Text";

        /// <summary>
        /// Property to return the default file extension used by files generated by this content plug in.
        /// </summary>
        /// <remarks>
        /// Plug in developers can override this to default the file name extension for their content when creating new content with <see cref="GetDefaultContentAsync(string, HashSet{string})"/>.
        /// </remarks>
        protected override GorgonFileExtension DefaultFileExtension => new(".txt", "Text files");
        #endregion

        #region Methods.
        // This method is not explicitly required, but is included here to set up default metadata attributes for our content.
        // It's only called if the plug in needs to determine if the file can be opened or not. Typically this is done when 
        // a file has no metadata, and thus no simple means of determining which plug in can open the file. 

        /// <summary>
        /// Function to update the metadata for a file that is missing metadata attributes.
        /// </summary>
        /// <param name="attributes">The attributes to update.</param>
        /// <returns><b>true</b> if the metadata needs refreshing for the file, <b>false</b> if not.</returns>
        private bool UpdateFileMetadataAttributes(Dictionary<string, string> attributes)
        {
            bool needsRefresh = false;

            // For our example, we only need the bare minimum for metadata. This is usually the type of content that is contained 
            // within the file.
            // For more complicated systems, we may need to put in codec information, or other specific items of data that aren't 
            // to be stored in the file.
            if ((attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string currentContentType))
                && (string.Equals(currentContentType, ContentTypeID, StringComparison.OrdinalIgnoreCase)))
            {
                attributes.Remove(CommonEditorConstants.ContentTypeAttr);
                needsRefresh = true;
            }

            if (attributes.ContainsKey("TextFont"))
            {
                attributes.Remove("TextFont");
                needsRefresh = true;
            }

            if (attributes.ContainsKey("TextColor"))
            {
                attributes.Remove("TextColor");
                needsRefresh = true;
            }

            if ((!attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out currentContentType))
                || (!string.Equals(currentContentType, CommonEditorContentTypes.ImageType, StringComparison.OrdinalIgnoreCase)))
            {
                attributes[CommonEditorConstants.ContentTypeAttr] = ContentTypeID;
                needsRefresh = true;
            }

            return needsRefresh;
        }

        /// <summary>Function to register plug in specific search keywords with the system search.</summary>
        /// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
        /// <param name="searchService">The search service to use for registration.</param>
        protected override void OnRegisterSearchKeywords<T>(ISearchService<T> searchService)
        {
            // This is used to define specific keywords when searching for files in the main editor window.  
            // We have none for this specific plug in type, so for now we leave it empty.
        }


        // For plug ins that have persistent settings, we provide a means of accessing those settings through the Settings menu option
        // on the File tab. 
        //
        // These plug in settings systems require a panel with the settings that you wish to allow the user to alter. To activate the 
        // settings this method is used to return the view model for the settings. The editor will read this value and create the view 
        // for the settings.
        //
        // This view model can be used by the content view model to update the view based on the settings in real time as needed.
        //
        // As mentioned in the comment below, we must register the view model first so the application knows how to create the view 
        // (the editor knows absolutely nothing about the content plug in beyond this interface, and thus doesn't know how to create the 
        // view). 

        /// <summary>Function to retrieve the settings interface for this plug in.</summary>
        /// <returns>The settings interface view model.</returns>
        /// <remarks>
        ///   <para>
        /// Implementors who wish to supply customizable settings for their plug ins from the main "Settings" area in the application can override this method and return a new view model based on
        /// the base <see cref="ISettingsCategory"/> type. Returning <b>null</b> will mean that the plug in does not have settings that can be managed externally.
        /// </para>
        ///   <para>
        /// Plug ins must register the view associated with their settings panel via the <see cref="ViewFactory.Register{T}(Func{Control})"/> method when the plug in first loaded,
        /// or else the panel will not show in the main settings area.
        /// </para>
        /// </remarks>
        protected override ISettingsCategory OnGetSettings() => _settingsViewModel;

        /// <summary>Function to retrieve the default content name, and data.</summary>
        /// <param name="generatedName">A default name generated by the application.</param>
        /// <param name="metadata">Custom metadata for the content.</param>
        /// <returns>The default content name along with the content data serialized as a byte array. If either the name or data are <b>null</b>, then the user cancelled..</returns>
        /// <remarks>
        ///   <para>
        /// Plug in authors may override this method so a custom UI can be presented when creating new content, or return a default set of data and a default name, or whatever they wish.
        /// </para>
        ///   <para>
        /// If an empty string (or whitespace) is returned for the name, then the <paramref name="generatedName" /> will be used.
        /// </para>
        /// </remarks>
        protected override Task<(string name, RecyclableMemoryStream data)> OnGetDefaultContentAsync(string generatedName, ProjectItemMetadata metadata)
        {
            // This is the method that we can use to prompt the user for a new name, and/or provide defaults for our content, and create a 
            // default piece of content that can be persisted to the file system.
            //
            // You'll notice that we send back both the name, and a byte array. The byte array is what will contain the data to write to the 
            // file system. Since the main application doesn't know anything about handling your content, we have to provide it back in the 
            // most basic form possible, hence we use a byte array.

            // For our example, we'll use some text that we have embedded in the resource section of the plug in, and dump that back.
            // Normally you'd want to pop up a dialog of some sort to allow the user to name the content, but that's overkill for this 
            // example, so we'll use the default name provided by the editor.

            byte[] defaultText = Encoding.UTF8.GetBytes(Resources.DEFAULT_TEXT);
            var stream = CommonEditorResources.MemoryStreamManager.GetStream(defaultText) as RecyclableMemoryStream;

            return Task.FromResult<(string, RecyclableMemoryStream)>((generatedName, stream));
        }

        /// <summary>Function to open a content object from this plugin.</summary>
        /// <param name="file">The file that contains the content.</param>
        /// <param name = "fileManager" > The file manager used to access other content files.</param>
        /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
        /// <param name="undoService">The undo service for the plug in.</param>
        /// <returns>A new IEditorContent object.</returns>
        /// <remarks>
        /// The <paramref name="scratchArea" /> parameter is the file system where temporary files to store transitory information for the plug in is stored. This file system is destroyed when the
        /// application or plug in is shut down, and is not stored with the project.
        /// </remarks>
        protected async override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService)
        {
            // This is the method that actually does all the work of setting up the content for editing. 
            // 
            // Even after creating a new piece of content, this method is still called.
            //
            // In here, we create and initialize our view model(s) for use with the content editor and read our content data into it.
            var content = new TextContent();
            StreamReader reader = null;

            try
            {
                // To start, we'll need to bring in our text content.
                // For this, we have to use the ContentFileManager service provided by the host application. This service allows us 
                // to perform simple read/write operations within the editor file system.
                //
                // We are given a copy of our content file by this method, so we can open a stream and read all of the text data.
                Stream textStream = ContentFileManager.OpenStream(file.Path, FileMode.Open);
                reader = new StreamReader(textStream, Encoding.UTF8, false, 80000, false);
                string text = await reader.ReadToEndAsync();
                reader.Dispose();

                // Trim the text down to 8,192 (16 KB) characters.  We don't need to render a book here.
                if (text.Length > 8192)
                {
                    text = text[..8192];
                }

                // This is a view model used for editing the color of the text.
                // Much like the main view on the text content control, which has a sub control panel for color picking, the main 
                // view model for the text content control will have a child view model for the color picker control.
                //
                // To activate the panel, we merely have to set the CurrentPanel property on the main view model to an instance 
                // of the panel view model and the control will activate and the user can access the associated editor control.
                var textColor = new TextColor();
                textColor.Initialize(new HostedPanelViewModelParameters(HostContentServices));

                // This is a service that we can use to modify the text. It's here to show how to use a service as a means of 
                // keeping in-line with MVVM rules.
                var textEditor = new TextEditorService();

                // Our concrete class for the view models will always have an Initialize method. This method is used to pass in services, 
                // data, etc... to the view model so we can perform operations and have access to functionality from the host application.
                //
                // A more complicated set up would define other services that take the basic services and provide more specialized 
                // functionality and pass that in to the view model via a custom injection class.
                content.Initialize(new TextContentParameters(text, textColor, _settingsViewModel, textEditor, undoService, fileManager, file, HostContentServices));

                return content;
            }
            finally
            {
                reader?.Dispose();
            }
        }

        /// <summary>Function to provide clean up for the plugin.</summary>
        protected override void OnShutdown()
        {
            // As mentioned in the OnInitialize method, this is where we dispose of resources and 
            // clean up our plug in data.
            //
            // Like the OnInitialize, this is called one time only when the plug in is unloaded at 
            // application shutdown.

            // This being an image resource, we should dispose of it right away.
            _noThumbnail?.Dispose();

            // If our application had settings, this is what we'd use to write the settings back to the disk.
            if (_settings is not null)
            {
                // Persist any settings.
                HostContentServices.ContentPlugInService.WriteContentSettings(SettingsName, _settings);
            }

            // And finally, always unregister the view model + view linkage.
            ViewFactory.Unregister<ISettings>();
            ViewFactory.Unregister<ITextContent>();

            base.OnShutdown();
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize()
        {
            // This is the main initialization function for the plug in.
            // This is called once while the plug in is being loaded at application start up.
            // Please note that some functionality may not be present at this time (the 
            // content file system specifically), and will cause an exception if used. 
            //
            // This method is typically meant for one-time initialization of resources or 
            // data required by the plug in. For resources that require clean up (e.g. 
            // IDisposable resources), the plug in should clean those up in the OnShutDown 
            // method.

            // At this point, we'll read in the settings for the plug in.
            // This will contain our default configuration for the plug in and editor(s) contained within.
            TextContentSettings settings = HostContentServices.ContentPlugInService.ReadContentSettings<TextContentSettings>(SettingsName);
            if (settings is not null)
            {
                _settings = settings;
            }

            // Setup a view model for the plug in settings.
            // We can use this to adjust or retrieve the plug in settings for the plug in from within our view model(s).
            _settingsViewModel = new Settings();
            _settingsViewModel.Initialize(new SettingsParameters(_settings, HostContentServices));

            // Set up our default preview image for the content now. We only need to do this one 
            // time, so this is the best place to do it.
            _noThumbnail = Resources.textviewer_no_thumb_96x96.ToGorgonImage();

            // This registers our view model interface with a callback method used to create 
            // the actual view (the control that is placed in our main area on the editor 
            // window). The editor application will use this to create our internal control(s) 
            // so that they can be displayed within the editor window. Without this, the 
            // editor will not know how to display our content as it has no knowledge of the 
            // content other than its a bunch of data. 
            ViewFactory.Register<ITextContent>(() => new TextContentView());

            // This registers our settings view model interface and the callback used to create 
            // the view. This view will show up under the Settings item on the File tab.            
            ViewFactory.Register<ISettings>(() => new TextContentSettingsPanel());
        }

        /// <summary>Function to determine if the content plugin can open the specified file.</summary>
        /// <param name="filePath">The path to the file to evaluate.</param>
        /// <returns>
        ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath" /> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        public bool CanOpenContent(string filePath)
        {
            // This code will tell the editor whether the content at the specified path can be opened by this 
            // plugin or not.
            //
            // This is necessary because there might be a specific format to the data, or some other unique 
            // property that must exist and examining the filename extension is not extensive enough for that.
            // The best way to determine that is to open the file and check its header contents. And we can 
            // do exactly that in this method.
            //             
            // However, for this simple example we'll just check the file extension.
            if (!filePath?.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                return false;
            }

            // Get the file here, we'll need to update its metadata so we can store some basic infor about the 
            // file for the editor.
            IContentFile file = ContentFileManager.GetFile(filePath);
            Debug.Assert(file is not null, $"File '{filePath}' doesn't exist, but it should!");

            // Just set up basic metadata for the file.
            // The metadata for a file contains extra information that the editor uses to help determine how 
            // best handle loading the file data. It can also contain various pieces of plug in specific 
            // information that can be evaluated when the content is opened or being edited.
            UpdateFileMetadataAttributes(file.Metadata.Attributes);

            return true;
        }

        // This returns the icon to be displayed in the file explorer panel in the application. 
        // The editor will link this icon to the SmallIconID GUID supplied in the properties above and 
        // will display this next to the file name.

        /// <summary>
        /// Function to retrieve the small icon for the content plug in.
        /// </summary>
        /// <returns>An image for the small icon.</returns>
        public Image GetSmallIcon() => Resources.text_viewer_example_20x20;

        /// <summary>Function to retrieve a thumbnail for the content.</summary>
        /// <param name="contentFile">The content file used to retrieve the data to build the thumbnail with.</param>
        /// <param name="filePath">The path to the thumbnail file to write into.</param>
        /// <param name="cancelToken">The token used to cancel the thumbnail generation.</param>
        /// <returns>A <see cref="IGorgonImage"/> containing the thumbnail image data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile" />, or the <paramref name="filePath" /> parameter is <b>null</b>.</exception>
        public Task<IGorgonImage> GetThumbnailAsync(IContentFile contentFile, string filePath, CancellationToken cancelToken)
        {
            if (contentFile is null)
            {
                throw new ArgumentNullException(nameof(contentFile));
            }

            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            // This is where we'd provide a thumbnail for our preview pane in the editor. 
            // For more complicated plug ins, we'd build up an image with the representation of our content 
            // (max of 256x256 resolution) to display in the editor.
            //
            // However, since this example doesn't really need that, just send back the default.
            // Note that we Clone the image so that the plug in will retain ownership of the image data.
            // Failure to do so may cause corruption.

            return Task.FromResult(_noThumbnail.Clone());
        }

        // This should be pretty self explanatory.  This returns our icon for a new text content file.
        // The editor will link this icon to the NewIconID GUID supplied in the properties above and 
        // will use this to notify when a new text content file should be created.        

        /// <summary>Function to retrieve the icon used for new content creation.</summary>
        /// <returns>An image for the icon.</returns>
        public Image GetNewIcon() => Resources.textviewer_example_new_24x24;
        #endregion

        #region Constructor/Finalizer.
        // When we construct the plug in object, we'll need to send back a friendly description 
        // for display purposes.

        /// <summary>Initializes a new instance of the ImageEditorPlugIn class.</summary>
        public TextViewerPlugin()
            : base("An example plugin for the Gorgon Editor that views text.")
        {
            SmallIconID = Guid.NewGuid();
            NewIconID = Guid.NewGuid();
        }
        #endregion
    }
}
