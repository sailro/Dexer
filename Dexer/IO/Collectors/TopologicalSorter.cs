/*
    Dexer, open source framework for .DEX files (Dalvik Executable Format)
    Copyright (C) 2010 Sebastien LEBRETON

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;

/* Taken from (great thanks)
 * http://stackoverflow.com/questions/1982592/topological-sorting-using-linq
 */

interface IPartialComparer<T>
{
    int? PartialCompare(T x, T y);
}

class ReferenceEqualityComparer<T> : IEqualityComparer<T>
{
    public bool Equals(T x, T y)
    {
        return Object.ReferenceEquals(x, y);
    }

    public int GetHashCode(T obj)
    {
        return obj.GetHashCode();
    }
}

class TopologicalSorter
{
    class DepthFirstSearch<TElement, TKey>
    {
        readonly IEnumerable<TElement> _elements;
        readonly Func<TElement, TKey> _selector;
        readonly IPartialComparer<TKey> _comparer;
        HashSet<TElement> _visited;
        Dictionary<TElement, TKey> _keys;
        List<TElement> _sorted;

        public DepthFirstSearch(
            IEnumerable<TElement> elements,
            Func<TElement, TKey> selector,
            IPartialComparer<TKey> comparer
        )
        {
            _elements = elements;
            _selector = selector;
            _comparer = comparer;
            var referenceComparer = new ReferenceEqualityComparer<TElement>();
            _visited = new HashSet<TElement>(referenceComparer);
            _keys = elements.ToDictionary(
                e => e,
                e => _selector(e),
                referenceComparer
            );
            _sorted = new List<TElement>();
        }

        public IEnumerable<TElement> VisitAll()
        {
            foreach (var element in _elements)
            {
                Visit(element);
            }
            return _sorted;
        }

        void Visit(TElement element)
        {
            if (!_visited.Contains(element))
            {
                _visited.Add(element);
                var predecessors = _elements.Where(
                    e => _comparer.PartialCompare(_keys[e], _keys[element]) < 0
                );
                foreach (var e in predecessors)
                {
                    Visit(e);
                }
                _sorted.Add(element);
            }
        }
    }

    public IEnumerable<TElement> TopologicalSort<TElement>(
        IEnumerable<TElement> elements,
        IPartialComparer<TElement> comparer
    )
    {
        var search = new DepthFirstSearch<TElement, TElement>(
            elements,
            element => element,
            comparer
        );
        return search.VisitAll();
    }

    public IEnumerable<TElement> TopologicalSort<TElement, TKey>(
        IEnumerable<TElement> elements,
        Func<TElement, TKey> selector, IPartialComparer<TKey> comparer
    )
    {
        var search = new DepthFirstSearch<TElement, TKey>(
            elements,
            selector,
            comparer
        );
        return search.VisitAll();
    }
}
