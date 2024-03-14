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
using System.IO;
using System.Linq;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Services;

/// <summary>
/// A system used to search through the file system for files.
/// </summary>
internal class FileSystemSearchSystem
    : ISearchService<IFile>
{
    #region Variables.
    // The root of the file system.
    private readonly IDirectory _rootDirectory;
    // The type of search keywords that can be used.
    private readonly Dictionary<string, string> _searchKeywords = new(StringComparer.CurrentCultureIgnoreCase)
    {
        {
            Resources.GOREDIT_SEARCH_KEYWORD_CONTENT_TYPE,
            CommonEditorConstants.ContentTypeAttr
        },
        {
            Resources.GOREDIT_SEARCH_KEYWORD_DEPENDENCIES,
            Resources.GOREDIT_SEARCH_KEYWORD_DEPENDENCIES
        },
        {
            Resources.GOREDIT_SEARCH_KEYWORD_DEPENDS_ON,
            Resources.GOREDIT_SEARCH_KEYWORD_DEPENDS_ON
        }        
    };
    #endregion

    #region Methods.
    /// <summary>
    /// Function to parse the search text passed from the user to extract specific keywords.
    /// </summary>
    /// <param name="searchText">The search text to evaluate.</param>
    /// <returns>A tuple containing the type of keyword, the keyword value, and the search string.</returns>
    private (string keyword, string value, string search) ParseSearchString(string searchText)
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
            return (string.Empty, null, null);
        }

        // Get the keyword value.
        string keywordValue = searchText = searchText[(searchKeyword.Key.Length + 1)..].TrimStart();
        int spaceIndex = keywordValue.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase);

        if (string.IsNullOrWhiteSpace(keywordValue))
        {
            return (string.Empty, null, null);
        }

        if (spaceIndex > -1)
        {
            keywordValue = searchText[..spaceIndex];
            searchText = searchText[spaceIndex..].TrimStart();
        }
        else
        {
            searchText = string.Empty;
        }

        return (searchKeyword.Value, keywordValue, searchText);
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
    /// Function to check if the requested keyword value is within the attribute metadata for the file node content.
    /// </summary>
    /// <param name="file">The file to evaluate.</param>
    /// <param name="attributeName">The name of the attribute to look up.</param>
    /// <param name="attributeValue">The value to compare.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    private bool CheckItemAttribute(IFile file, string attributeName, string attributeValue)
    {
        if (file.Metadata is null)
        {
            return false;
        }

        if ((string.IsNullOrWhiteSpace(attributeName)) || (string.IsNullOrWhiteSpace(attributeValue)))
        {
            return false;
        }

        return file.Metadata.Attributes.TryGetValue(attributeName, out string value) && string.Equals(value, attributeValue, StringComparison.CurrentCultureIgnoreCase);
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
    /// Function to determine if a file name matches the specified wildcard mode.
    /// </summary>
    /// <param name="mode">The wild card mode currently active.</param>
    /// <param name="file">The file to evaluate.</param>
    /// <param name="modeSearchText">The search text with the wild card info stripped.</param>
    /// <returns></returns>
    private bool MatchesWildcard(SearchMode mode, IFile file, string modeSearchText)
    {
        switch (mode)
        {
            case SearchMode.OnlyWord when string.Equals(file.Name, modeSearchText, StringComparison.CurrentCultureIgnoreCase):
            case SearchMode.StartsWith when file.Name.StartsWith(modeSearchText, StringComparison.CurrentCultureIgnoreCase):
            case SearchMode.EndsWith when file.Name.EndsWith(modeSearchText, StringComparison.CurrentCultureIgnoreCase):
            case SearchMode.Contains when (file.Name.IndexOf(modeSearchText, StringComparison.CurrentCultureIgnoreCase) != -1):
            case SearchMode.All:
                return true;                    
        }

        return false;
    }

    /// <summary>
    /// Function to check special keywords prior to peforming a regular search.
    /// </summary>
    /// <param name="keyword">The keyword to evaluate.</param>
    /// <param name="mode">The current search mode.</param>
    /// <param name="modeSearchText">The text from the search.</param>
    /// <param name="searchResults">The list of search results to populate.</param>
    /// <returns><b>true</b> if regular search should be stopped, <b>false</b> if it should continue.</returns>
    private bool CheckKeyword(string keyword, SearchMode mode, string modeSearchText, List<IFile> searchResults)
    {
        IEnumerable<IFile> files = _rootDirectory.Directories.Traverse(d => d.Directories).SelectMany(f => f.Files).Concat(_rootDirectory.Files);

        if (string.Equals(keyword, Resources.GOREDIT_SEARCH_KEYWORD_DEPENDENCIES, StringComparison.CurrentCultureIgnoreCase))
        {
            IFile searchFile = files.FirstOrDefault(item => MatchesWildcard(mode, item, modeSearchText));

            if (searchFile is null)
            {
                return true;
            }

            // Find the files.
            foreach (string dependencyPath in searchFile.Metadata.DependsOn.SelectMany(item => item.Value))
            {
                IFile dependencyFile = files.FirstOrDefault(item => string.Equals(item.FullPath, dependencyPath, StringComparison.CurrentCultureIgnoreCase));

                if (dependencyFile is not null)
                {
                    searchResults.Add(dependencyFile);
                }
            }

            return true;
        }

        if (string.Equals(keyword, Resources.GOREDIT_SEARCH_KEYWORD_DEPENDS_ON, StringComparison.CurrentCultureIgnoreCase))
        {
            IFile[] searchFiles = files.Where(item => MatchesWildcard(mode, item, modeSearchText)).ToArray();

            if (searchFiles.Length == 0)
            {
                return true;
            }

            // Find the files.
            foreach (IFile dependencyFile in files.Where(item => Array.IndexOf(searchFiles, item) == -1))
            {
                foreach (IFile searchFile in searchFiles)
                {
                    if ((!searchResults.Contains(dependencyFile)) && (dependencyFile.Metadata.DependsOn.SelectMany(item => item.Value)
                                                                                                       .Any(item => string.Equals(searchFile.FullPath, item, StringComparison.CurrentCultureIgnoreCase))))
                    {
                        searchResults.Add(dependencyFile);
                    }
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Function to perform the actual search.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>A list of items that match the search, or <b>null</b> search should be disabled, or an empty list if no matches were found.</returns>
    public IEnumerable<IFile> Search(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return Array.Empty<IFile>();
        }

        // Extract any keyword that might be embedded in the start of the search text.
        (string keyword, string keywordValue, string updatedSearchText) = ParseSearchString(searchText);

        // If we specified a valid keyword, then use the search text that follows it. Otherwise, just use the text as-is.
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            searchText = updatedSearchText;
        }

        (SearchMode mode, string modeSearchText) = ExtractSearchMode(string.IsNullOrWhiteSpace(searchText) ? keywordValue : searchText);

        // Test code for search:
        var searchResults = new List<IFile>();

        if ((!string.IsNullOrWhiteSpace(keyword)) && (CheckKeyword(keyword, mode, modeSearchText, searchResults)))
        {
            return searchResults;
        }

        foreach (IFile node in _rootDirectory.Directories.Traverse(d => d.Directories).SelectMany(f => f.Files).Concat(_rootDirectory.Files))
        {
            if (searchResults.Contains(node))
            {
                continue;
            }

            if ((!string.IsNullOrWhiteSpace(keyword)) && (!CheckItemAttribute(node, keyword, keywordValue)))
            {
                continue;
            }                

            if (MatchesWildcard(mode, node, modeSearchText))
            {
                searchResults.Add(node);
            }
        }

        return searchResults;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FileSystemSearchSystem"/> class.</summary>
    /// <param name="rootDirectory">The root directory for the file system.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="rootDirectory" /> parameter is <strong>null</strong>.</exception>
    public FileSystemSearchSystem(IDirectory rootDirectory) => _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
    #endregion
}
