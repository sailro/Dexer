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

using System;
using System.Collections.Generic;
using Dexer.Core;
using Dexer.Metadata;

namespace Dexer.IO.Collectors
{
    internal class ArgumentComparer : IComparer<AnnotationArgument>
    {
        private static int CompareValue(Object x, Object y)
        {
            var xf = ValueFormat.GetFormat(x);
            var yf = ValueFormat.GetFormat(y);

            var result = xf.CompareTo(yf);

            if (result != 0)
                return result;

            switch(xf)
            {
                case ValueFormats.Byte:
                case ValueFormats.Short:
                case ValueFormats.Char:
                case ValueFormats.Int:
                case ValueFormats.Long:
                case ValueFormats.Float:
                case ValueFormats.Double:
                case ValueFormats.Boolean:
                case ValueFormats.String:
                    return ((IComparable) x).CompareTo(y);
                case ValueFormats.Null:
                    return 0;
                case ValueFormats.Type:
                    return new TypeReferenceComparer().Compare((TypeReference) x, (TypeReference) y);
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    return new FieldReferenceComparer().Compare((FieldReference)x, (FieldReference)y);
                case ValueFormats.Method:
                    return new MethodReferenceComparer().Compare((MethodReference)x, (MethodReference)y);
                case ValueFormats.Annotation:
                    return new AnnotationComparer().Compare((Annotation)x, (Annotation)y);
                case ValueFormats.Array:

                    var ax = (Array) x;
                    var ay = (Array) y;
                    for (int i = 0; i < Math.Min(ax.Length, ay.Length); i++)
                    {
                        result = CompareValue(ax.GetValue(i), ay.GetValue(i));
                        if (result != 0)
                            return result;
                    }

                    if (ax.Length > ay.Length)
                        return 1;

                    if (ay.Length > ax.Length)
                        return -1;

                    return 0;
                default:
                    throw new NotImplementedException(xf.ToString());

            }            
        }

        public int Compare(AnnotationArgument x, AnnotationArgument y)
        {
            var result = String.Compare(x.Name, y.Name, StringComparison.Ordinal);
            return result != 0 ? result : CompareValue(x.Value, y.Value);
        }
    }
}
