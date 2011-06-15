#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Tuesday, May 22, 2007 3:59:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;
using Drawing = System.Drawing;
using GorgonLibrary.Graphics.Tools.PropBag;
using Flobbster.Windows.Forms;
using GorgonLibrary.Internal;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Object representing the interface to the sprite editor.
	/// </summary>
	public class SpriteDocument
		: NamedObject, ICloneable, IPropertyBagObject
	{
		#region Variables.
		private formMain _owner = null;								// Owning form for this object.
		private Sprite _sprite = null;								// Sprite currently being edited.
		private bool _changed;										// Flag to indicate that the sprite has been changed.
		private bool _xml;											// Flag to indicate that the sprite is an xml file.		
		private PropertyBag _bag = null;							// Property bag.
		private bool _includeInAnimations = true;					// Flag to indicate that we should include this sprite in animations.
		#endregion

		#region Events.
		/// <summary>
		/// Event called when the sprite changes.
		/// </summary>
		public event EventHandler SpriteChanged = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the sprite has changed.
		/// </summary>
		public bool Changed
		{
			get
			{
				return _changed;
			}
			set
			{
				_changed = value;
				if (SpriteChanged != null)
					SpriteChanged(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Property to return the sprite currently being edited.
		/// </summary>
		public Sprite Sprite
		{
			get
			{
				return _sprite;
			}
		}

		/// <summary>
		/// Property to set or return the name of the document.
		/// </summary>
		[PropertyInclude(), PropertyCategory("File"), PropertyDescription("The file name and path of the sprite."), PropertyDefault("")]
		public string FilePath
		{
			get
			{
				return _sprite.Filename;
			}
		}

		/// <summary>
		/// Property to set or return the name of the document.
		/// </summary>
		[PropertyInclude(true), PropertyCategory("Design"), PropertyDescription("The name of the sprite.")]
		public new string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
                _sprite.Name = value;
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the size of the sprite.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Design"), PropertyDescription("Sets whether to flip the sprite horizontally or not."), PropertyDefault(false)]
		public bool HorizontalFlip
		{
			get
			{
				return _sprite.HorizontalFlip;
			}
			set
			{
				_sprite.HorizontalFlip = value;
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the size of the sprite.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Design"), PropertyDescription("Sets whether to flip the sprite vertically or not."), PropertyDefault(false)]
		public bool VerticalFlip
		{
			get
			{
				return _sprite.VerticalFlip;
			}
			set
			{
				_sprite.VerticalFlip = value;
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the size of the sprite.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Design"), PropertyDescription("The width and height of the sprite in relation to its bound image.")]
		public Drawing.SizeF Size
		{
			get
			{
				return _sprite.Size;
			}
			set
			{
				_sprite.Size = value;
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the size of the sprite.
		/// </summary>
		[PropertyInclude(false, "", typeof(GorgonPointFConverter)), PropertyCategory("Design"), PropertyDescription("Sets axis (or origin, hotspot, etc...) of the sprite.")]
		public Drawing.PointF Axis
		{
			get
			{
				return _sprite.Axis;
			}
			set
			{
				_sprite.Axis = value;
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the axis alignment.
		/// </summary>
		[PropertyInclude(false, "", typeof(Drawing.ContentAlignment)), PropertyCategory("Design"), PropertyDescription("Sets axis (or origin, hotspot, etc...) of the sprite.")]
		public Drawing.ContentAlignment AxisAlignment
		{
			get
			{
				Vector2D axisAlign = Vector2D.Zero;		// Axis alignment.

				axisAlign.X = MathUtility.Round(_sprite.Width / 2.0f);
				axisAlign.Y = MathUtility.Round(_sprite.Height / 2.0f);
				
				if ((MathUtility.EqualFloat(_sprite.Axis.X, 0.0f, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, 0.0f, 0.5f)))
					return Drawing.ContentAlignment.TopLeft;
				if ((MathUtility.EqualFloat(_sprite.Axis.X, axisAlign.X, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, 0.0f, 0.5f)))
					return Drawing.ContentAlignment.TopCenter;
				if ((MathUtility.EqualFloat(_sprite.Axis.X, _sprite.Width - 1.0f, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, 0.0f, 0.5f)))
					return Drawing.ContentAlignment.TopRight;
				if ((MathUtility.EqualFloat(_sprite.Axis.X, 0.0f, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, axisAlign.Y, 0.5f)))
					return Drawing.ContentAlignment.MiddleLeft;
				if ((MathUtility.EqualFloat(_sprite.Axis.X, axisAlign.X, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, axisAlign.Y, 0.5f)))
					return Drawing.ContentAlignment.MiddleCenter;
				if ((MathUtility.EqualFloat(_sprite.Axis.X, _sprite.Width - 1.0f, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, axisAlign.Y, 0.5f)))
					return Drawing.ContentAlignment.MiddleRight;
				if ((MathUtility.EqualFloat(_sprite.Axis.X, 0.0f, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, _sprite.Height - 1.0f, 0.5f)))
					return Drawing.ContentAlignment.BottomLeft;
				if ((MathUtility.EqualFloat(_sprite.Axis.X, axisAlign.X, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, _sprite.Height - 1.0f, 0.5f)))
					return Drawing.ContentAlignment.BottomCenter;
				if ((MathUtility.EqualFloat(_sprite.Axis.X, _sprite.Width, 0.5f)) && (MathUtility.EqualFloat(_sprite.Axis.Y, _sprite.Height - 1.0f, 0.5f)))
					return Drawing.ContentAlignment.BottomRight;

				return 0;
			}
			set
			{
				Vector2D axisAlign = Vector2D.Zero;		// Axis alignment.

				switch (value)
				{
					case Drawing.ContentAlignment.TopCenter:
						axisAlign.X = _sprite.Width / 2.0f;
						break;
					case Drawing.ContentAlignment.TopRight:
						axisAlign.X = _sprite.Width - 1.0f;
						break;
					case Drawing.ContentAlignment.MiddleCenter:
						axisAlign.X = _sprite.Width / 2.0f;
						axisAlign.Y = _sprite.Height / 2.0f;
						break;
					case Drawing.ContentAlignment.MiddleLeft:
						axisAlign.Y = _sprite.Height / 2.0f;
						break;
					case Drawing.ContentAlignment.MiddleRight:
						axisAlign.X = _sprite.Width - 1.0f;
						axisAlign.Y = _sprite.Height / 2.0f;
						break;
					case Drawing.ContentAlignment.BottomLeft:
						axisAlign.Y = _sprite.Height - 1.0f;
						break;
					case Drawing.ContentAlignment.BottomCenter:
						axisAlign.X = _sprite.Width / 2.0f;
						axisAlign.Y = _sprite.Height - 1.0f;
						break;
					case Drawing.ContentAlignment.BottomRight:
						axisAlign.X = _sprite.Width - 1.0f;
						axisAlign.Y = _sprite.Height - 1.0f;
						break;
				}

				_sprite.SetAxis(MathUtility.Round(axisAlign.X), MathUtility.Round(axisAlign.Y));
				Changed = true;

				_owner.SpriteManager.RefreshPropertyGrid();
			}
		}

		/// <summary>
		/// Property to set or return the size of the sprite.
		/// </summary>
		[PropertyInclude(false, "", typeof(GorgonPointFConverter)), PropertyCategory("Design"), PropertyDescription("Sets the source point in the bound image to grab the sprite from.")]
		public Drawing.PointF ImageLocation
		{
			get
			{
				return _sprite.ImageOffset;
			}
			set
			{
				_sprite.ImageOffset = value;
				Changed = true;
			}
		}

		/// <summary>
		/// Property to return whether the sprite was saved as an XML document or not.
		/// </summary>
		[PropertyInclude(), PropertyDefault(false), PropertyCategory("File"), PropertyDescription("Displays whether the sprite is XML or not.")]
		public bool IsXML
		{
			get
			{
				return _xml;
			}
		}

		/// <summary>
		/// Property to set or return the bound image.
		/// </summary>
		[PropertyInclude(false, typeof(GorgonImageTypeEditor), typeof(GorgonImageTypeConverter)), 
			PropertyDefault(""), PropertyCategory("Design"), PropertyDescription("Set the name of the image bound to the sprite.")]
		public Image BoundImage
		{
			get
			{
				return _sprite.Image;
			}
			set
			{			
				Bind(value);				
			}
		}

		/// <summary>
		/// Property to set or return the sprite color.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Appearance"), PropertyDescription("Sets the diffuse color of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.Color DiffuseColor
		{
			get
			{
				return _sprite.Color;
			}
			set
			{
				_sprite.Color = value;
				_sprite.Opacity = value.A;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the border color when the wrapping mode is set to Border.
		/// </summary>
		[PropertyInclude(false), PropertyCategory("Appearance"), PropertyDescription("Sets the color of the border when the wrapping mode is set to Border."), PropertyDefault("0x0")]
		public Drawing.Color BorderColor
		{
			get
			{
				return _sprite.BorderColor;
			}
			set
			{
				_sprite.BorderColor = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the upper left sprite vertex offset.
		/// </summary>
		[PropertyInclude(false, "", typeof(GorgonPointFConverter)), PropertyCategory("Vertex"), PropertyDescription("Sets the offset of the upper left vertex of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.PointF UpperLeftOffset
		{
			get
			{
				return _sprite.GetSpriteVertexOffset(VertexLocations.UpperLeft);
			}
			set
			{
				_sprite.SetSpriteVertexOffset(VertexLocations.UpperLeft, value);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the upper right sprite vertex offset.
		/// </summary>
		[PropertyInclude(false, "", typeof(GorgonPointFConverter)), PropertyCategory("Vertex"), PropertyDescription("Sets the offset of the upper right vertex of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.PointF UpperRightOffset
		{
			get
			{
				return _sprite.GetSpriteVertexOffset(VertexLocations.UpperRight);
			}
			set
			{
				_sprite.SetSpriteVertexOffset(VertexLocations.UpperRight, value);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the lower left sprite vertex offset.
		/// </summary>
		[PropertyInclude(false, "", typeof(GorgonPointFConverter)), PropertyCategory("Vertex"), PropertyDescription("Sets the offset of the lower left vertex of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.PointF LowerLeftOffset
		{
			get
			{
				return _sprite.GetSpriteVertexOffset(VertexLocations.LowerLeft);
			}
			set
			{
				_sprite.SetSpriteVertexOffset(VertexLocations.LowerLeft, value);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the lower right sprite vertex offset.
		/// </summary>
		[PropertyInclude(false, "", typeof(GorgonPointFConverter)), PropertyCategory("Vertex"), PropertyDescription("Sets the offset of the lower right vertex of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.PointF LowerRightOffset
		{
			get
			{
				return _sprite.GetSpriteVertexOffset(VertexLocations.LowerRight);
			}
			set
			{
				_sprite.SetSpriteVertexOffset(VertexLocations.LowerRight, value);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the upper left sprite vertex color.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Vertex"), PropertyDescription("Sets the diffuse color of the upper left vertex of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.Color UpperLeftDiffuse
		{
			get
			{
				return _sprite.GetSpriteVertexColor(VertexLocations.UpperLeft);
			}
			set
			{
				_sprite.SetSpriteVertexColor(VertexLocations.UpperLeft, value);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the lower left sprite vertex color.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Vertex"), PropertyDescription("Sets the diffuse color of the lower left vertex of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.Color LowerLeftDiffuse
		{
			get
			{
				return _sprite.GetSpriteVertexColor(VertexLocations.LowerLeft);
			}
			set
			{
				_sprite.SetSpriteVertexColor(VertexLocations.LowerLeft, value);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the upper right sprite vertex color.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Vertex"), PropertyDescription("Sets the diffuse color of the upper right vertex of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.Color UpperRightDiffuse
		{
			get
			{
				return _sprite.GetSpriteVertexColor(VertexLocations.UpperRight);
			}
			set
			{
				_sprite.SetSpriteVertexColor(VertexLocations.UpperRight, value);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the lower right sprite vertex color.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Vertex"), PropertyDescription("Sets the diffuse color of the lower right vertex of the sprite."), PropertyDefault("0xFFFFFFFF")]
		public Drawing.Color LowerRightDiffuse
		{
			get
			{
				return _sprite.GetSpriteVertexColor(VertexLocations.LowerRight);
			}
			set
			{
				_sprite.SetSpriteVertexColor(VertexLocations.LowerRight, value);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the upper right sprite alpha.
		/// </summary>
		[PropertyInclude(false, "", typeof(System.Windows.Forms.OpacityConverter)), PropertyCategory("Vertex"), PropertyDescription("Sets the alpha of the upper right vertex of the sprite."), PropertyDefault((double)1.0)]
		public double UpperRightAlpha
		{
			get
			{
				return ((double)_sprite.GetSpriteVertexColor(VertexLocations.UpperRight).A) / 255.0;
			}
			set
			{
				if (value > 1.0)
					value = 1.0;
				if (value < 0.0)
					value = 0.0;
				_sprite.SetSpriteVertexColor(VertexLocations.UpperRight, Drawing.Color.FromArgb((byte)Math.Round(255.0 * value, 0, MidpointRounding.ToEven), _sprite.GetSpriteVertexColor(VertexLocations.UpperRight)));
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the upper left sprite alpha.
		/// </summary>
		[PropertyInclude(false, "", typeof(System.Windows.Forms.OpacityConverter)), PropertyCategory("Vertex"), PropertyDescription("Sets the alpha of the upper left vertex of the sprite."), PropertyDefault((double)1.0)]
		public double UpperLeftAlpha
		{
			get
			{
				return ((double)_sprite.GetSpriteVertexColor(VertexLocations.UpperLeft).A) / 255.0;
			}
			set
			{
				if (value > 1.0)
					value = 1.0;
				if (value < 0.0)
					value = 0.0;
				_sprite.SetSpriteVertexColor(VertexLocations.UpperLeft, Drawing.Color.FromArgb((byte)Math.Round(255.0 * value, 0, MidpointRounding.ToEven), _sprite.GetSpriteVertexColor(VertexLocations.UpperLeft)));
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the lower right sprite alpha.
		/// </summary>
		[PropertyInclude(false, "", typeof(System.Windows.Forms.OpacityConverter)), PropertyCategory("Vertex"), PropertyDescription("Sets the alpha of the lower right vertex of the sprite."), PropertyDefault((double)1.0)]
		public double LowerRightAlpha
		{
			get
			{
				return ((double)_sprite.GetSpriteVertexColor(VertexLocations.LowerRight).A) / 255.0;
			}
			set
			{
				if (value > 1.0)
					value = 1.0;
				if (value < 0.0)
					value = 0.0;
				_sprite.SetSpriteVertexColor(VertexLocations.LowerRight, Drawing.Color.FromArgb((byte)Math.Round(255.0 * value, 0, MidpointRounding.ToEven), _sprite.GetSpriteVertexColor(VertexLocations.LowerRight)));
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the lower left sprite alpha.
		/// </summary>
		[PropertyInclude(false, "", typeof(System.Windows.Forms.OpacityConverter)), PropertyCategory("Vertex"), PropertyDescription("Sets the alpha of the lower left vertex of the sprite."), PropertyDefault((double)1.0)]
		public double LowerLeftAlpha
		{
			get
			{
				return ((double)_sprite.GetSpriteVertexColor(VertexLocations.LowerLeft).A) / 255.0;
			}
			set
			{
				if (value > 1.0)
					value = 1.0;
				if (value < 0.0)
					value = 0.0;
				_sprite.SetSpriteVertexColor(VertexLocations.LowerLeft, Drawing.Color.FromArgb((byte)Math.Round(255.0 * value, 0, MidpointRounding.ToEven), _sprite.GetSpriteVertexColor(VertexLocations.LowerLeft)));
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite opacity.
		/// </summary>
		[PropertyInclude(false, "", typeof(System.Windows.Forms.OpacityConverter)), PropertyCategory("Appearance"), PropertyDescription("Sets the opacity of the sprite."), PropertyDefault((double)1.0)]
		public double Opacity
		{
			get
			{
				return ((double)_sprite.Opacity) / 255.0;
			}
			set
			{
				if (value > 1.0)
					value = 1.0;
				if (value < 0.0)
					value = 0.0;

				_sprite.Opacity = (byte)Math.Round(255.0 * value, 0, MidpointRounding.ToEven);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite alpha masking function is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Alpha Masking"), PropertyDescription("Sets whether to inherit the alpha mask function from the global states."), PropertyDefault(true)]
		public bool InheritAlphaMaskFunction
		{
			get
			{
				return _sprite.InheritAlphaMaskFunction;
			}
			set
			{
				_sprite.InheritAlphaMaskFunction = value;

				SetSpecReadOnly(_bag.Properties["AlphaMaskFunction"], value, true);
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite alpha masking value is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Alpha Masking"), PropertyDescription("Sets whether to inherit the alpha mask value from the global states."), PropertyDefault(true)]
		public bool InheritAlphaMaskValue
		{
			get
			{
				return _sprite.InheritAlphaMaskValue;
			}
			set
			{
				_sprite.InheritAlphaMaskValue = value;

				SetSpecReadOnly(_bag.Properties["AlphaMaskValue"], value, true);
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite alpha masking function.
		/// </summary>
		[PropertyInclude(false, typeof(CompareFunctions)), PropertyCategory("Alpha Masking"), PropertyDescription("Sets alpha mask function for the sprite.")]
		public CompareFunctions AlphaMaskFunction
		{
			get
			{
				return _sprite.AlphaMaskFunction;
			}
			set
			{
				_sprite.AlphaMaskFunction = value;

				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite alpha masking value.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Alpha Masking"), PropertyDescription("Sets alpha mask value for the sprite."), PropertyDefault(0)]
		public int AlphaMaskValue
		{
			get
			{
				return _sprite.AlphaMaskValue;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > 255)
					value = 255;

				_sprite.AlphaMaskValue = value;

				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite blending mode is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Blending"), PropertyDescription("Sets whether to inherit the blending values from the global states."), PropertyDefault(true)]
		public bool InheritBlending
		{
			get
			{
				return _sprite.InheritBlending;
			}
			set
			{
				_sprite.InheritBlending = value;

				SetSpecReadOnly(_bag.Properties["SourceBlend"], value, false);
				SetSpecReadOnly(_bag.Properties["DestinationBlend"], value, false);
				SetSpecReadOnly(_bag.Properties["BlendingPreset"], value, false);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite source blend function.
		/// </summary>
		[PropertyInclude(false, typeof(AlphaBlendOperation)), PropertyCategory("Blending"), PropertyDescription("Sets source blending function for the sprite.")]
		public AlphaBlendOperation SourceBlend
		{
			get
			{
				return _sprite.SourceBlend;
			}
			set
			{
				_sprite.SourceBlend = value;

				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite destination blend function.
		/// </summary>
		[PropertyInclude(false, typeof(AlphaBlendOperation)), PropertyCategory("Blending"), PropertyDescription("Sets destination blending function for the sprite.")]
		public AlphaBlendOperation DestinationBlend
		{
			get
			{
				return _sprite.DestinationBlend;
			}
			set
			{
				_sprite.DestinationBlend = value;

				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite alpha masking function.
		/// </summary>
		[PropertyInclude(false, typeof(BlendingModes)), PropertyCategory("Blending"), PropertyDescription("Sets a preset blend mode for the sprite.")]
		public BlendingModes BlendingPreset
		{
			get
			{
				return _sprite.BlendingMode;
			}
			set
			{
				_sprite.BlendingMode = value;
				
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite smoothing mode is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Smoothing"), PropertyDescription("Sets whether to inherit the smoothing value from the global states."), PropertyDefault(true)]
		public bool InheritSmoothing
		{
			get
			{
				return _sprite.InheritSmoothing;
			}
			set
			{
				_sprite.InheritSmoothing = value;

				SetSpecReadOnly(_bag.Properties["Smoothing"], value, true);
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite smoothing function.
		/// </summary>
		[PropertyInclude(false, typeof(Smoothing)), PropertyCategory("Smoothing"), PropertyDescription("Sets smoothing function for the sprite.")]
		public Smoothing Smoothing
		{
			get
			{
				return _sprite.Smoothing;
			}
			set
			{
				_sprite.Smoothing = value;

				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite horizontal wrapping mode is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Wrapping"), PropertyDescription("Sets whether to inherit the horizontal wrapping mode from the global states."), PropertyDefault(true)]
		public bool InheritHorizontalWrapping
		{
			get
			{
				return _sprite.InheritHorizontalWrapping ;
			}
			set
			{
				_sprite.InheritHorizontalWrapping = value;

				SetSpecReadOnly(_bag.Properties["HorizontalWrapping"], value, true); 
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite vertical wrapping mode is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Wrapping"), PropertyDescription("Sets whether to inherit the vertical wrapping mode from the global states."), PropertyDefault(true)]
		public bool InheritVerticalWrapping
		{
			get
			{
				return _sprite.InheritVerticalWrapping;
			}
			set
			{
				_sprite.InheritVerticalWrapping = value;

				SetSpecReadOnly(_bag.Properties["VerticalWrapping"], value, true);
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite horizontal wrapping mode.
		/// </summary>
		[PropertyInclude(false, typeof(ImageAddressing)), PropertyCategory("Wrapping"), PropertyDescription("Sets the horizontal wrapping mode for the sprite.")]
		public ImageAddressing HorizontalWrapping
		{
			get
			{
				return _sprite.HorizontalWrapMode;
			}
			set
			{
				_sprite.HorizontalWrapMode = value;
				SetSpecReadOnly(_bag.Properties["BorderColor"], !(value == ImageAddressing.Border || VerticalWrapping == ImageAddressing.Border), true);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite vertical wrapping mode.
		/// </summary>
		[PropertyInclude(false, typeof(ImageAddressing)), PropertyCategory("Wrapping"), PropertyDescription("Sets the vertical wrapping mode for the sprite.")]
		public ImageAddressing VerticalWrapping
		{
			get
			{
				return _sprite.VerticalWrapMode;
			}
			set
			{
				_sprite.VerticalWrapMode = value;
				SetSpecReadOnly(_bag.Properties["BorderColor"], !(value == ImageAddressing.Border || HorizontalWrapping == ImageAddressing.Border), true);
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether to inherit the depth testing function.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Depth"), PropertyDescription("Sets whether to inherit the depth test function from the global states."), PropertyDefault(true)]
		public bool InheritDepthTestFunction
		{
			get
			{
				return _sprite.InheritDepthTestFunction;
			}
			set
			{
				_sprite.InheritDepthTestFunction = value;
				SetSpecReadOnly(_bag.Properties["DepthTestFunction"], value, true);
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether to inherit the depth bias.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Depth"), PropertyDescription("Sets whether to inherit the depth bias from the global states."), PropertyDefault(true)]
		public bool InheritDepthBias
		{
			get
			{
				return _sprite.InheritDepthBias;
			}
			set
			{
				_sprite.InheritDepthBias = value;
				SetSpecReadOnly(_bag.Properties["DepthBufferBias"], value, true);
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether to inherit the depth write enabled flag.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Depth"), PropertyDescription("Sets whether to inherit the depth write enabled flag from the global states."), PropertyDefault(true)]
		public bool InheritDepthWriteEnabled
		{
			get
			{
				return _sprite.InheritDepthWriteEnabled;
			}
			set
			{
				_sprite.InheritDepthWriteEnabled = value;
				SetSpecReadOnly(_bag.Properties["DepthWriteEnabled"], value, true);
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the depth testing function.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Depth"), PropertyDescription("Sets the depth test function for the sprite."), PropertyDefault(true)]
		public CompareFunctions DepthTestFunction
		{
			get
			{
				return _sprite.DepthTestFunction;
			}
			set
			{
				_sprite.DepthTestFunction = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the depth buffer bias.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Depth"), PropertyDescription("Sets the depth bias for the sprite."), PropertyDefault(true)]
		public float DepthBufferBias
		{
			get
			{
				return _sprite.DepthBufferBias;
			}
			set
			{
				_sprite.DepthBufferBias = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the depth write flag.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Depth"), PropertyDescription("Sets the depth write enabled flag for the sprite."), PropertyDefault(true)]
		public bool DepthWriteEnabled
		{
			get
			{
				return _sprite.DepthWriteEnabled;
			}
			set
			{
				_sprite.DepthWriteEnabled = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite stencil enable flag is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Stencil"), PropertyDescription("Sets whether to inherit the stencil enabled flag from the global states."), PropertyDefault(true)]
		public bool InheritStencilEnabled
		{
			get
			{
				return _sprite.InheritStencilEnabled;
			}
			set
			{
				_sprite.InheritStencilEnabled = value;

				SetSpecReadOnly(_bag.Properties["StencilEnabled"], value, true);

				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite stencil fail operation is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Stencil"), PropertyDescription("Sets whether to inherit the stencil fail operation from the global states."), PropertyDefault(true)]
		public bool InheritStencilFailOperation
		{
			get
			{
				return _sprite.InheritStencilFailOperation;
			}
			set
			{
				_sprite.InheritStencilFailOperation = value;
								
				SetSpecReadOnly(_bag.Properties["StencilFailOperation"], value, true);

				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite stencil mask value is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Stencil"), PropertyDescription("Sets whether to inherit the stencil masking value from the global states."), PropertyDefault(true)]
		public bool InheritStencilMask
		{
			get
			{
				return _sprite.InheritStencilMask;
			}
			set
			{
				_sprite.InheritStencilMask = value;

				SetSpecReadOnly(_bag.Properties["StencilMask"], value, true);
				
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite stencil pass operation is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Stencil"), PropertyDescription("Sets whether to inherit the stencil pass operation from the global states."), PropertyDefault(true)]
		public bool InheritStencilPassOperation
		{
			get
			{
				return _sprite.InheritStencilPassOperation;
			}
			set
			{
				_sprite.InheritStencilPassOperation = value;

				SetSpecReadOnly(_bag.Properties["StencilPassOperation"], value, true);

				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite stencil reference value is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Stencil"), PropertyDescription("Sets whether to inherit the stencil reference value from the global states."), PropertyDefault(true)]
		public bool InheritStencilRefValue
		{
			get
			{
				return _sprite.InheritStencilReference;
			}
			set
			{
				_sprite.InheritStencilReference = value;

				SetSpecReadOnly(_bag.Properties["StencilRefValue"], value, true);

				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite stencil Z-fail operation is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Stencil"), PropertyDescription("Sets whether to inherit the stencil z-fail operation from the global states."), PropertyDefault(true)]
		public bool InheritStencilZFailOperation
		{
			get
			{
				return _sprite.InheritStencilZFailOperation;
			}
			set
			{
				_sprite.InheritStencilZFailOperation = value;

				SetSpecReadOnly(_bag.Properties["StencilZFailOperation"], value, true);

				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite stencil compare operation is inherited or not.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Stencil"), PropertyDescription("Sets whether to inherit the stencil compare operation from the global states."), PropertyDefault(true)]
		public bool InheritStencilCompareOperation
		{
			get
			{
				return _sprite.InheritStencilCompare;
			}
			set
			{
				_sprite.InheritStencilCompare = value;

				SetSpecReadOnly(_bag.Properties["StencilCompareOperation"], value, true);

				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite stencil enabled flag.
		/// </summary>
		[PropertyInclude(), PropertyCategory("Stencil"), PropertyDescription("Sets whether to enable stencil support on this sprite."), PropertyDefault(false)]
		public bool StencilEnabled
		{
			get
			{
				return _sprite.StencilEnabled;
			}
			set
			{
				_sprite.StencilEnabled = value;

				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite stencil fail operation.
		/// </summary>
		[PropertyInclude(false, typeof(StencilOperations)), PropertyCategory("Stencil"), PropertyDescription("Sets the stencil fail operation.")]
		public StencilOperations StencilFailOperation
		{
			get
			{
				return _sprite.StencilFailOperation;
			}
			set
			{
				_sprite.StencilFailOperation = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite stencil mask value.
		/// </summary>
		[PropertyInclude(false, "", typeof(System.ComponentModel.Int32Converter)), PropertyCategory("Stencil"), PropertyDescription("Sets the stencil masking value."), PropertyDefault(0)]
		public int StencilMask
		{
			get
			{
				return _sprite.StencilMask;
			}
			set
			{
				_sprite.StencilMask = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite stencil pass operation.
		/// </summary>
		[PropertyInclude(false, typeof(StencilOperations)), PropertyCategory("Stencil"), PropertyDescription("Sets stencil pass operation.")]
		public StencilOperations StencilPassOperation
		{
			get
			{
				return _sprite.StencilPassOperation;
			}
			set
			{
				_sprite.StencilPassOperation = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite stencil reference value.
		/// </summary>
		[PropertyInclude(false, "", typeof(System.ComponentModel.Int32Converter)), PropertyCategory("Stencil"), PropertyDescription("Sets the stencil reference value.")]
		public int StencilRefValue
		{
			get
			{
				return _sprite.StencilReference;
			}
			set
			{
				_sprite.StencilReference = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite stencil Z-fail operation.
		/// </summary>
		[PropertyInclude(false, typeof(StencilOperations)), PropertyCategory("Stencil"), PropertyDescription("Sets the stencil z-fail operation.")]
		public StencilOperations StencilZFailOperation
		{
			get
			{
				return _sprite.StencilZFailOperation;
			}
			set
			{
				_sprite.StencilZFailOperation = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return the sprite stencil compare operation.
		/// </summary>
		[PropertyInclude(false, typeof(CompareFunctions)), PropertyCategory("Stencil"), PropertyDescription("Sets the stencil compare operation."), PropertyDefault(true)]
		public CompareFunctions StencilCompareOperation
		{
			get
			{
				return _sprite.StencilCompare;
			}
			set
			{
				_sprite.StencilCompare = value;
				_owner.SpriteManager.RefreshPropertyGrid();
				Changed = true;
			}
		}

		/// <summary>
		/// Property to set or return whether to include this sprite in other animations.
		/// </summary>
		[PropertyInclude(false, typeof(bool)), PropertyCategory("Project Settings"), PropertyDescription("Sets whether we can use this sprite in other animations for other sprites."), PropertyDefault(true)]
		public bool IncludeInAnimations
		{
			get
			{
				return _includeInAnimations;
			}
			set
			{
				_includeInAnimations = value;
				_owner.ProjectChanged = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update a property.
		/// </summary>
		/// <param name="spec">Property specification to update.</param>
		private void UpdatePropertySpec(PropertySpec spec)
		{
			if (_sprite == null)
				return;
			
			switch (spec.Name.ToLower())
			{
				case "upperleftdiffuse":
				case "upperrightdiffuse":
				case "lowerleftdiffuse":
				case "lowerrightdiffuse":
				case "diffusecolor":
					spec.DefaultValue = Drawing.Color.FromArgb(255, 255, 255, 255);
					break;
				case "axis":
				case "upperleftoffset":
				case "upperrightoffset":
				case "lowerleftoffset":
				case "lowerrightoffset":
					spec.DefaultValue = Drawing.PointF.Empty;
					break;				
			}
		}

		/// <summary>
		/// Function to set a specific property spec read-only.
		/// </summary>
		/// <param name="spec">Property specification.</param>
		/// <param name="readOnly">TRUE if read-only, FALSE if not.</param>
		/// <param name="refreshNow">TRUE to refresh immediately, FALSE to defer.</param>
		public void SetSpecReadOnly(PropertySpec spec, bool readOnly, bool refreshNow)
		{
			if (!readOnly)
				spec.Attributes = null;
			else
				spec.Attributes = new Attribute[] { System.ComponentModel.ReadOnlyAttribute.Yes };

			if (refreshNow)
				_owner.SpriteManager.RefreshPropertyGrid();
		}

		/// <summary>
		/// Function to set animated properties to read-only if keys exist for said properties.
		/// </summary>
		/// <param name="doNotRefresh">TRUE to defer the refreshing of the property grid, FALSE to refresh immediately.</param>
		public void SetAnimReadOnly(bool doNotRefresh)
		{
			if (_sprite == null)
				return;

			// Set color properties.
			if ((HasKeysForTrack("Color")) || (HasKeysForTrack("Opacity")))
			{
				SetSpecReadOnly(_bag.Properties["opacity"], true, false);
				SetSpecReadOnly(_bag.Properties["diffusecolor"], true, false);
				SetSpecReadOnly(_bag.Properties["upperleftdiffuse"], true, false);
				SetSpecReadOnly(_bag.Properties["upperrightdiffuse"], true, false);
				SetSpecReadOnly(_bag.Properties["lowerleftdiffuse"], true, false);
				SetSpecReadOnly(_bag.Properties["lowerrightdiffuse"], true, false);
				SetSpecReadOnly(_bag.Properties["upperleftalpha"], true, false);
				SetSpecReadOnly(_bag.Properties["upperrightalpha"], true, false);
				SetSpecReadOnly(_bag.Properties["lowerleftalpha"], true, false);
				SetSpecReadOnly(_bag.Properties["lowerrightalpha"], true, false);
			}
			else
			{
				SetSpecReadOnly(_bag.Properties["opacity"], false, false);
				SetSpecReadOnly(_bag.Properties["diffusecolor"], false, false);
				SetSpecReadOnly(_bag.Properties["upperleftdiffuse"], false, false);
				SetSpecReadOnly(_bag.Properties["upperrightdiffuse"], false, false);
				SetSpecReadOnly(_bag.Properties["lowerleftdiffuse"], false, false);
				SetSpecReadOnly(_bag.Properties["lowerrightdiffuse"], false, false);
				SetSpecReadOnly(_bag.Properties["upperleftalpha"], false, false);
				SetSpecReadOnly(_bag.Properties["upperrightalpha"], false, false);
				SetSpecReadOnly(_bag.Properties["lowerleftalpha"], false, false);
				SetSpecReadOnly(_bag.Properties["lowerrightalpha"], false, false);
			}

			if (HasKeysForTrack("AlphaMaskValue"))
			{
				SetSpecReadOnly(_bag.Properties["inheritalphamaskvalue"], true, false);
				SetSpecReadOnly(_bag.Properties["alphamaskvalue"], true, false);
			}
			else
			{
				SetSpecReadOnly(_bag.Properties["inheritalphamaskvalue"], false, false);
				SetSpecReadOnly(_bag.Properties["alphamaskvalue"], false, false);
			}

			// Set transformation properties.
			if (HasKeysForTrack("Axis"))
			{
				SetSpecReadOnly(_bag.Properties["axis"], true, false);
				SetSpecReadOnly(_bag.Properties["axisalignment"], true, false);
			}
			else
			{
				SetSpecReadOnly(_bag.Properties["axis"], false, false);
				SetSpecReadOnly(_bag.Properties["axisalignment"], false, false);
			}

			if (HasKeysForTrack("Image"))
				SetSpecReadOnly(_bag.Properties["boundimage"], true, false);
			else
				SetSpecReadOnly(_bag.Properties["boundimage"], false, false);

			if ((HasKeysForTrack("ImageOffset")) || (HasKeysForTrack("Image")))
				SetSpecReadOnly(_bag.Properties["imagelocation"], true, false);
			else
				SetSpecReadOnly(_bag.Properties["imagelocation"], false, false);

			if ((HasKeysForTrack("Size")) || (HasKeysForTrack("Image")))
				SetSpecReadOnly(_bag.Properties["size"], true, false);
			else
				SetSpecReadOnly(_bag.Properties["size"], false, false);

			if (!doNotRefresh)
				_owner.SpriteManager.RefreshPropertyGrid();
		}

		/// <summary>
		/// Function to determine if a particular animation track has keys set.
		/// </summary>
		/// <param name="trackName">Name of the track to query.</param>
		/// <returns>TRUE if there are keys, FALSE if not.</returns>
		public bool HasKeysForTrack(string trackName)
		{
			return (from animations in _sprite.Animations
					where animations.Tracks.Contains(trackName) && animations.Tracks[trackName].KeyCount > 0
					select animations.Tracks[trackName]).Count() > 0;
		}

		/// <summary>
		/// Function to refresh the property list.
		/// </summary>
		public void RefreshProperties()
		{
			// Reset all property bag default values to the sprite values.
			foreach (PropertySpec spec in _bag.Properties)
			{
				PropertySpecEventArgs e = new PropertySpecEventArgs(spec, null);	// Event args.

				UpdatePropertySpec(spec);
				GetValue(this, e);

				// Don't default the inherited values.
				if (!spec.Name.ToLower().StartsWith("inherit"))
					spec.DefaultValue = e.Value;
				else
					SetValue(this, e);
			}

			SetAnimReadOnly(false);
		}
	
		/// <summary>
		/// Function to save the sprite.
		/// </summary>
		/// <param name="fileName">Filename to use.</param>
		/// <param name="xml">TRUE to save as XML, FALSE to save as binary.</param>
		public void Save(string fileName, bool xml)
		{
			try
			{
				if (string.IsNullOrEmpty(fileName))
					fileName = _sprite.Filename;

				// Save the sprite.
				_sprite.Save(fileName, xml);
				_xml = xml;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(_owner, "Unable to save '" + fileName + "'.", ex);
			}
		}

		/// <summary>
		/// Function to load a sprite.
		/// </summary>
		/// <param name="fileName">Filename to load.</param>
		public void Load(string fileName)
		{
			Stream stream = null;				// File stream.
			byte[] data = null;					// File data.
			string spriteGuts = string.Empty;	// Sprite file data.

			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			try
			{
				// This is a shitty way to do it, but we need to determine if the file is XML or not.
				stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

				// Only read the first 128 bytes.
				data = new byte[stream.Length];

				// Reset the position.
				stream.Read(data, 0, data.Length);
				stream.Position = 0;

				// Attempt to convert to a string.
				spriteGuts = Encoding.UTF8.GetString(data);

				// If we have the standard XML file header, then it's XML.
				_xml = spriteGuts.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");

				// Read the sprite from the stream.
				_sprite = GorgonLibrary.Graphics.Sprite.FromStream(stream, _xml);
				Name = _sprite.Name;

				RefreshProperties();

				_changed = false;
				_owner.SpriteManager.RefreshPropertyGrid();
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
				stream = null;

				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			}
		}

		/// <summary>
		/// Function to create the sprite object.
		/// </summary>
		/// <param name="boundImage">Image to bind.</param>
		public void Create(Image boundImage)
		{
			if (boundImage != null)
			{
				if (boundImage.ImageType == ImageType.RenderTarget)
					_sprite = new Sprite(Name, boundImage.RenderImage, boundImage.RenderImage.Width, boundImage.RenderImage.Height);
				else
					_sprite = new Sprite(Name, boundImage, new Vector2D(boundImage.Width, boundImage.Height));
			}
			else
				_sprite = new Sprite(Name);

			RefreshProperties();
			Changed = true;
		}

		/// <summary>
		/// Function to set the sprite source region.
		/// </summary>
		/// <param name="region">Source dimensions of the sprite.</param>
		public void SetRegion(Drawing.RectangleF region)
		{
			_sprite.ImageRegion = region;			
			Changed = true;			
		}

		/// <summary>
		/// Function to bind the current sprite to an image.
		/// </summary>
		/// <param name="image">Image to bind with, NULL to unbind.</param>
		public void Bind(Image image)
		{
			string previousImageName = string.Empty;			// Previous image name.
			try
			{
				if (_sprite.Image != null)
					previousImageName = _sprite.Image.Name;
				_sprite.Image = image;

				if (_sprite.Animations.Count > 0)
				{
					// Get all the keys that are bound to the same image (i.e. the image with the same name).
					var images = from anim in _sprite.Animations
									from imageTrack in anim.Tracks
									from key in imageTrack
									let imageKey = key as KeyImage
									where (imageKey != null) && (anim.Tracks.Contains("Image")) && (imageTrack.KeyCount > 0) && (((imageKey.Image != null) && (string.Compare(imageKey.Image.Name, previousImageName, true) == 0)) || (imageKey.Image == null))
									select imageKey;

					foreach (var key in images)
						key.Image = image;
				}

				_sprite.Refresh();
				Changed = true;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(_owner, "Unable to bind the requested image to '" + Name + "'.", ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="owner">Owner of this object.</param>
		public SpriteDocument(string name, formMain owner)
			: base(name)
		{
			
			_owner = owner;
			_bag = PropertyEnumerator.CreatePropertyBag(this);

			foreach (PropertySpec spec in _bag.Properties)
				UpdatePropertySpec(spec);

			SetAnimReadOnly(false);
		}
		#endregion

		#region ICloneable Members
		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public object Clone()
		{
			SpriteDocument clone = new SpriteDocument(Name + ".Clone", _owner);	// Clone of the document.

			clone._sprite = _sprite.Clone() as Sprite;
			clone._changed = true;
			clone._xml = false;

			clone.RefreshProperties();

			return clone;
		}
		#endregion

		#region IPropertyBagObject Members
		#region Properties.
		/// <summary>
		/// Property to return the property bag for the object.
		/// </summary>		
		public PropertyBag PropertyBag
		{
			get 
			{
				return _bag;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set a value for the property.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		public void SetValue(object sender, Flobbster.Windows.Forms.PropertySpecEventArgs e)
		{
			try
			{
				// We can't set read-only elements.
				if (!GetType().GetProperty(e.Property.Name).CanWrite)
					return;

				GetType().GetProperty(e.Property.Name).SetValue(this, e.Value, null);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(_owner, ex);
			}
		}

		/// <summary>
		/// Function to set a value for the property.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		public void GetValue(object sender, Flobbster.Windows.Forms.PropertySpecEventArgs e)
		{
			switch (e.Property.Name.ToLower())
			{
				case "boundimage":
					if ((BoundImage != null) && (BoundImage.RenderImage != null))
						e.Value = BoundImage.RenderImage.Image;
					else
						e.Value = BoundImage;
					break;
				default:
					e.Value = GetType().GetProperty(e.Property.Name).GetValue(this, null);
					break;
			}
		}
		#endregion
		#endregion
	}
}
