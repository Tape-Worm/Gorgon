using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A track that will animate properties with a Vector2 data type.
	/// </summary>
	class GorgonTrackVector2
		: GorgonAnimationTrack
	{
		#region Variables.
		private static Func<IAnimated, Vector2> _getProperty = null;			// Get property method.
		private static Action<IAnimated, Vector2> _setProperty = null;			// Set property method.
		#endregion

		#region Properties.
		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the property value assigned to the track.
		/// </summary>
		/// <param name="keyValues">Values to use when updating.</param>
		/// <param name="key">The key to work on.</param>
		/// <param name="time">Time to reference, in milliseconds.</param>
		protected override void GetTweenKey(ref GorgonAnimationTrack.NearestKeys keyValues, out IKeyFrame key, float time)
		{
			// Just use the previous key if we're at 0.
			if (time == 0)
			{
				key = keyValues.PreviousKey;
				return;
			}

			GorgonKeyVector2 next = (GorgonKeyVector2)keyValues.NextKey;
			GorgonKeyVector2 prev = (GorgonKeyVector2)keyValues.PreviousKey;
            key = new GorgonKeyVector2(time, Vector2.Lerp(prev.Value, next.Value, time));
		}

        /// <summary>
        /// Function to apply the key value to the object properties.
        /// </summary>
        /// <param name="key">Key to apply to the properties.</param>
        protected internal override void ApplyKey(ref IKeyFrame key)
        {
            GorgonKeyVector2 value = (GorgonKeyVector2)key;
            _setProperty(Animation.Owner, value.Value);
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTrackVector2" /> class.
		/// </summary>
		/// <param name="animation">The animation that owns this track.</param>
		/// <param name="property">Property information.</param>
		internal GorgonTrackVector2(GorgonAnimationClip animation, GorgonAnimationCollection.AnimatedProperty property)
			: base(animation, property)
		{
			if (_getProperty == null)
				_getProperty = BuildGetAccessor<Vector2>();
			if (_setProperty == null)
				_setProperty = BuildSetAccessor<Vector2>();
		}
		#endregion
	}
}
