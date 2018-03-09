using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Source-based implementation of <see cref="IMethodInfo"/>.
    /// </summary>
    class SourceMethodInfo : IMethodInfo
    {
        private readonly CompilationContext _compilationContext;
        private readonly IMethodSymbol _methodSymbol;

        public SourceMethodInfo(CompilationContext compilationContext, IMethodSymbol methodSymbol)
        {
            _compilationContext = compilationContext;
            _methodSymbol = methodSymbol;
        }

        bool IMethodInfo.IsAbstract => throw new NotImplementedException();

        bool IMethodInfo.IsGenericMethodDefinition => _methodSymbol.IsGenericMethod;

        bool IMethodInfo.IsPublic => throw new NotImplementedException();

        bool IMethodInfo.IsStatic => throw new NotImplementedException();

        string IMethodInfo.Name => _methodSymbol.MetadataName;

        ITypeInfo IMethodInfo.ReturnType => throw new NotImplementedException();

        ITypeInfo IMethodInfo.Type => throw new NotImplementedException();

        IEnumerable<IAttributeInfo> IMethodInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            if (_methodSymbol.TryGetMatchingAttributes(_compilationContext, assemblyQualifiedAttributeTypeName, out var matchingAttributes))
            {
                return matchingAttributes;
            }

            return Enumerable.Empty<IAttributeInfo>();
        }

        IEnumerable<ITypeInfo> IMethodInfo.GetGenericArguments()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IParameterInfo> IMethodInfo.GetParameters()
        {
            foreach (var parameter in _methodSymbol.Parameters)
            {
                yield return new SourceParameterInfo(_compilationContext, parameter);
            }
        }

        IMethodInfo IMethodInfo.MakeGenericMethod(params ITypeInfo[] typeArguments)
        {
            throw new NotImplementedException();
        }
    }
}
