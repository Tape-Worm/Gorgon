#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 13, 2016 8:45:14 PM
// 
#endregion

using System;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Information used to create a texture object.
	/// </summary>
	public class GorgonTexture1DInfo 
		: IGorgonTexture1DInfo
	{
		#region Properties.
	    /// <summary>
	    /// Property to return the name of the texture.
	    /// </summary>
	    public string Name
	    {
	        get;
	    }

        /// <summary>
        /// Property to set or return the width of the texture, in pixels.
        /// </summary>
        public int Width
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the number of array levels for a texture.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this value is greater than 0, the texture will be used as a texture array. 
        /// </para>
        /// <para>
        /// This value is defaulted to 1.
        /// </para>
        /// </remarks>
        public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format of the texture.
		/// </summary>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip-map levels for the texture.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the texture is multisampled, this value must be set to 1.
		/// </para>
		/// <para>
		/// This value is defaulted to 1.
		/// </para>
		/// </remarks>
		public int MipLevels
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the intended usage flags for this texture.
		/// </summary>
		/// <remarks>
		/// This value is defaulted to <c>Default</c>.
		/// </remarks>
		public ResourceUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the flags to determine how the texture will be bound with the pipeline when rendering.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the <see cref="Usage"/> property is set to <c>Staging</c>, then the texture must be created with a value of <see cref="TextureBinding.None"/> as staging textures do not 
		/// support bindings of any kind. If this value is set to anything other than <see cref="TextureBinding.None"/>, an exception will be thrown.
		/// </para>
		/// <para>
		/// This value is defaulted to <see cref="TextureBinding.ShaderResource"/>. 
		/// </para>
		/// </remarks>
		public TextureBinding Binding
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture1DInfo"/> class.
		/// </summary>
		/// <param name="info">A <see cref="IGorgonTexture1DInfo"/> to copy settings from.</param>
		/// <param name="newName">[Optional] The new name for the texture.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		public GorgonTexture1DInfo(IGorgonTexture1DInfo info, string newName = null)
		{
		    if (info == null)
		    {
                throw new ArgumentNullException(nameof(info));
		    }

		    Name = string.IsNullOrEmpty(newName) ? info.Name : newName;
		    Format = info.Format;
			ArrayCount = info.ArrayCount;
			Binding = info.Binding;
			MipLevels = info.MipLevels;
			Usage = info.Usage;
			Width = info.Width;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture1DInfo"/> class.
		/// </summary>
		/// <param name="name">[Optional] The name of the texture.</param>
		public GorgonTexture1DInfo(string name = null)
		{
		    Name = string.IsNullOrEmpty(name) ? GorgonGraphicsResource.GenerateName(GorgonTexture1D.NamePrefix) : name;
			Binding = TextureBinding.ShaderResource;
			Usage = ResourceUsage.Default;
			MipLevels = 1;
			ArrayCount = 1;
		}
		#endregion
	}
}
