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
// Created: Wednesday, February 6, 2013 5:52:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// Image codecs for reading/writing image/texture data.
	/// </summary>
	/// <remarks>Codecs are used to encode and decode image data to and from a file, buffer or stream.  Gorgon currently has support for the following codecs built in:
	/// <list type="bullet">
	///		<item>
	///			<description>DDS</description>
	///		</item>
	///		<item>
	///			<description>TGA</description>
	///		</item>
	///		<item>
	///			<description>PNG (WIC)</description>
	///		</item>
	///		<item>
	///			<description>BMP (WIC)</description>
	///		</item>
	///		<item>
	///			<description>JPG (WIC)</description>
	///		</item>
	///		<item>
	///			<description>WMP (WIC)</description>
	///		</item>
	///		<item>
	///			<description>TIF (WIC)</description>
	///		</item>
	/// </list>
	/// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
	/// then it is required in order to read/save the files in those formats.</para>
	/// <para>These codecs are system codecs and are always present.  However, a user may define their own codec to read and write images.  To do so, the user must create a codec object that inherits from 
	/// <see cref="GorgonLibrary.IO.GorgonImageCodec">GorgonImageCodec</see> and then register it with the <see cref="GorgonLibrary.IO.GorgonImageCodecs.RegisterCodec">RegisterCodec</see> method.</para>
	/// </remarks>
	public static class GorgonImageCodecs
	{
		#region Variables.
		private static GorgonImageCodecCollection _internal = null;		// Internal codecs.
		private static GorgonImageCodecCollection _codecs = null;		// List of registered codecs.
		private static GorgonCodecDDS _dds = null;						// DDS file format.
		private static GorgonCodecTGA _tga = null;						// TGA file format.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the internal list of registered codecs.
		/// </summary>
		internal static GorgonImageCodecCollection InternalCodecs
		{
			get
			{
				return _internal;
			}
		}

		/// <summary>
		/// Property to return the registered codecs.
		/// </summary>
		public static GorgonImageCodecCollection RegisteredCodecs
		{
			get
			{
				return _codecs;
			}
		}

		/// <summary>
		/// Property to return the TGA codec.
		/// </summary>
		public static GorgonCodecTGA TGA
		{
			get
			{
				return _tga;
			}
		}

		/// <summary>
		/// Property to return the DDS codec.
		/// </summary>
		public static GorgonCodecDDS DDS
		{
			get
			{
				return _dds;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to register a custom image codec with Gorgon.
		/// </summary>
		/// <param name="codec">The image codec to register.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the codec is already registered.</exception>
		/// <remarks>Use this method to register any custom image codecs with the system.  Users may wish to create their own image format for various reasons.  To get Gorgon to read/write the image 
		/// format, a codec that inherits from <see cref="GorgonLibrary.IO.GorgonImageCodec">GorgonImageCodec</see> must be created.  Then, register the codec with the 
		/// Gorgon via this method so that Gorgon will know how to read and write the image.</remarks>
		public static void RegisterCodec(GorgonImageCodec codec)
		{
			if (codec == null)
			{
				throw new ArgumentNullException("codec");
			}

			if ((_codecs.Contains(codec)) || (_codecs.Contains(codec.Codec)) || (_internal.Contains(codec)) || (_internal.Contains(codec.Codec)))
			{
				throw new ArgumentException("The codec '" + codec.Codec + "' is already registered.", "codec");
			}

			_codecs.Add(codec);
			_internal.Add(codec);
		}

		/// <summary>
		/// Function to unregister a custom image codec from the system.
		/// </summary>
		/// <param name="codec">The image codec to unregister.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when if the codec was not found.</exception>
		/// <remarks>Use this to remove a custom image codec from Gorgon.  
		/// <para>Note that an attempt to unregister a system codec (DDS, GIF, PNG, TGA, BMP, WMP, TIF) will be ignored.</para></remarks>
		public static void UnregisterCodec(GorgonImageCodec codec)
		{
			if (codec == null)
			{
				throw new ArgumentNullException("codec");
			}

			if (!_codecs.Contains(codec))
			{
				throw new KeyNotFoundException("The codec '" + codec.Codec + "' was not registered.");
			}

			_codecs.Remove(codec);

			// Remove from the global codec list.
			if (_internal.Contains(codec))
			{
				_internal.Remove(codec);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonImageCodecs" /> class.
		/// </summary>
		static GorgonImageCodecs()
		{
			_dds = new GorgonCodecDDS();
			_tga = new GorgonCodecTGA();

			_codecs = new GorgonImageCodecCollection();
			_internal = new GorgonImageCodecCollection();

			// Add our system codecs.
			_internal.Add(_dds);
			_internal.Add(_tga);
		}
		#endregion
	}
}
