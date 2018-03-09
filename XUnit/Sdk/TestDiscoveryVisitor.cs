using System;
using System.Collections.Concurrent;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    class TestDiscoveryVisitor : IMessageSink, IDisposable
    {
        private bool _disposed = false;

        public TestDiscoveryVisitor()
        {
            Finished = new ManualResetEvent(initialState: false);
            TestCases = new ConcurrentQueue<ITestCase>();
        }

        public ManualResetEvent Finished { get; }

        public ConcurrentQueue<ITestCase> TestCases { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Finished.Dispose();
            _disposed = true;
        }

        /// <inheritdoc/>
        public bool OnMessage(IMessageSinkMessage message)
        {
            var discoveryMessage = message as ITestCaseDiscoveryMessage;
            if (discoveryMessage != null)
                TestCases.Enqueue(discoveryMessage.TestCase);

            if (!_disposed && message is IDiscoveryCompleteMessage)
                Finished.Set();

            return true;
        }
    }
}
