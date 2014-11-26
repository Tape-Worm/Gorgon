﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GorgonLibrary.Examples.Properties {
	/// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GorgonLibrary.Examples.Properties.Resources", typeof(Resources).Assembly);
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
        internal static System.Drawing.Bitmap Default_128x128 {
            get {
                object obj = ResourceManager.GetObject("Default_128x128", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;Examples&gt;
        ///  &lt;Category name=&quot;GorgonLibrary.Common&quot;&gt;
        ///    &lt;Example icon=&quot;1&quot; caption=&quot;Example 1 - Core functions&quot; path=&quot;Gorgon.Common\Example001\bin\Debug\Example001.exe&quot;&gt;
        ///      The common library houses several utility functions such as:
        ///      
        ///      &amp;#x00b7; Basic dialogs.     &amp;#x00b7; System information.
        ///      &amp;#x00b7; Various extensions.      &amp;#x00b7; Debugging utilities.
        ///      &amp;#x00b7; Plug-in support.     &amp;#x00b7; Log files.
        ///      &amp;#x00b7; Base named objec [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Examples {
            get {
                return ResourceManager.GetString("Examples", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No root node found in example list..
        /// </summary>
        internal static string GOR_NO_ROOT_EXAMPLES {
            get {
                return ResourceManager.GetString("GOR_NO_ROOT_EXAMPLES", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loading....
        /// </summary>
        internal static string GOR_PLEASE_WAIT {
            get {
                return ResourceManager.GetString("GOR_PLEASE_WAIT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Gorgon_2_x_Logo_Full {
            get {
                object obj = ResourceManager.GetObject("Gorgon_2_x_Logo_Full", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
