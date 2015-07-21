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
// Created: Monday, June 17, 2013 8:26:51 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Math;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Compute shader states.
	/// </summary>
	public class GorgonComputeShaderState
        : GorgonShaderState<GorgonComputeShader>
    {
        #region Classes.
        /// <summary>
        /// A list of unordered access views.
        /// </summary>
        public sealed class ShaderUnorderedAccessViews
            : IList<GorgonUnorderedAccessView>
        {
            #region Variables.
            private readonly GorgonUnorderedAccessView[] _unorderedViews;
            private readonly GorgonComputeShaderState _shader;
            #endregion

            #region Properties.
            /// <summary>
            /// Property to return the number of buffers.
            /// </summary>
            public int Count => _unorderedViews.Length;

	        /// <summary>
            /// Property to set or return a constant buffer at the specified index.
            /// </summary>
            /// <exception cref="GorgonException">Thrown when an unordered access view or its resource is already bound to another slot.</exception>
            /// <remarks>When binding an unordered access view, ensure that it is not already bound or that the resource the view is attached to is not bound elsewhere in pipeline or on the same 
            /// stage of the pipeline.</remarks>
            public GorgonUnorderedAccessView this[int index]
            {
                get
                {
                    return _unorderedViews[index];
                }
                set
                {
                    if (_unorderedViews[index] == value)
                    {
                        return;
                    }

                    var D3DView = new[] { value == null ? null : value.D3DView };
                    var initialCounts = new[]
                        {
                            -1
                        };
#if DEBUG
					ValidateBinding(value);
#endif

                    _unorderedViews[index] = value;

                    if (value != null)
                    {
                        var structuredBuffer = value as GorgonStructuredBufferUnorderedAccessView;

                        if (structuredBuffer != null)
                        {
                            initialCounts[index] = structuredBuffer.InitialCount;
                        }
                    }

                    _shader.Graphics.Context.ComputeShader.SetUnorderedAccessViews(index, D3DView, initialCounts);
                }
            }
            #endregion

            #region Methods.
#if DEBUG
			/// <summary>
			/// Function to validate the unordered access view binding.
			/// </summary>
			/// <param name="view">View to validate.</param>
			private void ValidateBinding(GorgonUnorderedAccessView view)
			{
				if (view == null)
				{
					return;
				}

				if (IndexOf(view) != -1)
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(Properties.Resources.GORGFX_VIEW_ALREADY_BOUND, IndexOf(view)));
				}

				for (int i = 0; i < Count; i++)
				{
					if ((this[i] != null) && (this[i].Resource == view.Resource))
					{
						throw new GorgonException(GorgonResult.CannotBind,
						                          string.Format(Properties.Resources.GORGFX_VIEW_RESOURCE_ALREADY_BOUND,
						                                        view.Resource.Name,
						                                        i,
						                                        typeof(GorgonRenderTargetView).FullName));
					}
				}

				var outputViews = _shader.Graphics.Output.GetUnorderedAccessViews();

				if (Array.IndexOf(outputViews, view) != -1)
				{
					throw new GorgonException(GorgonResult.CannotBind, Properties.Resources.GORGFX_VIEW_ALREADY_BOUND);
				}
			}
