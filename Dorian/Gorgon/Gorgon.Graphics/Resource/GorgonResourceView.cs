#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, February 12, 2012 7:37:43 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A view of a resource that can be bound to a shader.
	/// </summary>
	/// <remarks>A view allows a resource to be handled by a shader and optionally used to reinterpret the format of a resource.</remarks>
	public class GorgonResourceView
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;				// Flag to indicate whether the resource is disposed or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D shader resource view.
		/// </summary>
		internal D3D.ShaderResourceView D3DResourceView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the Direct3D unordered access resource view.
		/// </summary>
		internal D3D.UnorderedAccessView D3DUnorderedResourceView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the resource that is bound to this view.
		/// </summary>
		public GorgonResource Resource
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the format for the shader resource.
		/// </summary>
		public BufferFormat ShaderResourceFormat
		{
			get
			{
				if (D3DResourceView != null)
				{
					return (BufferFormat)D3DResourceView.Description.Format;
				}

				return BufferFormat.Unknown;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build a view for a texture resource.
		/// </summary>
		/// <param name="resource">The resource to build a view for.</param>
		private void BuildTextureView(GorgonTexture resource)
		{
			string textureType = GetType().Name;
			bool isMultisampled = resource.Settings.Multisampling.Count > 1 || resource.Settings.Multisampling.Quality > 0;
			D3D.ShaderResourceViewDescription desc = default(D3D.ShaderResourceViewDescription);
			D3D.UnorderedAccessViewDescription uavDesc = default(D3D.UnorderedAccessViewDescription);

			if (resource.ViewFormatInformation.IsTypeless)
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the shader view.  The format '" + resource.Settings.ViewFormat.ToString() + "' is untyped.  A view requires a typed format.");

			if (resource.Settings.Usage == BufferUsage.Staging)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a view for staging objects.");
			}

			// We cannot bind staging resources to the pipeline.
			if (resource.Settings.Usage != BufferUsage.Staging)
			{
				if ((resource.Settings.ViewFormat == BufferFormat.Unknown) || (resource.Settings.ViewFormat == resource.Settings.Format))
				{
					D3DResourceView = new D3D.ShaderResourceView(Graphics.D3DDevice, resource.D3DResource);

					// Create an unordered access view if we've requested one.
					if (resource.Settings.ViewIsUnordered)
					{
						D3DUnorderedResourceView = new D3D.UnorderedAccessView(Graphics.D3DDevice, resource.D3DResource);
					}
				}
				else
				{
					if (string.Compare(resource.ViewFormatInformation.Group, resource.FormatInformation.Group, true) != 0)
						throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the shader view.  The format '" + resource.Settings.Format.ToString() + "' and the view format '" + resource.Settings.ViewFormat.ToString() + "' are not part of the same group.");

					desc.Format = (GI.Format)resource.Settings.ViewFormat;

					// Determine view type.
					switch (textureType.ToLower())
					{
						case "gorgontexture1d":
							if (resource.Settings.ArrayCount <= 1)
							{
								desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture1D;
								uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture1D;
							}
							else
							{
								desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture1DArray;
								uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture1DArray;
							}

							desc.Texture1DArray = new D3D.ShaderResourceViewDescription.Texture1DArrayResource()
							{
								MipLevels = resource.Settings.MipCount,
								MostDetailedMip = 0,
								ArraySize = resource.Settings.ArrayCount,
								FirstArraySlice = 0
							};

							uavDesc.Texture1DArray = new D3D.UnorderedAccessViewDescription.Texture1DArrayResource()
							{
								ArraySize = resource.Settings.ArrayCount,
								FirstArraySlice = 0,
								MipSlice = 0
							};
							break;
						case "gorgontexture2d":
							if (resource.Settings.ArrayCount <= 1)
							{
								uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture2D;
							}
							else
							{
								uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture2DArray;
							}

							uavDesc.Texture2DArray = new D3D.UnorderedAccessViewDescription.Texture2DArrayResource()
							{
								ArraySize = resource.Settings.ArrayCount,
								FirstArraySlice = 0,
								MipSlice = 0
							};

							if (!resource.Settings.IsTextureCube)
							{
								if (isMultisampled)
								{
									if (resource.Settings.ArrayCount <= 1)
									{
										desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampled;
									}
									else
									{
										desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampledArray;
									}

									desc.Texture2DMSArray = new D3D.ShaderResourceViewDescription.Texture2DMultisampledArrayResource()
									{
										ArraySize = resource.Settings.ArrayCount,										
										FirstArraySlice = 0
									};
								}
								else
								{
									if (resource.Settings.ArrayCount <= 1)
										desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
									else
										desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DArray;

									desc.Texture2DArray = new D3D.ShaderResourceViewDescription.Texture2DArrayResource()
									{
										MipLevels = resource.Settings.MipCount,
										MostDetailedMip = 0,
										ArraySize = resource.Settings.ArrayCount,
										FirstArraySlice = 0
									};
								}
							}
							else
							{
								if (resource.Settings.ArrayCount <= 1)
								{
									desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
									desc.TextureCube = new D3D.ShaderResourceViewDescription.TextureCubeResource()
									{
										MipLevels = resource.Settings.MipCount,
										MostDetailedMip = 0
									};
								}
								else
								{
									desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCubeArray;
									desc.TextureCubeArray = new D3D.ShaderResourceViewDescription.TextureCubeArrayResource()
									{
										MipLevels = resource.Settings.MipCount,
										CubeCount = resource.Settings.ArrayCount / 6,
										First2DArrayFace = 0,
										MostDetailedMip = 0
									};
								}								
							}
							break;
						case "gorgontexture3d":
							desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D;
							desc.Texture3D = new D3D.ShaderResourceViewDescription.Texture3DResource()
							{
								MipLevels = resource.Settings.MipCount,
								MostDetailedMip = 0
							};

							uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture3D;
							uavDesc.Texture3D = new D3D.UnorderedAccessViewDescription.Texture3DResource()
							{
								FirstWSlice = 0,
								MipSlice = 0,
								WSize = resource.Settings.Depth
							};
							break;
						default:
							throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a resource view, the texture type '" + textureType + "' is unknown.");
					}

					try
					{
						D3DResourceView = new D3D.ShaderResourceView(Graphics.D3DDevice, resource.D3DResource, desc);
						D3DResourceView.DebugName = textureType + " '" + Name + "' Shader Resource View";
						if (resource.Settings.ViewIsUnordered)
						{
							D3DUnorderedResourceView = new D3D.UnorderedAccessView(Graphics.D3DDevice, resource.D3DResource, uavDesc);
							D3DUnorderedResourceView.DebugName = textureType + " '" + Name + "' Unordered Access Resource View";
						}
					}
					catch (SharpDX.SharpDXException sDXEx)
					{
						if ((uint)sDXEx.ResultCode.Code == 0x80070057)
							throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the shader view.  The format '" + resource.Settings.ViewFormat.ToString() + "' is not compatible or castable to '" + resource.Settings.Format.ToString() + "'.");
					}
				}
			}
		}

		/// <summary>
		/// Function to build the resource view for a shader buffer.
		/// </summary>
		/// <param name="buffer">Buffer to build the view for.</param>
		private void BuildBufferView(GorgonShaderBuffer buffer)
		{
			Type bufferType = buffer.GetType();
			D3D.ShaderResourceViewDescription srvDesc = new D3D.ShaderResourceViewDescription();
			D3D.UnorderedAccessViewDescription uavDesc = new D3D.UnorderedAccessViewDescription();
			D3D.UnorderedAccessViewBufferFlags uavFlags = D3D.UnorderedAccessViewBufferFlags.None;
			GorgonStructuredBuffer structuredBuffer = buffer as GorgonStructuredBuffer;

			if (structuredBuffer != null)
			{
				switch (structuredBuffer.StructuredBufferType)
				{
					case StructuredBufferType.AppendConsume:
						uavFlags = D3D.UnorderedAccessViewBufferFlags.Append;
						break;
					case StructuredBufferType.Counter:
						uavFlags = D3D.UnorderedAccessViewBufferFlags.Counter;
						break;
					default:
						uavFlags = D3D.UnorderedAccessViewBufferFlags.None;
						break;
				}

				srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer;
				srvDesc.BufferEx = new D3D.ShaderResourceViewDescription.ExtendedBufferResource()
				{
					ElementCount = structuredBuffer.ElementCount,
					FirstElement = 0,
					Flags = D3D.ShaderResourceViewExtendedBufferFlags.None
				};
				srvDesc.Format = GI.Format.Unknown;

				uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Buffer;
				uavDesc.Format = GI.Format.Unknown;
				uavDesc.Buffer = new D3D.UnorderedAccessViewDescription.BufferResource()
				{
					ElementCount = structuredBuffer.ElementCount,
					FirstElement = 0,
					Flags = uavFlags
				};

				D3DResourceView = new D3D.ShaderResourceView(Graphics.D3DDevice, buffer.D3DResource, srvDesc);
				D3DResourceView.DebugName = bufferType + " '" + Name + "' Shader Resource View";
				if (structuredBuffer.IsUnorderedAccess)
				{
					D3DUnorderedResourceView = new D3D.UnorderedAccessView(Graphics.D3DDevice, buffer.D3DResource, uavDesc);
					D3DUnorderedResourceView.DebugName = bufferType + " '" + Name + "' Unordered Access View";
				}
				return;
			}
		}

		/// <summary>
		/// Function to clean up the resource view objects.
		/// </summary>
		protected void CleanUp()
		{
			if (D3DResourceView != null)
			{
				Gorgon.Log.Print("Gorgon resource view {0}: Destroying D3D 11 shader resource view.", Diagnostics.LoggingLevel.Verbose, Name);
				D3DResourceView.Dispose();
			}

			if (D3DUnorderedResourceView != null)
			{
				Gorgon.Log.Print("Gorgon resource view {0}: Destroying D3D 11 unordered access resource view.", Diagnostics.LoggingLevel.Verbose, Name);
				D3DUnorderedResourceView.Dispose();
			}

			D3DResourceView = null;
			D3DUnorderedResourceView = null;
		}

		/// <summary>
		/// Function to build the resource view.
		/// </summary>
		internal void BuildResourceView()
		{
			GorgonShaderBuffer buffer = null;
			GorgonTexture texture = null;

			CleanUp();

			// Do nothing if we're not bound to a resource.
			if (Resource == null)
			{
				return;
			}

			// Determine the type of the resource.
			buffer = Resource as GorgonShaderBuffer;
			if (buffer != null)
			{
				BuildBufferView(buffer);
				return;
			}

			texture = Resource as GorgonTexture;
			if (texture != null)
			{
				BuildTextureView(texture);
				return;
			}

			throw new InvalidCastException("The resource type cannot have a view.");
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonResourceView" /> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		/// <param name="name">The name of the resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the parameter is an empty string.</exception>
		internal GorgonResourceView(GorgonGraphics graphics, string name)	
			: base(name)
		{
			Graphics = graphics;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Graphics.RemoveTrackedObject(this);

					CleanUp();
				}				

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
