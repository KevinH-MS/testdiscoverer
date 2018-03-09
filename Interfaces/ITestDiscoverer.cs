// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.UnitTesting.SourceBasedTestDiscovery
{
    internal interface ITestDiscoverer
    {
        /// <summary>
        /// The Uri of the test adapter that handles tests for this discoverer.
        /// </summary>
        Uri ExecutorUri { get; }

        /// <summary>
        /// Performs discovery on the supplied <paramref name="document"/> and calls
        /// <see cref="TestDiscoveryContext.ReportDiscoveredTest(TestCase)"/> to report discovered tests.
        /// </summary>
        /// <remarks>
        /// Must cancel discovery if cancellation is triggered via <see cref="TestDiscoveryContext.CancellationToken"/>.
        /// </remarks>
        Task AnalyzeDocumentAsync(DocumentTestDiscoveryContext context);

        /// <summary>
        /// Performs discovery on the supplied <paramref name="project"/> and calls
        /// <see cref="TestDiscoveryContext.ReportDiscoveredTest(TestCase)"/> to report discovered tests.
        /// </summary>
        /// <remarks>
        /// Must cancel discovery if cancellation is triggered via <see cref="TestDiscoveryContext.CancellationToken"/>.
        /// </remarks>
        Task AnalyzeProjectAsync(ProjectTestDiscoveryContext context);
    }
}
