using System;

namespace KRBTabControl
{
    public interface ICaptionRandomizer : IDisposable
    {
        /// <summary>
        /// Determines whether the randomizer effect is enable or not for tab control caption.
        /// </summary>
        bool IsRandomizerEnabled { get; set; }

        /// <summary>
        /// Determines whether the transparency effect is visible or not for tab control caption.
        /// </summary>
        bool IsTransparencyEnabled { get; set; }

        /// <summary>
        /// Gets or Sets, the red color component value of the caption bitmap.
        /// </summary>
        byte Red { get; set; }

        /// <summary>
        /// Gets or Sets, the green color component value of the caption bitmap.
        /// </summary>
        byte Green { get; set; }

        /// <summary>
        /// Gets or Sets, the blue color component value of the caption bitmap.
        /// </summary>
        byte Blue { get; set; }

        /// <summary>
        /// Gets or Sets, the alpha color component value of the caption bitmap.
        /// </summary>
        byte Transparency { get; set; }
    }
}