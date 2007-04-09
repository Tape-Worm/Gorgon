#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Saturday, July 09, 2005 5:32:17 PM
// 
#endregion

using System;
using SharpUtilities.Mathematics;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Class used to convert gorgon enumerations/values to Direct 3D values.
	/// </summary>
	public static class Converter
	{
		#region Methods.
		/// <summary>
		/// Function to convert a Gorgon image file format to a D3D image file format.
		/// </summary>
		/// <param name="format">Format to convert.</param>
		/// <returns>Converted format.</returns>
		static public D3D.ImageFileFormat Convert(ImageFileFormat format)
		{
			switch (format)
			{
				case ImageFileFormat.DDS:
					return D3D.ImageFileFormat.Dds;
				case ImageFileFormat.JPEG:
					return D3D.ImageFileFormat.Jpg;
				case ImageFileFormat.PNG:
					return D3D.ImageFileFormat.Png;
				case ImageFileFormat.TGA:
					return D3D.ImageFileFormat.Tga;
				case ImageFileFormat.DIB:
					return D3D.ImageFileFormat.Dib;
				case ImageFileFormat.PFM:
					return D3D.ImageFileFormat.Pfm;
				case ImageFileFormat.PPM:
					return D3D.ImageFileFormat.Ppm;
			}

			return D3D.ImageFileFormat.Bmp;
		}

		/// <summary>
		/// Function to convert a D3D image file format to a Gorgon image file format.
		/// </summary>
		/// <param name="format">Format to convert.</param>
		/// <returns>Converted format.</returns>
		static public ImageFileFormat Convert(D3D.ImageFileFormat format)
		{
			switch (format)
			{
				case D3D.ImageFileFormat.Dds:
					return ImageFileFormat.DDS;
				case D3D.ImageFileFormat.Jpg:
					return ImageFileFormat.JPEG;
				case D3D.ImageFileFormat.Png:
					return ImageFileFormat.PNG;
				case D3D.ImageFileFormat.Tga:
					return ImageFileFormat.TGA;
				case D3D.ImageFileFormat.Dib:
					return ImageFileFormat.DIB;
				case D3D.ImageFileFormat.Pfm:
					return ImageFileFormat.PFM;
				case D3D.ImageFileFormat.Ppm:
					return ImageFileFormat.PPM;
			}

			return ImageFileFormat.BMP;
		}

		/// <summary>
		/// Function to convert an imaging addressing mode into a D3D texture addressing mode.
		/// </summary>
		/// <param name="address">Addressing mode to convert.</param>
		/// <returns>D3D texture addressing.</returns>
		static public D3D.TextureAddress Convert(ImageAddressing address)
		{
			switch (address)
			{
				case ImageAddressing.Border:
					return D3D.TextureAddress.Border;
				case ImageAddressing.Clamp:
					return D3D.TextureAddress.Clamp;
				case ImageAddressing.Mirror:
					return D3D.TextureAddress.Mirror;
				case ImageAddressing.MirrorOnce:
					return D3D.TextureAddress.MirrorOnce;
			}

			return D3D.TextureAddress.Wrap;
		}

		/// <summary>
		/// Function to return the appropriate declaration usage based on our context.
		/// </summary>
		/// <param name="context">Context to convert.</param>
		/// <returns>An appropriate D3D declaration usage.</returns>
		static public D3D.DeclarationUsage Convert(VertexFieldContext context)
		{
			switch (context)
			{
				case VertexFieldContext.Binormal:
					return D3D.DeclarationUsage.BiNormal;
				case VertexFieldContext.BlendIndices:
					return D3D.DeclarationUsage.BlendIndices;
				case VertexFieldContext.BlendWeights:
					return D3D.DeclarationUsage.BlendWeight;
				case VertexFieldContext.Diffuse:
					return D3D.DeclarationUsage.Color;
				case VertexFieldContext.Normal:
					return D3D.DeclarationUsage.Normal;
				case VertexFieldContext.Position:
					return D3D.DeclarationUsage.Position;
				case VertexFieldContext.Specular:
					// Index should be 1.
					return D3D.DeclarationUsage.Color;
				case VertexFieldContext.Tangent:
					return D3D.DeclarationUsage.Tangent;
				case VertexFieldContext.TexCoords:
					return D3D.DeclarationUsage.TextureCoordinate;
			}

			throw new InvalidVertexFieldContextException(null);
		}

		/// <summary>
		/// Function to convert the field type into an appropriate D3D element type.
		/// </summary>
		/// <param name="fieldType">Field type to convert.</param>
		/// <returns>Appropriate D3D declaration data type.</returns>
		static public D3D.DeclarationType Convert(VertexFieldType fieldType)
		{
			switch (fieldType)
			{
				case VertexFieldType.Color:
					return D3D.DeclarationType.Color;
				case VertexFieldType.Float1:
					return D3D.DeclarationType.Float1;
				case VertexFieldType.Float2:
					return D3D.DeclarationType.Float2;
				case VertexFieldType.Short1:
				case VertexFieldType.Short3:
				case VertexFieldType.Float3:
					return D3D.DeclarationType.Float3;
				case VertexFieldType.Float4:
					return D3D.DeclarationType.Float4;
				case VertexFieldType.Short2:
					return D3D.DeclarationType.Short2;
				case VertexFieldType.Short4:
					return D3D.DeclarationType.Short4;
				case VertexFieldType.UByte4:
					return D3D.DeclarationType.Ubyte4;
			}

			throw new InvalidVertexFieldTypeException(null);
		}

		/// <summary>
		/// Function to return an appropriate D3D shading mode based on our shading mode.
		/// </summary>
		/// <param name="shademode">Shading mode to convert.</param>
		/// <returns>D3D shade mode.</returns>
		static public D3D.ShadeMode Convert(ShadingMode shademode)
		{
			switch(shademode)
			{
				case ShadingMode.Flat:
					return D3D.ShadeMode.Flat;
			}

			return D3D.ShadeMode.Gouraud;
		}

		/// <summary>
		/// Function to return a Gorgon shading mode based on a Direct 3D shade mode.
		/// </summary>
		/// <param name="shademode">Shade mode to convert.</param>
		/// <returns>Gorgon shading mode.</returns>
		static public ShadingMode Convert(D3D.ShadeMode shademode)
		{
			switch(shademode)
			{
				case D3D.ShadeMode.Flat:
					return ShadingMode.Flat;
			}

			return ShadingMode.Gouraud;
		}

		/// <summary>
		/// Function to return a D3D blending operation based on a Gorgon blending operation.
		/// </summary>
		/// <param name="alphaop">Alpha operation to convert.</param>
		/// <returns>D3D blending operation.</returns>
		static public D3D.Blend Convert(AlphaBlendOperation alphaop)
		{
			switch (alphaop)
			{
				case AlphaBlendOperation.BlendFactor:
					return D3D.Blend.BlendFactor;
				case AlphaBlendOperation.BothInverseSourceAlpha:
					return D3D.Blend.BothInvSourceAlpha;
				case AlphaBlendOperation.DestinationAlpha:
					return D3D.Blend.DestinationAlpha;
				case AlphaBlendOperation.DestinationColor:
					return D3D.Blend.DestinationColor;
				case AlphaBlendOperation.InverseBlendFactor:
					return D3D.Blend.InvBlendFactor;
				case AlphaBlendOperation.InverseDestinationAlpha:
					return D3D.Blend.InvDestinationAlpha;
				case AlphaBlendOperation.InverseDestinationColor:
					return D3D.Blend.InvDestinationColor;
				case AlphaBlendOperation.InverseSourceAlpha:
					return D3D.Blend.InvSourceAlpha;
				case AlphaBlendOperation.InverseSourceColor:
					return D3D.Blend.InvSourceColor;
				case AlphaBlendOperation.SourceAlpha:
					return D3D.Blend.SourceAlpha;
				case AlphaBlendOperation.SourceAlphaSaturation:
					return D3D.Blend.SourceAlphaSat;
				case AlphaBlendOperation.SourceColor:
					return D3D.Blend.SourceColor;
				case AlphaBlendOperation.Zero:
					return D3D.Blend.Zero;
			}

			return D3D.Blend.One;
		}

		/// <summary>
		/// Function to return an appropriate D3D fill mode based on our drawing mode.
		/// </summary>
		/// <param name="drawmode">Drawing mode to convert.</param>
		/// <returns>D3D fill mode.</returns>
		static public D3D.FillMode Convert(DrawingMode drawmode)
		{
			switch(drawmode)
			{
				case DrawingMode.Points:
					return D3D.FillMode.Point;
				case DrawingMode.Wireframe:
					return D3D.FillMode.WireFrame;
			}

			return D3D.FillMode.Solid;
		}

		/// <summary>
		/// Function to return a Gorgon drawing mode based on a Direct 3D fill mode.
		/// </summary>
		/// <param name="drawmode">Drawing mode to convert.</param>
		/// <returns>Gorgon drawing mode.</returns>
		static public DrawingMode Convert(D3D.FillMode drawmode)
		{
			switch(drawmode)
			{
				case D3D.FillMode.Point:
					return DrawingMode.Points;
				case D3D.FillMode.WireFrame:
					return DrawingMode.Wireframe;
			}

			return DrawingMode.Solid;
		}

		/// <summary>
		/// Function to return the appropriate D3D primitive style based on our primitive style.
		/// </summary>
		/// <param name="primitivetype">Style to convert.</param>
		/// <returns>D3D primitive type.</returns>
		static public D3D.PrimitiveType Convert(PrimitiveStyle primitivetype)
		{
			switch(primitivetype)
			{
				case PrimitiveStyle.LineList:
					return D3D.PrimitiveType.LineList;
				case PrimitiveStyle.LineStrip:
					return D3D.PrimitiveType.LineStrip;
				case PrimitiveStyle.PointList:
					return D3D.PrimitiveType.PointList;
				case PrimitiveStyle.TriangleFan:
					return D3D.PrimitiveType.TriangleFan;
				case PrimitiveStyle.TriangleStrip:
					return D3D.PrimitiveType.TriangleStrip;
				default:
					return D3D.PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Function to convert D3D presentation intervals to vsync intervals.
		/// </summary>
		/// <param name="interval">Interval to convert.</param>
		/// <returns>Equivalent vsync interval.</returns>
		public static VSyncIntervals Convert(D3D.PresentInterval interval)
		{
			switch (interval)
			{
				case D3D.PresentInterval.Immediate:
					return VSyncIntervals.IntervalNone;
				case D3D.PresentInterval.One:
					return VSyncIntervals.IntervalOne;
				case D3D.PresentInterval.Two:
					return VSyncIntervals.IntervalTwo;
				case D3D.PresentInterval.Three:
					return VSyncIntervals.IntervalThree;
				case D3D.PresentInterval.Four:
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
		public static D3D.PresentInterval Convert(VSyncIntervals interval)
		{
			switch (interval)
			{
				case VSyncIntervals.IntervalNone:
					return D3D.PresentInterval.Immediate;
				case VSyncIntervals.IntervalOne:
					return D3D.PresentInterval.One;
				case VSyncIntervals.IntervalTwo:
					return D3D.PresentInterval.Two;
				case VSyncIntervals.IntervalThree:
					return D3D.PresentInterval.Three;
				case VSyncIntervals.IntervalFour:
					return D3D.PresentInterval.Four;
				default:
					return D3D.PresentInterval.Immediate;
			}
		}

		/// <summary>
		/// Function to convert backbuffer formats to D3D formats.
		/// </summary>
		/// <param name="format">Backbuffer format to convert.</param>
		/// <returns>Equivalent D3D format.</returns>
		public static D3D.Format Convert(BackBufferFormats format)
		{
			switch (format)
			{
				case BackBufferFormats.BufferRGB101010A2:
					return D3D.Format.A2R10G10B10;
				case BackBufferFormats.BufferRGB555:
					return D3D.Format.X1R5G5B5;
				case BackBufferFormats.BufferRGB565:
					return D3D.Format.R5G6B5;
				case BackBufferFormats.BufferRGB888:
					return D3D.Format.X8R8G8B8;
				case BackBufferFormats.BufferP8:
					return D3D.Format.P8;
				default:
					throw new FormatNotSupportedException(format, null);
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
					throw new FormatNotSupportedException(format, null);
			}
		}

		/// <summary>
		/// Function to convert D3D formats to backbuffer formats.
		/// </summary>
		/// <param name="format">D3D format to convert.</param>
		/// <returns>Equivalent backbuffer format .</returns>
		public static BackBufferFormats Convert(D3D.Format format)
		{
			switch (format)
			{
				case D3D.Format.A2R10G10B10:
					return BackBufferFormats.BufferRGB101010A2;
				case D3D.Format.X1R5G5B5:
					return BackBufferFormats.BufferRGB555;
				case D3D.Format.R5G6B5:
					return BackBufferFormats.BufferRGB565;
				case D3D.Format.X8R8G8B8:
					return BackBufferFormats.BufferRGB888;
				case D3D.Format.P8:
					return BackBufferFormats.BufferP8;
				default:
					throw new FormatNotSupportedException("The Direct3D format " + format.ToString() + " is not supported.", null);
			}
		}

		/// <summary>
		/// Function to convert buffer usage flags to Direct 3D Usage flags.
		/// </summary>
		/// <param name="flags">Flags to convert.</param>
		/// <returns>D3D Usage flags.</returns>
		public static D3D.Usage Convert(BufferUsage flags)
		{
			D3D.Usage	d3dflags;		// Direct 3D flags.

			d3dflags = D3D.Usage.None;

			if ((flags & BufferUsage.Dynamic) != 0)
				d3dflags |= D3D.Usage.Dynamic;

			if ((flags & BufferUsage.WriteOnly) != 0)
				d3dflags |= D3D.Usage.WriteOnly;

			if ((flags & BufferUsage.ForceSoftware) != 0)
				d3dflags |= D3D.Usage.SoftwareProcessing;				
			
			return d3dflags;
		}

		/// <summary>
		/// Function to convert buffer locking flags to Direct 3D locking flags.
		/// </summary>
		/// <param name="flags">Flags to convert.</param>
		/// <returns>D3D lock flags.</returns>
		public static D3D.LockFlags Convert(BufferLockFlags flags)
		{
			switch(flags)
			{
				case BufferLockFlags.Discard:
					return D3D.LockFlags.Discard;
				case BufferLockFlags.NoOverwrite:
					return D3D.LockFlags.NoOverwrite;
				case BufferLockFlags.ReadOnly:
					return D3D.LockFlags.ReadOnly;
			}

			return D3D.LockFlags.None;
		}

		/// <summary>
		/// Function to convert a culling mode into a Direct 3D cull mode.
		/// </summary>
		/// <param name="cullmode">Cull mode to convert.</param>
		/// <returns>D3D culling mode.</returns>
		public static D3D.Cull Convert(CullingMode cullmode)
		{
			switch(cullmode)
			{
				case CullingMode.CounterClockwise:
					return D3D.Cull.CounterClockwise;
				case CullingMode.Clockwise:
					return D3D.Cull.Clockwise;
				default:
					return D3D.Cull.None;
			}			
		}

		/// <summary>
		/// Function to convert a D3D culling mode into a Gorgon culling mode.
		/// </summary>
		/// <param name="cullmode">D3D culling mode.</param>
		/// <returns>Gorgon culling mode.</returns>
		public static CullingMode Convert(D3D.Cull cullmode)
		{
			switch(cullmode)
			{
				case D3D.Cull.Clockwise:
					return CullingMode.Clockwise;
				case D3D.Cull.CounterClockwise:
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
		public static D3D.Compare Convert(CompareFunctions func)
		{
			switch(func)
			{
				case CompareFunctions.Always:
					return D3D.Compare.Always;
				case CompareFunctions.Equal:
					return D3D.Compare.Equal;
				case CompareFunctions.GreaterThan:
					return D3D.Compare.Greater;
				case CompareFunctions.GreaterThanOrEqual:
					return D3D.Compare.GreaterEqual;
				case CompareFunctions.LessThan:
					return D3D.Compare.Less;
				case CompareFunctions.Never:
					return D3D.Compare.Never;
				case CompareFunctions.NotEqual:
					return D3D.Compare.NotEqual;
				default:
					return D3D.Compare.LessEqual;
			}
		}

		/// <summary>
		/// Function to convert D3D comparison functions to Gorgon comparison functions.
		/// </summary>
		/// <param name="func">D3D comparison function to convert.</param>
		/// <returns>Gorgon comparison function.</returns>
		public static CompareFunctions Convert(D3D.Compare func)
		{
			switch(func)
			{
				case D3D.Compare.Always:
					return CompareFunctions.Always;
				case D3D.Compare.Equal:
					return CompareFunctions.Equal;
				case D3D.Compare.Greater:
					return CompareFunctions.GreaterThan;
				case D3D.Compare.GreaterEqual:
					return CompareFunctions.GreaterThanOrEqual;
				case D3D.Compare.Less:
					return CompareFunctions.LessThan;
				case D3D.Compare.Never:
					return CompareFunctions.Never;
				case D3D.Compare.NotEqual:
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
		public static D3D.DepthFormat Convert(DepthBufferFormats depthFormat)
		{
			switch (depthFormat)
			{
				case DepthBufferFormats.BufferLuminance16:
					return D3D.DepthFormat.L16;
				case DepthBufferFormats.BufferDepth15Stencil1:
					return D3D.DepthFormat.D15S1;
				case DepthBufferFormats.BufferDepth16Lockable:
					return D3D.DepthFormat.D16Lockable;
				case DepthBufferFormats.BufferDepth16:
					return D3D.DepthFormat.D16;
				case DepthBufferFormats.BufferDepth24:
					return D3D.DepthFormat.D24X8;
				case DepthBufferFormats.BufferDepth24Stencil4:
					return D3D.DepthFormat.D24X4S4;
				case DepthBufferFormats.BufferDepth24Stencil8:
					return D3D.DepthFormat.D24S8;
				case DepthBufferFormats.BufferDepth24Stencil8Lockable:
					return D3D.DepthFormat.D24SingleS8;
				case DepthBufferFormats.BufferDepth32:
					return D3D.DepthFormat.D32;
				case DepthBufferFormats.BufferDepth32Lockable:
					return D3D.DepthFormat.D32SingleLockable;
			}

			return D3D.DepthFormat.Unknown;
		}

		/// <summary>
		/// Function to convert a depth buffer format into a D3D compatiable format.
		/// </summary>
		/// <param name="depthFormat">Depth format to convert.</param>
		/// <returns>A compatiable Direct3D depth format.</returns>
		public static DepthBufferFormats Convert(D3D.DepthFormat depthFormat)
		{
			switch (depthFormat)
			{
				case D3D.DepthFormat.L16:
					return DepthBufferFormats.BufferLuminance16;
				case D3D.DepthFormat.D15S1:
					return DepthBufferFormats.BufferDepth15Stencil1;
				case D3D.DepthFormat.D16Lockable:
					return DepthBufferFormats.BufferDepth16Lockable;
				case D3D.DepthFormat.D16:
					return DepthBufferFormats.BufferDepth16;
				case D3D.DepthFormat.D24X8:
					return DepthBufferFormats.BufferDepth24;
				case D3D.DepthFormat.D24X4S4:
					return DepthBufferFormats.BufferDepth24Stencil4;
				case D3D.DepthFormat.D24S8:
					return DepthBufferFormats.BufferDepth24Stencil8;
				case D3D.DepthFormat.D24SingleS8:
					return DepthBufferFormats.BufferDepth24Stencil8Lockable;
				case D3D.DepthFormat.D32SingleLockable:
					return DepthBufferFormats.BufferDepth32Lockable;
				case D3D.DepthFormat.D32:
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
			return new Vector3D(vector.X, vector.Y,vector.Z);
		}

		/// <summary>
		/// Function to convert a Gorgon 3D vector into a D3D 3D vector.
		/// </summary>
		/// <param name="vector">Gorgon vector to convert.</param>
		/// <returns>D3D 3D vector.</returns>
		public static DX.Vector3 Convert(Vector3D vector)
		{
			return new DX.Vector3(vector.X, vector.Y,vector.Z);
		}

		/// <summary>
		/// Function to convert a D3D 4D vector into a Gorgon 4D vector.
		/// </summary>
		/// <param name="vector">D3D vector to convert.</param>
		/// <returns>Gorgon 4D vector.</returns>
		public static Vector4D Convert(DX.Vector4 vector)
		{
			return new Vector4D(vector.X, vector.Y,vector.Z,vector.W);
		}

		/// <summary>
		/// Function to convert a Gorgon 4D vector into a D3D 4D vector.
		/// </summary>
		/// <param name="vector">Gorgon vector to convert.</param>
		/// <returns>D3D 4D vector.</returns>
		public static DX.Vector4 Convert(Vector4D vector)
		{
			return new DX.Vector4(vector.X, vector.Y,vector.Z,vector.W);
		}

		/// <summary>
		/// Function to convert a Gorgon matrix into a D3D matrix.
		/// </summary>
		/// <param name="mat">Matrix to convert.</param>
		/// <returns>A D3D matrix.</returns>
		public static  DX.Matrix Convert(Matrix mat)
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
		/// Function to convert a quaternion into a D3D quaternion.
		/// </summary>
		/// <param name="q">Quaternion to convert.</param>
		/// <returns>D3D quaternion.</returns>
		public static DX.Quaternion Convert(Quaternion q)
		{
			return new DX.Quaternion(q.X, q.Y, q.Z, q.W);
		}

		/// <summary>
		/// Function to convert a quaternion into a D3D quaternion.
		/// </summary>
		/// <param name="q">Quaternion to convert.</param>
		/// <returns>D3D quaternion.</returns>
		public static Quaternion Convert(DX.Quaternion q)
		{
			return new Quaternion(q.X, q.Y, q.Z, q.W);
		}

		/// <summary>
		/// Function to convert a texture format into a Direct3D format.
		/// </summary>
		/// <param name="format">Format of the texture.</param>
		/// <returns>A Direct3D format.</returns>
		public static D3D.Format Convert(ImageBufferFormats format)
		{
			switch (format)
			{
				case ImageBufferFormats.BufferBGR161616A16:
					return D3D.Format.A16B16G16R16;
				case ImageBufferFormats.BufferBGR161616A16F:
					return D3D.Format.A16B16G16R16F;
				case ImageBufferFormats.BufferRGB555A1:
					return D3D.Format.A1R5G5B5;
				case ImageBufferFormats.BufferBGR101010A2:
					return D3D.Format.A2B10G10R10;
				case ImageBufferFormats.BufferRGB101010A2:
					return D3D.Format.A2R10G10B10;
				case ImageBufferFormats.BufferWVU101010A2:
					return D3D.Format.A2W10V10U10;
				case ImageBufferFormats.BufferBGR323232A32F:
					return D3D.Format.A32B32G32R32F;
				case ImageBufferFormats.BufferA4L4:
					return D3D.Format.A4L4;
				case ImageBufferFormats.BufferRGB444A4:
					return D3D.Format.A4R4G4B4;
				case ImageBufferFormats.BufferA8:
					return D3D.Format.A8;
				case ImageBufferFormats.BufferRGB888A8:
					return D3D.Format.A8R8G8B8;
				case ImageBufferFormats.BufferA8L8:
					return D3D.Format.A8L8;
				case ImageBufferFormats.BufferA8P8:
					return D3D.Format.A8P8;
				case ImageBufferFormats.BufferRGB332A8:
					return D3D.Format.A8R3G3B2;
				case ImageBufferFormats.BufferVU88Cx:
					return D3D.Format.CxV8U8;
				case ImageBufferFormats.BufferDXT1:
					return D3D.Format.Dxt1;
				case ImageBufferFormats.BufferDXT2:
					return D3D.Format.Dxt2;
				case ImageBufferFormats.BufferDXT3:
					return D3D.Format.Dxt3;
				case ImageBufferFormats.BufferDXT4:
					return D3D.Format.Dxt4;
				case ImageBufferFormats.BufferDXT5:
					return D3D.Format.Dxt5;
				case ImageBufferFormats.BufferGR1616:
					return D3D.Format.G16R16;
				case ImageBufferFormats.BufferGR1616F:
					return D3D.Format.G16R16F;
				case ImageBufferFormats.BufferGR3232F:
					return D3D.Format.G32R32F;
				case ImageBufferFormats.BufferG8RGB888:
					return D3D.Format.G8R8G8B8;
				case ImageBufferFormats.BufferL16:
					return D3D.Format.L16;
				case ImageBufferFormats.BufferVU55L6:
					return D3D.Format.L6V5U5;
				case ImageBufferFormats.BufferL8:
					return D3D.Format.L8;
				case ImageBufferFormats.BufferMulti2RGBA:
					return D3D.Format.Multi2Argb8;
				case ImageBufferFormats.BufferP8:
					return D3D.Format.P8;
				case ImageBufferFormats.BufferQWVU16161616:
					return D3D.Format.Q16W16V16U16;
				case ImageBufferFormats.BufferQWVU8888:
					return D3D.Format.Q8W8V8U8;
				case ImageBufferFormats.BufferR16F:
					return D3D.Format.R16F;
				case ImageBufferFormats.BufferR32F:
					return D3D.Format.R32F;
				case ImageBufferFormats.BufferRGB332:
					return D3D.Format.R3G3B2;
				case ImageBufferFormats.BufferRGB565:
					return D3D.Format.R5G6B5;
				case ImageBufferFormats.BufferRGB888:
					return D3D.Format.R8G8B8;
				case ImageBufferFormats.BufferRGB888G8:
					return D3D.Format.R8G8B8G8;
				case ImageBufferFormats.BufferVU1616:
					return D3D.Format.V16U16;
				case ImageBufferFormats.BufferVU88:
					return D3D.Format.V8U8;
				case ImageBufferFormats.BufferRGB555X1:
					return D3D.Format.X1R5G5B5;
				case ImageBufferFormats.BufferRGB444X4:
					return D3D.Format.X4R4G4B4;
				case ImageBufferFormats.BufferBGR888X8:
					return D3D.Format.X8B8G8R8;
				case ImageBufferFormats.BufferLVU888X8:
					return D3D.Format.X8L8V8U8;
				case ImageBufferFormats.BufferRGB888X8:
					return D3D.Format.X8R8G8B8;
				case ImageBufferFormats.BufferYUY2:
					return D3D.Format.Yuy2;
				default:
					return D3D.Format.Unknown;
			}
		}

		/// <summary>
		/// Function to convert a Direct3D format into a texture format.
		/// </summary>
		/// <param name="format">Direct 3D format.</param>
		/// <returns>A texture format.</returns>
		public static ImageBufferFormats ConvertD3DImageFormat(D3D.Format format)
		{
			switch (format)
			{
				case D3D.Format.A16B16G16R16:
					return ImageBufferFormats.BufferBGR161616A16;
				case D3D.Format.A16B16G16R16F:
					return ImageBufferFormats.BufferBGR161616A16F;
				case D3D.Format.A1R5G5B5:
					return ImageBufferFormats.BufferRGB555A1;
				case D3D.Format.A2B10G10R10:
					return ImageBufferFormats.BufferBGR101010A2;
				case D3D.Format.A2R10G10B10:
					return ImageBufferFormats.BufferRGB101010A2;
				case D3D.Format.A2W10V10U10:
					return ImageBufferFormats.BufferWVU101010A2;
				case D3D.Format.A32B32G32R32F:
					return ImageBufferFormats.BufferBGR323232A32F;
				case D3D.Format.A4L4:
					return ImageBufferFormats.BufferA4L4;
				case D3D.Format.A4R4G4B4:
					return ImageBufferFormats.BufferRGB444A4;
				case D3D.Format.A8:
					return ImageBufferFormats.BufferA8;
				case D3D.Format.A8R8G8B8:
					return ImageBufferFormats.BufferRGB888A8;
				case D3D.Format.A8L8:
					return ImageBufferFormats.BufferA8L8;
				case D3D.Format.A8P8:
					return ImageBufferFormats.BufferA8P8;
				case D3D.Format.A8R3G3B2:
					return ImageBufferFormats.BufferRGB332A8;
				case D3D.Format.CxV8U8:
					return ImageBufferFormats.BufferVU88Cx;
				case D3D.Format.Dxt1:
					return ImageBufferFormats.BufferDXT1;
				case D3D.Format.Dxt2:
					return ImageBufferFormats.BufferDXT2;
				case D3D.Format.Dxt3:
					return ImageBufferFormats.BufferDXT3;
				case D3D.Format.Dxt4:
					return ImageBufferFormats.BufferDXT4;
				case D3D.Format.Dxt5:
					return ImageBufferFormats.BufferDXT5;
				case D3D.Format.G16R16:
					return ImageBufferFormats.BufferGR1616;
				case D3D.Format.G16R16F:
					return ImageBufferFormats.BufferGR1616F;
				case D3D.Format.G32R32F:
					return ImageBufferFormats.BufferGR3232F;
				case D3D.Format.G8R8G8B8:
					return ImageBufferFormats.BufferG8RGB888;
				case D3D.Format.L16:
					return ImageBufferFormats.BufferL16;
				case D3D.Format.L6V5U5:
					return ImageBufferFormats.BufferVU55L6;
				case D3D.Format.L8:
					return ImageBufferFormats.BufferL8;
				case D3D.Format.Multi2Argb8:
					return ImageBufferFormats.BufferMulti2RGBA;
				case D3D.Format.P8:
					return ImageBufferFormats.BufferP8;
				case D3D.Format.Q16W16V16U16:
					return ImageBufferFormats.BufferQWVU16161616;
				case D3D.Format.Q8W8V8U8:
					return ImageBufferFormats.BufferQWVU8888;
				case D3D.Format.R16F:
					return ImageBufferFormats.BufferR16F;
				case D3D.Format.R32F:
					return ImageBufferFormats.BufferR32F;
				case D3D.Format.R3G3B2:
					return ImageBufferFormats.BufferRGB332;
				case D3D.Format.R5G6B5:
					return ImageBufferFormats.BufferRGB565;
				case D3D.Format.R8G8B8:
					return ImageBufferFormats.BufferRGB888;
				case D3D.Format.R8G8B8G8:
					return ImageBufferFormats.BufferRGB888G8;
				case D3D.Format.V16U16:
					return ImageBufferFormats.BufferVU1616;
				case D3D.Format.V8U8:
					return ImageBufferFormats.BufferVU88;
				case D3D.Format.X1R5G5B5:
					return ImageBufferFormats.BufferRGB555X1;
				case D3D.Format.X4R4G4B4:
					return ImageBufferFormats.BufferRGB444X4;
				case D3D.Format.X8B8G8R8:
					return ImageBufferFormats.BufferBGR888X8;
				case D3D.Format.X8L8V8U8:
					return ImageBufferFormats.BufferLVU888X8;
				case D3D.Format.X8R8G8B8:
					return ImageBufferFormats.BufferRGB888X8;
				case D3D.Format.Yuy2:
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
		public static D3D.TextureOperation Convert(ImageOperations op)
		{
			switch (op)
			{
				case ImageOperations.Additive:
					return D3D.TextureOperation.Add;
				case ImageOperations.AdditiveSigned:
					return D3D.TextureOperation.AddSigned;
				case ImageOperations.AdditiveSignedx2:
					return D3D.TextureOperation.AddSigned2X;
				case ImageOperations.AdditiveSmooth:
					return D3D.TextureOperation.AddSmooth;
				case ImageOperations.BlendCurrentAlpha:
					return D3D.TextureOperation.BlendCurrentAlpha;
				case ImageOperations.BlendDiffuseAlpha:
					return D3D.TextureOperation.BlendDiffuseAlpha;
				case ImageOperations.BlendFactorAlpha:
					return D3D.TextureOperation.BlendFactorAlpha;
				case ImageOperations.BlendPreMultipliedTextureAlpha:
					return D3D.TextureOperation.BlendTextureAlphaPM;
				case ImageOperations.BlendTextureAlpha:
					return D3D.TextureOperation.BlendTextureAlpha;
				case ImageOperations.BumpDotProduct:
					return D3D.TextureOperation.DotProduct3;
				case ImageOperations.BumpEnvironmentMap:
					return D3D.TextureOperation.BumpEnvironmentMap;
				case ImageOperations.BumpEnvironmentMapLuminance:
					return D3D.TextureOperation.BumpEnvironmentMapLuminance;
				case ImageOperations.Lerp:
					return D3D.TextureOperation.Lerp;
				case ImageOperations.Modulate:
					return D3D.TextureOperation.Modulate;
				case ImageOperations.ModulateAlphaAddColor:
					return D3D.TextureOperation.ModulateAlphaAddColor;
				case ImageOperations.ModulateColorAddAlpha:
					return D3D.TextureOperation.ModulateColorAddAlpha;
				case ImageOperations.ModulateInverseAlphaAddColor:
					return D3D.TextureOperation.ModulateInvAlphaAddColor;
				case ImageOperations.ModulateInverseColorAddAlpha:
					return D3D.TextureOperation.ModulateInvColorAddAlpha;
				case ImageOperations.Modulatex2:
					return D3D.TextureOperation.Modulate2X;
				case ImageOperations.Modulatex4:
					return D3D.TextureOperation.Modulate4X;
				case ImageOperations.MultiplyAdd:
					return D3D.TextureOperation.MultiplyAdd;
				case ImageOperations.PreModulate:
					return D3D.TextureOperation.PreModulate;
				case ImageOperations.SelectArgument1:
					return D3D.TextureOperation.SelectArg1;
				case ImageOperations.SelectArgument2:
					return D3D.TextureOperation.SelectArg2;
				case ImageOperations.Subtract:
					return D3D.TextureOperation.Subtract;
			}

			return D3D.TextureOperation.Disable;
		}

		/// <summary>
		/// Function to convert a Gorgon image operation argument to a D3D texture argument.
		/// </summary>
		/// <param name="arg">Gorgon operation argument to convert.</param>
		/// <returns>D3D texture argument.</returns>
		public static D3D.TextureArgument Convert(ImageOperationArguments arg)
		{
			switch (arg)
			{
				case ImageOperationArguments.AlphaReplicate:
					return D3D.TextureArgument.AlphaReplicate;
				case ImageOperationArguments.Complement:
					return D3D.TextureArgument.Complement;
				case ImageOperationArguments.Constant:
					return D3D.TextureArgument.Constant;
				case ImageOperationArguments.Diffuse:
					return D3D.TextureArgument.Diffuse;
				case ImageOperationArguments.Specular:
					return D3D.TextureArgument.Specular;
				case ImageOperationArguments.Temp:
					return D3D.TextureArgument.Temp;
				case ImageOperationArguments.Texture:
					return D3D.TextureArgument.TextureColor;
				case ImageOperationArguments.TextureFactor:
					return D3D.TextureArgument.TFactor;
			}

			return D3D.TextureArgument.Current;
		}

		/// <summary>
		/// Function to convert a Gorgon image filter into a D3D texture filter.
		/// </summary>
		/// <param name="filter">Gorgon image filter.</param>
		/// <returns>D3D texture filter.</returns>
		public static D3D.TextureFilter Convert(ImageFilters filter)
		{
			switch (filter)
			{
				case ImageFilters.Anisotropic:
					return D3D.TextureFilter.Anisotropic;
				case ImageFilters.Bilinear:
					return D3D.TextureFilter.Linear;
				case ImageFilters.Point:
					return D3D.TextureFilter.Point;
				case ImageFilters.GaussianQuadratic:
					return D3D.TextureFilter.GaussianQuad;
				case ImageFilters.PyramidalQuadratic:
					return D3D.TextureFilter.PyramidalQuad;
			}
			return D3D.TextureFilter.None;
		}

		/// <summary>
		/// Function to convert a Gorgon stencil operation into a D3D stencil operation.
		/// </summary>
		/// <param name="operation">Gorgon stencil operation.</param>
		/// <returns>A D3D stencil operation.</returns>
		public static D3D.StencilOperation Convert(StencilOperations operation)
		{
			switch (operation)
			{
				case StencilOperations.Decrement:
					return D3D.StencilOperation.Decrement;
				case StencilOperations.DecrementSaturate:
					return D3D.StencilOperation.DecrementSaturation;
				case StencilOperations.Increment:
					return D3D.StencilOperation.Increment;
				case StencilOperations.IncrementSaturate:
					return D3D.StencilOperation.IncrementSaturation;
				case StencilOperations.Invert:
					return D3D.StencilOperation.Invert;
				case StencilOperations.Replace:
					return D3D.StencilOperation.Replace;
				case StencilOperations.Zero:
					return D3D.StencilOperation.Zero;
			}

			return D3D.StencilOperation.Keep;
		}
		#endregion
	}
}
