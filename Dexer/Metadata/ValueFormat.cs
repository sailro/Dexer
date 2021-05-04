/* Dexer Copyright (c) 2010-2021 Sebastien Lebreton

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
using System;

namespace Dexer.Metadata
{
	public static class ValueFormat
	{
		public static ValueFormats GetFormat(object value)
		{
			return value switch
			{
				byte or sbyte => ValueFormats.Byte,
				short or ushort => ValueFormats.Short,
				char => ValueFormats.Char,
				int or uint => ValueFormats.Int,
				long or ulong => ValueFormats.Long,
				float => ValueFormats.Float,
				double => ValueFormats.Double,
				bool => ValueFormats.Boolean,
				string => ValueFormats.String,
				TypeReference => ValueFormats.Type,
				FieldDefinition { IsEnum: true } => ValueFormats.Enum,
				FieldReference => ValueFormats.Field,
				MethodReference => ValueFormats.Method,
				Array => ValueFormats.Array,
				Annotation => ValueFormats.Annotation,
				null => ValueFormats.Null,
				_ => throw new ArgumentException("Unexpected format"),
			};
		}
	}
}
