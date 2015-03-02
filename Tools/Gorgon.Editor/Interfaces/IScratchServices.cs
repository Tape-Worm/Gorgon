namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Services pertaining to the usage of the scratch area.
	/// </summary>
	internal interface IScratchServices
	{
		/// <summary>
		/// Property to return the scratch area interface.
		/// </summary>
		IScratchArea ScratchArea
		{
			get;
		}

		/// <summary>
		/// Property to return the locator UI used to change the location of a scratch area.
		/// </summary>
		IScratchLocator ScratchLocator
		{
			get;
		}
	}
}