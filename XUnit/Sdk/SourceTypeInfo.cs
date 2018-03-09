using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Source-based implementation of <see cref="ITypeInfo"/>.
    /// </summary>
    class SourceTypeInfo : ITypeInfo
    {
        private static readonly SymbolDisplayFormat QualifiedNameFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        private readonly CompilationContext _compilationContext;
        private readonly INamedTypeSymbol _typeSymbol;

        private string _qualifiedName;

        public SourceTypeInfo(CompilationContext compilationContext, INamedTypeSymbol typeSymbol)
        {
            _compilationContext = compilationContext;
            _typeSymbol = typeSymbol;
        }

        IAssemblyInfo ITypeInfo.Assembly => _compilationContext.Assembly;

        ITypeInfo ITypeInfo.BaseType => throw new NotImplementedException();

        IEnumerable<ITypeInfo> ITypeInfo.Interfaces => throw new NotImplementedException();

        bool ITypeInfo.IsAbstract => _typeSymbol.IsAbstract;

        bool ITypeInfo.IsGenericParameter => throw new NotImplementedException();

        bool ITypeInfo.IsGenericType => throw new NotImplementedException();

        bool ITypeInfo.IsSealed => _typeSymbol.IsSealed;

        bool ITypeInfo.IsValueType => throw new NotImplementedException();

        string ITypeInfo.Name => _qualifiedName ?? (_qualifiedName = ToDisplayString(_typeSymbol));

        private static string ToDisplayString(INamedTypeSymbol typeSymbol)
        {
            var parts = typeSymbol.ToDisplayParts(QualifiedNameFormat);
            if (parts.Length < 1)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var index = 0;

            while (index < parts.Length && (parts[index].Kind == SymbolDisplayPartKind.NamespaceName || parts[index].Kind == SymbolDisplayPartKind.Operator || parts[index].Kind == SymbolDisplayPartKind.Punctuation))
            {
                builder.Append(parts[index].ToString());
                index++;
            }

            while (index < parts.Length)
            {
                if (parts[index].Kind == SymbolDisplayPartKind.Operator || parts[index].Kind == SymbolDisplayPartKind.Punctuation)
                {
                    builder.Append('+');
                }
                else
                {
                    builder.Append(parts[index].ToString());
                }

                index++;
            }

            return builder.ToString();
        }

        IEnumerable<IAttributeInfo> ITypeInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            if (_typeSymbol.TryGetMatchingAttributes(_compilationContext, assemblyQualifiedAttributeTypeName, out var matchingAttributes))
            {
                return matchingAttributes;
            }

            return Enumerable.Empty<IAttributeInfo>();
        }

        IEnumerable<ITypeInfo> ITypeInfo.GetGenericArguments()
        {
            throw new NotImplementedException();
        }

        IMethodInfo ITypeInfo.GetMethod(string methodName, bool includePrivateMethod)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IMethodInfo> ITypeInfo.GetMethods(bool includePrivateMethods)
        {
            HashSet<IMethodSymbol> overridenMethods = null;

            var type = _typeSymbol;
            while (type.BaseType != null)
            {
                foreach (var member in type.GetMembers())
                {
                    if (member.Kind != SymbolKind.Method)
                    {
                        continue;
                    }

                    var method = (IMethodSymbol)member;
                    if (includePrivateMethods || method.DeclaredAccessibility == Accessibility.Public)
                    {
                        var overridenMethod = method.OverriddenMethod;
                        if (overridenMethod != null)
                        {
                            if (overridenMethods == null)
                            {
                                overridenMethods = new HashSet<IMethodSymbol>();
                            }

                            if (!overridenMethods.Add(overridenMethod))
                            {
                                continue;
                            }
                        }

                        if (overridenMethods != null && overridenMethods.Contains(method))
                        {
                            continue;
                        }

                        yield return new SourceMethodInfo(_compilationContext, method);
                    }
                }

                type = type.BaseType;
            }
        }
    }
}
