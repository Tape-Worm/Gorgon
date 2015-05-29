using Gorgon.Core;

namespace Gorgon.Editor
{
	/// <summary>
	/// A representation of a packed editor file in the application.
	/// </summary>
	public interface IEditorFileSystem 
		: INamedObject
	{
		/// <summary>
		/// Property to return whether the file has been changed or not.
		/// </summary>
		bool HasChanged
		{
			get;
		}

		/// <summary>
		/// Property to return the full path to the file.
		/// </summary>
		string FullName
		{
			get;
		}
	}
}