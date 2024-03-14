using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gorgon.Input.Test.Mock
{
    class MockKeyboard
        : IGorgonKeyboard
    {
        private Form _parentForm;

        #region IGorgonKeyboard Members

        public IGorgonInputService InputService
        {
            get;
            private set;        
        }

        public IGorgonKeyboardInfo2 Info
        {
            get;
        }
    

        public MockKeyboard(IGorgonInputService service, IGorgonKeyboardInfo2 keyboard)
        {
            Info = keyboard;
            InputService = service;
        }

        #region IGorgonInputDevice Members

        public Guid UUID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsPolled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Control Window
        {
            get;
            private set;
        }

        public bool IsAcquired
        {
            get;
            set;
        }

        public bool IsExclusive
        {
            get;
            private set;
        }

        private void OnBindWindow(Control window, bool exclusive)
        {
            _parentForm = window.FindForm();

            Assert.IsNotNull(_parentForm);

            _parentForm.Deactivate += ParentForm_Deactivate;    
            window.LostFocus += ParentForm_Deactivate;
            window.EnabledChanged += Window_EnabledChanged;
        }

        private void Window_EnabledChanged(object sender, EventArgs eventArgs)
        {
            if ((!IsAcquired) || (Window == null) || (Window.Enabled))
            {
                return;
            }

            IsAcquired = false;
        }

        private void ParentForm_Deactivate(object sender, EventArgs eventArgs)
        {
            if (!IsAcquired)
            {
                return;
            }

            IsAcquired = false;
        }

        public void BindWindow(Control window, bool exclusive = false)
        {
            if (window != Window)
            {
                UnbindWindow();    
            }

            OnBindWindow(window, exclusive);

            IsExclusive = exclusive;
            Window = window;
        }

        public void UnbindWindow()
        {
            if (Window == null)
            {
                return;
            }

            _parentForm.Deactivate -= ParentForm_Deactivate;
            Window.LostFocus -= ParentForm_Deactivate;
            Window.EnabledChanged += Window_EnabledChanged;

            IsAcquired = false;
            IsExclusive = false;
            Window = null;
        }
    

        #region IGorgonKeyboard Members

        public event EventHandler<GorgonKeyboardEventArgs2> KeyDown;

        public event EventHandler<GorgonKeyboardEventArgs2> KeyUp;

        public string KeyToCharacter(Keys key, Keys modifier)
        {
            throw new NotImplementedException();
        }

    
    }
}
