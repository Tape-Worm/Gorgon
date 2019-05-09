#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: May 7, 2019 11:50:04 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gorgon.Editor.Services;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;

namespace Gorgon.Editor.TextureAtlasTool
{
    /// <summary>
    /// The sprite file list view model.
    /// </summary>
    internal class SpriteFiles
        : ViewModelBase<SpriteFilesParameters>, ISpriteFiles
    {
        #region Variables.
        // The busy state service.
        private IBusyStateService _busyService;
        // The messaging display service.
        private IMessageDisplayService _messageDisplay;
        // The service used to search through the files.
        private ISearchService<IContentFileExplorerSearchEntry> _searchService;
        // The list of selected files.
        private readonly List<ContentFileExplorerFileEntry> _selected = new List<ContentFileExplorerFileEntry>();
        // Flag to indicate that the file selector is active.
        private bool _isActive = true;
        #endregion

        #region Properties.
        /// <summary>Property to set or return whether the sprite loader is active or not.</summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isActive = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the sprite file entries.
        /// </summary>
        public IReadOnlyList<ContentFileExplorerDirectoryEntry> SpriteFileEntries
        {
            get;
            private set;
        }

        /// <summary>Property to return the list of selected files.</summary>
        public IReadOnlyList<ContentFileExplorerFileEntry> SelectedFiles
        {
            get => _selected;
            private set
            {
                if (value == null)
                {
                    value = Array.Empty<ContentFileExplorerFileEntry>();
                }

                if (value.SequenceEqual(_selected))
                {
                    return;
                }

                OnPropertyChanging();
                _selected.Clear();
                _selected.AddRange(value);
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the command used to search through the file list.</summary>
        public IEditorCommand<string> SearchCommand
        {
            get;
        }

        /// <summary>Property to set or return the command used confirm loading of the sprite files.</summary>
        public IEditorCommand<object> ConfirmLoadCommand
        {
            get;
            set;
        }

        /// <summary>Property to return the command used to cancel loading of the sprite files.</summary>
        public IEditorCommand<object> CancelCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the File control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var entry = (ContentFileExplorerFileEntry)sender;

            switch (e.PropertyName)
            {
                case nameof(ContentFileExplorerFileEntry.IsSelected):
                    if (entry.IsSelected)
                    {
                        if (_selected.Contains(entry))
                        {
                            break;
                        }

                        _selected.Add(entry);
                    }
                    else
                    {
                        _selected.Remove(entry);
                    }

                    NotifyPropertyChanged(nameof(SelectedFiles));
                    break;
            }            
        }

        /// <summary>
        /// Function to determine if search is available or not.
        /// </summary>
        /// <param name="text">The search text, not used.</param>
        /// <returns><b>true</b> if the search is available, <b>false</b> if not.</returns>
        private bool CanSearch(string text) => SpriteFileEntries.Count > 0;

        /// <summary>
        /// Function to search the sprite files.
        /// </summary>
        /// <param name="text">The search term.</param>
        private void DoSearch(string text)
        {
            _busyService.SetBusy();

            try
            {
                IEnumerable<IContentFileExplorerSearchEntry> results = _searchService.Search(text);

                for (int i = 0; i < SpriteFileEntries.Count; ++i)
                {
                    ContentFileExplorerDirectoryEntry entry = SpriteFileEntries[i];

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        entry.IsVisible = true;

                        for (int j = 0; j < entry.Files.Count; ++j)
                        {
                            ContentFileExplorerFileEntry fileEntry = entry.Files[j];
                            fileEntry.IsVisible = true;
                        }
                        continue;
                    }

                    entry.IsVisible = results.Any(item => item == entry);

                    for (int j = 0; j < entry.Files.Count; ++j)
                    {
                        ContentFileExplorerFileEntry fileEntry = entry.Files[j];

                        bool found = results.Any(item => item == fileEntry);
                        fileEntry.IsVisible = found;
                        if (found)
                        {
                            fileEntry.Parent.IsVisible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORTAG_ERR_SEARCH);
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to cancel the file selector.
        /// </summary>
        private void DoCancel()
        {
            try
            {
                IsActive = false;
            }
            catch (Exception ex)
            {
                Log.Print("Error closing file browser.", Diagnostics.LoggingLevel.Simple);
                Log.LogException(ex);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(SpriteFilesParameters injectionParameters)
        {
            _busyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(injectionParameters.BusyService), nameof(injectionParameters));
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
            _searchService = injectionParameters.SearchService ?? throw new ArgumentMissingException(nameof(injectionParameters.SearchService), nameof(injectionParameters));
            SpriteFileEntries = injectionParameters.Entries ?? throw new ArgumentMissingException(nameof(injectionParameters.Entries), nameof(injectionParameters));

            _selected.Clear();
            _selected.AddRange(injectionParameters.Entries.SelectMany(item => item.Files).Where(item => item.IsSelected));
        }

        /// <summary>Function called when the associated view is loaded.</summary>
        public override void OnLoad()
        {
            base.OnLoad();

            foreach (ContentFileExplorerFileEntry file in SpriteFileEntries.SelectMany(item => item.Files))
            {
                file.PropertyChanged += File_PropertyChanged;
            }
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            foreach (ContentFileExplorerFileEntry file in SpriteFileEntries.SelectMany(item => item.Files))
            {
                file.PropertyChanged -= File_PropertyChanged;
            }

            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SpriteFiles"/> class.</summary>
        public SpriteFiles()
        {
            SearchCommand = new EditorCommand<string>(DoSearch, CanSearch);
            CancelCommand = new EditorCommand<object>(DoCancel);
        }
        #endregion
    }
}
