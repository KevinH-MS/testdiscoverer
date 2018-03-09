using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    static class SymbolExtensions
    {
        public static bool TryGetMatchingAttributes(this ISymbol symbol, CompilationContext compilationContext, string assemblyQualifiedAttributeTypeName, out IList<IAttributeInfo> matchingAttributes)
        {
            matchingAttributes = null;

            var attributes = symbol.GetAttributesIncludingInherited();
            if (attributes.IsEmpty)
            {
                return false;
            }

            if (TryGetTypeAndAssemblyName(assemblyQualifiedAttributeTypeName, out var typeName, out var assemblyName))
            {
                var targetType = compilationContext.GetTypeByMetadataName(typeName);

                if (targetType == null)
                {
                    return false;
                }

                foreach (var attribute in attributes)
                {
                    if (compilationContext.IsAssignableTo(attribute.AttributeClass, targetType))
                    {
                        if (matchingAttributes == null)
                        {
                            matchingAttributes = new List<IAttributeInfo>();
                        }

                        matchingAttributes.Add(new SourceAttributeInfo(compilationContext, attribute));
                    }
                }
            }

            return matchingAttributes != null;
        }

        private static ImmutableArray<AttributeData> GetAttributesIncludingInherited(this ISymbol symbol)
        {
            var attributes = symbol.GetAttributes();

            HashSet<INamedTypeSymbol> appliedAttributeClasses = null;
            ImmutableArray<AttributeData>.Builder attributesIncludingInherited = null;
            switch (symbol.Kind)
            {
                case SymbolKind.NamedType:
                    var baseType = ((INamedTypeSymbol)symbol).BaseType;
                    while (baseType.BaseType != null)
                    {
                        AppendInheritedAttributes(attributes, baseType, ref appliedAttributeClasses, ref attributesIncludingInherited);

                        baseType = baseType.BaseType;
                    }

                    break;
                case SymbolKind.Method:
                    var overridenMethod = ((IMethodSymbol)symbol).OverriddenMethod;
                    if (overridenMethod != null)
                    {
                        AppendInheritedAttributes(attributes, overridenMethod, ref appliedAttributeClasses, ref attributesIncludingInherited);
                    }

                    break;
            }

            return attributesIncludingInherited?.ToImmutableArray() ?? attributes;
        }

        private static void AppendInheritedAttributes(ImmutableArray<AttributeData> attributes, ISymbol baseSymbol, ref HashSet<INamedTypeSymbol> appliedAttributeClasses, ref ImmutableArray<AttributeData>.Builder builder)
        {
            var attributeUsageAttribute = baseSymbol.ContainingAssembly.GetTypeByMetadataName("System.AttributeUsageAttribute");
            foreach (var baseAttribute in baseSymbol.GetAttributes())
            {
                if (baseAttribute.IsInherited(attributeUsageAttribute))
                {
                    if (builder == null)
                    {
                        appliedAttributeClasses = new HashSet<INamedTypeSymbol>(attributes.Select(a => a.AttributeClass));
                        builder = attributes.ToBuilder();
                    }

                    if (appliedAttributeClasses.Add(baseAttribute.AttributeClass))
                    {
                        builder.Add(baseAttribute);
                    }
                }
            }
        }

        private static bool TryGetTypeAndAssemblyName(string assemblyQualifiedAttributeTypeName, out string typeName, out string assemblyName)
        {
            typeName = null;
            assemblyName = null;

            // TODO: Parse full syntax supported by SerializationHelper.GetType...
            var parts = assemblyQualifiedAttributeTypeName.Split(',');

            if (parts.Length < 1)
            {
                return false;
            }

            typeName = parts[0].Trim();

            if (parts.Length > 1)
            {
                assemblyName = parts[1].Trim();
            }

            return true;
        }

        private static bool IsInherited(this AttributeData @this, INamedTypeSymbol attributeUsageAttribute)
        {
            foreach (var attribute in @this.AttributeClass.GetAttributes())
            {
                // TODO: AttributeUsageAttribute itself may also be inherited, but only the most derived one counts...
                if (attribute.AttributeClass != attributeUsageAttribute)
                {
                    continue;
                }

                foreach (var argument in attribute.NamedArguments)
                {
                    if (argument.Key == "Inherited" && argument.Value.Value is bool value)
                    {
                        return value;
                    }
                }
            }

            return true;
        }
    }
}
