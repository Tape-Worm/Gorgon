using Gorgon.Core;

namespace Gorgon.Editor
{
	internal interface IEditorFileSystem : IGorgonNamedObject
	{
		/// <summary>
		/// Property to return whether the file has been changed or not.
		/// </summary>
		bool HasChanged
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the full path to the file.
		/// </summary>
		string FullName
		{
			get;
			set;
		}
	}
}