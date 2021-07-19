#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: May 21, 2019 12:29:13 PM
// 
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Gorgon.Animation;

namespace Gorgon.Examples
{
    /// <summary>
    /// A planet object.
    /// </summary>
    /// <remarks>
    /// This will represent a planetary body using 3D spheres as layers that are composited on top of one another.
    /// </remarks>
    internal class Planet
    {
        #region Variables.
        // Animation controller for the entity.
        private readonly List<MeshAnimationController> _animController;
        // The world position of this entity.
        private Vector3 _position;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the meshes used to render the layers of this planet.
        /// </summary>
        public IReadOnlyList<PlanetaryLayer> Layers
        {
            get;
        }

        /// <summary>
        /// Property to set or return the position of the planet on a 2D plane and the depth level.
        /// </summary>
        /// <remarks>
        /// This example uses an orthographic camera and mixes 3D and 2D elements. Since orthographic cameras are constant Z, having a depth value won't resize the planet, but it will still record the 
        /// object as having a depth value.
        /// </remarks>
        public ref Vector3 Position => ref _position;

        /// <summary>
        /// Property to set or return the rotation of the planet.
        /// </summary>
        public Vector3 Rotation
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the entity during a frame.
        /// </summary>
        public void Update()
        {
            float depth = _position.Z;

            for (int meshIndex = 0; meshIndex < Layers.Count; ++meshIndex)
            {
                MeshAnimationController controller = _animController[meshIndex];
                MoveableMesh mesh = Layers[meshIndex].Mesh;
                IGorgonAnimation animation = Layers[meshIndex].Animation;

                if (animation is null)
                {
                    mesh.Rotation = Rotation;
                }

                mesh.Position = new Vector3(_position.X, _position.Y, depth);
                mesh.Rotation = Rotation;

                if ((animation is not null) && (controller.State != AnimationState.Playing))
                {
                    controller.Play(mesh, animation);
                }

                controller.Update();
                // Increase the depth of the mesh so that we 
                depth -= 0.02f;
            }
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="Planet"/> class.</summary>
        /// <param name="layers">The layers for this planet.</param>
        public Planet(IEnumerable<PlanetaryLayer> layers)
        {
            Layers = layers.ToArray();
            _animController = new List<MeshAnimationController>();

            for (int i = 0; i < Layers.Count; ++i)
            {
                _animController.Add(new MeshAnimationController());
            }
        }
        #endregion
    }
}