#endif
            /// <summary>
            /// Function to reset the state of the unordered access views.
            /// </summary>
            internal void Reset()
            {
                var views = new D3D.UnorderedAccessView[_unorderedViews.Length];

                for (int i = 0; i < _unorderedViews.Length; i++)
                {
                    views[i] = null;
                    _unorderedViews[i] = null;
                }

                _shader.Graphics.Context.ComputeShader.SetUnorderedAccessViews(0, views);
            }

            /// <summary>
            /// Function to unbind an unordered access view.
            /// </summary>
            /// <param name="view">View to unbind.</param>
            internal void Unbind(GorgonUnorderedAccessView view)
            {
                for (int i = 0; i < _unorderedViews.Length; i++)
                {
                    if (_unorderedViews[i] != view)
                    {
                        continue;
                    }

                    this[i] = null;
                }
            }

            /// <summary>
            /// Function unbind all views with the specified resource.
            /// </summary>
            /// <param name="resource">Resource to unbind.</param>
            internal void UnbindResource(GorgonResource resource)
            {
                if (resource == null)
                {
                    return;
                }

                var indices =
                    _unorderedViews.Where(item => item != null && item.Resource == resource)
                              .Select(IndexOf)
                              .Where(index => index != -1);

                foreach (var index in indices)
                {
                    this[index] = null;
                }
            }

            /// <summary>
            /// Function to return the resource assigned to a view at the specified index.
            /// </summary>
            /// <typeparam name="TR">Type of resource.</typeparam>
            /// <param name="index">Index of the resource to look up.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is outside of the available resource view slots.</exception>
            /// <exception cref="System.InvalidCastException">Thrown when the type of resource at the specified index is not the requested type.</exception>
            /// <returns>The resource assigned to the view at the specified index, or NULL if nothing is assigned to the specified index.</returns>
            public TR GetResource<TR>(int index)
                where TR : GorgonResource
            {
				index.ValidateRange("index", 0, _unorderedViews.Length);

                var resourceView = _unorderedViews[index];

#if DEBUG
                if ((resourceView != null) && (resourceView.Resource != null) && (!(resourceView.Resource is TR)))
                {
                    throw new InvalidCastException(string.Format(Properties.Resources.GORGFX_VIEW_RESOURCE_NOT_TYPE, index,
                                                                 typeof(TR).FullName));
                }
#endif

                if (resourceView == null)
                {
                    return null;
                }

                return (TR)resourceView.Resource;
            }

            /// <summary>
            /// Function to return the index of the resource with the specified name.
            /// </summary>
            /// <param name="name">Name of the resource.</param>
            /// <returns>The index of the resource if found, -1 if not.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
            /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
            /// <remarks>This only looks up resources bound to a view that's bound to the pipeline.</remarks>
            public int IndexOf(string name)
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(Properties.Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
                }

                for (int i = 0; i < _unorderedViews.Length; i++)
                {
                    var resource = _unorderedViews[i];

                    if ((resource != null)
                        && (resource.Resource != null)
                        && (string.Equals(name, resource.Resource.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return i;
                    }
                }

                return -1;
            }

            /// <summary>
            /// Function to determine if the list contains a resource with the specified name.
            /// </summary>
            /// <param name="name">Name of the resource.</param>
            /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
            /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
            /// <remarks>This only looks up resources bound to a view that's bound to a pipeline.</remarks>
            public bool Contains(string name)
            {
                return IndexOf(name) > -1;
            }

            /// <summary>
            /// Function to return the index of an unordered access view bound to a particular resource.
            /// </summary>
            /// <param name="resource">Resource to look up.</param>
            /// <returns>The index of the resource if found, -1 if not.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
            public int IndexOf(GorgonResource resource)
            {
                if (resource == null)
                {
                    throw new ArgumentNullException(nameof(resource));
                }

                for (int i = 0; i < _unorderedViews.Length; i++)
                {
                    var view = _unorderedViews[i];

                    if ((view != null)
                        && (view.Resource == resource))
                    {
                        return i;
                    }
                }

                return -1;
            }

            /// <summary>
            /// Function to determine if a view is bound and is also bound to the specified resource.
            /// </summary>
            /// <param name="resource">Resource to look up.</param>
            /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
            public bool Contains(GorgonResource resource)
            {
                return IndexOf(resource) > -1;
            }

            /// <summary>
            /// Function to set a range of unordered access views all at once.
            /// </summary>
            /// <param name="slot">Starting slot for the buffer.</param>
            /// <param name="views">Buffers to set.</param>
            /// <remarks>This will bind unordered access views at the same time.  An unordered access view or its resource must not already be bound to the shader at another index, or an exception will be thrown.
            /// <para>Passing NULL (<i>Nothing</i> in VB.Net) to the <paramref name="views"/> parameter will set the bindings to empty (starting at <paramref name="slot"/>).</para>
            /// </remarks>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> is less than 0, or greater than the available number of resource view slots.</exception>
            /// <exception cref="GorgonException">Thrown when one of the views in the <paramref name="views"/> parameter is already bound to another slot or has a resource bound to another slot.</exception>
            public void SetRange(int slot, GorgonUnorderedAccessView[] views)
            {
                int count = _unorderedViews.Length - slot;

				slot.ValidateRange("slot", 0, _unorderedViews.Length);

                if (views != null)
                {
                    count = views.Length.Min(_unorderedViews.Length);
                }

                var D3DViews = new D3D.UnorderedAccessView[count];
                var initialCounts = new int[count];

                for (int i = 0; i < count; i++)
                {
                    GorgonUnorderedAccessView buffer = null;
                    var bufferIndex = i + slot;
                    
                    if (views != null)
                    {
                        buffer = views[i];
                    }

#if DEBUG
					ValidateBinding(buffer);
#endif

                    _unorderedViews[bufferIndex] = buffer;
                    D3DViews[i] = buffer != null ? buffer.D3DView : null;
                    initialCounts[i] = -1;

                    if (buffer == null)
                    {
                        continue;
                    }

                    var structView = buffer as GorgonStructuredBufferUnorderedAccessView;

                    if (structView != null)
                    {
                        initialCounts[i] = structView.InitialCount;
                    }
                }

                _shader.Graphics.Context.ComputeShader.SetUnorderedAccessViews(slot, D3DViews, initialCounts);
            }
            #endregion

            #region Constructor/Destructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="ShaderUnorderedAccessViews"/> class.
            /// </summary>
            /// <param name="shader">Shader stage state.</param>
            internal ShaderUnorderedAccessViews(GorgonComputeShaderState shader)
            {
                _unorderedViews = new GorgonUnorderedAccessView[D3D.ComputeShaderStage.UnorderedAccessViewSlotCount];
                _shader = shader;
            }
            #endregion

            #region IList<GorgonUnorderedAccessView> Members
            /// <summary>
            /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
            /// <returns>
            /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="item"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
            public int IndexOf(GorgonUnorderedAccessView item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                return Array.IndexOf(_unorderedViews, item);
            }

            /// <summary>
            /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
            /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
            /// <exception cref="System.NotSupportedException">This method is not used.</exception>
            void IList<GorgonUnorderedAccessView>.Insert(int index, GorgonUnorderedAccessView item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the item to remove.</param>
            /// <exception cref="System.NotSupportedException">This method is not used.</exception>
            void IList<GorgonUnorderedAccessView>.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }
            #endregion

            #region ICollection<GorgonUnorderedAccessView> Members
            #region Properties.
            /// <summary>
            /// Property to return whether this list is read only or not.
            /// </summary>
            public bool IsReadOnly => false;

	        #endregion

            #region Methods.
            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <exception cref="System.NotSupportedException">This method is not used.</exception>
            void ICollection<GorgonUnorderedAccessView>.Add(GorgonUnorderedAccessView item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <exception cref="System.NotSupportedException">This method is not used.</exception>
            void ICollection<GorgonUnorderedAccessView>.Clear()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="item" /> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
            public bool Contains(GorgonUnorderedAccessView item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                return _unorderedViews.Contains(item);
            }

            /// <summary>
            /// Function to copy the contents of this list to an array.
            /// </summary>
            /// <param name="array">Array to copy into.</param>
            /// <param name="arrayIndex">Index in the array to start writing at.</param>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="array"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/> parameter is less than 0 or not less than the length of the array.</exception>
            public void CopyTo(GorgonUnorderedAccessView[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if ((arrayIndex < 0) || (arrayIndex >= array.Length))
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                }

                int count = (array.Length - arrayIndex).Min(_unorderedViews.Length);

                for (int i = 0; i < count; i++)
                {
                    array[i + arrayIndex] = _unorderedViews[i];
                }
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </returns>
            /// <exception cref="System.NotSupportedException">This method is not used.</exception>
            bool ICollection<GorgonUnorderedAccessView>.Remove(GorgonUnorderedAccessView item)
            {
                throw new NotSupportedException();
            }
            #endregion
            #endregion

            #region IEnumerable<GorgonUnorderedAccessView> Members
            /// <summary>
            /// Function to return an enumerator for the list.
            /// </summary>
            /// <returns>The enumerator for the list.</returns>
            public IEnumerator<GorgonUnorderedAccessView> GetEnumerator()
            {
                // ReSharper disable LoopCanBeConvertedToQuery
                // ReSharper disable ForCanBeConvertedToForeach
                for (int i = 0; i < _unorderedViews.Length; i++)
                {
                    yield return _unorderedViews[i];
                }
                // ReSharper restore ForCanBeConvertedToForeach
                // ReSharper restore LoopCanBeConvertedToQuery
            }

            #endregion

            #region IEnumerable Members
            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _unorderedViews.GetEnumerator();
            }
            #endregion
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of unordered access views bound to the compute shader.
        /// </summary>
        public ShaderUnorderedAccessViews UnorderedAccessViews
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
		/// Property to set or return the current shader.
		/// </summary>
		protected override void SetCurrent()
        {
	        Graphics.Context.ComputeShader.Set(Current == null ? null : Current.D3DShader);
        }

		/// <summary>
		/// Function to set resources for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="resources">Resources to update.</param>
		protected override void SetResources(int slot, int count, D3D.ShaderResourceView[] resources)
		{
            if (count == 1)
		    {
		        Graphics.Context.ComputeShader.SetShaderResource(slot, resources[0]);
		    }
		    else
		    {
		        Graphics.Context.ComputeShader.SetShaderResources(slot, count, resources);
		    }
		}

		/// <summary>
		/// Function to set the texture samplers for a shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="samplers">Samplers to update.</param>
		protected override void SetSamplers(int slot, int count, D3D.SamplerState[] samplers)
		{
            if (count == 1)
		    {
		        Graphics.Context.ComputeShader.SetSampler(slot, samplers[0]);
		    }
		    else
		    {
		        Graphics.Context.ComputeShader.SetSamplers(slot, count, samplers);
		    }
		}

		/// <summary>
		/// Function to set constant buffers for the shader.
		/// </summary>
		/// <param name="slot">Slot to start at.</param>
		/// <param name="count"></param>
		/// <param name="buffers">Constant buffers to update.</param>
		protected override void SetConstantBuffers(int slot, int count, D3D.Buffer[] buffers)
		{
            if (count == 1)
		    {
		        Graphics.Context.ComputeShader.SetConstantBuffer(slot, buffers[0]);
		    }
		    else
		    {
		        Graphics.Context.ComputeShader.SetConstantBuffers(slot, count, buffers);
		    }
		}

        /// <summary>
        /// Function to reset the compute shader state.
        /// </summary>
        internal override void Reset()
        {
            base.Reset();
            UnorderedAccessViews.Reset();
        }

        /// <summary>
        /// Function to execute the current compute shader.
        /// </summary>
        /// <param name="threadCountX">Number of threads on the X group.</param>
        /// <param name="threadCountY">Number of threads on the Y group.</param>
        /// <param name="threadCountZ">Number of threads on the Z group.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="threadCountX"/>, <paramref name="threadCountY"/> or the <paramref name="threadCountZ"/> parameters are less than 
        /// 0 or greater than 65535.</exception>
        /// <remarks>Call dispatch to execute the commands in the compute shader.  A compute shader can be run on multiple threads in parallel within a threading group.  Use a particular threading group 
        /// within the shader by using a 3D vector (float3).</remarks>
	    public void Dispatch(int threadCountX, int threadCountY, int threadCountZ)
        {
			threadCountX.ValidateRange("threadCountX", 0, 65535);
			threadCountY.ValidateRange("threadCountY", 0, 65535);
			threadCountZ.ValidateRange("threadCountZ", 0, 65535);

            Graphics.Context.Dispatch(threadCountX, threadCountY, threadCountZ);
        }

        /// <summary>
        /// Function to execute the current compute shader using an indirect argument buffer.
        /// </summary>
        /// <param name="indirectArgsBuffer">The indirect argument buffer to use.</param>
        /// <param name="alignedOffset">The byte aligned offset into the buffer to start at.</param>
        /// <remarks>Call dispatch to execute the commands in the compute shader.  A compute shader can be run on multiple threads in parallel within a threading group.  Use a particular threading group 
        /// within the shader by using a 3D vector (float3).
        /// <para>The <paramref name="indirectArgsBuffer"/> must be loaded with data that matches the argument list of <see cref="Dispatch(int, int, int)"/>.</para></remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="indirectArgsBuffer"/> was NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="alignedOffset"/> parameter is less than 0 or not less than the number of bytes in the buffer.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the buffer passed in through <paramref name="indirectArgsBuffer"/> was not created as an indirect argument buffer.</exception>
        public void Dispatch(GorgonBuffer indirectArgsBuffer, int alignedOffset)
        {
			indirectArgsBuffer.ValidateObject("indirectArgsBuffer");
            alignedOffset.ValidateRange("alignedOffset", 0, indirectArgsBuffer.SizeInBytes);

#if DEBUG
            if (!indirectArgsBuffer.Settings.AllowIndirectArguments)
            {
                throw new ArgumentException(Properties.Resources.GORGFX_BUFFER_NOT_INDIRECT, nameof(indirectArgsBuffer));
            }
#endif
            Graphics.Context.DispatchIndirect(indirectArgsBuffer.D3DBuffer, alignedOffset);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonComputeShaderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
        protected internal GorgonComputeShaderState(GorgonGraphics graphics)
			: base(graphics)
		{
            UnorderedAccessViews = new ShaderUnorderedAccessViews(this);
		}
		#endregion
	}
}