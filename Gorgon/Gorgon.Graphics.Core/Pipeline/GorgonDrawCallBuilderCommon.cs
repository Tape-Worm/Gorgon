#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: May 23, 2018 12:18:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D = SharpDX.Direct3D;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Math;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Common functionality for the a draw call fluent builder.
    /// </summary>
    /// <typeparam name="TB">The type of builder.</typeparam>
    /// <typeparam name="TDc">The type of draw call.</typeparam>
    public abstract class GorgonDrawCallBuilderCommon<TB, TDc>
        where TB : GorgonDrawCallBuilderCommon<TB, TDc>
        where TDc : GorgonDrawCallCommon, new()
    {
        #region Variables.
        // The final call object to return.
        private TDc _finalCall;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the draw call being edited.
        /// </summary>
        protected TDc WorkingDrawCall
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the properties of the draw call from the working copy to the final copy.
        /// </summary>
        /// <param name="finalCopy">The object representing the finalized copy.</param>
        /// <returns></returns>
        protected abstract void Update(TDc finalCopy);

        /// <summary>
        /// Function to set primitive topology for the draw call.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public TB PrimitiveTopology(D3D.PrimitiveTopology topology)
        {
            WorkingDrawCall.PrimitiveTopology = topology;
            return (TB)this;
        }

        /// <summary>
        /// Function to set the vertex buffer bindings for the draw call.
        /// </summary>
        /// <param name="layout">The input layout to use.</param>
        /// <param name="bindings">The vertex buffer bindings to set.</param>
        /// <returns>The fluent builder interface.</returns>
        public TB VertexBufferBindings(GorgonInputLayout layout, params GorgonVertexBufferBinding[] bindings)
        {
            WorkingDrawCall.UpdateVertexBufferBindings(layout, bindings);
            return (TB)this;
        }

        /// <summary>
        /// Function to return the draw call.
        /// </summary>
        /// <returns>The draw call created or updated by this builder.</returns>
        public TDc Build()
        {
            if (_finalCall == null)
            {
                _finalCall = new TDc();
            }

            _finalCall.UpdateVertexBufferBindings(WorkingDrawCall.InputLayout, WorkingDrawCall.VertexBufferBindings);
            _finalCall.PrimitiveTopology = WorkingDrawCall.PrimitiveTopology;

            Update(_finalCall);

            return _finalCall;
        }

        /// <summary>
        /// Function to reset the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public virtual TB Reset()
        {
            WorkingDrawCall.UpdateVertexBufferBindings(null, null);
            WorkingDrawCall.PrimitiveTopology = D3D.PrimitiveTopology.TriangleList;
            return (TB)this;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawCallBuilder"/> class.
        /// </summary>
        /// <param name="callToUpdate">[Optional] A previous draw call to update.</param>
        protected GorgonDrawCallBuilderCommon(TDc callToUpdate = null)
        {
            WorkingDrawCall = new TDc
                              {
                                  PrimitiveTopology = D3D.PrimitiveTopology.TriangleList
                              };

            _finalCall = callToUpdate;
        }
        #endregion
    }
}
