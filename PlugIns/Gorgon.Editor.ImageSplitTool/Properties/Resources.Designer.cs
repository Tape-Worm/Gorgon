﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gorgon.Editor.ImageSplitTool.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Gorgon.Editor.ImageSplitTool.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap folder_20x20 {
            get {
                object obj = ResourceManager.GetObject("folder_20x20", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Output folder.
        /// </summary>
        internal static string GORIST_CAPTION_FOLDER_SELECT {
            get {
                return ResourceManager.GetString("GORIST_CAPTION_FOLDER_SELECT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The splitting operation has been cancelled.
        ///
        ///Would you like to close this window?.
        /// </summary>
        internal static string GORIST_CONFIRM_CLOSE {
            get {
                return ResourceManager.GetString("GORIST_CONFIRM_CLOSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file &apos;{0}&apos; already exists.
        ///
        ///Would you like to overwrite it?.
        /// </summary>
        internal static string GORIST_CONFIRM_FILE_EXISTS {
            get {
                return ResourceManager.GetString("GORIST_CONFIRM_FILE_EXISTS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A tool for splitting a texture atlas into smaller images using the sprites associated with the atlas..
        /// </summary>
        internal static string GORIST_DESC_BUTTON {
            get {
                return ResourceManager.GetString("GORIST_DESC_BUTTON", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select a folder to save the images and sprites into..
        /// </summary>
        internal static string GORIST_DESC_FOLDER_SELECT {
            get {
                return ResourceManager.GetString("GORIST_DESC_FOLDER_SELECT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file &apos;{0}&apos; is open in the editor and cannot be written to.
        ///
        ///Please close the file, and try again..
        /// </summary>
        internal static string GORIST_ERR_CANNOT_ACCESS_FILESYSTEM_LOCK {
            get {
                return ResourceManager.GetString("GORIST_ERR_CANNOT_ACCESS_FILESYSTEM_LOCK", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error browsing the file system folders.
        ///
        ///Please try again..
        /// </summary>
        internal static string GORIST_ERR_FOLDER_SELECT {
            get {
                return ResourceManager.GetString("GORIST_ERR_FOLDER_SELECT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error launching the atlas image splitter tool.
        ///
        ///Please try again..
        /// </summary>
        internal static string GORIST_ERR_LAUNCH {
            get {
                return ResourceManager.GetString("GORIST_ERR_LAUNCH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error saving the sprites.
        ///
        ///Please try again..
        /// </summary>
        internal static string GORIST_ERR_SAVE {
            get {
                return ResourceManager.GetString("GORIST_ERR_SAVE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error searching the files.
        ///
        ///Please try again..
        /// </summary>
        internal static string GORIST_ERR_SEARCH {
            get {
                return ResourceManager.GetString("GORIST_ERR_SEARCH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error splitting the atlas.
        ///
        ///Please try again..
        /// </summary>
        internal static string GORIST_ERR_SPLIT {
            get {
                return ResourceManager.GetString("GORIST_ERR_SPLIT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Images.
        /// </summary>
        internal static string GORIST_GROUP_BUTTON {
            get {
                return ResourceManager.GetString("GORIST_GROUP_BUTTON", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Gorgon Image Splitter.
        /// </summary>
        internal static string GORIST_PLUGIN_DESC {
            get {
                return ResourceManager.GetString("GORIST_PLUGIN_DESC", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Atlas Splitter.
        /// </summary>
        internal static string GORIST_TEXT_BUTTON {
            get {
                return ResourceManager.GetString("GORIST_TEXT_BUTTON", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loading image....
        /// </summary>
        internal static string GORIST_TEXT_LOADING {
            get {
                return ResourceManager.GetString("GORIST_TEXT_LOADING", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Processing &apos;{0}&apos;....
        /// </summary>
        internal static string GORIST_TEXT_PROCESSING {
            get {
                return ResourceManager.GetString("GORIST_TEXT_PROCESSING", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select an image..
        /// </summary>
        internal static string GORIST_TEXT_SELECT_IMAGE {
            get {
                return ResourceManager.GetString("GORIST_TEXT_SELECT_IMAGE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap image_split_16x16 {
            get {
                object obj = ResourceManager.GetObject("image_split_16x16", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap image_split_48x48 {
            get {
                object obj = ResourceManager.GetObject("image_split_48x48", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Transparency_Pattern {
            get {
                object obj = ResourceManager.GetObject("Transparency_Pattern", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
