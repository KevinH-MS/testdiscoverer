using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.UnitTesting.SourceBasedTestDiscovery;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    class XunitProtoTestDiscoverer : ITestDiscoverer
    {
        private class DummySourceInformationProvider : ISourceInformationProvider
        {
            public static readonly DummySourceInformationProvider Instance = new DummySourceInformationProvider();

            void IDisposable.Dispose()
            {
            }

            ISourceInformation ISourceInformationProvider.GetSourceInformation(ITestCase testCase)
            {
                throw new NotImplementedException();
            }
        }

        private void TrySendDiscoveredTestCases(TestDiscoveryContext context, TestDiscoveryVisitor discoverySink)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            while (discoverySink.TestCases.TryDequeue(out var test))
            {
                var testCase = new TestCase(
                    $"{test.TestMethod.TestClass.Class.Name}.{test.TestMethod.Method.Name}",
                    ExecutorUri,
                    context.Source)
                {
                    DisplayName = test.DisplayName,
                    CodeFilePath = test.SourceInformation?.FileName,
                    LineNumber = test.SourceInformation?.LineNumber ?? 0
                };

                var traits = test.Traits;
                foreach (var key in traits.Keys)
                {
                    foreach (var value in traits[key])
                    {
                        testCase.Traits.Add(new Trait(key, value));
                    }
                }

                context.ReportDiscoveredTest(testCase);
            }
        }

        Task ITestDiscoverer.AnalyzeDocumentAsync(DocumentTestDiscoveryContext context)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(context.CancellationToken);
            }

            using (var discoverySink = new TestDiscoveryVisitor())
            using (var xunit2 = new Xunit2Discoverer(
                AppDomainSupport.Denied,
                sourceInformationProvider: DummySourceInformationProvider.Instance,
                assemblyInfo: new SourceAssemblyInfo(context),
                xunitExecutionAssemblyPath: XunitExecutionAssemblyPath.Value,
                verifyAssembliesOnDisk: false))
            {
                xunit2.Find(includeSourceInformation: false, messageSink: discoverySink, discoveryOptions: TestFrameworkOptions.ForDiscovery());

                while (!discoverySink.Finished.WaitOne(50))
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    TrySendDiscoveredTestCases(context, discoverySink);
                }

                TrySendDiscoveredTestCases(context, discoverySink);
            }

            return context.CancellationToken.IsCancellationRequested ?
                Task.FromCanceled(context.CancellationToken) :
                Task.CompletedTask;
        }

        Task ITestDiscoverer.AnalyzeProjectAsync(ProjectTestDiscoveryContext context)
        {
            throw new NotImplementedException();
        }

        public Uri ExecutorUri => new Uri("executor://xunit/VsTestRunner2");

        // TODO: Need to somehow resolve actual nuget references of project and choose correct
        //       execution implementation for target runtime.
        private Lazy<string> XunitExecutionAssemblyPath = new Lazy<string>(() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "xunit.execution.desktop.dll"), isThreadSafe: true);
    }
}
