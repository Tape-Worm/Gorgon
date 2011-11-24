#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, July 25, 2011 8:10:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DXGI;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Various buffer formats supported for textures, rendertargets, swap chains and display modes.
	/// </summary>
	public enum GorgonBufferFormat
	{
		/// <summary>
		/// Unknown format.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// The R32G32B32A32 format.
		/// </summary>
		R32G32B32A32 = 1,
		/// <summary>
		/// The R32G32B32A32_Float format.
		/// </summary>
		R32G32B32A32_Float = 2,
		/// <summary>
		/// The R32G32B32A32_UInt format.
		/// </summary>
		R32G32B32A32_UInt = 3,
		/// <summary>
		/// The R32G32B32A32_Int format.
		/// </summary>
		R32G32B32A32_Int = 4,
		/// <summary>
		/// The R32G32B32 format.
		/// </summary>
		R32G32B32 = 5,
		/// <summary>
		/// The R32G32B32_Float format.
		/// </summary>
		R32G32B32_Float = 6,
		/// <summary>
		/// The R32G32B32_UInt format.
		/// </summary>
		R32G32B32_UInt = 7,
		/// <summary>
		/// The R32G32B32_Int format.
		/// </summary>
		R32G32B32_Int = 8,
		/// <summary>
		/// The R16G16B16A16 format.
		/// </summary>
		R16G16B16A16 = 9,
		/// <summary>
		/// The R16G16B16A16_Float format.
		/// </summary>
		R16G16B16A16_Float = 10,
		/// <summary>
		/// The R16G16B16A16_UIntNormal format.
		/// </summary>
		R16G16B16A16_UIntNormal = 11,
		/// <summary>
		/// The R16G16B16A16_UInt format.
		/// </summary>
		R16G16B16A16_UInt = 12,
		/// <summary>
		/// The R16G16B16A16_IntNormal format.
		/// </summary>
		R16G16B16A16_IntNormal = 13,
		/// <summary>
		/// The R16G16B16A16_Int format.
		/// </summary>
		R16G16B16A16_Int = 14,
		/// <summary>
		/// The R32G32 format.
		/// </summary>
		R32G32 = 15,
		/// <summary>
		/// The R32G32_Float format.
		/// </summary>
		R32G32_Float = 16,
		/// <summary>
		/// The R32G32_UInt format.
		/// </summary>
		R32G32_UInt = 17,
		/// <summary>
		/// The R32G32_Int format.
		/// </summary>
		R32G32_Int = 18,
		/// <summary>
		/// The R32G8X24 format.
		/// </summary>
		R32G8X24 = 19,
		/// <summary>
		/// The D32_Float_S8X24_UInt format.
		/// </summary>
		D32_Float_S8X24_UInt = 20,
		/// <summary>
		/// The R32_Float_X8X24 format.
		/// </summary>
		R32_Float_X8X24 = 21,
		/// <summary>
		/// The X32_G8X24_UInt format.
		/// </summary>
		X32_G8X24_UInt = 22,
		/// <summary>
		/// The R10G10B10A2 format.
		/// </summary>
		R10G10B10A2 = 23,
		/// <summary>
		/// The R10G10B10A2_UIntNormal format.
		/// </summary>
		R10G10B10A2_UIntNormal = 24,
		/// <summary>
		/// The R10G10B10A2_UInt format.
		/// </summary>
		R10G10B10A2_UInt = 25,
		/// <summary>
		/// The R11G11B10_Float format.
		/// </summary>
		R11G11B10_Float = 26,
		/// <summary>
		/// The R8G8B8A8 format.
		/// </summary>
		R8G8B8A8 = 27,
		/// <summary>
		/// The R8G8B8A8_UIntNormal format.
		/// </summary>
		R8G8B8A8_UIntNormal = 28,
		/// <summary>
		/// The R8G8B8A8_UIntNormal_sRGB format.
		/// </summary>
		R8G8B8A8_UIntNormal_sRGB = 29,
		/// <summary>
		/// The R8G8B8A8_UInt format.
		/// </summary>
		R8G8B8A8_UInt = 30,
		/// <summary>
		/// The R8G8B8A8_IntNormal format.
		/// </summary>
		R8G8B8A8_IntNormal = 31,
		/// <summary>
		/// The R8G8B8A8_Int format.
		/// </summary>
		R8G8B8A8_Int = 32,
		/// <summary>
		/// The R16G16 format.
		/// </summary>
		R16G16 = 33,
		/// <summary>
		/// The R16G16_Float format.
		/// </summary>
		R16G16_Float = 34,
		/// <summary>
		/// The R16G16_UIntNormal format.
		/// </summary>
		R16G16_UIntNormal = 35,
		/// <summary>
		/// The R16G16_UInt format.
		/// </summary>
		R16G16_UInt = 36,
		/// <summary>
		/// The R16G16_IntNormal format.
		/// </summary>
		R16G16_IntNormal = 37,
		/// <summary>
		/// The R16G16_Int format.
		/// </summary>
		R16G16_Int = 38,
		/// <summary>
		/// The R32 format.
		/// </summary>
		R32 = 39,
		/// <summary>
		/// The D32_Float format.
		/// </summary>
		D32_Float = 40,
		/// <summary>
		/// The R32_Float format.
		/// </summary>
		R32_Float = 41,
		/// <summary>
		/// The R32_UInt format.
		/// </summary>
		R32_UInt = 42,
		/// <summary>
		/// The R32_Int format.
		/// </summary>
		R32_Int = 43,
		/// <summary>
		/// The R24G8 format.
		/// </summary>
		R24G8 = 44,
		/// <summary>
		/// The D24_UIntNormal_S8_UInt format.
		/// </summary>
		D24_UIntNormal_S8_UInt = 45,
		/// <summary>
		/// The R24_UIntNormal_X8 format.
		/// </summary>
		R24_UIntNormal_X8 = 46,
		/// <summary>
		/// The X24_G8_UInt format.
		/// </summary>
		X24_G8_UInt = 47,
		/// <summary>
		/// The R8G8 format.
		/// </summary>
		R8G8 = 48,
		/// <summary>
		/// The R8G8_UIntNormal format.
		/// </summary>
		R8G8_UIntNormal = 49,
		/// <summary>
		/// The R8G8_UInt format.
		/// </summary>
		R8G8_UInt = 50,
		/// <summary>
		/// The R8G8_IntNormal format.
		/// </summary>
		R8G8_IntNormal = 51,
		/// <summary>
		/// The R8G8_Int format.
		/// </summary>
		R8G8_Int = 52,
		/// <summary>
		/// The R16 format.
		/// </summary>
		R16 = 53,
		/// <summary>
		/// The R16_Float format.
		/// </summary>
		R16_Float = 54,
		/// <summary>
		/// The D16_UIntNormal format.
		/// </summary>
		D16_UIntNormal = 55,
		/// <summary>
		/// The R16_UIntNormal format.
		/// </summary>
		R16_UIntNormal = 56,
		/// <summary>
		/// The R16_UInt format.
		/// </summary>
		R16_UInt = 57,
		/// <summary>
		/// The R16_IntNormal format.
		/// </summary>
		R16_IntNormal = 58,
		/// <summary>
		/// The R16_Int format.
		/// </summary>
		R16_Int = 59,
		/// <summary>
		/// The R8 format.
		/// </summary>
		R8 = 60,
		/// <summary>
		/// The R8_UIntNormal format.
		/// </summary>
		R8_UIntNormal = 61,
		/// <summary>
		/// The R8_UInt format.
		/// </summary>
		R8_UInt = 62,
		/// <summary>
		/// The R8_IntNormal format.
		/// </summary>
		R8_IntNormal = 63,
		/// <summary>
		/// The R8_Int format.
		/// </summary>
		R8_Int = 64,
		/// <summary>
		/// The A8_UIntNormal format.
		/// </summary>
		A8_UIntNormal = 65,
		/// <summary>
		/// The R1_UIntNormal format.
		/// </summary>
		R1_UIntNormal = 66,
		/// <summary>
		/// The R9G9B9E5_SharedExp format.
		/// </summary>
		R9G9B9E5_SharedExp = 67,
		/// <summary>
		/// The R8G8_B8G8_UIntNormal format.
		/// </summary>
		R8G8_B8G8_UIntNormal = 68,
		/// <summary>
		/// The G8R8_G8B8_UIntNormal format.
		/// </summary>
		G8R8_G8B8_UIntNormal = 69,
		/// <summary>
		/// The BC1 format.
		/// </summary>
		BC1 = 70,
		/// <summary>
		/// The BC1_UIntNormal format.
		/// </summary>
		BC1_UIntNormal = 71,
		/// <summary>
		/// The BC1_UIntNormal_sRGB format.
		/// </summary>
		BC1_UIntNormal_sRGB = 72,
		/// <summary>
		/// The BC2 format.
		/// </summary>
		BC2 = 73,
		/// <summary>
		/// The BC2_UIntNormal format.
		/// </summary>
		BC2_UIntNormal = 74,
		/// <summary>
		/// The BC2_UIntNormal_sRGB format.
		/// </summary>
		BC2_UIntNormal_sRGB = 75,
		/// <summary>
		/// The BC3 format.
		/// </summary>
		BC3 = 76,
		/// <summary>
		/// The BC3_UIntNormal format.
		/// </summary>
		BC3_UIntNormal = 77,
		/// <summary>
		/// The BC3_UIntNormal_sRGB format.
		/// </summary>
		BC3_UIntNormal_sRGB = 78,
		/// <summary>
		/// The BC4 format.
		/// </summary>
		BC4 = 79,
		/// <summary>
		/// The BC4_UIntNormal format.
		/// </summary>
		BC4_UIntNormal = 80,
		/// <summary>
		/// The BC4_IntNormal format.
		/// </summary>
		BC4_IntNormal = 81,
		/// <summary>
		/// The BC5 format.
		/// </summary>
		BC5 = 82,
		/// <summary>
		/// The BC5_UIntNormal format.
		/// </summary>
		BC5_UIntNormal = 83,
		/// <summary>
		/// The BC5_IntNormal format.
		/// </summary>
		BC5_IntNormal = 84,
		/// <summary>
		/// The B5G6R5_UIntNormal format.
		/// </summary>
		B5G6R5_UIntNormal = 85,
		/// <summary>
		/// The B5G5R5A1_UIntNormal format.
		/// </summary>
		B5G5R5A1_UIntNormal = 86,
		/// <summary>
		/// The B8G8R8A8_UIntNormal format.
		/// </summary>
		B8G8R8A8_UIntNormal = 87,
		/// <summary>
		/// The B8G8R8X8_UIntNormal format.
		/// </summary>
		B8G8R8X8_UIntNormal = 88,
		/// <summary>
		/// The R10G10B10_XR_BIAS_A2_UIntNormal format.
		/// </summary>
		R10G10B10_XR_BIAS_A2_UIntNormal = 89,
		/// <summary>
		/// The B8G8R8A8 format.
		/// </summary>
		B8G8R8A8 = 90,
		/// <summary>
		/// The B8G8R8A8_UIntNormal_sRGB format.
		/// </summary>
		B8G8R8A8_UIntNormal_sRGB = 91,
		/// <summary>
		/// The B8G8R8X8 format.
		/// </summary>
		B8G8R8X8 = 92,
		/// <summary>
		/// The B8G8R8X8_UIntNormal_sRGB format.
		/// </summary>
		B8G8R8X8_UIntNormal_sRGB = 93,
		/// <summary>
		/// The BC6H format.
		/// </summary>
		BC6H = 94,
		/// <summary>
		/// The BC6H_UF16 format.
		/// </summary>
		BC6H_UF16 = 95,
		/// <summary>
		/// The BC6H_SF16 format.
		/// </summary>
		BC6H_SF16 = 96,
		/// <summary>
		/// The BC7 format.
		/// </summary>
		BC7 = 97,
		/// <summary>
		/// The BC7_UIntNormal format.
		/// </summary>
		BC7_UIntNormal = 98,
		/// <summary>
		/// The BC7_UIntNormal_sRGB format.
		/// </summary>
		BC7_UIntNormal_sRGB = 99
	}

	/// <summary>
	/// Types used in a format.
	/// </summary>
	[Flags()]
	public enum FormatTypes
	{
		/// <summary>
		/// Typeless.
		/// </summary>
		/// <remarks>This flag is mutually exclusive.</remarks>
		Untyped = 0,
		/// <summary>
		/// Type is an integer.
		/// </summary>
		Integer = 1,
		/// <summary>
		/// Type is unsigned.
		/// </summary>
		Unsigned = 2,
		/// <summary>
		/// Type is normalized.
		/// </summary>
		Normalized = 4,
		/// <summary>
		/// Type uses sRGB data.
		/// </summary>
		sRGB = 8,
		/// <summary>
		/// Type is a floating point value.
		/// </summary>
		FloatingPoint = 16
	}

	/// <summary>
	/// Retrieves information about the format.
	/// </summary>
	public static class GorgonBufferFormatInfo
	{
		#region Classes.
		/// <summary>
		/// Information about a specific GorgonBufferFormat.
		/// </summary>
		public class GorgonFormatData
		{
			#region Value Types.
			/// <summary>
			/// Information about a specific component.
			/// </summary>
			public struct ComponentInformation
				: INamedObject
			{
				#region Variables.
				private string _name;
				private int _shift;
				private long _mask;
				private FormatTypes _type;
				private int _bitDepth;
				#endregion

				#region Properties.
				/// <summary>
				/// Property to return the bit shift value for the component within the packed format.
				/// </summary>
				public int Shift
				{
					get
					{
						return _shift;
					}
				}

				/// <summary>
				/// Property to return the masking value for the component.
				/// </summary>
				public long Mask
				{
					get
					{
						return _mask;
					}
				}

				/// <summary>
				/// Property to return the type of the component.
				/// </summary>
				public FormatTypes Type
				{
					get
					{
						return _type;
					}
				}

				/// <summary>
				/// Property to return the bit depth for the format.
				/// </summary>
				public int BitDepth
				{
					get
					{
						return _bitDepth;
					}
				}
				#endregion

				#region Constructor.
				/// <summary>
				/// Initializes a new instance of the <see cref="ComponentInformation"/> struct.
				/// </summary>
				/// <param name="name">The name of the component.</param>
				/// <param name="type">The type for the component.</param>
				/// <param name="bitDepth">The bit depth of the component.</param>
				/// <param name="shift">The bit shift amount for the component.</param>
				/// <param name="mask">The masking value for the component.</param>
				internal ComponentInformation(string name, FormatTypes type, int bitDepth, int shift, long mask)
				{
					_name = name;
					_type = type;
					_bitDepth = bitDepth;
					_shift = shift;
					_mask = mask;
				}
				#endregion

				#region INamedObject Members
				/// <summary>
				/// Property to return the name of the component.
				/// </summary>
				public string Name
				{
					get
					{
						return _name;
					}
				}
				#endregion
			}
			#endregion

			#region Classes.
			/// <summary>
			/// Layout for the format.
			/// </summary>
			public class FormatComponents
				: IEnumerable<ComponentInformation>
			{
				#region Variables.
				private IDictionary<string, ComponentInformation> _layout = null;
				private System.Collections.ObjectModel.ReadOnlyCollection<string> _names = null;
				#endregion

				#region Properties.
				/// <summary>
				/// Property to return the number of components in the format.
				/// </summary>
				public int Count
				{
					get
					{
						return _layout.Count;
					}
				}

				/// <summary>
				/// Property to return the component information.
				/// </summary>
				/// <param name="name"></param>
				/// <returns></returns>
				public ComponentInformation this[string name]
				{
					get
					{
						if (HasComponent(name))
							return _layout[name.ToLower()];
						else
							return new ComponentInformation(name, FormatTypes.Untyped, 0, 0, 0);
					}
				}

				/// <summary>
				/// Property to return a list of names in the component list.
				/// </summary>
				public System.Collections.ObjectModel.ReadOnlyCollection<string> ComponentNames
				{
					get
					{
						return _names;
					}
				}
				#endregion

				#region Methods.
				/// <summary>
				/// Function to add information for a component.
				/// </summary>
				/// <param name="info"></param>
				internal void Add(ComponentInformation info)
				{
					_layout.Add(info.Name.ToLower(), info);
				}

				/// <summary>
				/// Function to retrieve the total bit depth for the components.
				/// </summary>
				/// <returns>The total bit depth for the components.</returns>
				internal int GetBitDepth()
				{
					return _layout.Sum(item => item.Value.BitDepth);
				}

				/// <summary>
				/// Function to add the names of the components.
				/// </summary>
				internal void AddNames()
				{
					var names = (from name in _layout
								select name.Value.Name).ToArray();

					_names = new System.Collections.ObjectModel.ReadOnlyCollection<string>(names);
				}

				/// <summary>
				/// Function to return whether the format contains a particular component.
				/// </summary>
				/// <param name="name">Name of the component.</param>
				/// <returns>TRUE if the component exists in this format, FALSE if not.</returns>
				public bool HasComponent(string name)
				{
					return _layout.ContainsKey(name.ToLower());
				}
				#endregion

				#region Constructor.
				/// <summary>
				/// Initializes a new instance of the <see cref="FormatComponents"/> class.
				/// </summary>
				internal FormatComponents()
				{
					_layout = new Dictionary<string, ComponentInformation>();					
				}
				#endregion

				#region IEnumerable<ComponentInformation> Members
				/// <summary>
				/// Returns an enumerator that iterates through the collection.
				/// </summary>
				/// <returns>
				/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
				/// </returns>
				public IEnumerator<ComponentInformation> GetEnumerator()
				{
					foreach (var info in _layout)
						yield return info.Value;
				}
				#endregion

				#region IEnumerable Members
				/// <summary>
				/// Returns an enumerator that iterates through a collection.
				/// </summary>
				/// <returns>
				/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
				/// </returns>
				System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
				{
					return GetEnumerator();
				}
				#endregion
			}
			#endregion

			#region Variables.
			private int _bitDepth = 0;
			private int _sizeInBytes = 0;
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the format.
			/// </summary>
			public GorgonBufferFormat Format
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return information about the components in the format.
			/// </summary>
			public FormatComponents Components
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the bit depth for the format.
			/// </summary>
			public int BitDepth
			{
				get
				{
					if (_bitDepth == 0)
						_bitDepth = Components.GetBitDepth();

					return _bitDepth;
				}
			}

			/// <summary>
			/// Property to return the size of the format, in bytes.
			/// </summary>
			public int SizeInBytes
			{
				get
				{
					if (_sizeInBytes == 0)
					{
						// Can't have a data type smaller than 1 bit.
						if (BitDepth >= 8)
							_sizeInBytes = BitDepth / 8;
						else
							_sizeInBytes = 1;
					}

					return _sizeInBytes;
				}
			}

			/// <summary>
			/// Property to return whether the format has a depth component.
			/// </summary>
			public bool HasDepth
			{
				get
				{
					return Components.HasComponent("D");
				}
			}

			/// <summary>
			/// Property to return whether the format has a stencil component.
			/// </summary>
			public bool HasStencil
			{
				get
				{
					return Components.HasComponent("S");
				}
			}

			/// <summary>
			/// Property to return whether the format has an alpha component.
			/// </summary>
			public bool HasAlpha
			{
				get
				{
					return Components.HasComponent("A");
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to parse the layout information.
			/// </summary>
			/// <param name="layoutInfo">Layout information.</param>
			private void GetLayout(Tuple<string, FormatTypes> layoutInfo)
			{
				string packedComponent = layoutInfo.Item1;
				StringBuilder numbers = new StringBuilder(layoutInfo.Item1.Length);
				StringBuilder letters = new StringBuilder(layoutInfo.Item1.Length);
				int offset = 0;
				
				for (int i = 0; i < packedComponent.Length; i++)
				{
					if (char.IsNumber(packedComponent[i]))
					{
						numbers.Append(packedComponent[i]);
						letters.Append(' ');
					}

					if (char.IsLetter(packedComponent[i]))
					{
						letters.Append(packedComponent[i]);
						numbers.Append(' ');
					}
				}

				var componentNames = letters.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				var componentDepths = numbers.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				// These should be the same length.  If not, then we'll just have to exit.
				if (componentNames.Length != componentDepths.Length)
					return;

				if (Components.Count > 0)
					offset = Components.Last().Shift + Components.Last().BitDepth;

				for (int i = componentNames.Length - 1; i >= 0; i--)
				{
					int bitDepth = 0;
					int nameCounter = 1;
					string name = componentNames[i];

					bitDepth = Convert.ToInt32(componentDepths[i]);
					while (Components.HasComponent(name))
					{
						name = componentNames[i] + "." + nameCounter.ToString();
						nameCounter++;
					}

					Components.Add(new ComponentInformation(name, layoutInfo.Item2, bitDepth, offset, ((long)System.Math.Pow(2, bitDepth)) - 1));
					offset += bitDepth;
				}
			}

			/// <summary>
			/// Function to parse the format.
			/// </summary>
			/// <param name="format">Format to parse.</param>
			private void ParseFormat(GorgonBufferFormat format)
			{
				string[] types = new string[] { "UInt", "Int", "UIntNormal", "IntNormal", "Float", "SharedExp" };
				string formatString = format.ToString();
				string originalString = formatString;
				IList<Tuple<string, FormatTypes>> components = new List<Tuple<string, FormatTypes>>();

				// Don't process the compression formats.
				if (formatString.StartsWith("BC", StringComparison.CurrentCultureIgnoreCase))
					return;

				// Split out the major component types.
				while (formatString.Length > 0)
				{
					FormatTypes formatType = FormatTypes.Untyped;
					int index = formatString.IndexOf('_');
					string part = string.Empty;

					if (index > -1)
						part = formatString.Substring(0, index);
					else
					{
						part = formatString;
						formatString = string.Empty;
					}

					string stringType = types.Where(item => string.Compare(part, item, true) == 0).FirstOrDefault();

					// No type?  Then this is a component.
					if ((string.IsNullOrEmpty(stringType)) && (string.Compare(part, "sRGB", true) != 0))
						components.Add(new Tuple<string, FormatTypes>(part, formatType));
					else
					{
						// The format of the enumeration value should be <components>[_<type>][_<components>_[type]]
						if (components.Count != 0)
						{
							switch (stringType)
							{
								case "UInt":
									formatType = FormatTypes.Unsigned | FormatTypes.Integer;
									break;
								case "UIntNormal":
									formatType = FormatTypes.Unsigned | FormatTypes.Integer | FormatTypes.Normalized;
									break;
								case "Int":
									formatType = FormatTypes.Integer;
									break;
								case "IntNormal":
									formatType = FormatTypes.Integer | FormatTypes.Normalized;
									break;
								case "SharedExp":
								case "Float":
									formatType = FormatTypes.FloatingPoint;
									break;
							}

							// If this is an sRGB format, then apply it to the format type.
							if (originalString.EndsWith("_sRGB", StringComparison.CurrentCultureIgnoreCase))
								formatType |= FormatTypes.sRGB;

							// Look for the previous component, and assign the type.
							for (int i = components.Count - 1; i >= 0; i--)
							{
								// Replace previously untyped.
								if (components[i].Item2 == FormatTypes.Untyped)
									components[i] = new Tuple<string, FormatTypes>(components[i].Item1, formatType);
							}
						}
					}

					if (!string.IsNullOrEmpty(formatString))
						formatString = formatString.Substring(index + 1);
				}

				foreach (var component in components.Reverse())
					GetLayout(component);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GorgonFormatData"/> class.
			/// </summary>
			/// <param name="format">The format to evaluate.</param>
			internal GorgonFormatData(GorgonBufferFormat format)
			{
				Format = format;
				Components = new FormatComponents();

				if (format != GorgonBufferFormat.Unknown)
				{
					ParseFormat(format);
					Components.AddNames();
				}
			}
			#endregion
		}
		#endregion

		#region Variables.
		private static IDictionary<GorgonBufferFormat, GorgonFormatData> _formats = null;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build the format information.
		/// </summary>
		private static void GetFormatInfo()
		{
			GorgonBufferFormat[] formats = (GorgonBufferFormat[])Enum.GetValues(typeof(GorgonBufferFormat));

			System.IO.StreamWriter writer = new System.IO.StreamWriter("c:/users/mike/desktop/formats.txt");
			foreach (var format in formats)
			{
				GorgonFormatData data = new GorgonFormatData(format);
				_formats.Add(format, data);
				writer.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", data.Format, data.BitDepth, data.SizeInBytes, data.HasAlpha, data.HasDepth, data.HasStencil );

				foreach (var comp in data.Components.OrderByDescending(item => item.Shift))
				{
					writer.WriteLine("{0}, {1}, {2}, 0x{3:x}, {4}", comp.Name, comp.Type, comp.BitDepth, comp.Mask, comp.Shift);
				}
				writer.WriteLine();
			}


			writer.Close();
		}

		/// <summary>
		/// Function to retrieve information about a format.
		/// </summary>
		/// <param name="format">Format to retrieve information about.</param>
		/// <returns>The information for the format.  If the format is unknown, then the data for the Unknown GorgonBufferFormat will be returned.</returns>
		public static GorgonFormatData GetInfo(GorgonBufferFormat format)
		{
			if (!_formats.ContainsKey(format))
				return _formats[GorgonBufferFormat.Unknown];

			return _formats[format];
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonBufferFormatInfo"/> class.
		/// </summary>
		static GorgonBufferFormatInfo()
		{
			_formats = new Dictionary<GorgonBufferFormat, GorgonFormatData>();
			GetFormatInfo();
		}
		#endregion
	}
}
