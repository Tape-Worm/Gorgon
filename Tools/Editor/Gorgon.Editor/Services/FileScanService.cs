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
// Created: December 17, 2018 1:17:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Editor.Content;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to scan the files in the file system for content plugin associations.
    /// </summary>
    internal class FileScanService : IFileScanService
    {
        #region Variables.
        // The content plugins to scan through.
        private readonly IContentPluginManagerService _contentPlugins;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to compare two dependency dictionaries for equality.
        /// </summary>
        /// <param name="first">The first dictionary to compare.</param>
        /// <param name="second">The second dictionary to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        private bool CompareDependencyLists(Dictionary<string, string> first, Dictionary<string, string> second)
        {
            foreach (KeyValuePair<string, string> firstItem in first)
            {
                if ((!second.TryGetValue(firstItem.Key, out string secondValue))
                    || (!string.Equals(firstItem.Value, secondValue, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Function to perform the scan used to determine whether a content file has an associated plugin or not.
        /// </summary>
        /// <param name="node">The node to scan.</param>
        /// <param name="contentFileManager">The file manager used to manage content file data.</param>
        /// <param name="scanProgress">The callback method used to report progress of the scan.</param>
        /// <param name="deepScan"><b>true</b> to perform a more time consuming scan, or <b>false</b> to just scan by file name extension.</param>
        /// <param name="forceScan">[Optional] <b>true</b> to force the scan, even if content metadata is already available, or <b>false</b> to skip files with content metadata already.</param>
        /// <returns><b>true</b> if the content plugin metadata was updated, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="node"/>, <paramref name="contentFileManager"/> or the <paramref name="scanProgress"/> parameter is <b>null</b>.</exception>        
        public bool Scan(IFileExplorerNodeVm node, IContentFileManager contentFileManager, Action<string, int, int> scanProgress, bool deepScan, bool forceScan = false)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (contentFileManager == null)
            {
                throw new ArgumentNullException(nameof(contentFileManager));
            }

            if (scanProgress == null)
            {
                throw new ArgumentNullException(nameof(scanProgress));
            }

            IEnumerable<IFileExplorerNodeVm> contentFiles;            
            int fileCount;

            if (node.Children.Count > 0)
            {
                contentFiles = node.Children.Traverse(n => n.Children)
                                             .Where(n => ((n.Metadata != null) && (n.IsContent) && ((forceScan) || (n.Metadata.ContentMetadata == null))));
                fileCount = contentFiles.Count();
            }
            else
            {
                contentFiles = new IFileExplorerNodeVm[] { node };
                fileCount = 1;
            }

            if (fileCount == 0)
            {
                return false;
            }

            bool result = false;
            int count = 0;
            var prevDeps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach(IContentFile contentFile in contentFiles.OfType<IContentFile>())
            {                
                string pluginName = contentFile.Metadata.PluginName;

                if (forceScan)
                {
                    contentFile.Metadata.ContentMetadata = null;
                    contentFile.Metadata.Dependencies.Clear();
                }

                prevDeps.Clear();
                foreach (KeyValuePair<string, string> dep in contentFile.Metadata.Dependencies)
                {
                    prevDeps[dep.Key] = dep.Value;
                }                

                if (_contentPlugins.AssignContentPlugin(contentFile, contentFileManager, !deepScan))
                {                    
                    if ((!string.Equals(pluginName, contentFile.Metadata.PluginName, StringComparison.OrdinalIgnoreCase))
                        || (!CompareDependencyLists(contentFile.Metadata.Dependencies, prevDeps)))
                    {
                        result = true;
                    }
                }

                scanProgress.Invoke(contentFile.Path, ++count, fileCount);
            }

            return result;
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.Services.FileScanService"/> class.</summary>
        /// <param name="contentPlugins">The application content plugin manager service.</param>
        /// <exception cref="ArgumentNullException"> Thrown when the <paramref name="contentPlugins"/> parameter is <strong>null</strong>.</exception>
        public FileScanService(IContentPluginManagerService contentPlugins) => _contentPlugins = contentPlugins ?? throw new ArgumentNullException(nameof(contentPlugins));
        #endregion
    }
}
