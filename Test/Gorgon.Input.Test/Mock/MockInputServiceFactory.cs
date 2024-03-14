using System.Collections.Generic;
using System.Reflection;

namespace Gorgon.Input.Test.Mock
{
    class MockInputServiceFactory
        : IGorgonInputServiceFactory
    {

        #region IGorgonInputServiceFactory Members

        public IGorgonInputService CreateService(string servicePluginName)
        {
            return new MockInputService();
        }

        public IEnumerable<IGorgonInputService> CreateServices(AssemblyName pluginAssembly = null)
        {
            return new[]
                   {
                       new MockInputService(),
                       new MockInputService()
                   };
        }

        #endregion
    }
}
