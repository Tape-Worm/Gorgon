using Gorgon.Graphics;

namespace Gorgon.IO
{
	/// <summary>
	/// Options for the GDI+ image texture import.
	/// </summary>
	/// <remarks>Use this to override the default beahvior of the FromGDI methods.</remarks>
	public class GorgonGDIOptions
	{
		#region Properties.
		/// <summary>
		/// Property to set or return a target width for the image.
		/// </summary>
		/// <remarks>
		/// This will resize the image width to the size specified.  If left at 0, then the default width from the GDI+ image will be used.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return a target height for the image.
		/// </summary>
		/// <remarks>
		/// This will resize the image height to the size specified.  If left at 0, then the default height from the GDI+ image will be used.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth for the resulting texture.
		/// </summary>
		/// <remarks>
		/// Setting this value will use an array of images as each depth slice in the volume texture.  Ensure that the array has the required number 
		/// of elements for each slice in each mip map level.  Each slice decreases by a power of 2 as the mip level increases.
		/// <para>Slices are arranged in the array as follows:</para>
		/// <code>
		/// Element[0]: Slice 0, Mip level 0
		/// Element[1]: Slice 1, Mip level 0
		/// Element[2]: Slice 2, Mip level 0
		/// Element[3]: Slice 3, Mip level 0
		/// Element[4]: Slice 4, Mip level 1
		/// Element[5]: Slice 5, Mip level 1
		/// </code>
		/// <para>Note that we only have 2 slices in the last 2 elements in the array, this is because the 2nd mip level has decreased the depth by a power of 2.</para>
		/// <para>This property is for 3D volume textures only, and is ignored on all other texture types.</para></remarks>
		public int Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the format to convert the image into.
		/// </summary>
		/// <remarks>
		/// Set this property to Unknown to convert the image to the closest matching format.
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of mip-map levels in the image.
		/// </summary>
		/// <remarks>Setting this value will create the requested number of mip-maps from the image.  If passing in an array of GDI+ images, then the 
		/// array will be used as the source for mip-maps.  The array should be formatted like this:
		/// <code>
		/// Element[0]: Mip level 0
		/// Element[1]: Mip level 1
		/// Element[2]: Mip level 2
		/// Element[3]: Mip level 3
		/// etc...
		/// </code>
		/// <para>If an array of images is used, then ensure that there are enough elements in the array to accomodate the requested number of mip maps.</para>
		/// <para>If this value is set to 0 and no image array is present, then Gorgon will generate a full mip-map chain.</para>
		/// <para>The default value is 1.</para>
		/// </remarks>
		public int MipCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the number of array indices in the image.
		/// </summary>
		/// <remarks>Setting this value will create array images from an array of GDI+ images.  The GDI+ Image array should be formatted like this: 
		/// <code>
		/// Element[0]: Array Index 0, Mip level 0
		/// Element[1]: Array Index 0, Mip level 1
		/// Element[2]: Array Index 1, Mip level 0
		/// Element[3]: Array Index 1, Mip level 1
		/// </code>
		/// <para>Note that each array should have the same number of mip levels.</para>
		/// <para>Arrays only apply to 1D and 2D images, 3D images will ignore this parameter.</para>
		/// <para>The default value is 1.</para>
		/// </remarks>
		public int ArrayCount
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling applied to the texture.
		/// </summary>
		/// <remarks>
		/// Set this value to apply multisampling to the texture.  
		/// <para>Note that if multisampling is applied, then the mip-map count and array count must be set to 1.  If these values are not set to 1, then this value will be ignored.</para>
		/// <para>This property is for <see cref="Gorgon.Graphics.GorgonTexture2D">2D textures</see> only, for <see cref="Gorgon.Graphics.GorgonImageData">image data</see> or other texture types it is ignored.</para>
		/// <para>The default value is a count of 1 and a quality of 0 (No multisampling).</para>
		/// </remarks>
		public GorgonMultisampling Multisampling
		{
			get;
			set;
		}
        
		/// <summary>
		/// Property to set or return the dithering type to use on images with lower bit depths.
		/// </summary>
		/// <remarks>The default value is None.</remarks>
		public ImageDithering Dither
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the filtering type to use on images that are resized.
		/// </summary>
		/// <remarks>This value has no effect if the size of the image is not changed.  The value also affects the mip-map chain if it is generated.
		/// <para>The default value is Point.</para>
		/// </remarks>
		public ImageFilter Filter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the usage flag for the texture generated from the GDI+ image(s).
		/// </summary>
		/// <remarks>The default value is Default.</remarks>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to use clipping on the image if it needs resizing.
		/// </summary>
		/// <remarks>This value will turn off scaling and instead crop the GDI+ image if it's too large to fit in the destination image.  If the image is smaller, then 
		/// it will be left at its current size.
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool UseClipping
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the shader view format for a texture loaded with this codec.
		/// </summary>
		/// <remarks>This changes how the texture is sampled/viewed in a shader.  When this value is set to Unknown the view format is taken from the texture format.
		/// <para>This property is for <see cref="Gorgon.Graphics.GorgonTexture">textures</see> only, for <see cref="Gorgon.Graphics.GorgonImageData">image data</see> it is ignored.</para>
		/// <para>This property is only applied when decoding an image, otherwise it is ignored.</para>
		/// <para>The default value is Unknown.</para>
		/// </remarks>
		public BufferFormat ViewFormat
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
		/// <para>If this value is set to TRUE, it will automatically change the format of the texture to the equivalent typeless format.  This is necessary because UAVs cannot be 
		/// used with typed texture resources.</para>
		/// <para>To check to see if a format is supported for UAV, use the <see cref="Gorgon.Graphics.GorgonVideoDevice.SupportsUnorderedAccessViewFormat">GorgonVideoDevice.SupportsUnorderedAccessViewFormat</see> 
		/// Function to determine if the format is supported.</para>
		/// <para>This property is for <see cref="Gorgon.Graphics.GorgonTexture">textures</see> only, for <see cref="Gorgon.Graphics.GorgonImageData">image data</see> it is ignored.</para>
		/// <para>This property is only applied when decoding an image, otherwise it is ignored.</para>
		/// <para>The default value is FALSE.</para>
		/// </remarks>
		public bool AllowUnorderedAccess
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGDIOptions" /> class.
		/// </summary>
		public GorgonGDIOptions()
		{
			Width = 0;
			Height = 0;
			Depth = 1;
			Format = BufferFormat.Unknown;
			MipCount = 1;
			ArrayCount = 1;
			Dither = ImageDithering.None;
			Filter = ImageFilter.Point;
			Usage = BufferUsage.Default;
			UseClipping = false;
			ViewFormat = BufferFormat.Unknown;
			AllowUnorderedAccess = false;
		}
		#endregion
	}
}