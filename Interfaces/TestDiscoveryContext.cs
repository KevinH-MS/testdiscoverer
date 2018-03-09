// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.CodeAnalysis.UnitTesting.SourceBasedTestDiscovery
{
    /// <summary>
    /// Context for test discovery analysis.
    /// </summary>
    internal abstract class TestDiscoveryContext
    {
        private readonly Action<TestCase> _reportDiscoveredTest;

        /// <summary>
        /// Value to use when initializing <see cref="TestCase.Source"/> property (expected to be the project's full "bin" output path).
        /// </summary>
        internal string Source { get; }

        /// <summary>
        /// A <see cref="CancellationToken"/> via which cancellation can be requested for the current test discovery operation.
        /// </summary>
        internal CancellationToken CancellationToken { get; }

        protected TestDiscoveryContext(
            Action<TestCase> reportDiscoveredTest,
            string source,
            CancellationToken cancellationToken)
        {
            _reportDiscoveredTest = reportDiscoveredTest;
            Source = source;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Reports the discovery of a <paramref name="test"/> defined in the supplied <paramref name="tree"/>.
        /// </summary>
        internal void ReportDiscoveredTest(TestCase test)
        {
            ThrowIfInvalid(test);
            _reportDiscoveredTest(test);
        }

        private static void ThrowIfInvalid(TestCase test)
        {
            if (string.IsNullOrWhiteSpace(test.FullyQualifiedName))
            {
                throw new ArgumentException(nameof(test.FullyQualifiedName));
            }
            else if (string.IsNullOrWhiteSpace(test.DisplayName))
            {
                throw new ArgumentException(nameof(test.DisplayName));
            }
            else if (string.IsNullOrWhiteSpace(test.Source))
            {
                throw new ArgumentException(nameof(test.Source));
            }
            else if (string.IsNullOrWhiteSpace(test.ExecutorUri.AbsoluteUri))
            {
                throw new ArgumentException(nameof(test.ExecutorUri));
            }
        }
    }
}
