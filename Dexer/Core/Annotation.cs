/* Dexer Copyright (c) 2010-2023 Sebastien Lebreton

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

using System.Text;
using Dexer.Metadata;

namespace Dexer.Core;

public class Annotation(ClassReference type) : IEquatable<Annotation>
{
	public ClassReference Type { get; } = type;
	public List<AnnotationArgument> Arguments { get; } = [];
	public AnnotationVisibility Visibility { get; set; }

	public override string ToString()
	{
		var builder = new StringBuilder();
		builder.Append(Type);
		builder.Append("(");
		for (var i = 0; i < Arguments.Count; i++)
		{
			if (i > 0)
				builder.Append(", ");

			builder.Append(Arguments[i]);
		}

		builder.Append(")");
		return builder.ToString();
	}

	public bool Equals(Annotation? other)
	{
		if (other == null)
			return false;

		if (!Type.Equals(other.Type) || !Arguments.Count.Equals(other.Arguments.Count))
			return false;

		return !Arguments.Where((t, i) => !t.Equals(other.Arguments[i])).Any();
	}

	public override bool Equals(object? obj)
	{
		return obj is Annotation annotation && Equals(annotation);
	}

	public override int GetHashCode()
	{
		var builder = new StringBuilder();
		builder.AppendLine(TypeDescriptor.Encode(Type));

		foreach (var argument in Arguments)
		{
			builder.Append($"{argument.Name}=");
			if (ValueFormat.GetFormat(argument.Value) == ValueFormats.Array)
			{
				if (argument.Value is not Array array)
					throw new ArgumentException();

				for (var i = 0; i < array.Length; i++)
				{
					if (i > 0)
						builder.Append(",");
					builder.Append(array.GetValue(i));
				}
			}
			else
				builder.Append(argument.Value);

			builder.AppendLine();
		}

		return builder.ToString().GetHashCode();
	}
}
