using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.UnitTesting.SourceBasedTestDiscovery;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Source-based implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    class SourceAssemblyInfo : IAssemblyInfo
    {
        private readonly TestDiscoveryContext _context;

        private CompilationContext _compilationContext;
        private IEnumerable<ITypeInfo> _publicTypes;
        private SyntaxTree _syntaxTree;

        public SourceAssemblyInfo(TestDiscoveryContext context)
        {
            _context = context;
        }

        string IAssemblyInfo.AssemblyPath => null;

        string IAssemblyInfo.Name => TryGetCompilationContext(out var compilationContext) ? compilationContext.AssemblyName : null;

        IEnumerable<IAttributeInfo> IAssemblyInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            if (TryGetCompilationContext(out var compilationContext) &&
                compilationContext.AssemblySymbol.TryGetMatchingAttributes(compilationContext, assemblyQualifiedAttributeTypeName, out var matchingAttributes))
            {
                return matchingAttributes;
            }

            return Enumerable.Empty<IAttributeInfo>();
        }

        ITypeInfo IAssemblyInfo.GetType(string typeName)
        {
            throw new NotImplementedException();
        }

        IEnumerable<ITypeInfo> IAssemblyInfo.GetTypes(bool includePrivateTypes)
        {
            // Don't bother caching requests that include private types, because we don't usually ask for those,
            // meaning we can avoid creating ITypeInfos for the private types in the normal case...
            if (includePrivateTypes || _publicTypes == null)
            {
                var types = new List<ITypeInfo>();
                if (TryGetCompilationContext(out var compilationContext) && TryGetSyntax(out var syntaxTree))
                {
                    var publicTypes = new List<ITypeInfo>();
                    var model = compilationContext.GetSemanticModel(syntaxTree);
                    var topLevelNodes = syntaxTree.GetRoot(_context.CancellationToken).ChildNodes();
                    AppendTypesRecursive(types, publicTypes, compilationContext, model, topLevelNodes, _context.CancellationToken);

                    _publicTypes = publicTypes.Any() ? publicTypes : Enumerable.Empty<ITypeInfo>();
                }

                return types.Any() ? types : Enumerable.Empty<ITypeInfo>();
            }

            return _publicTypes;
        }

        private static void AppendTypesRecursive(IList<ITypeInfo> types, IList<ITypeInfo> publicTypes, CompilationContext compilationContext, SemanticModel model, IEnumerable<SyntaxNode> nodes, CancellationToken cancellationToken)
        {
            foreach (var node in nodes)
            {
                if (model.GetDeclaredSymbol(node, cancellationToken) is INamespaceOrTypeSymbol namespaceOrType)
                {
                    if (namespaceOrType.IsType)
                    {
                        var typeInfo = new SourceTypeInfo(compilationContext, (INamedTypeSymbol)namespaceOrType);
                        types.Add(typeInfo);

                        // TODO: Doesn't correctly handle nested types...
                        if (namespaceOrType.DeclaredAccessibility == Accessibility.Public)
                        {
                            publicTypes.Add(typeInfo);
                        }
                    }

                    AppendTypesRecursive(types, publicTypes, compilationContext, model, node.ChildNodes(), cancellationToken);
                }
            }
        }

        private bool TryGetCompilationContext(out CompilationContext compilationContext)
        {
            if (_compilationContext == null && _context is DocumentTestDiscoveryContext documentContext)
            {
                var compilation = documentContext.Document.Project.GetCompilationAsync(documentContext.CancellationToken).GetAwaiter().GetResult();
                _compilationContext = new CompilationContext(this, compilation);
            }

            compilationContext = _compilationContext;

            return compilationContext != null;
        }

        private bool TryGetSyntax(out SyntaxTree syntaxTree)
        {
            if (_syntaxTree == null && _context is DocumentTestDiscoveryContext documentContext)
            {
                _syntaxTree = documentContext.Document.GetSyntaxTreeAsync(documentContext.CancellationToken).GetAwaiter().GetResult();
            }

            syntaxTree = _syntaxTree;

            return syntaxTree != null;
        }
    }
}
