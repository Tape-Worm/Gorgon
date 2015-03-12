#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, March 12, 2015 12:05:00 AM
// 
#endregion

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A default content model used when there is no content.
	/// </summary>
	class NoContentModel
		: ContentModel
	{
		#region Variables.
		// The graphics interface.
		private readonly IGraphicsService _graphicsService;
		// The application settings.
		private readonly IEditorSettings _settings;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the content provides a view or not.
		/// </summary>
		public override bool HasView
		{
			get
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the content object.
		/// </summary>
		/// <returns>
		/// The new content object.
		/// </returns>
		protected override IContentData CreateContent()
		{
			return new NoContent
			       {
				       Name = "DefaultContent"
			       };
		}

		/// <summary>
		/// Function to create the content view.
		/// </summary>
		/// <returns>
		/// The content view.
		/// </returns>
		protected override IContentPanel CreateView()
		{
			IContentData content = Content;

			return new ContentPanel(content, new NoContentRenderer(_graphicsService.GetGraphics(), _settings, content))
			       {
				       CaptionVisible = false,
				       Name = "DefaultContentPanel"
			       };
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="NoContentModel"/> class.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <param name="settings">The application settings.</param>
		public NoContentModel(IGraphicsService graphicsService, IEditorSettings settings)
		{
			_graphicsService = graphicsService;
			_settings = settings;
		}
		#endregion
	}
}
