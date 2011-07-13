/*
    Dexer, open source framework for .DEX files (Dalvik Executable Format)
    Copyright (C) 2010 Sebastien LEBRETON

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using Dexer.Core;
using Dexer.Metadata;

namespace Dexer.IO.Collector
{
    internal class ArgumentComparer : IComparer<AnnotationArgument>
    {
        private int CompareValue(Object x, Object y)
        {
            var xf = ValueFormat.GetFormat(x);
            var yf = ValueFormat.GetFormat(y);

            int result = xf.CompareTo(yf);

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
                    return ((IComparable) x).CompareTo((IComparable) y);
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

                    Array ax = (Array) x;
                    Array ay = (Array) y;
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
            int result = x.Name.CompareTo(y.Name);

            if (result != 0)
                return result;

            return CompareValue(x.Value, y.Value);
        }
    }
}
