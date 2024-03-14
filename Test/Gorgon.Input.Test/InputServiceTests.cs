using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Input.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gorgon.Input.Test
{
    [TestClass]
    public class InputServiceTests
    {
        [TestMethod]
        public void KeyboardCreateTest()
        {
            IGorgonInputServiceFactory serviceFactory = new MockInputServiceFactory();
            IGorgonInputService service = serviceFactory.CreateService("My.Input.Service");
            IReadOnlyList<IGorgonKeyboardInfo2> keyboards = service.EnumerateKeyboards();
            IGorgonKeyboard keyboard;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (MockControl control = new MockControl())
            {
                keyboard = new MockKeyboard(service, keyboards[0]);
                keyboard.BindWindow(control);

                control.Show();
                control.FormClosing += (sender, args) =>
                                       {
                                           keyboard.UnbindWindow();
                                       };

                keyboard.IsAcquired = true;

                Application.Run(control);
            }
        }

        [TestMethod]
        public void ServiceEnumDevicesTest()
        {
            IGorgonInputServiceFactory serviceFactory = new MockInputServiceFactory();
            IGorgonInputService service = serviceFactory.CreateService("My.Input.Service");
            IReadOnlyList<IGorgonKeyboardInfo2> keyboards = service.EnumerateKeyboards();
            IReadOnlyList<IGorgonMouseInfo2> mice = service.EnumerateMice();
            IReadOnlyList<IGorgonJoystickInfo2> joysticks = service.EnumerateJoysticks();

            Assert.IsNotNull(keyboards);
            Assert.IsNotNull(mice);
            Assert.IsNotNull(joysticks);

            Assert.IsTrue(keyboards.Any());
            Assert.IsTrue(mice.Any());
            Assert.IsTrue(joysticks.Any());
        }

        [TestMethod]
        public void InputServiceFactoryTest()
        {
            IGorgonInputServiceFactory serviceFactory = new MockInputServiceFactory();
            IGorgonInputService service = serviceFactory.CreateService("My.Input.Service");

            Assert.IsNotNull(service);

            IEnumerable<IGorgonInputService> services = serviceFactory.CreateServices();

            Assert.IsNotNull(services);
            Assert.IsTrue(services.Any());
        }
    }
}
