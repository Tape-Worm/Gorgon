namespace Gorgon.Renderers
{
	/// <summary>
	/// Used on objects that can defer the texture assignment until later.
	/// </summary>
	public interface IDeferredTextureLoad
	{
		/// <summary>
		/// Property to set or return the name of the deferred texture.
		/// </summary>
		string DeferredTextureName
		{
			get;
			set;
		}

		/// <summary>
		/// Function to assign a deferred texture.
		/// </summary>
		/// <remarks>If there are multiple textures with the same name, then the first texture will be chosen.</remarks>
		void GetDeferredTexture();
	}
}
