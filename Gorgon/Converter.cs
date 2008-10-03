#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Saturday, July 09, 2005 5:32:17 PM
// 
#endregion

using System;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Class used to perform conversion between various components and Gorgon.
	/// </summary>
	/// <remarks>This class is used to convert between Direct 3D data types and Gorgon's data types.</remarks>
	public static class Converter
	{
		#region Methods.
		/// <summary>
		/// Function to convert a Gorgon viewport into a D3D viewport.
		/// </summary>
		/// <param name="viewport">Viewport to convert.</param>
		/// <returns>A D3D viewport.</returns>
		static public DX.Viewport Convert(Viewport viewport)
		{
			DX.Viewport result;			// Resultant viewport.

			result = new DX.Viewport();
			result.X = viewport.View.Left;
			result.Y = viewport.View.Top;
			result.Width = viewport.View.Width;
			result.Height = viewport.View.Height;
			result.MinZ = 0.0f;
			result.MaxZ = 1.0f;

			return result;
		}

		/// <summary>
		/// Function to convert a Gorgon image file format to a D3D image file format.
		/// </summary>
		/// <param name="format">Format to convert.</param>
		/// <returns>Converted format.</returns>
		static public D3D9.ImageFileFormat Convert(ImageFileFormat format)
		{
			switch (format)
			{
				case ImageFileFormat.DDS:
					return D3D9.ImageFileFormat.Dds;
				case ImageFileFormat.JPEG:
					return D3D9.ImageFileFormat.Jpg;
				case ImageFileFormat.PNG:
					return D3D9.ImageFileFormat.Png;
				case ImageFileFormat.TGA:
					return D3D9.ImageFileFormat.Tga;
				case ImageFileFormat.DIB:
					return D3D9.ImageFileFormat.Dib;
				case ImageFileFormat.PFM:
					return D3D9.ImageFileFormat.Pfm;
				case ImageFileFormat.PPM:
					return D3D9.ImageFileFormat.Ppm;
			}

			return D3D9.ImageFileFormat.Bmp;
		}

		/// <summary>
		/// Function to convert a D3D image file format to a Gorgon image file format.
		/// </summary>
		/// <param name="format">Format to convert.</param>
		/// <returns>Converted format.</returns>
		static public ImageFileFormat Convert(D3D9.ImageFileFormat format)
		{
			switch (format)
			{
				case D3D9.ImageFileFormat.Dds:
					return ImageFileFormat.DDS;
				case D3D9.ImageFileFormat.Jpg:
					return ImageFileFormat.JPEG;
				case D3D9.ImageFileFormat.Png:
					return ImageFileFormat.PNG;
				case D3D9.ImageFileFormat.Tga:
					return ImageFileFormat.TGA;
				case D3D9.ImageFileFormat.Dib:
					return ImageFileFormat.DIB;
				case D3D9.ImageFileFormat.Pfm:
					return ImageFileFormat.PFM;
				case D3D9.ImageFileFormat.Ppm:
					return ImageFileFormat.PPM;
			}

			return ImageFileFormat.BMP;
		}

		/// <summary>
		/// Function to convert an imaging addressing mode into a D3D texture addressing mode.
		/// </summary>
		/// <param name="address">Addressing mode to convert.</param>
		/// <returns>D3D texture addressing.</returns>
		static public D3D9.TextureAddress Convert(ImageAddressing address)
		{
			switch (address)
			{
				case ImageAddressing.Border:
					return D3D9.TextureAddress.Border;
				case ImageAddressing.Clamp:
					return D3D9.TextureAddress.Clamp;
				case ImageAddressing.Mirror:
					return D3D9.TextureAddress.Mirror;
				case ImageAddressing.MirrorOnce:
					return D3D9.TextureAddress.MirrorOnce;
			}

			return D3D9.TextureAddress.Wrap;
		}

		/// <summary>
		/// Function to return the appropriate declaration usage based on our context.
		/// </summary>
		/// <param name="context">Context to convert.</param>
		/// <returns>An appropriate D3D declaration usage.</returns>
		static public D3D9.DeclarationUsage Convert(VertexFieldContext context)
		{
			switch (context)
			{
				case VertexFieldContext.Binormal:
					return D3D9.DeclarationUsage.Binormal;
				case VertexFieldContext.BlendIndices:
					return D3D9.DeclarationUsage.BlendIndices;
				case VertexFieldContext.BlendWeights:
					return D3D9.DeclarationUsage.BlendWeight;
				case VertexFieldContext.Diffuse:
					return D3D9.DeclarationUsage.Color;
				case VertexFieldContext.Normal:
					return D3D9.DeclarationUsage.Normal;
				case VertexFieldContext.Position:
					return D3D9.DeclarationUsage.Position;
				case VertexFieldContext.Specular:
					// Index should be 1.
					return D3D9.DeclarationUsage.Color;
				case VertexFieldContext.Tangent:
					return D3D9.DeclarationUsage.Tangent;
				case VertexFieldContext.TexCoords:
					return D3D9.DeclarationUsage.TextureCoordinate;
			}

			throw new GorgonException(GorgonErrors.InvalidFormat, "Vertex field context '" + context.ToString() + "' has no Direct3D equivalent.");
		}

		/// <summary>
		/// Function to convert the field type into an appropriate D3D element type.
		/// </summary>
		/// <param name="fieldType">Field type to convert.</param>
		/// <returns>Appropriate D3D declaration data type.</returns>
		static public D3D9.DeclarationType Convert(VertexFieldType fieldType)
		{
			switch (fieldType)
			{
				case VertexFieldType.Color:
					return D3D9.DeclarationType.Color;
				case VertexFieldType.Float1:
					return D3D9.DeclarationType.Float1;
				case VertexFieldType.Float2:
					return D3D9.DeclarationType.Float2;
				case VertexFieldType.Short1:
				case VertexFieldType.Short3:
				case VertexFieldType.Float3:
					return D3D9.DeclarationType.Float3;
				case VertexFieldType.Float4:
					return D3D9.DeclarationType.Float4;
				case VertexFieldType.Short2:
					return D3D9.DeclarationType.Short2;
				case VertexFieldType.Short4:
					return D3D9.DeclarationType.Short4;
				case VertexFieldType.UByte4:
					return D3D9.DeclarationType.Ubyte4;
			}

			throw new GorgonException(GorgonErrors.InvalidFormat, "Vertex field type '" + fieldType.ToString() + "' has no Direct3D equivalent.");
		}

		/// <summary>
		/// Function to return an appropriate D3D shading mode based on our shading mode.
		/// </summary>
		/// <param name="shademode">Shading mode to convert.</param>
		/// <returns>D3D shade mode.</returns>
		static public D3D9.ShadeMode Convert(ShadingMode shademode)
		{

			switch (shademode)
			{
				case ShadingMode.Flat:
					return D3D9.ShadeMode.Flat;
			}

			return D3D9.ShadeMode.Gouraud;
		}

		/// <summary>
		/// Function to return a Gorgon shading mode based on a Direct 3D shade mode.
		/// </summary>
		/// <param name="shademode">Shade mode to convert.</param>
		/// <returns>Gorgon shading mode.</returns>
		static public ShadingMode Convert(D3D9.ShadeMode shademode)
		{
			switch (shademode)
			{
				case D3D9.ShadeMode.Flat:
					return ShadingMode.Flat;
			}

			return ShadingMode.Gouraud;
		}

		/// <summary>
		/// Function to return a D3D blending operation based on a Gorgon blending operation.
		/// </summary>
		/// <param name="alphaop">Alpha operation to convert.</param>
		/// <returns>D3D blending operation.</returns>
		static public D3D9.Blend Convert(AlphaBlendOperation alphaop)
		{
			switch (alphaop)
			{
				case AlphaBlendOperation.BlendFactor:
					return D3D9.Blend.BlendFactor;
				case AlphaBlendOperation.BothInverseSourceAlpha:
					return D3D9.Blend.BothInverseSourceAlpha;
				case AlphaBlendOperation.DestinationAlpha:
					return D3D9.Blend.DestinationAlpha;
				case AlphaBlendOperation.DestinationColor:
					return D3D9.Blend.DestinationColor;
				case AlphaBlendOperation.InverseBlendFactor:
					return D3D9.Blend.InverseBlendFactor;
				case AlphaBlendOperation.InverseDestinationAlpha:
					return D3D9.Blend.InverseDestinationAlpha;
				case AlphaBlendOperation.InverseDestinationColor:
					return D3D9.Blend.InverseDestinationColor;
				case AlphaBlendOperation.InverseSourceAlpha:
					return D3D9.Blend.InverseSourceAlpha;
				case AlphaBlendOperation.InverseSourceColor:
					return D3D9.Blend.InverseSourceColor;
				case AlphaBlendOperation.SourceAlpha:
					return D3D9.Blend.SourceAlpha;
				case AlphaBlendOperation.SourceAlphaSaturation:
					return D3D9.Blend.SourceAlphaSaturated;
				case AlphaBlendOperation.SourceColor:
					return D3D9.Blend.SourceColor;
				case AlphaBlendOperation.Zero:
					return D3D9.Blend.Zero;
			}

			return D3D9.Blend.One;
		}

		/// <summary>
		/// Function to return an appropriate D3D fill mode based on our drawing mode.
		/// </summary>
		/// <param name="drawmode">Drawing mode to convert.</param>
		/// <returns>D3D fill mode.</returns>
		static public D3D9.FillMode Convert(DrawingMode drawmode)
		{
			switch (drawmode)
			{
				case DrawingMode.Points:
					return D3D9.FillMode.Point;
				case DrawingMode.Wireframe:
					return D3D9.FillMode.Wireframe;
			}

			return D3D9.FillMode.Solid;
		}

		/// <summary>
		/// Function to return a Gorgon drawing mode based on a Direct 3D fill mode.
		/// </summary>
		/// <param name="drawmode">Drawing mode to convert.</param>
		/// <returns>Gorgon drawing mode.</returns>
		static public DrawingMode Convert(D3D9.FillMode drawmode)
		{
			switch (drawmode)
			{
				case D3D9.FillMode.Point:
					return DrawingMode.Points;
				case D3D9.FillMode.Wireframe:
					return DrawingMode.Wireframe;
			}

			return DrawingMode.Solid;
		}

		/// <summary>
		/// Function to return the appropriate D3D primitive style based on our primitive style.
		/// </summary>
		/// <param name="primitivetype">Style to convert.</param>
		/// <returns>D3D primitive type.</returns>
		static public D3D9.PrimitiveType Convert(PrimitiveStyle primitivetype)
		{
			switch (primitivetype)
			{
				case PrimitiveStyle.LineList:
					return D3D9.PrimitiveType.LineList;
				case PrimitiveStyle.LineStrip:
					return D3D9.PrimitiveType.LineStrip;
				case PrimitiveStyle.PointList:
					return D3D9.PrimitiveType.PointList;
				case PrimitiveStyle.TriangleFan:
					return D3D9.PrimitiveType.TriangleFan;
				case PrimitiveStyle.TriangleStrip:
					return D3D9.PrimitiveType.TriangleStrip;
				default:
					return D3D9.PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Function to convert D3D presentation intervals to vsync intervals.
		/// </summary>
		/// <param name="interval">Interval to convert.</param>
		/// <returns>Equivalent vsync interval.</returns>
		public static VSyncIntervals Convert(D3D9.PresentInterval interval)
		{
			switch (interval)
			{
				case D3D9.PresentInterval.Immediate:
					return VSyncIntervals.IntervalNone;
				case D3D9.PresentInterval.One:
					return VSyncIntervals.IntervalOne;
				case D3D9.PresentInterval.Two:
					return VSyncIntervals.IntervalTwo;
				case D3D9.PresentInterval.Three:
					return VSyncIntervals.IntervalThree;
				case D3D9.PresentInterval.Four:
					return VSyncIntervals.IntervalFour;
				default:
					return VSyncIntervals.IntervalNone;
			}
		}

		/// <summary>
		/// Function to convert vsync intervals to D3D presentation intervals.
		/// </summary>
		/// <param name="interval">Interval to convert.</param>
		/// <returns>Equivalent presentation interval.</returns>
		public static D3D9.PresentInterval Convert(VSyncIntervals interval)
		{
			switch (interval)
			{
				case VSyncIntervals.IntervalNone:
					return D3D9.PresentInterval.Immediate;
				case VSyncIntervals.IntervalOne:
					return D3D9.PresentInterval.One;
				case VSyncIntervals.IntervalTwo:
					return D3D9.PresentInterval.Two;
				case VSyncIntervals.IntervalThree:
					return D3D9.PresentInterval.Three;
				case VSyncIntervals.IntervalFour:
					return D3D9.PresentInterval.Four;
				default:
					return D3D9.PresentInterval.Immediate;
			}
		}

		/// <summary>
		/// Function to convert backbuffer formats to D3D formats.
		/// </summary>
		/// <param name="format">Backbuffer format to convert.</param>
		/// <returns>Equivalent D3D format.</returns>
		public static D3D9.Format Convert(BackBufferFormats format)
		{
			switch (format)
			{
				case BackBufferFormats.BufferRGB101010A2:
					return D3D9.Format.A2R10G10B10;
				case BackBufferFormats.BufferRGB555:
					return D3D9.Format.X1R5G5B5;
				case BackBufferFormats.BufferRGB565:
					return D3D9.Format.R5G6B5;
				case BackBufferFormats.BufferRGB888:
					return D3D9.Format.X8R8G8B8;
				case BackBufferFormats.BufferP8:
					return D3D9.Format.P8;
				default:
					throw new GorgonException(GorgonErrors.InvalidFormat, format.ToString() + " is not a valid format.");
			}
		}

		/// <summary>
		/// Function to convert backbuffer formats to image format.
		/// </summary>
		/// <param name="format">Backbuffer format to convert.</param>
		/// <returns>Equivalent image format.</returns>
		public static ImageBufferFormats ConvertToImageFormat(BackBufferFormats format)
		{
			switch (format)
			{
				case BackBufferFormats.BufferRGB101010A2:
					return ImageBufferFormats.BufferRGB101010A2;
				case BackBufferFormats.BufferRGB555:
					return ImageBufferFormats.BufferRGB555X1;
				case BackBufferFormats.BufferRGB565:
					return ImageBufferFormats.BufferRGB565;
				case BackBufferFormats.BufferRGB888:
					return ImageBufferFormats.BufferRGB888X8;
				default:
					throw new GorgonException(GorgonErrors.InvalidFormat, format.ToString() + " is not a valid format.");
			}
		}

		/// <summary>
		/// Function to convert D3D formats to backbuffer formats.
		/// </summary>
		/// <param name="format">D3D format to convert.</param>
		/// <returns>Equivalent backbuffer format .</returns>
		public static BackBufferFormats Convert(D3D9.Format format)
		{
			switch (format)
			{
				case D3D9.Format.A2R10G10B10:
					return BackBufferFormats.BufferRGB101010A2;
				case D3D9.Format.X1R5G5B5:
					return BackBufferFormats.BufferRGB555;
				case D3D9.Format.R5G6B5:
					return BackBufferFormats.BufferRGB565;
				case D3D9.Format.X8R8G8B8:
					return BackBufferFormats.BufferRGB888;
				case D3D9.Format.P8:
					return BackBufferFormats.BufferP8;
				default:
					throw new GorgonException(GorgonErrors.InvalidFormat, format.ToString() + " is not a valid format.");
			}
		}

		/// <summary>
		/// Function to convert buffer usage flags to Direct 3D Usage flags.
		/// </summary>
		/// <param name="flags">Flags to convert.</param>
		/// <returns>D3D Usage flags.</returns>
		public static D3D9.Usage Convert(BufferUsages flags)
		{
			D3D9.Usage d3dflags;		// Direct 3D flags.

			d3dflags = D3D9.Usage.None;

			if ((flags & BufferUsages.Dynamic) != 0)
				d3dflags |= D3D9.Usage.Dynamic;

			if ((flags & BufferUsages.WriteOnly) != 0)
				d3dflags |= D3D9.Usage.WriteOnly;

			if ((flags & BufferUsages.ForceSoftware) != 0)
				d3dflags |= D3D9.Usage.SoftwareProcessing;

			return d3dflags;
		}

		/// <summary>
		/// Function to convert buffer locking flags to Direct 3D locking flags.
		/// </summary>
		/// <param name="flags">Flags to convert.</param>
		/// <returns>D3D lock flags.</returns>
		public static D3D9.LockFlags Convert(BufferLockFlags flags)
		{
			switch (flags)
			{
				case BufferLockFlags.Discard:
					return D3D9.LockFlags.Discard;
				case BufferLockFlags.NoOverwrite:
					return D3D9.LockFlags.NoOverwrite;
				case BufferLockFlags.ReadOnly:
					return D3D9.LockFlags.ReadOnly;
			}

			return D3D9.LockFlags.None;
		}

		/// <summary>
		/// Function to convert a culling mode into a Direct 3D cull mode.
		/// </summary>
		/// <param name="cullmode">Cull mode to convert.</param>
		/// <returns>D3D culling mode.</returns>
		public static D3D9.Cull Convert(CullingMode cullmode)
		{
			switch (cullmode)
			{
				case CullingMode.CounterClockwise:
					return D3D9.Cull.Counterclockwise;
				case CullingMode.Clockwise:
					return D3D9.Cull.Clockwise;
				default:
					return D3D9.Cull.None;
			}
		}

		/// <summary>
		/// Function to convert a D3D culling mode into a Gorgon culling mode.
		/// </summary>
		/// <param name="cullmode">D3D culling mode.</param>
		/// <returns>Gorgon culling mode.</returns>
		public static CullingMode Convert(D3D9.Cull cullmode)
		{
			switch (cullmode)
			{
				case D3D9.Cull.Clockwise:
					return CullingMode.Clockwise;
				case D3D9.Cull.Counterclockwise:
					return CullingMode.CounterClockwise;
				default:
					return CullingMode.None;
			}
		}

		/// <summary>
		/// Function to convert Goron comparison functions to D3D functions.
		/// </summary>
		/// <param name="func">Comparison function to convert.</param>
		/// <returns>D3D comparison function.</returns>
		public static D3D9.Compare Convert(CompareFunctions func)
		{
			switch (func)
			{
				case CompareFunctions.Always:
					return D3D9.Compare.Always;
				case CompareFunctions.Equal:
					return D3D9.Compare.Equal;
				case CompareFunctions.GreaterThan:
					return D3D9.Compare.Greater;
				case CompareFunctions.GreaterThanOrEqual:
					return D3D9.Compare.GreaterEqual;
				case CompareFunctions.LessThan:
					return D3D9.Compare.Less;
				case CompareFunctions.Never:
					return D3D9.Compare.Never;
				case CompareFunctions.NotEqual:
					return D3D9.Compare.NotEqual;
				default:
					return D3D9.Compare.LessEqual;
			}
		}

		/// <summary>
		/// Function to convert D3D comparison functions to Gorgon comparison functions.
		/// </summary>
		/// <param name="func">D3D comparison function to convert.</param>
		/// <returns>Gorgon comparison function.</returns>
		public static CompareFunctions Convert(D3D9.Compare func)
		{
			switch (func)
			{
				case D3D9.Compare.Always:
					return CompareFunctions.Always;
				case D3D9.Compare.Equal:
					return CompareFunctions.Equal;
				case D3D9.Compare.Greater:
					return CompareFunctions.GreaterThan;
				case D3D9.Compare.GreaterEqual:
					return CompareFunctions.GreaterThanOrEqual;
				case D3D9.Compare.Less:
					return CompareFunctions.LessThan;
				case D3D9.Compare.Never:
					return CompareFunctions.Never;
				case D3D9.Compare.NotEqual:
					return CompareFunctions.NotEqual;
				default:
					return CompareFunctions.LessThanOrEqual;
			}
		}

		/// <summary>
		/// Function to convert a depth buffer format into a D3D compatiable format.
		/// </summary>
		/// <param name="depthFormat">Depth format to convert.</param>
		/// <returns>A compatiable Direct3D depth format.</returns>
		public static D3D9.Format ConvertDepthFormat(DepthBufferFormats depthFormat)
		{
			switch (depthFormat)
			{
				case DepthBufferFormats.BufferLuminance16:
					return D3D9.Format.L16;
				case DepthBufferFormats.BufferDepth15Stencil1:
					return D3D9.Format.D15S1;
				case DepthBufferFormats.BufferDepth16Lockable:
					return D3D9.Format.D16Lockable;
				case DepthBufferFormats.BufferDepth16:
					return D3D9.Format.D16;
				case DepthBufferFormats.BufferDepth24:
					return D3D9.Format.D24X8;
				case DepthBufferFormats.BufferDepth24Stencil4:
					return D3D9.Format.D24X4S4;
				case DepthBufferFormats.BufferDepth24Stencil8:
					return D3D9.Format.D24S8;
				case DepthBufferFormats.BufferDepth24Stencil8Lockable:
					return D3D9.Format.D24SingleS8;
				case DepthBufferFormats.BufferDepth32:
					return D3D9.Format.D32;
				case DepthBufferFormats.BufferDepth32Lockable:
					return D3D9.Format.D32SingleLockable;
			}

			return D3D9.Format.Unknown;
		}

		/// <summary>
		/// Function to convert a depth buffer format into a D3D compatiable format.
		/// </summary>
		/// <param name="depthFormat">Depth format to convert.</param>
		/// <returns>A compatiable Direct3D depth format.</returns>
		public static DepthBufferFormats ConvertDepthFormat(D3D9.Format depthFormat)
		{
			switch (depthFormat)
			{
				case D3D9.Format.L16:
					return DepthBufferFormats.BufferLuminance16;
				case D3D9.Format.D15S1:
					return DepthBufferFormats.BufferDepth15Stencil1;
				case D3D9.Format.D16Lockable:
					return DepthBufferFormats.BufferDepth16Lockable;
				case D3D9.Format.D16:
					return DepthBufferFormats.BufferDepth16;
				case D3D9.Format.D24X8:
					return DepthBufferFormats.BufferDepth24;
				case D3D9.Format.D24X4S4:
					return DepthBufferFormats.BufferDepth24Stencil4;
				case D3D9.Format.D24S8:
					return DepthBufferFormats.BufferDepth24Stencil8;
				case D3D9.Format.D24SingleS8:
					return DepthBufferFormats.BufferDepth24Stencil8Lockable;
				case D3D9.Format.D32SingleLockable:
					return DepthBufferFormats.BufferDepth32Lockable;
				case D3D9.Format.D32:
					return DepthBufferFormats.BufferDepth32;
				default:
					return DepthBufferFormats.BufferUnknown;
			}
		}

		/// <summary>
		/// Function to convert a D3D 2D vector into a Gorgon 2D vector.
		/// </summary>
		/// <param name="vector">D3D vector to convert.</param>
		/// <returns>Gorgon 2D vector.</returns>
		public static Vector2D Convert(DX.Vector2 vector)
		{
			return new Vector2D(vector.X, vector.Y);
		}

		/// <summary>
		/// Function to convert a Gorgon 2D vector into a D3D 2D vector.
		/// </summary>
		/// <param name="vector">Gorgon vector to convert.</param>
		/// <returns>D3D 2D vector.</returns>
		public static DX.Vector2 Convert(Vector2D vector)
		{
			return new DX.Vector2(vector.X, vector.Y);
		}

		/// <summary>
		/// Function to convert a D3D 3D vector into a Gorgon 3D vector.
		/// </summary>
		/// <param name="vector">D3D vector to convert.</param>
		/// <returns>Gorgon 3D vector.</returns>
		public static Vector3D Convert(DX.Vector3 vector)
		{
			return new Vector3D(vector.X, vector.Y, vector.Z);
		}

		/// <summary>
		/// Function to convert a Gorgon 3D vector into a D3D 3D vector.
		/// </summary>
		/// <param name="vector">Gorgon vector to convert.</param>
		/// <returns>D3D 3D vector.</returns>
		public static DX.Vector3 Convert(Vector3D vector)
		{
			return new DX.Vector3(vector.X, vector.Y, vector.Z);
		}

		/// <summary>
		/// Function to convert a D3D 4D vector into a Gorgon 4D vector.
		/// </summary>
		/// <param name="vector">D3D vector to convert.</param>
		/// <returns>Gorgon 4D vector.</returns>
		public static Vector4D Convert(DX.Vector4 vector)
		{
			return new Vector4D(vector.X, vector.Y, vector.Z, vector.W);
		}

		/// <summary>
		/// Function to convert a Gorgon 4D vector into a D3D 4D vector.
		/// </summary>
		/// <param name="vector">Gorgon vector to convert.</param>
		/// <returns>D3D 4D vector.</returns>
		public static DX.Vector4 Convert(Vector4D vector)
		{
			return new DX.Vector4(vector.X, vector.Y, vector.Z, vector.W);
		}

		/// <summary>
		/// Function to convert an array of Gorgon 4D vectors into an array of D3D 4D vectors.
		/// </summary>
		/// <param name="vector">Gorgon vectors to convert.</param>
		/// <returns>D3D 4D vectors.</returns>
		public static DX.Vector4[] Convert(Vector4D[] vector)
		{
			DX.Vector4[] vectors = new SlimDX.Vector4[vector.Length];	// Create vector array.

			// Copy.
			for (int i = 0; i < vector.Length; i++)
				vectors[i] = Convert(vector[i]);

			// Send the array back.
			return vectors;
		}

		/// <summary>
		/// Function to convert an array of Gorgon matrices into an array of D3D 4D matrices.
		/// </summary>
		/// <param name="matrix">Gorgon matrices to convert.</param>
		/// <returns>D3D matrices.</returns>
		public static DX.Matrix[] Convert(Matrix[] matrix)
		{
			DX.Matrix[] matrices = new SlimDX.Matrix[matrix.Length];	// Create matrix array.

			// Copy.
			for (int i = 0; i < matrix.Length; i++)
				matrices[i] = Convert(matrix[i]);

			// Send the array back.
			return matrices;
		}

		/// <summary>
		/// Function to convert a Gorgon matrix into a D3D matrix.
		/// </summary>
		/// <param name="mat">Matrix to convert.</param>
		/// <returns>A D3D matrix.</returns>
		public static DX.Matrix Convert(Matrix mat)
		{
			DX.Matrix ret = new DX.Matrix();	// D3D Matrix.

			// Copy transposed.
			ret.M11 = mat.m11; ret.M21 = mat.m12; ret.M31 = mat.m13; ret.M41 = mat.m14;
			ret.M12 = mat.m21; ret.M22 = mat.m22; ret.M32 = mat.m23; ret.M42 = mat.m24;
			ret.M13 = mat.m31; ret.M23 = mat.m32; ret.M33 = mat.m33; ret.M43 = mat.m34;
			ret.M14 = mat.m41; ret.M24 = mat.m42; ret.M34 = mat.m43; ret.M44 = mat.m44;

			return ret;
		}

		/// <summary>
		/// Function to convert a D3D matrix into a Gorgon matrix.
		/// </summary>
		/// <param name="mat">D3D matrix to convert.</param>
		/// <returns>A matrix.</returns>
		public static Matrix Convert(DX.Matrix mat)
		{
			Matrix ret = new Matrix();	// D3D Matrix.

			// Copy transposed.
			ret.m11 = mat.M11; ret.m21 = mat.M12; ret.m31 = mat.M13; ret.m41 = mat.M14;
			ret.m12 = mat.M21; ret.m22 = mat.M22; ret.m32 = mat.M23; ret.m42 = mat.M24;
			ret.m13 = mat.M31; ret.m23 = mat.M32; ret.m33 = mat.M33; ret.m43 = mat.M34;
			ret.m14 = mat.M41; ret.m24 = mat.M42; ret.m34 = mat.M43; ret.m44 = mat.M44;

			return ret;
		}

		/// <summary>
		/// Function to convert a texture format into a Direct3D format.
		/// </summary>
		/// <param name="format">Format of the texture.</param>
		/// <returns>A Direct3D format.</returns>
		public static D3D9.Format Convert(ImageBufferFormats format)
		{
			switch (format)
			{
				case ImageBufferFormats.BufferBGR161616A16:
					return D3D9.Format.A16B16G16R16;
				case ImageBufferFormats.BufferBGR161616A16F:
					return D3D9.Format.A16B16G16R16F;
				case ImageBufferFormats.BufferRGB555A1:
					return D3D9.Format.A1R5G5B5;
				case ImageBufferFormats.BufferBGR101010A2:
					return D3D9.Format.A2B10G10R10;
				case ImageBufferFormats.BufferRGB101010A2:
					return D3D9.Format.A2R10G10B10;
				case ImageBufferFormats.BufferWVU101010A2:
					return D3D9.Format.A2W10V10U10;
				case ImageBufferFormats.BufferBGR323232A32F:
					return D3D9.Format.A32B32G32R32F;
				case ImageBufferFormats.BufferA4L4:
					return D3D9.Format.A4L4;
				case ImageBufferFormats.BufferRGB444A4:
					return D3D9.Format.A4R4G4B4;
				case ImageBufferFormats.BufferA8:
					return D3D9.Format.A8;
				case ImageBufferFormats.BufferRGB888A8:
					return D3D9.Format.A8R8G8B8;
				case ImageBufferFormats.BufferA8L8:
					return D3D9.Format.A8L8;
				case ImageBufferFormats.BufferA8P8:
					return D3D9.Format.A8P8;
				case ImageBufferFormats.BufferRGB332A8:
					return D3D9.Format.A8R3G3B2;
				case ImageBufferFormats.BufferVU88Cx:
					return D3D9.Format.CxV8U8;
				case ImageBufferFormats.BufferDXT1:
					return D3D9.Format.Dxt1;
				case ImageBufferFormats.BufferDXT2:
					return D3D9.Format.Dxt2;
				case ImageBufferFormats.BufferDXT3:
					return D3D9.Format.Dxt3;
				case ImageBufferFormats.BufferDXT4:
					return D3D9.Format.Dxt4;
				case ImageBufferFormats.BufferDXT5:
					return D3D9.Format.Dxt5;
				case ImageBufferFormats.BufferGR1616:
					return D3D9.Format.G16R16;
				case ImageBufferFormats.BufferGR1616F:
					return D3D9.Format.G16R16F;
				case ImageBufferFormats.BufferGR3232F:
					return D3D9.Format.G32R32F;
				case ImageBufferFormats.BufferL16:
					return D3D9.Format.L16;
				case ImageBufferFormats.BufferVU55L6:
					return D3D9.Format.L6V5U5;
				case ImageBufferFormats.BufferL8:
					return D3D9.Format.L8;
				case ImageBufferFormats.BufferMulti2RGBA:
					return D3D9.Format.Multi2Argb8;
				case ImageBufferFormats.BufferP8:
					return D3D9.Format.P8;
				case ImageBufferFormats.BufferQWVU16161616:
					return D3D9.Format.Q16W16V16U16;
				case ImageBufferFormats.BufferQWVU8888:
					return D3D9.Format.Q8W8V8U8;
				case ImageBufferFormats.BufferR16F:
					return D3D9.Format.R16F;
				case ImageBufferFormats.BufferR32F:
					return D3D9.Format.R32F;
				case ImageBufferFormats.BufferRGB332:
					return D3D9.Format.R3G3B2;
				case ImageBufferFormats.BufferRGB565:
					return D3D9.Format.R5G6B5;
				case ImageBufferFormats.BufferRGB888:
					return D3D9.Format.R8G8B8;
				case ImageBufferFormats.BufferVU1616:
					return D3D9.Format.V16U16;
				case ImageBufferFormats.BufferVU88:
					return D3D9.Format.V8U8;
				case ImageBufferFormats.BufferRGB555X1:
					return D3D9.Format.X1R5G5B5;
				case ImageBufferFormats.BufferRGB444X4:
					return D3D9.Format.X4R4G4B4;
				case ImageBufferFormats.BufferBGR888X8:
					return D3D9.Format.X8B8G8R8;
				case ImageBufferFormats.BufferLVU888X8:
					return D3D9.Format.X8L8V8U8;
				case ImageBufferFormats.BufferRGB888X8:
					return D3D9.Format.X8R8G8B8;
				case ImageBufferFormats.BufferYUY2:
					return D3D9.Format.Yuy2;
				default:
					return D3D9.Format.Unknown;
			}
		}

		/// <summary>
		/// Function to convert a Direct3D format into a texture format.
		/// </summary>
		/// <param name="format">Direct 3D format.</param>
		/// <returns>A texture format.</returns>
		public static ImageBufferFormats ConvertD3DImageFormat(D3D9.Format format)
		{
			switch (format)
			{
				case D3D9.Format.A16B16G16R16:
					return ImageBufferFormats.BufferBGR161616A16;
				case D3D9.Format.A16B16G16R16F:
					return ImageBufferFormats.BufferBGR161616A16F;
				case D3D9.Format.A1R5G5B5:
					return ImageBufferFormats.BufferRGB555A1;
				case D3D9.Format.A2B10G10R10:
					return ImageBufferFormats.BufferBGR101010A2;
				case D3D9.Format.A2R10G10B10:
					return ImageBufferFormats.BufferRGB101010A2;
				case D3D9.Format.A2W10V10U10:
					return ImageBufferFormats.BufferWVU101010A2;
				case D3D9.Format.A32B32G32R32F:
					return ImageBufferFormats.BufferBGR323232A32F;
				case D3D9.Format.A4L4:
					return ImageBufferFormats.BufferA4L4;
				case D3D9.Format.A4R4G4B4:
					return ImageBufferFormats.BufferRGB444A4;
				case D3D9.Format.A8:
					return ImageBufferFormats.BufferA8;
				case D3D9.Format.A8R8G8B8:
					return ImageBufferFormats.BufferRGB888A8;
				case D3D9.Format.A8L8:
					return ImageBufferFormats.BufferA8L8;
				case D3D9.Format.A8P8:
					return ImageBufferFormats.BufferA8P8;
				case D3D9.Format.A8R3G3B2:
					return ImageBufferFormats.BufferRGB332A8;
				case D3D9.Format.CxV8U8:
					return ImageBufferFormats.BufferVU88Cx;
				case D3D9.Format.Dxt1:
					return ImageBufferFormats.BufferDXT1;
				case D3D9.Format.Dxt2:
					return ImageBufferFormats.BufferDXT2;
				case D3D9.Format.Dxt3:
					return ImageBufferFormats.BufferDXT3;
				case D3D9.Format.Dxt4:
					return ImageBufferFormats.BufferDXT4;
				case D3D9.Format.Dxt5:
					return ImageBufferFormats.BufferDXT5;
				case D3D9.Format.G16R16:
					return ImageBufferFormats.BufferGR1616;
				case D3D9.Format.G16R16F:
					return ImageBufferFormats.BufferGR1616F;
				case D3D9.Format.G32R32F:
					return ImageBufferFormats.BufferGR3232F;
				case D3D9.Format.L16:
					return ImageBufferFormats.BufferL16;
				case D3D9.Format.L6V5U5:
					return ImageBufferFormats.BufferVU55L6;
				case D3D9.Format.L8:
					return ImageBufferFormats.BufferL8;
				case D3D9.Format.Multi2Argb8:
					return ImageBufferFormats.BufferMulti2RGBA;
				case D3D9.Format.P8:
					return ImageBufferFormats.BufferP8;
				case D3D9.Format.Q16W16V16U16:
					return ImageBufferFormats.BufferQWVU16161616;
				case D3D9.Format.Q8W8V8U8:
					return ImageBufferFormats.BufferQWVU8888;
				case D3D9.Format.R16F:
					return ImageBufferFormats.BufferR16F;
				case D3D9.Format.R32F:
					return ImageBufferFormats.BufferR32F;
				case D3D9.Format.R3G3B2:
					return ImageBufferFormats.BufferRGB332;
				case D3D9.Format.R5G6B5:
					return ImageBufferFormats.BufferRGB565;
				case D3D9.Format.R8G8B8:
					return ImageBufferFormats.BufferRGB888;
				case D3D9.Format.V16U16:
					return ImageBufferFormats.BufferVU1616;
				case D3D9.Format.V8U8:
					return ImageBufferFormats.BufferVU88;
				case D3D9.Format.X1R5G5B5:
					return ImageBufferFormats.BufferRGB555X1;
				case D3D9.Format.X4R4G4B4:
					return ImageBufferFormats.BufferRGB444X4;
				case D3D9.Format.X8B8G8R8:
					return ImageBufferFormats.BufferBGR888X8;
				case D3D9.Format.X8L8V8U8:
					return ImageBufferFormats.BufferLVU888X8;
				case D3D9.Format.X8R8G8B8:
					return ImageBufferFormats.BufferRGB888X8;
				case D3D9.Format.Yuy2:
					return ImageBufferFormats.BufferYUY2;
				default:
					return ImageBufferFormats.BufferUnknown;
			}
		}

		/// <summary>
		/// Function to convert a Gorgon image operation to a D3D texture operation.
		/// </summary>
		/// <param name="op">Gorgon operation to convert.</param>
		/// <returns>D3D texture operation.</returns>
		public static D3D9.TextureOperation Convert(ImageOperations op)
		{
			switch (op)
			{
				case ImageOperations.Additive:
					return D3D9.TextureOperation.Add;
				case ImageOperations.AdditiveSigned:
					return D3D9.TextureOperation.AddSigned;
				case ImageOperations.AdditiveSignedx2:
					return D3D9.TextureOperation.AddSigned2X;
				case ImageOperations.AdditiveSmooth:
					return D3D9.TextureOperation.AddSmooth;
				case ImageOperations.BlendCurrentAlpha:
					return D3D9.TextureOperation.BlendCurrentAlpha;
				case ImageOperations.BlendDiffuseAlpha:
					return D3D9.TextureOperation.BlendDiffuseAlpha;
				case ImageOperations.BlendFactorAlpha:
					return D3D9.TextureOperation.BlendFactorAlpha;
				case ImageOperations.BlendPreMultipliedTextureAlpha:
					return D3D9.TextureOperation.BlendTextureAlphaPM;
				case ImageOperations.BlendTextureAlpha:
					return D3D9.TextureOperation.BlendTextureAlpha;
				case ImageOperations.BumpDotProduct:
					return D3D9.TextureOperation.DotProduct3;
				case ImageOperations.BumpEnvironmentMap:
					return D3D9.TextureOperation.BumpEnvironmentMap;
				case ImageOperations.BumpEnvironmentMapLuminance:
					return D3D9.TextureOperation.BumpEnvironmentMapLuminance;
				case ImageOperations.Lerp:
					return D3D9.TextureOperation.Lerp;
				case ImageOperations.Modulate:
					return D3D9.TextureOperation.Modulate;
				case ImageOperations.ModulateAlphaAddColor:
					return D3D9.TextureOperation.ModulateAlphaAddColor;
				case ImageOperations.ModulateColorAddAlpha:
					return D3D9.TextureOperation.ModulateColorAddAlpha;
				case ImageOperations.ModulateInverseAlphaAddColor:
					return D3D9.TextureOperation.ModulateInvAlphaAddColor;
				case ImageOperations.ModulateInverseColorAddAlpha:
					return D3D9.TextureOperation.ModulateInvColorAddAlpha;
				case ImageOperations.Modulatex2:
					return D3D9.TextureOperation.Modulate2X;
				case ImageOperations.Modulatex4:
					return D3D9.TextureOperation.Modulate4X;
				case ImageOperations.MultiplyAdd:
					return D3D9.TextureOperation.MultiplyAdd;
				case ImageOperations.PreModulate:
					return D3D9.TextureOperation.Premodulate;
				case ImageOperations.SelectArgument1:
					return D3D9.TextureOperation.SelectArg1;
				case ImageOperations.SelectArgument2:
					return D3D9.TextureOperation.SelectArg2;
				case ImageOperations.Subtract:
					return D3D9.TextureOperation.Subtract;
			}

			return D3D9.TextureOperation.Disable;
		}

		/// <summary>
		/// Function to convert a Gorgon image operation argument to a D3D texture argument.
		/// </summary>
		/// <param name="arg">Gorgon operation argument to convert.</param>
		/// <returns>D3D texture argument.</returns>
		public static D3D9.TextureArgument Convert(ImageOperationArguments arg)
		{
			switch (arg)
			{
				case ImageOperationArguments.AlphaReplicate:
					return D3D9.TextureArgument.AlphaReplicate;
				case ImageOperationArguments.Complement:
					return D3D9.TextureArgument.Complement;
				case ImageOperationArguments.Constant:
					return D3D9.TextureArgument.Constant;
				case ImageOperationArguments.Diffuse:
					return D3D9.TextureArgument.Diffuse;
				case ImageOperationArguments.Specular:
					return D3D9.TextureArgument.Specular;
				case ImageOperationArguments.Temp:
					return D3D9.TextureArgument.Temp;
				case ImageOperationArguments.Texture:
					return D3D9.TextureArgument.Texture;
				case ImageOperationArguments.TextureFactor:
					return D3D9.TextureArgument.TFactor;
			}

			return D3D9.TextureArgument.Current;
		}

		/// <summary>
		/// Function to convert a Gorgon image filter into a D3D texture filter.
		/// </summary>
		/// <param name="filter">Gorgon image filter.</param>
		/// <returns>D3D texture filter.</returns>
		public static D3D9.TextureFilter Convert(ImageFilters filter)
		{
			switch (filter)
			{
				case ImageFilters.Anisotropic:
					return D3D9.TextureFilter.Anisotropic;
				case ImageFilters.Bilinear:
					return D3D9.TextureFilter.Linear;
				case ImageFilters.Point:
					return D3D9.TextureFilter.Point;
				case ImageFilters.GaussianQuadratic:
					return D3D9.TextureFilter.GaussianQuad;
				case ImageFilters.PyramidalQuadratic:
					return D3D9.TextureFilter.PyramidalQuad;
			}
			return D3D9.TextureFilter.None;
		}

		/// <summary>
		/// Function to convert a Gorgon stencil operation into a D3D stencil operation.
		/// </summary>
		/// <param name="operation">Gorgon stencil operation.</param>
		/// <returns>A D3D stencil operation.</returns>
		public static D3D9.StencilOperation Convert(StencilOperations operation)
		{
			switch (operation)
			{
				case StencilOperations.Decrement:
					return D3D9.StencilOperation.Decrement;
				case StencilOperations.DecrementSaturate:
					return D3D9.StencilOperation.DecrementSaturate;
				case StencilOperations.Increment:
					return D3D9.StencilOperation.Increment;
				case StencilOperations.IncrementSaturate:
					return D3D9.StencilOperation.IncrementSaturate;
				case StencilOperations.Invert:
					return D3D9.StencilOperation.Invert;
				case StencilOperations.Replace:
					return D3D9.StencilOperation.Replace;
				case StencilOperations.Zero:
					return D3D9.StencilOperation.Zero;
			}

			return D3D9.StencilOperation.Keep;
		}
		#endregion
	}
}
