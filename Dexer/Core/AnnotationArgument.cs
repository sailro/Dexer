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

using Dexer.Metadata;
using System.Text;

namespace Dexer.Core;

public class AnnotationArgument(string name, object? value) : IEquatable<AnnotationArgument>
{
	public string Name { get; } = name;
	public object? Value { get; } = value;

	public override string ToString()
	{
		var builder = new StringBuilder();
		builder.Append(Name);
		builder.Append(":");
		builder.Append(Value);
		return builder.ToString();
	}

	public bool Equals(AnnotationArgument other)
	{
		if (other == null)
			return false;

		return Name.Equals(other.Name)
		       && ValueFormat.GetFormat(Value).Equals(ValueFormat.GetFormat(other.Value))
		       && (ValueFormat.GetFormat(Value) == ValueFormats.Array && ArrayEquals((Array)Value!, (Array)other.Value!) || Equals(Value, other.Value));
	}

	internal static bool ArrayEquals(Array array1, Array array2)
	{
		if (array1.Length != array2.Length)
			return false;

		return !array1.Cast<object>().Where((_, i) => !array1.GetValue(i).Equals(array2.GetValue(i))).Any();
	}
}
