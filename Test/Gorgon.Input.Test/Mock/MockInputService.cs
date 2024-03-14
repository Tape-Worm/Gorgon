using System.Collections.Generic;

namespace Gorgon.Input.Test.Mock
{
    class MockInputService
        : IGorgonInputService
    {
        #region IGorgonInputService Members

        public IReadOnlyList<IGorgonKeyboardInfo2> EnumerateKeyboards()
        {
            var devices = new List<IGorgonKeyboardInfo2>();

            IGorgonKeyboardInfo2 info = new MockKeyboardInfo();

            devices.Add(info);

            return devices;
        }

        public IReadOnlyList<IGorgonMouseInfo2> EnumerateMice()
        {
            return new List<IGorgonMouseInfo2>();
        }

        public IReadOnlyList<IGorgonJoystickInfo2> EnumerateJoysticks()
        {
            return new List<IGorgonJoystickInfo2>();
        }
        #endregion
    }
}
