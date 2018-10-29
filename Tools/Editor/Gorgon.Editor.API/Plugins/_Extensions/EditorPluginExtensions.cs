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
// Created: October 12, 2018 12:52:19 PM
// 
#endregion

using Gorgon.Editor.Properties;

namespace Gorgon.Editor.Plugins
{
    /// <summary>
    /// Extension functionality relating to editor plug ins.
    /// </summary>
    public static class EditorPluginExtensions
    {
        /// <summary>
        /// Function to retrieve a friendly description of a <see cref="PluginType"/> value.
        /// </summary>
        /// <param name="pluginType">The plug in type to evaluate.</param>
        /// <returns>The friendly description.</returns>
        public static string GetDescription(this PluginType pluginType)
        {
            switch (pluginType)
            {
                case PluginType.Writer:
                    return Resources.GOREDIT_PLUGIN_TYPE_WRITER;
                case PluginType.Content:
                    return Resources.GOREDIT_PLUGIN_TYPE_CONTENT;
                case PluginType.Tool:
                    return Resources.GOREDIT_PLUGIN_TYPE_TOOL;
                default:
                    return Resources.GOREDIT_PLUGIN_TYPE_UNKNOWN;
            }
        }
    }
}
