#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, March 2, 2013 12:47:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Type of plug-in.
	/// </summary>
	public enum PlugInType
	{
		/// <summary>
		/// Plug-in writes out content packed files in a specific format.
		/// </summary>
		/// <remarks>File readers are implemented using the GorgonFileSystem objects, and are not specific to the editor.</remarks>
		FileWriter = 0,
		/// <summary>
		/// Plug-in is used to create content.
		/// </summary>
		Content = 1
	}

	/// <summary>
	/// An interface for editor plug-ins.
	/// </summary>
	public abstract class EditorPlugIn
		: GorgonPlugIn
	{
		#region Properties.
        /// <summary>
        /// Property to return the graphics interface for the application.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get
            {
                return Program.Graphics;
            }
        }

		/// <summary>
		/// Property to return the type of plug-in.
		/// </summary>
		/// <remarks>Implementors must provide one of the PlugInType enumeration values.</remarks>
		public abstract PlugInType PlugInType
		{
			get;
		}        
		#endregion

		#region Methods.
        /// <summary>
        /// Function to determine if a plug-in can be used.
        /// </summary>
        /// <returns>A string containing a list of reasons why the plug-in is not valid for use, or an empty string if the control is not valid for use.</returns>
        protected internal abstract string ValidatePlugIn();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		/// <remarks>
		/// Objects that implement this base class should pass in a hard coded description on the base constructor.
		/// </remarks>
		protected EditorPlugIn(string description)
			: base(description)
		{
		}
		#endregion
	}

    /// <summary>
    /// An interface for content plug-ins.
    /// </summary>
    public abstract class ContentPlugIn
        : EditorPlugIn, IDisposable
    {
        #region Variables.

        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the file extensions (and descriptions) for this content type.
		/// </summary>
		/// <remarks>This dictionary contains the file extension including the leading period as the key (in lowercase), and a tuple containing the file extension, and a description of the file (for display).</remarks>
		public IDictionary<string, Tuple<string, string>> FileExtensions
		{
			get;
			private set;
		}

		/// <summary>
        /// Property to return the type of plug-in.
        /// </summary>
        /// <remarks>Implementors must provide one of the PlugInType enumeration values.</remarks>
        public override PlugInType PlugInType
        {
            get 
            {
                return Editor.PlugInType.Content;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a content object interface.
        /// </summary>
        /// <returns>A new content object interface.</returns>
        protected abstract ContentObject OnCreateContentObject();

        /// <summary>
        /// Function to create a content object interface.
        /// </summary>
        /// <returns>A new content object interface.</returns>
        internal ContentObject CreateContentObject()
        {
            ContentObject result = OnCreateContentObject();

            if (result != null)
            {
                result.InitializeContent();
            }

            return result;
        }


        /// <summary>
        /// Function to create a tool strip menu item.
        /// </summary>
        /// <param name="name">Name of the menu item.</param>
        /// <param name="text">Text to display on the menu item.</param>
        /// <param name="icon">Icon to show on the menu item.</param>
        /// <returns>A new tool strip menu item.</returns>
        protected ToolStripMenuItem CreateMenuItem(string name, string text, Image icon)
        {
            var result = new ToolStripMenuItem(text, icon);
            result.Name = name;
            result.Tag = this;

            return result;
        }
        
        /// <summary>
		/// Function to return the icon for the content.
		/// </summary>
		/// <returns>The 16x16 image for the content.</returns>
		public virtual Image GetContentIcon()
		{
			return Properties.Resources.unknown_document_16x16;
		}
		
		/// <summary>
		/// Function to retrieve the create menu item for this content.
		/// </summary>
		/// <returns>The menu item for this </returns>
		public virtual ToolStripMenuItem GetCreateMenuItem()
		{
			return null;
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPlugIn"/> class.
        /// </summary>
        /// <param name="description">Optional description of the plug-in.</param>
        /// <remarks>
        /// Objects that implement this base class should pass in a hard coded description on the base constructor.
        /// </remarks>
        protected ContentPlugIn(string description)
            : base(description)
        {
			FileExtensions = new Dictionary<string, Tuple<string, string>>();
        }
        #endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}

    /// <summary>
    /// An interface for file output plug-ins.
    /// </summary>
    public abstract class FileWriterPlugIn
        : EditorPlugIn, IDisposable
    {
        #region Variables.
		private float _compressAmount = 0.05f;			// Compression amount.
		private formProcess _processForm = null;		// Processing form.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return whether a cancelation was requested or not.
		/// </summary>
		protected bool CancelRequested
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the plug-in supports writing compressed files.
		/// </summary>
		public abstract bool SupportsCompression
		{
			get;
		}

		/// <summary>
		/// Property to set or return the percentage for compression (if supported).
		/// </summary>
		/// <remarks>This value is within a range of 0..1.  0 will use the lowest amount of compression, and 1 will use the maximum compression.
		/// <para>When using the lowest compression, the file will save quickly, but will be larger.  When saving with maximum compression, the file will 
		/// save slowly (sometimes very slowly) but will yield the smallest file size.</para>
		/// <para>The default value is 0.5f.</para>
		/// </remarks>
		public float Compression
		{
			get
			{
				return _compressAmount;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				if (value > 1.0f)
				{
					value = 1.0f;
				}

				_compressAmount = value;
			}
		}

        /// <summary>
        /// Property to return the file extensions (and descriptions) for this content type.
        /// </summary>
        /// <remarks>This dictionary contains the file extension including the leading period as the key (in lowercase), and a tuple containing the file extension, and a description of the file (for display).</remarks>
        public IDictionary<string, Tuple<string, string>> FileExtensions
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the file system for the scratch files.
        /// </summary>
        public GorgonFileSystem ScratchFileSystem
        {
            get
            {
                return Program.ScratchFiles;
            }
        }

        /// <summary>
        /// Property to return the type of plug-in.
        /// </summary>
        /// <remarks>Implementors must provide one of the PlugInType enumeration values.</remarks>
        public override PlugInType PlugInType
        {
            get
            {
                return Editor.PlugInType.FileWriter;
            }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to write the file to the specified path.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <param name="token">Token used to cancel the task.</param>
		protected abstract void WriteFile(string path, CancellationToken token);

		/// <summary>
		/// Function to update the status display.
		/// </summary>
		/// <param name="text">Text to display.</param>
		/// <param name="value">Value to put in the progress meter.</param>
		protected void UpdateStatus(string text, int value)
		{
			if (_processForm != null)
			{
				_processForm.UpdateStatusText(text);
				_processForm.SetProgress(value);
			}
		}

		/// <summary>
		/// Function to set whether we can cancel the save operation or not.
		/// </summary>
		/// <param name="canCancel"></param>
		protected void CanCancel(bool canCancel)
		{
			if (_processForm != null)
			{
				_processForm.EnableCancel(canCancel);
			}
		}

		/// <summary>
		/// Function to perform a cancelation on the operation.
		/// </summary>
		internal void CancelOperation()
		{
			CancelRequested = true;
		}

		/// <summary>
		/// Function to save the file.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>TRUE if the file was saved successfully, FALSE if not.</returns>
		public bool Save(string path)
		{
			using (_processForm = new formProcess())
			{
				_processForm.progressMeter.Style = ProgressBarStyle.Marquee;
				_processForm.labelStatus.Text = "Saving...";
				_processForm.Text = "Saving " + System.IO.Path.GetFileName(path).Ellipses(35, true);

				using (var cancelToken = new CancellationTokenSource(Int32.MaxValue))
				{
					var token = cancelToken.Token;

					_processForm.Task = Task.Factory.StartNew(() => WriteFile(path, token), token);

					if (_processForm.ShowDialog() == DialogResult.Cancel)
					{
						cancelToken.Cancel();
						return false;
					}
				}
			}

			return true;
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileWriterPlugIn"/> class.
        /// </summary>
        /// <param name="description">Optional description of the plug-in.</param>
        /// <remarks>
        /// Objects that implement this base class should pass in a hard coded description on the base constructor.
        /// </remarks>
        protected FileWriterPlugIn(string description)
            : base(description)
        {
            FileExtensions = new Dictionary<string, Tuple<string, string>>();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

	/// <summary>
	/// Event parameters for the file write event.
	/// </summary>
	public class FileWriteEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the file being processed.
		/// </summary>
		public GorgonFileSystemFileEntry File
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FileWriteEventArgs"/> class.
		/// </summary>
		/// <param name="file">The file.</param>
		public FileWriteEventArgs(GorgonFileSystemFileEntry file)
		{
			File = file;
		}
		#endregion
	}
}
