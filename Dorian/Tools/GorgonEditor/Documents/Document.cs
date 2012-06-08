using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using KRBTabControl;
using SlimMath;
using Aga.Controls.Tree;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// A document for the editor.
	/// </summary>
	abstract class Document		
		: INamedObject, IDisposable
	{
		#region Events.
		/// <summary>
		/// Event fired when the properties of the document are updated.
		/// </summary>
		[Browsable(false)]
		public event EventHandler PropertyUpdated;
		#endregion

		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private string _name = string.Empty;					// Name of the document.
		private bool _needsSave = false;						// Flag to indicate that the document needs to be saved.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to disable the property updated dispatch event from being fired.
		/// </summary>
		protected bool NoDispatchUpdate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow the document to be closed or not.
		/// </summary>
		protected bool AllowClose
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the swap chain used for rendering in this document.
		/// </summary>
		protected GorgonSwapChain SwapChain
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the document can be saved or not.
		/// </summary>
		[Browsable(false)]
		public bool CanSave
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the document can be opened or not.
		/// </summary>
		[Browsable(false)]
		public bool CanOpen
		{
			get;
			protected set;
		}
		
		/// <summary>
		/// Property to return the type descriptor for this document.
		/// </summary>
		[Browsable(false)]
		public DocumentTypeDescriptor TypeDescriptor
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the folder that owns this document.
		/// </summary>
		[Browsable(false)]
		public ProjectFolder Folder
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the tree node that is attached to this document.
		/// </summary>
		[Browsable(false)]
		public Node TreeNode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the property grid that this document is using.
		/// </summary>
		[Browsable(false)]
		public PropertyGrid PropertyGrid
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether this document needs to be saved or not.
		/// </summary>
		[Browsable(false)]
		public bool NeedsSave
		{
			get
			{
				return _needsSave;
			}
			set
			{
				_needsSave = value;
				if (value)
				{
					if (!Tab.Text.EndsWith("*"))
						Tab.Text = Name + "*";
				}
				else
				{
					if (Tab.Text.EndsWith("*"))
						Tab.Text = Name;
				}
			}
		}

		/// <summary>
		/// Property to return the tab page that this document resides in.
		/// </summary>
		[Browsable(false)]
		public TabPageEx Tab
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the editor control for this document.
		/// </summary>
		[Browsable(false)]
		public Control Control
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the control that will receive rendering.
		/// </summary>
		/// <remarks>The user is responsible for maintaining/disposing of this window since it may be a child window of the main document control.</remarks>
		[Browsable(false)]
		public Control RenderWindow
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the name for the document.
		/// </summary>
		[Browsable(true), Category("Design"), Description("Provides a name for the object.")]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					return;

				if (string.Compare(_name, value, false) != 0)
				{
					DocumentCollection documents = Program.Project.Documents;

					string previousName = _name;
					_name = value;

					// Update the collection.
					if (Folder != null)
						documents = Folder.Documents;

					documents.Rename(previousName, value);

					Tab.Text = value;
					Tab.Name = "tab" + Name;
					TreeNode.Text = Name;

					NeedsSave = true;
					DispatchUpdateNotification();
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the object uses the property manager.
		/// </summary>
		[Browsable(false)]
		public bool HasProperties
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the type of document.
		/// </summary>
		[Browsable(false)]
		public abstract string DocumentType
		{
			get;
		}

		/// <summary>
		/// Property to return the tab image index.
		/// </summary>
		[Browsable(false)]
		public int TabImageIndex
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the swap chain and any associated resources.
		/// </summary>
		private void CreateSwapChain()
		{
			RenderWindow = GetRenderWindow();

			if (RenderWindow != null)
			{
				// Create a swap chain for our renderer.
				SwapChain = Program.Graphics.Output.CreateSwapChain("Document.SwapChain." + Name, new GorgonSwapChainSettings()
				{
					BufferCount = 2,
					DepthStencilFormat = BufferFormat.Unknown,
					Flags = SwapChainUsageFlags.RenderTarget,
					Format = BufferFormat.R8G8B8A8_UIntNormal,
					IsWindowed = true,
					Window = RenderWindow
				});
			}
		}

		/// <summary>
		/// Function to dispatch an update notification.
		/// </summary>
		protected virtual void DispatchUpdateNotification()
		{			
			if ((PropertyUpdated != null) && (!NoDispatchUpdate))
				PropertyUpdated(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function to draw to the control.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		/// <returns>Number of vertical retraces to wait.</returns>
		protected abstract int Draw(GorgonFrameRate timing);

		/// <summary>
		/// Function to initialize graphics and other items for the document.
		/// </summary>
		protected abstract void LoadResources();

		/// <summary>
		/// Function to initialize the editor control.
		/// </summary>
		/// <returns>The editor control.</returns>
		protected abstract Control InitializeEditorControl();

		/// <summary>
		/// Function to retrieve the rendering window.
		/// </summary>
		/// <returns>The rendering window.</returns>
		protected abstract Control GetRenderWindow();

		/// <summary>
		/// Function to release any resources when the document is terminated.
		/// </summary>
		protected abstract void ReleaseResources();

		/// <summary>
		/// Function to call for rendering.
		/// </summary>
		/// <param name="timing">Timing data to pass to the method.</param>
		/// <returns>TRUE to continue running, FALSE to exit.</returns>
		internal void RenderMethod(GorgonFrameRate timing)
		{
			if ((SwapChain == null) || (RenderWindow == null))
				return;

			Program.Renderer.Target = SwapChain;
			Program.Renderer.Clear(RenderWindow.BackColor);
			Program.Renderer.Render(Draw(timing));
		}

		/// <summary>
		/// Function to retrieve default values for properties with the DefaultValue attribute.
		/// </summary>
		internal void SetDefaults()
		{
			foreach (var descriptor in TypeDescriptor)
			{
				if (descriptor.HasDefaultValue)
					descriptor.DefaultValue = descriptor.GetValue<object>();
			}
		}

		/// <summary>
		/// Function to perform any resource loading that we'll need.
		/// </summary>
		internal void InitializeResources()
		{
			CreateSwapChain();

			// Load any resources that we'll need.
			LoadResources();
		}
		
		/// <summary>
		/// Function to initialize the document.
		/// </summary>
		internal void InitializeDocument()
		{
			Tab = new TabPageEx(Name);
			Tab.ImageIndex = TabImageIndex;
			Tab.Name = "tab" + Name;
			Tab.Tag = this;
			Control = InitializeEditorControl();

			// Add the tab for this document to our tab control.
			Tab.Controls.Add(Control);
			Tab.IsClosable = AllowClose;
		}

		/// <summary>
		/// Function to terminate the document (but still keep it alive).
		/// </summary>
		public void TerminateDocument()
		{
			ReleaseResources();

			if (SwapChain != null)
				SwapChain.Dispose();

			SwapChain = null;

			if (Control != null)
				Control.Dispose();

			if ((Tab != null) && (!Tab.IsClosable))
				Tab.Dispose();

			RenderWindow = null;
			Control = null;
			Tab = null;
		}

		/// <summary>
		/// Function to import a document.
		/// </summary>
		/// <param name="filePath">Path to the document file.</param>
		public abstract void Import(string filePath);

		/// <summary>
		/// Function to export a document
		/// </summary>
		/// <param name="filePath"></param>
		public abstract void Export(string filePath);
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Document"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="allowClose">TRUE to allow the document to close, FALSE to keep open.</param>
		/// <param name="folder">Folder that contains the document.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected Document(string name, bool allowClose, ProjectFolder folder)
		{
			GorgonDebug.AssertParamString(name, "name");

			CanOpen = false;
			CanSave = false;
			TabImageIndex = -1;
			AllowClose = allowClose;
			Folder = folder;
			_name = name;

			TypeDescriptor = new DocumentTypeDescriptor(this);
			TypeDescriptor.Enumerate(GetType());
			
			TreeNode = new Node(name);
			TreeNode.Image = Properties.Resources.unknown_document_16x16;
			TreeNode.Tag = this;			
			
			// Add to parent folder nodes.
			if (folder != null)
				folder.TreeNode.Nodes.Add(TreeNode);
			else
				Program.Project.RootNode.Nodes.Add(TreeNode);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (TreeNode.Parent != null)
						TreeNode.Parent.Nodes.Remove(TreeNode);
					TerminateDocument();
				}

				_disposed = true;
			}
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

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string INamedObject.Name
		{
			get 
			{
				return _name;
			}
		}
		#endregion
	}
}
