using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KRBTabControl
{
    partial class KRBTabControl
    {
        public class SelectedIndexChangingEventArgs : EventArgs, IDisposable
        {
            #region Instance Member

            private bool cancel = false;
            private int tabPageIndex = -1;
            private TabPageEx tabPage = null;

            #endregion

            #region Constructor

            public SelectedIndexChangingEventArgs(TabPageEx tabPage, int tabPageIndex)
            {
                this.tabPage = tabPage;
                this.tabPageIndex = tabPageIndex;
            }

            #endregion

            #region Property

            /// <summary>
            /// Gets or sets a value indicating whether the event should be canceled.
            /// </summary>
            public bool Cancel
            {
                get { return cancel; }
                set
                {
                    if (!value.Equals(cancel))
                        cancel = value;
                }
            }

            /// <summary>
            /// Gets the zero-based index of the TabPageEx in the KRBTabControl.TabPages collection.
            /// </summary>
            public int TabPageIndex
            {
                get { return tabPageIndex; }
            }

            /// <summary>
            /// Gets the TabPageEx the event is occurring for.
            /// </summary>
            public TabPageEx TabPage
            {
                get { return tabPage; }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        public class ContextMenuShownEventArgs : EventArgs, IDisposable
        {
            #region Instance Member

            private ContextMenuStrip contextMenu = null;
            private Point menuLocation = Point.Empty;

            #endregion

            #region Constructor

            public ContextMenuShownEventArgs(ContextMenuStrip contextMenu, Point menuLocation)
            {
                this.contextMenu = contextMenu;
                this.menuLocation = menuLocation;
            }

            #endregion

            #region Property

            /// <summary>
            /// Gets the drop-down menu of the control.It shows when a user clicks the drop-down icon on the caption.
            /// </summary>
            public ContextMenuStrip ContextMenu
            {
                get { return contextMenu; }
            }

            /// <summary>
            /// Gets or sets the drop-down menu location on the screen coordinates.
            /// </summary>
            public Point MenuLocation
            {
                get { return menuLocation; }
                set 
                { 
                    menuLocation = value;
                }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion
        }
    }
}