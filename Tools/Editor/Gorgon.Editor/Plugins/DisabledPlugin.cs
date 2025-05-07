
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
// Created: November 10, 2018 8:32:44 PM
// 

using Gorgon.Core;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor.Plugins;

/// <summary>
/// A plugin that was disabled for a reason
/// </summary>
internal class DisabledPlugin
    : IDisabledPlugin
{
    /// <summary>
    /// Property to return the code to indicate how the plugin was disabled.
    /// </summary>
    public DisabledReasonCode ReasonCode
    {
        get;
    }

    /// <summary>
    /// Property to return a description that explains why a plugin was disabled.
    /// </summary>
    public string Description
    {
        get;
    }

    /// <summary>
    /// Property to return the name of the disabled plugin.
    /// </summary>
    public string PluginName
    {
        get;
    }

    /// <summary>Property to return the assembly path.</summary>
    public string Path
    {
        get;
    }

    /// <summary>Initializes a new instance of the DisabledPlugin class.</summary>
    /// <param name="reasonCode">The code to indicate how the plugin was disabled.</param>
    /// <param name="PluginName">Name of the Plugin that was disabled.</param>
    /// <param name="desc">The human readable description that explains why the plugin was disabled.</param>
    /// <param name="path">The path to the plugin assembly.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="PluginName" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="PluginName" /> parameter is empty.</exception>
    public DisabledPlugin(DisabledReasonCode reasonCode, string pluginName, string desc, string path)
    {
        if (PluginName is null)
        {
            throw new ArgumentNullException(nameof(PluginName));
        }

        if (string.IsNullOrWhiteSpace(PluginName))
        {
            throw new ArgumentEmptyException(nameof(PluginName));
        }

        ReasonCode = reasonCode;
        PluginName = pluginName;
        Description = string.IsNullOrWhiteSpace(desc) ? Resources.GOREDIT_TEXT_DISABLED_plugin_DEFAULT_TEXT : desc.Trim();
        Path = path ?? string.Empty;
    }
}
