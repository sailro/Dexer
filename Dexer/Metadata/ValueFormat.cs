/* Dexer Copyright (c) 2010-2013 Sebastien LEBRETON

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
            else if (value is short || value is ushort)
                return ValueFormats.Short;
            else if (value is char)
                return ValueFormats.Char;
            else if (value is int || value is uint)
                return ValueFormats.Int;
            else if (value is long || value is ulong)
                return ValueFormats.Long;
            else if (value is float)
                return ValueFormats.Float;
            else if (value is double)
                return ValueFormats.Double;
            else if (value is bool)
                return ValueFormats.Boolean;
            else if (value is string)
                return ValueFormats.String;
            else if (value is TypeReference)
                return ValueFormats.Type;
            else if (value is FieldReference)
            {
                if (value is FieldDefinition && (value as FieldDefinition).IsEnum)
                    return ValueFormats.Enum;

                return ValueFormats.Field;
            }
            else if (value is MethodReference)
                return ValueFormats.Method;
            else if (value is Array)
                return ValueFormats.Array;
            else if (value is Annotation)
                return ValueFormats.Annotation;
            else if (value == null)
                return ValueFormats.Null;
            else
                throw new ArgumentException("Unexpected format");
        }
    }
}
