using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics.Core;
using SharpDX.Direct3D11;
using BlendOperation = Gorgon.Graphics.Core.BlendOperation;
using LogicOperation = Gorgon.Graphics.Core.LogicOperation;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A base class for a 2D renderable object.
    /// </summary>
    public abstract class Gorgon2DRenderable
        : IEquatable<Gorgon2DRenderable>
    {
        #region Properties.
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
            protected set;
        }

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

            return ((Texture == other.Texture)
                    && (TextureSampler == other.TextureSampler));
        }
        #endregion

        #region Methods.

        #endregion
    }
}
