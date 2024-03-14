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
// Created: August 26, 2018 11:23:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.Views;

/// <summary>
/// The control to display recent documents.
/// </summary>
internal partial class StageRecent
    : EditorBaseControl, IDataContext<IRecent>
{
    #region Properties.
    /// <summary>Property to return the data context assigned to this view.</summary>        
    [Browsable(false)]
    public IRecent ViewModel
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return whether or not the recent items list has any items in it.
    /// </summary>
    [Browsable(false)]
    public bool HasItems => ViewModel?.Files.Count > 0;
    #endregion

    #region Methods.
    /// <summary>Handles the CollectionChanged event of the Files control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RecentItem item;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                item = (RecentItem)e.NewItems[0];
                AddRecentItem(item);
                break;
            case NotifyCollectionChangedAction.Remove:
                item = (RecentItem)e.OldItems[0];                    
                RemoveRecentItem(item);
                break;
            case NotifyCollectionChangedAction.Reset:
                FillList();
                break;
        }
    }

    /// <summary>Handles the Layout event of the PanelRecentItems control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="LayoutEventArgs"/> instance containing the event data.</param>
    private void PanelRecentItems_Layout(object sender, LayoutEventArgs e)
    {
        if (PanelRecentItems.Controls.Count == 0)
        {
            return;
        }

        PanelRecentItems.Controls[0].Dock = DockStyle.None;
        PanelRecentItems.Controls[0].Width = PanelRecentItems.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - PanelRecentItems.Controls[0].Margin.Horizontal;

        for (int i = 1; i < PanelRecentItems.Controls.Count; ++i)
        {
            PanelRecentItems.Controls[i].Dock = DockStyle.Top;
        }
    }

    /// <summary>Handles the DeleteItem event of the Button control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Button_DeleteItem(object sender, EventArgs e)
    {
        var button = (RecentItemButton)sender;

        if ((ViewModel?.DeleteItemCommand is null) || (!ViewModel.DeleteItemCommand.CanExecute(button.RecentItem)))
        {
            return;
        }

        ViewModel.DeleteItemCommand.Execute(button.RecentItem);
    }

    /// <summary>Handles the Click event of the Button control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Button_Click(object sender, EventArgs e)
    {
        var button = (RecentItemButton)sender;

        if ((ViewModel?.OpenProjectCommand is null) || (button.RecentItem is null))
        {
            return;
        }

        if (!ViewModel.OpenProjectCommand.CanExecute(button.RecentItem))
        {
            return;
        }

        ViewModel.OpenProjectCommand.Execute(button.RecentItem);
    }

    /// <summary>
    /// Function to remove a recent item from the list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    private void RemoveRecentItem(RecentItem item)
    {
        RecentItemButton theButton = PanelRecentItems.Controls.OfType<RecentItemButton>().FirstOrDefault(buttonItem => buttonItem.RecentItem == item);

        if (theButton is null)
        {
            return;
        }

        theButton.DeleteItem -= Button_DeleteItem;
        theButton.Click -= Button_Click;
        theButton.Dispose();
    }

    /// <summary>
    /// Function to add a recent item button to the list.
    /// </summary>
    /// <param name="item">The recent item to add.</param>
    private void AddRecentItem(RecentItem item)
    {
        if ((!IsHandleCreated) || (IsDesignTime))
        {
            return;
        }

        IEnumerable<RecentItemButton> buttons = PanelRecentItems.Controls.OfType<RecentItemButton>();
        RecentItemButton button = buttons.FirstOrDefault(btn => string.Equals(btn.Name, item.FilePath, StringComparison.OrdinalIgnoreCase));

        if (button is null)
        {
            button = new RecentItemButton
            {
                Name = item.FilePath
            };

            button.SuspendLayout();

            button.AutoSize = false;
            button.Click += Button_Click;
            button.DeleteItem += Button_DeleteItem;
        }
        else
        {
            button.SuspendLayout();
        }

        button.RecentItem = item;

        int index = 0;

        foreach (RecentItemButton otherButton in PanelRecentItems.Controls.OfType<RecentItemButton>())
        {
            if (otherButton.RecentItem.LastUsedDate > item.LastUsedDate)
            {
                index++;
            }
        }

        if (!PanelRecentItems.Controls.Contains(button))
        {
            PanelRecentItems.Controls.Add(button);
        }
        PanelRecentItems.Controls.SetChildIndex(button, index);

        button.ResumeLayout(true);
    }

    /// <summary>
    /// Function to clear all the items in the list.
    /// </summary>
    private void ClearItems()
    {
        while (PanelRecentItems.Controls.Count > 0)
        {
            var button = (RecentItemButton)PanelRecentItems.Controls[0];
            button.Click -= Button_Click;
            button.DeleteItem -= Button_DeleteItem;
            button.Dispose();
        }
    }

    /// <summary>
    /// Function to fill the list of recent items.
    /// </summary>
    private void FillList()
    {
        try
        {
            if ((!IsHandleCreated) || (IsDesignTime))
            {
                return;
            }

            PanelRecentItems.SuspendLayout();

            ClearItems();

            foreach (RecentItem item in ViewModel.Files.OrderBy(item => item.LastUsedDate))
            {
                AddRecentItem(item);
            }
        }
        finally
        {
            PanelRecentItems.ResumeLayout();
        }
    }

    /// <summary>
    /// Function unassign any data context events.
    /// </summary>
    private void UnassignEvents()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.Files.CollectionChanged -= Files_CollectionChanged;

        ViewModel.Unload();
        ViewModel = null;
    }

    /// <summary>Raises the <a href="http://msdn.microsoft.com/en-us/library/system.windows.forms.usercontrol.load.aspx" target="_blank">Load</a> event.</summary>
    /// <param name="e">An <a href="http://msdn.microsoft.com/en-us/library/system.eventargs.aspx" target="_blank">EventArgs</a> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ViewModel?.Load();

        FillList();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(IRecent dataContext)
    {
        UnassignEvents();

        ViewModel = dataContext;

        if ((IsDesignTime) || (ViewModel is null))
        {
            return;
        }

        ViewModel.Files.CollectionChanged += Files_CollectionChanged;
    }        
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="StageRecent"/> class.
    /// </summary>
    public StageRecent() => InitializeComponent();
    #endregion
}
