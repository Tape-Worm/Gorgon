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
// Created: Tuesday, January 31, 2012 12:38:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using GorgonLibrary.Diagnostics;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Manages the display of the graphics data.
	/// </summary>
	public sealed class GorgonOutputMerger
	{
		/// <summary>
		/// A list of render targets to bind.
		/// </summary>
		public sealed class RenderTargetList
			: IList<GorgonRenderTarget>
		{
			#region Variables.
			private readonly GorgonGraphics _graphics;
			private readonly IList<GorgonRenderTarget> _targets;
			private readonly D3D.RenderTargetView[] _views;
			private GorgonDepthStencil _depthStencilBuffer;
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the number of render targets.
			/// </summary>
			public int Count
			{
				get
				{
					return _targets.Count;
				}
			}

			/// <summary>
			/// Property to set or return the depth/stencil buffer.
			/// </summary>
			public GorgonDepthStencil DepthStencilBuffer
			{
				get
				{
					return _depthStencilBuffer;
				}
				set
				{
					SetDepthStencil(value);
				}
			}

			/// <summary>
			/// Property to return a render target by name if bound.
			/// </summary>
			/// <remarks>This property is read-only.  To set a render target, use the default property that uses an index.</remarks>
			public GorgonRenderTarget this[string name]
			{
				get
				{
					int index = IndexOf(name);
					if (index > -1)
						return _targets[index];

					throw new KeyNotFoundException("The render target '" + name + "' was not bound.");
				}
			}

			/// <summary>
			/// Property to set or return a render target binding.
			/// </summary>
			/// <remarks>This will set the depth/stencil buffer to the one that's assigned to the render target.  If there is a need to set a separate depth/stencil, then use then 
			/// <see cref="M:GorgonLibrary.Graphics.GorgonOutputMerger.GorgonRenderTargetList.SetRenderTarget">SetRenderTarget</see> method.</remarks>
			public GorgonRenderTarget this[int index]
			{
				get
				{
					return _targets[index];
				}
				set
				{
					SetRenderTarget(index, value, value == null ? null : value.DepthStencil);
				}
			}
			#endregion

			#region Methods.
            /// <summary>
            /// Function to validate the render target being set.
            /// </summary>
            /// <param name="newTarget">The new render target being set.</param>
            private void ValidateTargets(GorgonRenderTarget newTarget)
            {
                // If we're turning off a target, then leave.
                if (newTarget == null)
                {
                    return;
                }
                
                // TODO: Add depth component when we have 3D targets.
                // Ensure the dimensions of the targets are the same.
                if (!this.All(
                        item =>
                        item == null || item.Settings.Width != newTarget.Settings.Width
                        || item.Settings.Height != newTarget.Settings.Height))
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(
                                                  "Cannot bind the render target '{0}', the width and height of the target must be the same as all other targets bound to the pipeline.",
                                                  newTarget.Name));
                }

                if (!this.All(item => item == null || item.Settings.MultiSample.Count == newTarget.Settings.MultiSample.Count))
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(
                                                  "Cannot bind the render target '{0}', the multi-sampling settings of the target must be the same as all other targets bound to the pipeline.",
                                                  newTarget.Name));
                }

                if (!this.All(
                        item =>
                        item == null || item.Texture.Settings.ArrayCount == newTarget.Texture.Settings.ArrayCount))
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(
                                                  "Cannot bind the render target '{0}', the array count settings of the target must be the same as all other targets bound to the pipeline.",
                                                  newTarget.Name));
                }
            }
			/// <summary>
			/// Function to determine if the render targets have the same bit depth.
			/// </summary>
			/// <param name="format">The format to check.</param>
			/// <param name="index">Index of the render target being set.</param>
			/// <returns>TRUE if the bit depths are the same, FALSE if not.</returns>
			private bool HasSameBitDepth(BufferFormat format, int index)
			{
				int bitDepth = GorgonBufferFormatInfo.GetInfo(format).BitDepth;

				if (_graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.SM2_a_b)
					return true;

				for (int i = 0; i < _targets.Count; i++)
				{
					if (index == i)
						continue;

					if (_targets[i] != null)
					{
						int otherBitDepth = GorgonBufferFormatInfo.GetInfo(_targets[i].Settings.Format).BitDepth;

						if (otherBitDepth != bitDepth)
							return false;
					}
				}

				return true;
			}

			/// <summary>
			/// Function to re-seat a render target after it's been altered.
			/// </summary>
			/// <param name="target">Target to re-seat.</param>
			internal void ReSeat(GorgonRenderTarget target)
			{
				int index = IndexOf(target);

				if (index > -1)
				{
					this[index] = null;
					this[index] = target;
				}
			}

            /// <summary>
            /// Function to re-seat a depth/stencil buffer after it's been altered.
            /// </summary>
            /// <param name="depthStencil">The depth/stencil buffer that is to be reseated.</param>
            internal void ReSeat(GorgonDepthStencil depthStencil)
            {
                if (DepthStencilBuffer != depthStencil)
                {
                    return;
                }

                DepthStencilBuffer = null;
                DepthStencilBuffer = depthStencil;
            }

			/// <summary>
			/// Function to determine if a render target is bound by its name.
			/// </summary>
			/// <param name="name">Name of the render target.</param>
			/// <returns>TRUE if the render target was found, FALSE if not.</returns>
			public bool Contains(string name)
			{
				GorgonDebug.AssertParamString(name, "name");

				return IndexOf(name) != -1;
			}

			/// <summary>
			/// Function to determine the index of a bound render target by its name.
			/// </summary>
			/// <param name="name">Name of the render target to look up.</param>
			/// <returns>The index of the render target, -1 if not found.</returns>
			public int IndexOf(string name)
			{
				GorgonDebug.AssertParamString(name, "name");

				for (int i = 0; i < _targets.Count; i++)
				{
					if ((_targets[i] != null) && (string.Compare(name, _targets[i].Name, true) == 0))
						return i;
				}

				return -1;
			}

			/// <summary>
			/// Function to set the depth buffer.
			/// </summary>
			/// <param name="depthBuffer">Depth buffer to set.</param>
			public void SetDepthStencil(GorgonDepthStencil depthBuffer)
			{
				D3D.DepthStencilView view = (depthBuffer == null ? null : depthBuffer.D3DDepthStencilView);

#if DEBUG
                // Ensure the depth/stencil multi-sample settings are the same.  Otherwise, an exception should be thrown.
			    if (depthBuffer != null)
			    {
			        if (!this.All(item => item == null || item.Settings.MultiSample.Count == depthBuffer.Settings.MultiSample.Count))
			        {
			            throw new GorgonException(GorgonResult.CannotBind,
			                                      string.Format(
			                                          "The depth/stencil buffer '{0}' has different multi-sample settings than the render target(s) that are bound to the pipeline.",
			                                          depthBuffer.Name));
			        }

			        if (!this.All(item => item == null || item.Texture.Settings.ArrayCount == depthBuffer.Texture.Settings.ArrayCount))
			        {
			            throw new GorgonException(GorgonResult.CannotBind,
			                                      string.Format(
			                                          "The depth/stencil buffer '{0}' has a different array count than the render target(s) that are bound to the pipeline.",
			                                          depthBuffer.Name));
			        }

                    // TODO: Add code to check dimensions and ensure they match the targets.
                    // This is not explcitly stated in the documentation, but it's a good bet that there's going to be issues with having a depth buffer 
                    // that has a different width/height/depth than the render target texture is going to cause problems.
                }
#endif
                
				_depthStencilBuffer = depthBuffer;
				_graphics.Context.OutputMerger.SetTargets(view, _views);
			}

			/// <summary>
			/// Function to bind a render target and a depth/stencil buffer.
			/// </summary>
			/// <param name="index">Index to bind at.</param>
			/// <param name="target">Target to bind.</param>
			/// <param name="depthStencil">Depth/stencil buffer to bind.</param>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0 or greater than the number of allowed targets.</exception>
			/// <exception cref="GorgonLibrary.GorgonException">Thrown if the current video device is only SM 2.0 and the targets are not the same bit depth.</exception>
			public void SetRenderTarget(int index, GorgonRenderTarget target, GorgonDepthStencil depthStencil)
			{
				GorgonDebug.AssertParamRange(index, 0, 8, true, false, "startIndex");

#if DEBUG
				if (target != null)
				{
					if (!HasSameBitDepth(target.Settings.Format, index))
						throw new GorgonException(GorgonResult.CannotBind, "Cannot bind the render target, targets must be of the same bit depth for SM_2_a_b video devices.");
				}
#endif
				
				// If we've got the target bound to a texture slot, then remove it before assigning it.
				// Otherwise D3D11 will throw up a warning in the debug output.
				if (target != null)
				    _graphics.Shaders.Unbind(target.Texture);

				_targets[index] = target;
				_views[index] = target == null ? null : target.D3DRenderTarget;
				SetDepthStencil(depthStencil);
			}

			/// <summary>
			/// Function to set a range of render targets.
			/// </summary>
			/// <param name="targets">Render targets to set.</param>
			/// <param name="depthStencil">The depth/stencil buffer to use.</param>
			/// <param name="startIndex">The starting index that will be bound.</param>
			/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="targets"/> parameter will set the bindings to empty (starting at <paramref name="startIndex"/>).</remarks>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0 or greater than the number of allowed targets.</exception>
			/// <exception cref="GorgonLibrary.GorgonException">Thrown if the current video device is SM 2.0 and the targets are not the same bit depth.</exception>
			public void SetRenderTargetRange(IEnumerable<GorgonRenderTarget> targets, GorgonDepthStencil depthStencil, int startIndex)
			{
				int count = _targets.Count - startIndex;

				GorgonDebug.AssertParamRange(startIndex, 0, 8, true, false, "startIndex");

				for (int i = 0; i < count; i++)
				{
					GorgonRenderTarget target = null;

					if (targets != null)
					{
						target = targets.ElementAt(i);
						if (target != null)
						{
#if DEBUG
							if (!HasSameBitDepth(target.Settings.Format, i + startIndex))
								throw new GorgonException(GorgonResult.CannotBind, "Cannot bind the render target, targets must be of the same bit depth for SM_2_a_b video devices.");
#endif

							// If we've got the target bound to a texture slot, then remove it before assigning it.
							// Otherwise D3D11 will throw up a warning in the debug output.
							_graphics.Shaders.Unbind(target.Texture);
						}
					}
					_targets[i + startIndex] = target;
					_views[i + startIndex] = (target == null ? null : target.D3DRenderTarget);
				}

				SetDepthStencil(depthStencil);
			}

			/// <summary>
			/// Function to set a range of render targets.
			/// </summary>
			/// <param name="targets">Render targets to set.</param>
			/// <param name="depthStencil">The depth/stencil buffer to use.</param>
			/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="targets"/> parameter will set the bindings to empty.</remarks>
			public void SetRenderTargetRange(IEnumerable<GorgonRenderTarget> targets, GorgonDepthStencil depthStencil)
			{
				SetRenderTargetRange(targets, depthStencil, 0);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="RenderTargetList"/> class.
			/// </summary>
			/// <param name="graphics">Graphics interface.</param>
			internal RenderTargetList(GorgonGraphics graphics)
			{
				_graphics = graphics;
				if (graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
					_targets = new GorgonRenderTarget[4];
				else
					_targets = new GorgonRenderTarget[8];

				_views = new D3D.RenderTargetView[_targets.Count];
			}
			#endregion

			#region IList<GorgonRenderTarget> Members
			/// <summary>
			/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
			/// <returns>
			/// The index of item if found in the list; otherwise, -1.
			/// </returns>
			public int IndexOf(GorgonRenderTarget item)
			{
				return _targets.IndexOf(item);
			}

			/// <summary>
			/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which item should be inserted.</param>
			/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
			/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
			///   
			/// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
			void IList<GorgonRenderTarget>.Insert(int index, GorgonRenderTarget item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the item to remove.</param>
			/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
			///   
			/// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
			void IList<GorgonRenderTarget>.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			#endregion

			#region ICollection<GorgonRenderTarget> Members
			/// <summary>
			/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
			/// </summary>
			/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
			/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
			void ICollection<GorgonRenderTarget>.Add(GorgonRenderTarget item)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
			/// </summary>
			/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
			void ICollection<GorgonRenderTarget>.Clear()
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
			/// <returns>
			/// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
			/// </returns>
			public bool Contains(GorgonRenderTarget item)
			{
				return _targets.Contains(item);
			}

			/// <summary>
			/// Function to copy the bound targets to an array.
			/// </summary>
			/// <param name="array">The array to receive the targets.</param>
			/// <param name="arrayIndex">Index to start writing at.</param>
			public void CopyTo(GorgonRenderTarget[] array, int arrayIndex)
			{
				_targets.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
			/// </summary>
			/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
			public bool IsReadOnly
			{
				get 
				{
					return false;
				}
			}

			/// <summary>
			/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
			/// </summary>
			/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
			/// <returns>
			/// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
			/// </returns>
			/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
			bool ICollection<GorgonRenderTarget>.Remove(GorgonRenderTarget item)
			{
				throw new NotImplementedException();
			}
			#endregion

			#region IEnumerable<GorgonRenderTarget> Members
			/// <summary>
			/// Returns an enumerator that iterates through the collection.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<GorgonRenderTarget> GetEnumerator()
			{
				foreach (var item in _targets)
					yield return item;
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

		#region Variables.
		private readonly GorgonGraphics _graphics;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the blending render state interface.
		/// </summary>
		public GorgonBlendRenderState BlendingState
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the depth/stencil render state interface.
		/// </summary>
		public GorgonDepthStencilRenderState DepthStencilState
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the render target bindings.
		/// </summary>
		public RenderTargetList RenderTargets
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up resources used by the interface.
		/// </summary>
		internal void CleanUp()
		{
			if (BlendingState != null)
			{
				BlendingState.CleanUp();
			}

			if (DepthStencilState != null)
			{
				DepthStencilState.CleanUp();
			}

			BlendingState = null;
			DepthStencilState = null;
		}

		/// <summary>
		/// Function to draw polygons to the current render target.
		/// </summary>
		/// <param name="vertexStart">Vertex to start at.</param>
		/// <param name="vertexCount">Number of vertices to draw.</param>
		public void Draw(int vertexStart, int vertexCount)
		{
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.Draw(vertexCount, vertexStart);
		}

		/// <summary>
		/// Function to draw geometry of an unknown size.
		/// </summary>
		public void DrawAuto()
		{
#if DEBUG
			if (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
				throw new GorgonException(GorgonResult.AccessDenied, "SM 2.0 video devices cannot draw auto-generated data.");
#endif
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.DrawAuto();
		}

		/// <summary>
		/// Function to draw indexed polygons.
		/// </summary>
		/// <param name="indexStart">Starting index to use.</param>
		/// <param name="baseVertex">Vertex index added to each index.</param>
		/// <param name="indexCount">Number of indices to use.</param>
		public void DrawIndexed(int indexStart, int baseVertex, int indexCount)
		{
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.DrawIndexed(indexCount, indexStart, baseVertex);
		}

		/// <summary>
		/// Function to draw indexed instanced polygons.
		/// </summary>
		/// <param name="startInstance">A value added to each index.</param>
		/// <param name="indexStart">Starting index to use.</param>
		/// <param name="baseVertex">Vertex index added to each index.</param>
		/// <param name="instanceCount">Number of indices to use.</param>
		/// <param name="indexCount">Number of indices to read per instance.</param>
		public void DrawIndexedInstanced(int startInstance, int indexStart, int baseVertex, int instanceCount, int indexCount)
		{
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.DrawIndexedInstanced(indexCount, instanceCount, indexStart, baseVertex, startInstance);
		}

		/// <summary>
		/// Function to draw instanced polygons.
		/// </summary>
		/// <param name="startInstance">Value added to each index.</param>
		/// <param name="startVertex">Vertex to start at.</param>
		/// <param name="instanceCount">Number of instances to draw.</param>
		/// <param name="vertexCount">Number of vertices to draw.</param>
		public void DrawInstanced(int startInstance, int startVertex, int instanceCount, int vertexCount)
		{
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.DrawInstanced(vertexCount, instanceCount, startVertex, startInstance);
		}

		/// <summary>
		/// Function to draw indexed, instanced GPU generated data.
		/// </summary>
		/// <param name="buffer">Buffer holding the GPU generated data.</param>
		/// <param name="alignedAyteOffset">Number of bytes to start at within the buffer.</param>
		/// <param name="isIndexed">TRUE if the data is indexed, FALSE if not.</param>
		/// <remarks>This method is not supported by SM2_a_b or SM_4.x video devices.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown if the current video device is a SM2_a_b or SM4_x device.</exception>
		public void DrawInstancedIndirect(GorgonBuffer buffer, int alignedAyteOffset, bool isIndexed)
		{
			GorgonDebug.AssertNull<GorgonBuffer>(buffer, "buffer");

#if DEBUG
			if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) ||
				(_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4) ||
				(_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4_1))
				throw new InvalidOperationException("Cannot call DrawInstancedIndirect without a SM5 or better video device.");
#endif
			GorgonRenderStatistics.DrawCallCount++;
			if (isIndexed)
				_graphics.Context.DrawIndexedInstancedIndirect((D3D.Buffer)buffer.D3DResource, alignedAyteOffset);
			else
				_graphics.Context.DrawInstancedIndirect((D3D.Buffer)buffer.D3DResource, alignedAyteOffset);
		}

		/// <summary>
		/// Function to create a depth/stencil buffer.
		/// </summary>
		/// <param name="name">Name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings to apply to the depth/stencil buffer.</param>
		/// <returns>A new depth/stencil buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencilSettings.Format">GorgonDepthStencilSettings.Format</see> property is set to Unknown or is unsupported.</para>
		/// </exception>
		/// <remarks>
		/// A depth buffer may be paired with a swapchain or render target through its DepthStencil property.  When pairing the depth/stencil to the render target, Ensure that the depth/stencil buffer width, height and multisample settings match that of the render target that it is paired with.
		/// <para>The texture for a depth/stencil may be used in a shader for cards that have a feature level of SM_4_1 or better, and can be set to do so by setting the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencilSettings.TextureFormat">GorgonDepthStencilSettings.TextureFormat</see> property to a typeless format. 
		/// If this is attempted on a video device that has a feature level of SM_4_0 or below, then an exception will be raised.</para>
		/// </remarks>
		public GorgonDepthStencil CreateDepthStencil(string name, GorgonDepthStencilSettings settings)
		{
			GorgonDepthStencil depthBuffer = null;

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter must not be empty.", "name");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

			GorgonDepthStencil.ValidateSettings(_graphics, settings);

			depthBuffer = new GorgonDepthStencil(_graphics, name, settings);
			_graphics.AddTrackedObject(depthBuffer);
			depthBuffer.UpdateSettings();

			return depthBuffer;
		}

		/// <summary>
		/// Function to create a swap chain.
		/// </summary>
		/// <param name="name">Name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data or for the depth/stencil buffer.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is less than 0 or not less than the value returned by <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice.GetMultiSampleQuality</see>.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the current video device is a SM2_a_b video device and the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Flags">Flags</see> property is not equal to RenderTarget.</para>
		/// </exception>
		/// <remarks>This will create our output swap chains for display to a window or control.  All functionality for sending or retrieving data from the video device can be accessed through the swap chain.
		/// <para>Passing default settings for the <see cref="GorgonLibrary.Graphics.GorgonSwapChainSettings">settings parameters</see> will make Gorgon choose the closest possible settings appropriate for the video device and output that the window is on.  For example, passing NULL (Nothing in VB.Net) to 
		/// the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.VideoMode">GorgonSwapChainSettings.VideoMode</see> parameter will make Gorgon find the closest video mode available to the current window size and desktop format (for the output).</para>
		/// <para>If the multisampling quality in the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultiSample.Quality">GorgonSwapChainSettings.MultiSample.Quality</see> property is higher than what the video device can support, an exception will be raised.  To determine 
		/// what the maximum quality for the sample count for the video device should be, call the <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GorgonVideoDevice.GetMultiSampleQuality</see> method.</para>
		/// </remarks>
		public GorgonSwapChain CreateSwapChain(string name, GorgonSwapChainSettings settings)
		{
			GorgonSwapChain swapChain = null;

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter must not be empty.", "name");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

			GorgonSwapChain.ValidateSwapChainSettings(_graphics, settings);

			swapChain = new GorgonSwapChain(_graphics, name, settings);
			_graphics.AddTrackedObject(swapChain);
			swapChain.Initialize();

			return swapChain;
		}

		/// <summary>
		/// Function to create a render target.
		/// </summary>
		/// <param name="name">Name of the render target.</param>
		/// <param name="settings">Settings for the render target.</param>
		/// <returns>A new render target object.</returns>
		/// <remarks>This allows graphics data to be rendered on to a <see cref="GorgonLibrary.Graphics.GorgonTexture2D">texture</see>.
		/// <para>Unlike the <see cref="GorgonLibrary.Graphics.GorgonSwapChain">GorgonSwapChain</see> object (which is also a render target), no defaults will be set for the <paramref name="settings"/> except multisampling, and DepthFormat (defaults to Unknown).  
		/// </para>
		/// <para>If the multisampling quality in the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTarget.MultSample.Quality">GorgonRenderTarget.MultiSample.Quality</see> property is higher than what the video device can support, an exception will be raised.  To determine 
		/// what the maximum quality for the sample count for the video device should be, call the <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GorgonVideoDevice.GetMultiSampleQuality</see> method.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there is no <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">video device present on the graphics interface</see>.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTargetSettings.Width">Width</see> or <see cref="P:GorgonLibrary.Graphics.GorgonRenderTargetSettings.Width">Height</see> property is 0 or greater than the maximum size for a texture that a video device can support.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTargetSettings.Format">Format</see> property is unknown or is not a supported render target format.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is less than 0 or not less than the value returned by <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice.GetMultiSampleQuality</see>.</para>
		/// </exception>
		public GorgonRenderTarget CreateRenderTarget(string name, GorgonRenderTargetSettings settings)
		{
			GorgonRenderTarget target = null;

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter must not be empty.", "name");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

			GorgonRenderTarget.ValidateRenderTargetSettings(_graphics, settings);

			target = new GorgonRenderTarget(_graphics, name, settings);
			_graphics.AddTrackedObject(target);
			target.Initialize();

			return target;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonOutputMerger"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		internal GorgonOutputMerger(GorgonGraphics graphics)
		{
			_graphics = graphics;
			BlendingState = new GorgonBlendRenderState(_graphics);
			DepthStencilState = new GorgonDepthStencilRenderState(_graphics);
			RenderTargets = new RenderTargetList(_graphics);
		}
		#endregion
	}
}
