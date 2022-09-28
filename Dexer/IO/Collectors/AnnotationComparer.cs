﻿/* Dexer Copyright (c) 2010-2022 Sebastien Lebreton

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
using Dexer.Core;

namespace Dexer.IO.Collectors;

internal class AnnotationComparer : IComparer<Annotation>
{
	private readonly TypeReferenceComparer _typeReferenceComparer = new();
	private readonly ArgumentComparer _argumentComparer = new();

	public int Compare(Annotation x, Annotation y)
	{
		switch (x)
		{
			case null when y == null:
				return 0;
			case null:
				return -1;
		}

		if (y == null)
			return 1;

		var result = _typeReferenceComparer.Compare(x.Type, y.Type);

		if (result == 0)
			result = x.Visibility.CompareTo(y.Visibility);

		if (result != 0)
			return result;

		for (var i = 0; i < Math.Min(x.Arguments.Count, y.Arguments.Count); i++)
		{
			result = _argumentComparer.Compare(x.Arguments[i], y.Arguments[i]);
			if (result != 0)
				return result;
		}

		if (x.Arguments.Count > y.Arguments.Count)
			return 1;

		if (y.Arguments.Count > x.Arguments.Count)
			return -1;

		return result;
	}
}
