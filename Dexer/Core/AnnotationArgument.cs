/* Dexer Copyright (c) 2010 Sebastien LEBRETON

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
using System;

namespace Dexer.Core
{
    public class AnnotationArgument
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Name);
            builder.Append(":");
            builder.Append(Value);
            return builder.ToString();
        }

        internal ValueFormats Format
        {
            get
            {
                if (Value is byte)
                    return ValueFormats.Byte;
                else if (Value is short)
                    return ValueFormats.Short;
                else if (Value is char)
                    return ValueFormats.Char;
                else if (Value is int)
                    return ValueFormats.Int;
                else if (Value is long)
                    return ValueFormats.Long;
                else if (Value is float)
                    return ValueFormats.Float;
                else if (Value is double)
                    return ValueFormats.Double;
                else if (Value is bool)
                    return ValueFormats.Boolean;
                else if (Value is string)
                    return ValueFormats.String;
                else if (Value is TypeReference)
                    return ValueFormats.Type;
                else if (Value is FieldReference)
                {
                    if (Value is FieldDefinition && (Value as FieldDefinition).IsEnum)
                        return ValueFormats.Enum;

                    return ValueFormats.Field;
                }
                else if (Value is MethodReference)
                    return ValueFormats.Method;
                else if (Value is ArrayType)
                    return ValueFormats.Array;
                else if (Value is Annotation)
                    return ValueFormats.Annotation;
                else if (Value == null)
                    return ValueFormats.Null;
                else
                    throw new ArgumentException("Unexpected annotation value type");

            }
        }
    }
}

