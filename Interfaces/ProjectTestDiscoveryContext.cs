// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.CodeAnalysis.UnitTesting.SourceBasedTestDiscovery
{
    /// <summary>
    /// Context for project-level test discovery analysis.
    /// </summary>
    internal sealed class ProjectTestDiscoveryContext : TestDiscoveryContext
    {
        /// <summary>
        /// The project that is currently being analyzed to discover tests.
        /// </summary>
        internal Project Project { get; }

        internal ProjectTestDiscoveryContext(
            Project project,
            Action<TestCase> reportDiscoveredTest,
            string source,
            CancellationToken cancellationToken) :
                base(reportDiscoveredTest, source, cancellationToken)
                    => Project = project;
    }
}
