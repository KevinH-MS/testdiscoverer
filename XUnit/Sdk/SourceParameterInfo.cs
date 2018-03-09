using System;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Source-based implementation of <see cref="IParameterInfo"/>.
    /// </summary>
    class SourceParameterInfo : IParameterInfo
    {
        private readonly CompilationContext _compilationContext;
        private readonly IParameterSymbol _parameterSymbol;

        public SourceParameterInfo(CompilationContext compilationContext, IParameterSymbol parameterSymbol)
        {
            _compilationContext = compilationContext;
            _parameterSymbol = parameterSymbol;
        }

        string IParameterInfo.Name => _parameterSymbol.Name;

        ITypeInfo IParameterInfo.ParameterType => new SourceTypeInfo(_compilationContext, (INamedTypeSymbol)_parameterSymbol.Type);
    }
}
