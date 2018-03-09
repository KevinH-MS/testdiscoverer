using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Source-based implementation of <see cref="IAttributeInfo"/>.
    /// </summary>
    class SourceAttributeInfo : IAttributeInfo
    {
        private readonly CompilationContext _compilationContext;
        private readonly AttributeData _attributeData;

        public SourceAttributeInfo(CompilationContext compilationContext, AttributeData attributeData)
        {
            _compilationContext = compilationContext;
            _attributeData = attributeData;
        }

        IEnumerable<object> IAttributeInfo.GetConstructorArguments()
        {
            return EnumerateConstantValues(_attributeData.ConstructorArguments);
        }

        IEnumerable<object> EnumerateConstantValues(ImmutableArray<TypedConstant> constants)
        {
            foreach (var constant in constants)
            {
                yield return constant.Kind == TypedConstantKind.Array ? EnumerateConstantValues(constant.Values) : constant.Value;
            }
        }

        IEnumerable<IAttributeInfo> IAttributeInfo.GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            if (_attributeData.AttributeClass.TryGetMatchingAttributes(_compilationContext, assemblyQualifiedAttributeTypeName, out var matchingAttributes))
            {
                return matchingAttributes;
            }

            return Enumerable.Empty<IAttributeInfo>();
        }

        TValue IAttributeInfo.GetNamedArgument<TValue>(string argumentName)
        {
            foreach (var argument in _attributeData.NamedArguments)
            {
                if (argument.Key == argumentName)
                {
                    // TODO: Does this work correctly for arrays?
                    return (TValue)argument.Value.Value;
                }
            }

            return default(TValue);
        }
    }
}
