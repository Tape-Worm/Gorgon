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
// Created: December 12, 2018 4:43:24 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.Services;

/// <summary>
/// A system used to search through the file system for files.
/// </summary>
public class EditorContentSearchService
    : ISearchService<IContentFileExplorerSearchEntry>
{
    #region Variables.
    // The rows containing the editor files.
    private readonly IReadOnlyList<IContentFileExplorerSearchEntry> _rows;
    // The type of search keywords that can be used.
    private readonly Dictionary<string, string> _searchKeywords = new(StringComparer.CurrentCultureIgnoreCase)
    {
        { Resources.GOREDIT_SEARCH_DIRECTORY_TAG1, "directory" },
        { Resources.GOREDIT_SEARCH_DIRECTORY_TAG2, "directory" }
    };
    #endregion

    #region Methods.
    /// <summary>
    /// Function to parse the search text passed from the user to extract specific keywords.
    /// </summary>
    /// <param name="searchText">The search text to evaluate.</param>
    /// <returns>A tuple containing the type of keyword, and the search string.</returns>
    private (string keyword, string search) ParseSearchString(string searchText)
    {
        var searchKeyword = new KeyValuePair<string, string>(string.Empty, string.Empty);

        foreach (KeyValuePair<string, string> keyword in _searchKeywords)
        {
            if (!searchText.StartsWith(keyword.Key, StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }

            searchKeyword = keyword;
            break;
        }

        if ((string.IsNullOrWhiteSpace(searchKeyword.Value)) || (searchText.Length <= searchKeyword.Key.Length))
        {
            return (string.Empty, null);
        }

        // Get the keyword value.
        string keywordValue = searchText[(searchKeyword.Key.Length + 1)..].TrimStart();

        return string.IsNullOrWhiteSpace(keywordValue) ? (string.Empty, null) : (searchKeyword.Value, keywordValue);
    }

    /// <summary>
    /// Function to map a custom search keyword to a content attribute to allow for searching of content specific keywords.
    /// </summary>
    /// <param name="keyword">The keyword that the user will input.</param>
    /// <param name="attribute">The attribute in the content to map to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="keyword"/>, or the <paramref name="attribute"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="keyword"/>, or the <paramref name="attribute"/> parameter is empty.</exception>
    public void MapKeywordToContentAttribute(string keyword, string attribute)
    {
        if (keyword is null)
        {
            throw new ArgumentNullException(nameof(keyword));
        }

        if (attribute is null)
        {
            throw new ArgumentNullException(nameof(attribute));
        }

        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentEmptyException(nameof(keyword));
        }

        if (string.IsNullOrWhiteSpace(attribute))
        {
            throw new ArgumentEmptyException(nameof(attribute));
        }

        _searchKeywords[keyword] = attribute;
    }

    /// <summary>
    /// Function to extract the seach mode from the search text.
    /// </summary>
    /// <param name="searchText">The search text to evaluate.</param>
    /// <returns>The search mode and the raw search text.</returns>
    private (SearchMode mode, string searchText) ExtractSearchMode(string searchText)
    {
        SearchMode mode = SearchMode.Contains;

        // Check for search mode.
        if (searchText.Length > 1)
        {
            if ((searchText.Length >= 3) && (searchText[0] == '"') && (searchText[^1] == '"'))
            {
                mode = SearchMode.OnlyWord;
                searchText = searchText[1..^1];
            }
            else if (searchText[0] == '*')
            {
                mode = SearchMode.EndsWith;
                searchText = searchText[1..].Replace("*", string.Empty);
            }
            else if (searchText[^1] == '*')
            {
                mode = SearchMode.StartsWith;
                searchText = searchText[0..^1].Replace("*", string.Empty);
            }
        }
        else
        {
            if ((string.IsNullOrWhiteSpace(searchText)) || (string.Equals(searchText, "*", StringComparison.CurrentCultureIgnoreCase)))
            {
                mode = SearchMode.All;
            }
        }

        return (mode, searchText);
    }

    /// <summary>
    /// Function to perform the actual search.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>A list of items that match the search, or <b>null</b> search should be disabled, or an empty list if no matches were found.</returns>
    public IEnumerable<IContentFileExplorerSearchEntry> Search(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return Array.Empty<IContentFileExplorerSearchEntry>();
        }

        // Extract any keyword that might be embedded in the start of the search text.
        (string keyword, string updatedSearchText) = ParseSearchString(searchText);

        // If we specified a valid keyword, then use the search text that follows it. Otherwise, just use the text as-is.
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            searchText = updatedSearchText;
        }

        (SearchMode mode, string modeSearchText) = ExtractSearchMode(searchText);

        // Test code for search:
        var searchResults = new List<IContentFileExplorerSearchEntry>();
        var dirResults = new List<IContentFileExplorerSearchEntry>();
        bool isDirSearch = string.Equals(keyword, "directory", StringComparison.OrdinalIgnoreCase);

        foreach (IContentFileExplorerSearchEntry row in _rows)
        {
            if (searchResults.Contains(row))
            {
                continue;
            }

            string path = row.FullPath;

            switch (mode)
            {
                case SearchMode.OnlyWord when string.Equals(path, modeSearchText, StringComparison.CurrentCultureIgnoreCase):
                case SearchMode.StartsWith when path.StartsWith(modeSearchText, StringComparison.CurrentCultureIgnoreCase):
                case SearchMode.EndsWith when path.EndsWith(modeSearchText, StringComparison.CurrentCultureIgnoreCase):
                case SearchMode.Contains when (path.IndexOf(modeSearchText, StringComparison.CurrentCultureIgnoreCase) != -1):
                case SearchMode.All:
                    if (isDirSearch)
                    {
                        if (row.IsDirectory)
                        {
                            dirResults.Add(row);
                        }
                    }
                    else
                    {
                        searchResults.Add(row);
                    }
                    break;
                default:
                    // If we can't find what we're looking for on the full path, try just name.
                    path = row.Name;

                    switch (mode)
                    {
                        case SearchMode.OnlyWord when string.Equals(path, modeSearchText, StringComparison.CurrentCultureIgnoreCase):
                        case SearchMode.StartsWith when path.StartsWith(modeSearchText, StringComparison.CurrentCultureIgnoreCase):
                        case SearchMode.EndsWith when path.EndsWith(modeSearchText, StringComparison.CurrentCultureIgnoreCase):
                        case SearchMode.Contains when (path.IndexOf(modeSearchText, StringComparison.CurrentCultureIgnoreCase) != -1):
                        case SearchMode.All:
                            if (isDirSearch)
                            {
                                if (row.IsDirectory)
                                {
                                    dirResults.Add(row);
                                }
                            }
                            else
                            {
                                searchResults.Add(row);
                            }
                            break;
                    }
                    break;
            }
        }

        // If we have a directory search, then add all files under that directory.
        if (!isDirSearch)
        {
            return searchResults;
        }

        foreach (IContentFileExplorerSearchEntry entry in dirResults)
        {
            searchResults.AddRange(_rows.Where(item => string.Equals(item.FullPath, entry.FullPath + item.Name, StringComparison.OrdinalIgnoreCase)));
        }

        return searchResults;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="EditorContentSearchService"/> class.</summary>
    /// <param name="rows">The list of rows from the data grid containing the editor files.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="rows" /> parameter is <strong>null</strong>.</exception>
    public EditorContentSearchService(IReadOnlyList<IContentFileExplorerSearchEntry> rows) => _rows = rows ?? throw new ArgumentNullException(nameof(rows));
    #endregion
}
