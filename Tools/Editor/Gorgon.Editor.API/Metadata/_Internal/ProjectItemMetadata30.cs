
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
// Created: September 5, 2018 12:35:20 PM
// 



using Newtonsoft.Json;

namespace Gorgon.Editor.Metadata;

/// <summary>
/// Metadata for a project item that is included in the project
/// </summary>
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
internal class ProjectItemMetadata30
{

    /// <summary>
    /// Property to set or return the name of the plugin associated with the metadata file path.
    /// </summary>
    /// <remarks>
    /// If this value is <b>null</b>, then the plugin hasn't been set.  If it's an empty string, then no plugin is associated with this metadata.
    /// </remarks>
    [JsonProperty]
    public string PlugInName
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the custom attributes for this metadata.
    /// </summary>
    [JsonProperty]
    public Dictionary<string, string> Attributes
    {
        get;
        private set;
    } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Property to return the list of item paths that this item depends on.
    /// </summary>
    [JsonProperty(PropertyName = "Dependencies")]
    public Dictionary<string, string> DependsOn
    {
        get;
        private set;
    } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

}
