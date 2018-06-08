using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Defines the state to pass to a call to the <see cref="Gorgon2D.Begin"/> method.
    /// </summary>
    public sealed class Gorgon2DBatchState
    {
        #region Properties.
        /// <summary>
        /// Property to return the current blending state to apply.
        /// </summary>
        public GorgonBlendState BlendState
        {
            get;
            internal set;
        } = GorgonBlendState.Default;

        /// <summary>
        /// Property to return the current raster state to apply.
        /// </summary>
        public GorgonRasterState RasterState
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the current depth/stencil state to apply.
        /// </summary>
        public GorgonDepthStencilState DepthStencilState
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the current pixel shader to use.
        /// </summary>
        public GorgonPixelShader PixelShader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the current vertex shader to use.
        /// </summary>
        public GorgonVertexShader VertexShader
        {
            get;
            internal set;
        }
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Finalizer.

        #endregion

    }
}
