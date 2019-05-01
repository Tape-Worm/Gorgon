using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Timing;

namespace Gorgon.Examples
{
    /// <summary>
    /// Direction of cloaking effect.
    /// </summary>
    public enum CloakDirection
    {
        /// <summary>
        /// No cloaking.
        /// </summary>
        None = 0,
        /// <summary>
        /// Cloak the ship.
        /// </summary>
        Cloak = 1,
        /// <summary>
        /// Uncloak the ship.
        /// </summary>
        Uncloak = 2,
        /// <summary>
        /// Stop the pulse prior to uncloaking.
        /// </summary>
        UncloakStopPulse = 3,
        /// <summary>
        /// Pulse the cloak.
        /// </summary>
        CloakPulse = 4
    }


    /// <summary>
    /// Defines the states for cloaking the ship.
    /// </summary>
    public class CloakController
    {
        #region Variables.
        // The angle for the cloaking "pulse".
        private float _cloakPulseAngle = 180.0f;
        // The anggle of the cloaking pulse, in radians.
        private float _cloakAngleRads;
        // The base amount of cloak.
        private float _cloakAmount;
        // The direction to move in order to stop the pulse.
        private int _stopPulseDirection = 1;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the opacity.
        /// </summary>
        public float Opacity
        {
            get;
            private set;
        } = 1.0f;

        /// <summary>
        /// Property to return the amount of cloaking applied.
        /// </summary>
        public float CloakAmount
        {
            get
            {
                switch (Direction)
                {
                        case CloakDirection.Cloak:
                        case CloakDirection.Uncloak:
                            return _cloakAmount;
                        case CloakDirection.CloakPulse:
                        case CloakDirection.UncloakStopPulse:
                            return (_cloakAmount - ((_cloakAngleRads.Cos() + 1.0f) / 10.0f)).Min(0.25f).Max(0);
                        default:
                            return 0.0f;
                }
            }
        }

		/// <summary>
		/// Property to return the cloak direction.
		/// </summary>
		public CloakDirection Direction
        {
            get;
            private set;
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to initiate the cloak.
		/// </summary>
		public void Cloak()
        {
            _cloakPulseAngle = 180.0f;
            Direction = CloakDirection.Cloak;
        }

        /// <summary>
        /// Function to disable the cloak.
        /// </summary>
        public void Uncloak()
        {
            _stopPulseDirection = _cloakPulseAngle > 180.0f ? -1 : 1;
            Direction = CloakDirection.UncloakStopPulse;
        }

        /// <summary>
        /// Function to perform the updates to the cloak animation.
        /// </summary>
        public void Update()
        {
            switch (Direction)
            {
                case CloakDirection.Cloak:
                    Opacity -= 0.8f * GorgonTiming.Delta;
                    _cloakAmount += 0.12f * GorgonTiming.Delta;

                    _cloakAmount = _cloakAmount.Min(0.25f).Max(0);
                    Opacity = Opacity.Min(1.0f).Max(0);
                    
                    if ((_cloakAmount >= 0.25f) && (Opacity <= 0.0f))
                    {
                        Direction = CloakDirection.CloakPulse;
                        _cloakPulseAngle = 180.0f;
                        _cloakAngleRads = _cloakPulseAngle.ToRadians();
                    }
                    break;
                case CloakDirection.Uncloak:
                    if (_cloakAmount < 0.12f)
                    {
                        Opacity += 0.8f * GorgonTiming.Delta;
                    }

                    _cloakAmount -= 0.12f * GorgonTiming.Delta;

                    _cloakAmount = _cloakAmount.Min(0.25f).Max(0);
                    Opacity = Opacity.Min(1.0f).Max(0);

                    if ((_cloakAmount <= 0.0f) && (Opacity >= 1.0f))
                    {
                        Direction = CloakDirection.None;
                    }
                    break;
                case CloakDirection.UncloakStopPulse:

                    _cloakPulseAngle += (90.0f * GorgonTiming.Delta) * _stopPulseDirection;

                    if (((_cloakPulseAngle > 180.0f) && (_stopPulseDirection == 1))
                        || (_cloakPulseAngle < 180.0f) && (_stopPulseDirection == -1))
                    {
                        _cloakPulseAngle = 180.0f;
                        Direction = CloakDirection.Uncloak;
                    }

                    _cloakAngleRads = _cloakPulseAngle.ToRadians();
                    break;
                case CloakDirection.CloakPulse:
                    _cloakAngleRads = _cloakPulseAngle.ToRadians();
                    _cloakPulseAngle += GorgonRandom.RandomSingle(0.0f, 120.0f) * GorgonTiming.Delta;

                    if (_cloakPulseAngle > 360.0f)
                    {
                        _cloakPulseAngle -= 360.0f;
                    }
                    break;
            }
        }
        #endregion
    }
}
