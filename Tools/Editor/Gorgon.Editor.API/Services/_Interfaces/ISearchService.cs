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
// Created: December 12, 2018 5:23:41 PM
// 
#endregion

using Gorgon.Core;

namespace Gorgon.Editor.Services;

#region Enums.
/// <summary>
/// The mode of search as defined by the user.
/// </summary>
public enum SearchMode
{
    /// <summary>
    /// Search for the word that contains the value typed.
    /// </summary>
    Contains = 0,
    /// <summary>
    /// Search everything, use a single * to enable this.
    /// </summary>
    All = 1,
    /// <summary>
    /// Search for the exact word typed in.  Use quotes around the word to enable this.
    /// </summary>
    OnlyWord = 2,
    /// <summary>
    /// Search for the word that starts with the value typed.  End the search string with a * to enable this.
    /// </summary>
    StartsWith = 3,
    /// <summary>
    /// Search for the word that ends with the value typed.  Start the search string with a * to enable this.
    /// </summary>
    EndsWith = 4
}
#endregion

/// <summary>
/// An interface to provide access to a search service.
/// </summary>
/// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
public interface ISearchService<out T>
    where T : IGorgonNamedObject
{
    /// <summary>
    /// Function to map a custom search keyword to a content attribute to allow for searching of content specific keywords.
    /// </summary>
    /// <param name="keyword">The keyword that the user will input.</param>
    /// <param name="attribute">The attribute in the content to map to.</param>
    void MapKeywordToContentAttribute(string keyword, string attribute);

    /// <summary>
    /// Function to perform the actual search.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>A list of items that match the search, or <b>null</b> search should be disabled, or an empty list if no matches were found.</returns>
    IEnumerable<T> Search(string searchText);
}
