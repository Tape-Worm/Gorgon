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
// Created: Tuesday, June 4, 2013 7:27:33 PM
// 
#endregion

namespace Gorgon.Graphics
{
	/// <summary>
	/// Settings to describe a texture structure.
	/// </summary>
	public interface ITextureSettings		
		: IImageSettings
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the format for the default shader view.
		/// </summary>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  When this value is set to Unknown, then default view uses the texture format.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		BufferFormat ShaderViewFormat
		{
			get;
			set;
		}
        
		/// <summary>
		/// Property to set or return whether to allow an unordered access view of this texture.
		/// </summary>
		/// <remarks>This allows a texture to be accessed via an unordered access view in a shader.
		/// <para>Textures using an unordered access view can only use a typed (e.g. int, uint, float) format that belongs to the same group as the format assigned to the texture, 
		/// or R32_UInt/Int/Float (but only if the texture format is 32 bit).  Any other format will raise an exception.  Note that if the format is not set to R32_UInt/Int/Float, 
		/// then write-only access will be given to the UAV.</para> 
		/// <para>To check to see if a format is supported for UAV, use the <see cref="Gorgon.Graphics.GorgonVideoDevice.SupportsUnorderedAccessViewFormat">GorgonVideoDevice.SupportsUnorderedAccessViewFormat</see> 
		/// Function to determine if the format is supported.</para>
		/// <para>The default value is <b>false</b>.</para>
		/// </remarks>
		bool AllowUnorderedAccessViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether this is a cube texture.
		/// </summary>
		/// <remarks>When setting this value to <b>true</b>, ensure that the <see cref="Gorgon.Graphics.IImageSettings.ArrayCount">ArrayCount</see> property is set to a multiple of 6.
		/// <para>This only applies to 2D textures.  All other textures will return <b>false</b>.  The default value is <b>false</b>.</para></remarks>
		bool IsTextureCube
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling count/quality for the texture.
		/// </summary>
		/// <remarks>This only applies to 2D textures.  The default value is a count of 1, and a quality of 0 (no multisampling).
		/// <para>Note that multisampled textures cannot have sub resources (e.g. mipmaps), so the <see cref="Gorgon.Graphics.IImageSettings.MipCount">MipCount</see> should be set to 1.</para>
		/// </remarks>
		GorgonMultisampling Multisampling
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the usage for the texture.
		/// </summary>
		/// <remarks>The default value is Default.</remarks>
		BufferUsage Usage
		{
			get;
			set;
		}
		#endregion
	}
}