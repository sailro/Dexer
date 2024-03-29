﻿/* Dexer Copyright (c) 2010-2023 Sebastien Lebreton

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

using Dexer.Core;

namespace Dexer.IO.Collectors;

internal class MethodReferenceComparer : IComparer<MethodReference>
{
	private readonly PrototypeComparer _prototypeComparer = new();
	private readonly TypeReferenceComparer _typeReferenceComparer = new();
	private readonly StringComparer _stringComparer = new();

	public int Compare(MethodReference x, MethodReference y)
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

		var result = _typeReferenceComparer.Compare(x.Owner, y.Owner);

		if (result == 0)
			result = _stringComparer.Compare(x.Name, y.Name);

		if (result == 0)
			result = _prototypeComparer.Compare(x.Prototype, y.Prototype);

		return result;
	}
}
