using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.ConsoleApp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Tests
{
    public abstract class BaseTest
    {
        private static bool _isFirstCall = true;
        private static ServiceProvider _serviceProvider;
        private IServiceScope _scope;
        protected Storage Storage { get; private set; }

        [TestInitialize]
        public async Task BaseInitialize()
        {
            Storage = new Storage();
        }

        [TestCleanup]
        public void BaseCleanup()
        {
            _scope?.Dispose();
        }
    }
}
