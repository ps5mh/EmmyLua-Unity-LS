﻿using Microsoft.CodeAnalysis;

namespace unity.core;

public class CustomSymbolFinder
{
    public List<INamedTypeSymbol> GetAllSymbols(Compilation compilation, List<string> filterNamespace)
    {
        var visitor = new FindAllSymbolsVisitor(filterNamespace);
        visitor.Visit(compilation.GlobalNamespace);
        return visitor.AllTypeSymbols;
    }

    // public HashSet<string> Filter { get; set; } = new HashSet<string>();

    private class FindAllSymbolsVisitor : SymbolVisitor
    {
        private readonly List<string> _filter;

        public FindAllSymbolsVisitor(List<string> filter)
        {
            _filter = filter;
        }

        public List<INamedTypeSymbol> AllTypeSymbols { get; } = new List<INamedTypeSymbol>();

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (!IsAllowNamespacePrefix(symbol.ContainingNamespace) ||
                symbol.DeclaredAccessibility != Accessibility.Public) return;
            
            AllTypeSymbols.Add(symbol);
            foreach (var childSymbol in symbol.GetTypeMembers())
            {
                base.Visit(childSymbol);
            }
        }

        private bool IsAllowNamespacePrefix(INamespaceSymbol symbol)
        {
            if (symbol.IsGlobalNamespace)
            {
                return true;
            }

            var namespaceString = symbol.ToString();
            if (namespaceString != null)
            {
                foreach (var prefix in _filter)
                {
                    if (namespaceString.StartsWith(prefix))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}