namespace Gorgon.Input.Test.Mock
{
    class MockKeyboardInfo
        : IGorgonKeyboardInfo2
    {
        #region IGorgonKeyboardInfo2 Members

        public int KeyCount => 104;

        public int IndicatorCount => 3;

        public int FunctionKeyCount => 12;

        public KeyboardType KeyboardType => KeyboardType.USB;

        #endregion

        #region IGorgonInputDeviceInfo2 Members

        public string Description => "Mock 104 key keyboard.";

        public string HumanInterfaceDevicePath => "USB.Keyboard.1";

        public string ClassName => "Keyboard";

        public InputDeviceType InputDeviceType => InputDeviceType.Keyboard;

        #endregion
    }
}
