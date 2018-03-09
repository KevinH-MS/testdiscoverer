using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;
using CSharpCompilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation;
using VisualBasicCompilation = Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation;

namespace Xunit.Sdk
{
    // Provides a cache for lookups performed on a particular Compilation...
    class CompilationContext
    {
        private readonly IAssemblyInfo _assembly;
        private readonly Compilation _compilation;

        private readonly ConcurrentDictionary<string, INamedTypeSymbol> _typesByMetadataName = new ConcurrentDictionary<string, INamedTypeSymbol>();

        public CompilationContext(IAssemblyInfo assembly, Compilation compilation)
        {
            _assembly = assembly;
            _compilation = compilation;
        }

        public IAssemblyInfo Assembly => _assembly;

        public string AssemblyName => _compilation.AssemblyName;

        public IAssemblySymbol AssemblySymbol => _compilation.Assembly;

        public SemanticModel GetSemanticModel(SyntaxTree syntaxTree) => _compilation.GetSemanticModel(syntaxTree);

        public INamedTypeSymbol GetTypeByMetadataName(string fullyQualifiedMetadataName)
        {
            if (!_typesByMetadataName.TryGetValue(fullyQualifiedMetadataName, out var type))
            {
                type = _compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
                _typesByMetadataName.TryAdd(fullyQualifiedMetadataName, type);
            }

            return type;
        }

        public bool IsAssignableTo(INamedTypeSymbol source, INamedTypeSymbol destination)
        {
            switch (source.Language)
            {
                case LanguageNames.CSharp:
                    return ((CSharpCompilation)_compilation).ClassifyConversion(source, destination).IsImplicit;
                case LanguageNames.VisualBasic:
                    return ((VisualBasicCompilation)_compilation).ClassifyConversion(source, destination).IsWidening;
            }

            return false;
        }
    }
}
