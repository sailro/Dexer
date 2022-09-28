/* Dexer Copyright (c) 2010-2022 Sebastien Lebreton

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Dexer.IO.Collectors;
/* Taken from (great thanks)
 * http://stackoverflow.com/questions/1982592/topological-sorting-using-linq
 */

internal interface IPartialComparer<in T>
{
	int? PartialCompare(T x, T y);
}

internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>
{
	public bool Equals(T x, T y)
	{
		return ReferenceEquals(x, y);
	}

	public int GetHashCode(T obj)
	{
		return obj.GetHashCode();
	}
}

internal class TopologicalSorter
{
	private class DepthFirstSearch<TElement, TKey>
	{
		readonly IEnumerable<TElement> _elements;
		readonly IPartialComparer<TKey> _comparer;
		readonly HashSet<TElement> _visited;
		readonly Dictionary<TElement, TKey> _keys;
		readonly List<TElement> _sorted;

		public DepthFirstSearch(
			IList<TElement> elements,
			Func<TElement, TKey> selector,
			IPartialComparer<TKey> comparer
		)
		{
			_elements = elements;
			_comparer = comparer;
			var referenceComparer = new ReferenceEqualityComparer<TElement>();
			_visited = new HashSet<TElement>(referenceComparer);
			_keys = elements.ToDictionary(
				e => e,
				selector,
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
		IList<TElement> elements,
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
		IList<TElement> elements,
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
