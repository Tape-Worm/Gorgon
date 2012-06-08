using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using KRBTabControl;
using SlimMath;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// An unknown document type.
	/// </summary>
	[DocumentExtension(typeof(DocumentUnknown), null, null, false)]
	class DocumentUnknown
		: Document
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of document.
		/// </summary>
		[Browsable(false)]
		public override string DocumentType
		{
			get
			{
				return "Unknown";
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw to the control.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		/// <returns>Number of vertical retraces to wait.</returns>
		protected override int Draw(GorgonFrameRate timing)
		{
			return 2;
		}

		/// <summary>
		/// Function to initialize graphics and other items for the document.
		/// </summary>
		protected override void LoadResources()
		{
		}

		/// <summary>
		/// Function to initialize the editor control.
		/// </summary>
		/// <returns>
		/// The editor control.
		/// </returns>
		protected override Control InitializeEditorControl()
		{
			return null;
		}

		/// <summary>
		/// Function to retrieve the rendering window.
		/// </summary>
		/// <returns>
		/// The rendering window.
		/// </returns>
		protected override Control GetRenderWindow()
		{
			return null;
		}

		/// <summary>
		/// Function to release any resources when the document is terminated.
		/// </summary>
		protected override void ReleaseResources()
		{			
		}

		/// <summary>
		/// Function to import a document.
		/// </summary>
		/// <param name="filePath">Path to the document file.</param>
		public override void Import(string filePath)
		{
		}

		/// <summary>
		/// Function to export a document
		/// </summary>
		/// <param name="filePath"></param>
		public override void Export(string filePath)
		{			
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentUnknown"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="allowClose">TRUE to allow the document to close, FALSE to keep open.</param>
		/// <param name="folder">Folder that contains the document.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public DocumentUnknown(string name, bool allowClose, ProjectFolder folder)
			: base(name, allowClose, folder)
		{
		}
		#endregion
	}
}
