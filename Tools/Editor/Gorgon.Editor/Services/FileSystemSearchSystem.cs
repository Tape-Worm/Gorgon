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
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A system used to search through the file system for files.
    /// </summary>
    internal class FileSystemSearchSystem
        : ISearchService<IFileExplorerNodeVm>
    {
        #region Variables.
        // The root of the file system.
        private readonly IFileExplorerNodeVm _rootNode;
        // The type of search keywords that can be used.
        private readonly Dictionary<string, string> _searchKeywords = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
        {
            { Resources.GOREDIT_SEARCH_KEYWORD_CONTENT_TYPE, EditorContentCommon<IContentViewModelInjection>.ContentTypeAttr }
        };
        #endregion

        #region Properties.

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
            string keywordValue = searchText = searchText.Substring(searchKeyword.Key.Length + 1).TrimStart();
            int spaceIndex = keywordValue.IndexOf(" ", StringComparison.CurrentCultureIgnoreCase);

            if (string.IsNullOrWhiteSpace(keywordValue))
            {
                return (string.Empty, null, null);
            }

            if (spaceIndex > -1)
            {
                keywordValue = searchText.Substring(0, spaceIndex);
                searchText = searchText.Substring(spaceIndex).TrimStart();
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
            if (keyword == null)
            {
                throw new ArgumentNullException(nameof(keyword));
            }

            if (attribute == null)
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
        /// <param name="node">The file node to evaluate.</param>
        /// <param name="attributeName">The name of the attribute to look up.</param>
        /// <param name="attributeValue">The value to compare.</param>
        /// <returns></returns>
        private bool CheckItemAttribute(IFileExplorerNodeVm node, string attributeName, string attributeValue)
        {
            if ((!node.IsContent) || (node.Metadata == null))
            {
                return false;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((string.IsNullOrWhiteSpace(attributeName)) || (string.IsNullOrWhiteSpace(attributeValue)))
            {
                return false;
            }
#pragma warning restore IDE0046 // Convert to conditional expression
            return !node.Metadata.Attributes.TryGetValue(attributeName, out string value)
                ? false
                : string.Equals(value, attributeValue, StringComparison.CurrentCultureIgnoreCase);
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
                if ((searchText.Length > 3) && (searchText[0] == '"') && (searchText[searchText.Length - 1] == '"'))
                {
                    mode = SearchMode.OnlyWord;
                    searchText = searchText.Substring(1, searchText.Length - 2);
                }
                else if (searchText[0] == '*')
                {
                    mode = SearchMode.EndsWith;
                    searchText = searchText.Substring(1).Replace("*", string.Empty);
                }
                else if (searchText[searchText.Length - 1] == '*')
                {
                    mode = SearchMode.StartsWith;
                    searchText = searchText.Substring(0, searchText.Length - 1).Replace("*", string.Empty);
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
        public IEnumerable<IFileExplorerNodeVm> Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return new IFileExplorerNodeVm[0];
            }
            
            // Extract any keyword that might be embedded in the start of the search text.
            (string keyword, string keywordValue, string updatedSearchText) = ParseSearchString(searchText);

            // If we specified a valid keyword, then use the search text that follows it. Otherwise, just use the text as-is.
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                searchText = updatedSearchText;
            }

            (SearchMode mode, string modeSearchText) = ExtractSearchMode(searchText);            

            // Test code for search:
            var searchResults = new List<IFileExplorerNodeVm>();

            foreach (IFileExplorerNodeVm node in _rootNode.Children.Traverse(n => n.Children).Where(n => !n.AllowChildCreation))
            {
                if (searchResults.Contains(node))
                {
                    continue;
                }

                if ((!string.IsNullOrWhiteSpace(keyword)) && (!CheckItemAttribute(node, keyword, keywordValue)))
                {
                    continue;
                }

                switch (mode)
                {
                    case SearchMode.OnlyWord when string.Equals(node.Name, searchText, StringComparison.CurrentCultureIgnoreCase):
                    case SearchMode.StartsWith when node.Name.StartsWith(searchText, StringComparison.CurrentCultureIgnoreCase):
                    case SearchMode.EndsWith when node.Name.EndsWith(searchText, StringComparison.CurrentCultureIgnoreCase):
                    case SearchMode.Contains when (node.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) != -1):                        
                    case SearchMode.All:
                        searchResults.Add(node);
                        break;
                }
            }

            return searchResults;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.Services.FileSystemSearchSystem"/> class.</summary>
        /// <param name="rootNode">The root node for the file system.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="rootNode" /> parameter is <strong>null</strong>.</exception>
        public FileSystemSearchSystem(IFileExplorerNodeVm rootNode) => _rootNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
        #endregion
    }
}
