
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: October 12, 2018 12:48:16 PM
// 

using Gorgon.Editor.UI;
using Gorgon.Plugins;

namespace Gorgon.Editor.Plugins;

/// <summary>
/// The type of plugin supported by the editor
/// </summary>
/// <remarks>
/// <para>
/// This indicates the supported plugin types that the editor recognizes (with the exception of the <see cref="Unknown"/> value). Any plugin created for use with the application <b>must</b> 
/// be one of these types, otherwise it will not be loaded by the host application
/// </para>
/// </remarks>
public enum PluginType
{
    /// <summary>
    /// Plug in type is not known.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Plug in is used to write out editor files.
    /// </summary>
    Writer = 1,
    /// <summary>
    /// Plug in is used to build content.
    /// </summary>
    Content = 2,
    /// <summary>
    /// Plug in is a pack file reader.
    /// </summary>
    Reader = 3,
    /// <summary>
    /// Plug in is used for a utility.
    /// </summary>
    Tool = 4,
    /// <summary>
    /// Plug in is used to import content.
    /// </summary>
    ContentImporter = 5
}

/// <summary>
/// The base class for an editor plugin
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EditorPlugin"/> class
/// </remarks>
/// <param name="description">Optional description of the Plugin.</param>
public abstract class EditorPlugin(string description)
        : GorgonPlugin(description)
{

    // Empty string array for IsPluginAvailable.
    private readonly IReadOnlyList<string> _defaultPluginAvailablity = [];

    /// <summary>
    /// Property to return the common services from the host application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Plug in developers that implement a common plugin type based on this base type, should assign this value to allow access to the common services supplied by the host application.
    /// </para>
    /// <para>
    /// If the common services are not required, then this can be <b>null</b>.
    /// </para>
    /// <para>
    /// Plug in developers are responsible providing a mechanism for passing in the common services to the property.
    /// </para>
    /// </remarks>
    /// <seealso cref="IHostServices"/>
    protected IHostServices HostServices
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the type of this plugin.
    /// </summary>
    /// <remarks>
    /// The <see cref="Plugins.PluginType"/> returned for this property indicates the general plugin functionality. 
    /// </remarks>
    /// <seealso cref="Plugins.PluginType"/>
    public abstract PluginType PluginType
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the settings interface for this plugin.
    /// </summary>
    /// <returns>The settings interface view model.</returns>
    /// <remarks>
    /// <para>
    /// Implementors who wish to supply customizable settings for their plugins from the main "Settings" area in the application can override this method and return a new view model based on 
    /// the base <see cref="ISettingsCategory"/> type. Returning <b>null</b> will mean that the plugin does not have settings that can be managed externally.
    /// </para>
    /// <para>
    /// Plug ins must register the view associated with their settings panel via the <see cref="ViewFactory.Register{T}(Func{Control})"/> method when the plugin first loaded, 
    /// or else the panel will not show in the main settings area.
    /// </para>
    /// </remarks>
    protected virtual ISettingsCategory OnGetSettings() => null;

    /// <summary>
    /// Function to determine if this plugin is usable or not.
    /// </summary>
    /// <returns>A list of string values containing information about why this plugin may not be usable at this time.</returns>
    /// <remarks>
    /// <para>
    /// Plug in developers should override this method if the plugin has dependencies, and those dependencies cannot be located. This can also be used by a developer to disable the plugin should it 
    /// be necessary to do so.
    /// </para>
    /// <para>
    /// This method should never return <b>null</b>, only an empty array if no problems are present.
    /// </para>
    /// </remarks>
    protected virtual IReadOnlyList<string> OnGetPluginAvailability() => _defaultPluginAvailablity;

    /// <summary>
    /// Function to retrieve the settings interface for the plugin.
    /// </summary>
    /// <returns>The base settings view model.</returns>
    /// <remarks>
    /// <para>
    /// This will return a view model that can be used to modify plugin settings on the main settings area in the application. If this method returns <b>null</b>, then the settings will not show in 
    /// the main settings area of the application.
    /// </para>
    /// </remarks>
    public ISettingsCategory GetPluginSettings() => OnGetSettings();

    /// <summary>
    /// Function to determine if this plugin is usable or not.
    /// </summary>
    /// <returns>A list of string values containing information about why this plugin may not be usable at this time, or an empty array if the plugin can be used without issue.</returns>
    /// <remarks>
    /// <para>
    /// Use this to determine if the plugin is able to be used in the application. If it is not available, it may be that a dependency is broken, or some other issue is keeping the plugin from 
    /// working correctly.
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> IsPluginAvailable() => OnGetPluginAvailability();

}
