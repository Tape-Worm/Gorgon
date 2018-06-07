using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A base class for a 2D renderable object.
    /// </summary>
    public class Gorgon2DRenderable
        : IEquatable<Gorgon2DRenderable>
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the draw call for this renderable.
        /// </summary>
        internal GorgonDrawIndexCall AssociatedDrawCall
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the blend state for this renderable.
        /// </summary>
        public GorgonBlendState BlendState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the depth/stencil state for this renderable.
        /// </summary>
        public GorgonDepthStencilState DepthStencilState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the texture to render.
        /// </summary>
        public GorgonTexture2DView Texture
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the texture sampler to use when rendering.
        /// </summary>
        public GorgonSamplerState TextureSampler
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the vertices for this renderable.
        /// </summary>
        public Gorgon2DVertex[] Vertices
        {
            get;
        } = new Gorgon2DVertex[4];

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(Gorgon2DRenderable other)
        {
            if (other == null)
            {
                return false;
            }

            if (other == this)
            {
                return true;
            }

            return ((BlendState == other.BlendState)
                    && (DepthStencilState == other.DepthStencilState)
                    && (Texture == other.Texture)
                    && (TextureSampler == other.TextureSampler));
        }
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
