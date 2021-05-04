﻿/* Dexer Copyright (c) 2010-2021 Sebastien Lebreton

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
			if (value is byte || value is sbyte)
				return ValueFormats.Byte;
			if (value is short || value is ushort)
				return ValueFormats.Short;
			if (value is char)
				return ValueFormats.Char;
			if (value is int || value is uint)
				return ValueFormats.Int;
			if (value is long || value is ulong)
				return ValueFormats.Long;
			if (value is float)
				return ValueFormats.Float;
			if (value is double)
				return ValueFormats.Double;
			if (value is bool)
				return ValueFormats.Boolean;
			if (value is string)
				return ValueFormats.String;
			if (value is TypeReference)
				return ValueFormats.Type;
			if (value is FieldReference)
			{
				if (value is FieldDefinition && (value as FieldDefinition).IsEnum)
					return ValueFormats.Enum;

				return ValueFormats.Field;
			}

			if (value is MethodReference)
				return ValueFormats.Method;
			if (value is Array)
				return ValueFormats.Array;
			if (value is Annotation)
				return ValueFormats.Annotation;
			if (value == null)
				return ValueFormats.Null;
			throw new ArgumentException("Unexpected format");
		}
	}
}
