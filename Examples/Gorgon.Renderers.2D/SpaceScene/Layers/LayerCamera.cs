using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Timing;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// The camera controller for the layers.
    /// </summary>
    /// <remarks>
    /// This updates the view point of the user by offsetting all the layers by a specified amount.
    /// </remarks>
    internal class LayerCamera
    {
        #region Variables.
        // The layer to track.
        private Layer _trackingLayer;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of layers to control.
        /// </summary>
        public IReadOnlyList<Layer> Layers
        {
            get;
        }

        /// <summary>
        /// Property to set or return the layer that will be tracked by the controller.
        /// </summary>
        /// <remarks>
        /// The tracked layer will not have its offset updated.
        /// </remarks>
        public Layer TrackingLayer
        {
            get => _trackingLayer;
            set
            {
                _trackingLayer = value;

                if (_trackingLayer != null)
                {
                    _trackingLayer.Offset = DX.Vector2.Zero;
                }
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to set the position of all unlocked layers.
        /// </summary>
        /// <param name="position">The position to assign to the layers.</param>
        public void SetPosition(DX.Vector2 position)
        {
            TrackingLayer?.Update();

            for (int i = 0; i < Layers.Count; ++i)
            {
                Layer layer = Layers[i];
                if (layer == TrackingLayer)
                {
                    continue;
                }

                layer.Offset = position;
                layer.Update();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="LayerCamera"/> class.</summary>
        /// <param name="layers">The layers used by the application.</param>
        public LayerCamera(IEnumerable<Layer> layers) => Layers = layers.ToArray();
        #endregion
    }
}
