
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
// Created: October 28, 2018 4:03:36 PM
// 

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The view model for the recent files list
/// </summary>
internal class Recent
    : ViewModelBase<ViewModelCommonParameters, IHostContentServices>, IRecent
{

    // The recent items from the editor settings.
    private List<RecentItem> _recentItems;
    // The project manager for the application.
    private ProjectManager _projectManager;

    /// <summary>Property to return the list of recent files.</summary>
    public ObservableCollection<RecentItem> Files
    {
        get;
        private set;
    }

    /// <summary>Property to set or return the command used to open a project.</summary>
    public IEditorCommand<RecentItem> OpenProjectCommand
    {
        get;
        set;
    }

    /// <summary>Property to set or return the command used to delete a project.</summary>        
    public IEditorCommand<RecentItem> DeleteItemCommand
    {
        get;
    }

    /// <summary>
    /// Function to delete a project item.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private async void DoDeleteItemAsync(RecentItem args)
    {
        try
        {
            if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_DELETE_PROJECT_ITEM, args.FilePath)) == MessageResponse.No)
            {
                return;
            }

            ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_DELETING_PROJECT, args.FilePath.Ellipses(40, true)));

            // We will send the project to the recycle bin so it can be recovered if need be.
            await Task.Run(() => _projectManager.DeleteProject(args.FilePath));

            Files.Remove(args);
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_DELETING_PROJECT_ITEM, args.FilePath));
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>Handles the CollectionChanged event of the Files list.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The [NotifyCollectionChangedEventArgs] instance containing the event data.</param>
    private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        void AddItems()
        {
            foreach (RecentItem recentItem in e.NewItems.OfType<RecentItem>())
            {
                RecentItem listItem = _recentItems.FirstOrDefault(item => string.Equals(item.FilePath, recentItem.FilePath, StringComparison.OrdinalIgnoreCase));

                if (listItem is not null)
                {
                    return;
                }

                _recentItems.Add(recentItem);
            }
        }

        void RemoveItems()
        {
            foreach (RecentItem recentItem in e.OldItems.OfType<RecentItem>())
            {
                RecentItem listItem = _recentItems.FirstOrDefault(item => string.Equals(item.FilePath, recentItem.FilePath, StringComparison.OrdinalIgnoreCase));

                if (listItem is null)
                {
                    continue;
                }

                _recentItems.Remove(listItem);
            }
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddItems();
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveItems();
                break;
            case NotifyCollectionChangedAction.Replace:
                RemoveItems();
                AddItems();
                break;
            case NotifyCollectionChangedAction.Reset:
                _recentItems.Clear();

                foreach (RecentItem item in Files)
                {
                    _recentItems.Add(item);
                }
                break;
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(ViewModelCommonParameters injectionParameters)
    {
        _recentItems = injectionParameters.EditorSettings?.RecentFiles;
        _projectManager = injectionParameters.ProjectManager;

        // Only capture up to 150 items.
        Files = new ObservableCollection<RecentItem>(_recentItems.OrderByDescending(item => item.LastUsedDate).Take(150));

        // If there are more files in the settings than what we have in our list (due to the 150 item limit), then refresh the settings right now.
        if (Files.Count < _recentItems.Count)
        {
            _recentItems.Clear();
            _recentItems.AddRange(Files);
        }

        Files.CollectionChanged += Files_CollectionChanged;
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    protected override void OnUnload()
    {
        Unload();
        Files.CollectionChanged -= Files_CollectionChanged;
    }

    /// <summary>Initializes a new instance of the <see cref="Recent"/> class.</summary>
    public Recent() => DeleteItemCommand = new EditorCommand<RecentItem>(DoDeleteItemAsync, args => args is not null);

}
