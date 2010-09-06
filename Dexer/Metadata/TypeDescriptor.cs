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

using System;
using Dexer.Core;
using System.Text;

namespace Dexer.Metadata
{
	public class TypeDescriptor
	{
        internal static TypeReference Allocate(string tdString)
        {
            if (!string.IsNullOrEmpty(tdString))
            {
                char prefix = tdString[0];
                TypeDescriptors td = (TypeDescriptors)prefix;
                switch (td)
                {
                    case TypeDescriptors.Boolean:
                        return PrimitiveType.Boolean;
                    case TypeDescriptors.Byte:
                        return PrimitiveType.Byte;
                    case TypeDescriptors.Char:
                        return PrimitiveType.Char;
                    case TypeDescriptors.Double:
                        return PrimitiveType.Double;
                    case TypeDescriptors.Float:
                        return PrimitiveType.Float;
                    case TypeDescriptors.Int:
                        return PrimitiveType.Int;
                    case TypeDescriptors.Long:
                        return PrimitiveType.Long;
                    case TypeDescriptors.Short:
                        return PrimitiveType.Short;
                    case TypeDescriptors.Void:
                        return PrimitiveType.Void;
                    case TypeDescriptors.Array:
                        return new ArrayType();
                    case TypeDescriptors.FullyQualifiedName:
                        return new ClassReference();
                }
            }
            return null;
        }

        internal static void Fill(string tdString, TypeReference item, Dex context)
        {
            if (!string.IsNullOrEmpty(tdString))
            {
                char prefix = tdString[0];
                TypeDescriptors td = (TypeDescriptors)prefix;
                switch (td)
                {
                    case TypeDescriptors.Array:
                        ArrayType atype = (ArrayType)item;

                        TypeReference elementType = Allocate(tdString.Substring(1));
                        Fill(tdString.Substring(1), elementType, context);

                        atype.ElementType = context.Import(elementType);
                        break;
                    case TypeDescriptors.FullyQualifiedName:
                        ClassReference cref = (ClassReference)item;
                        cref.Fullname = tdString.Substring(1, tdString.Length-2);
                        break;
                }
            }
        }

        public static bool IsPrimitive(TypeDescriptors td) {
            return (td != TypeDescriptors.Array) && (td != TypeDescriptors.FullyQualifiedName);
        }

        public static string Encode(TypeReference tref)
        {
            StringBuilder result = new StringBuilder();

            result.Append((char) tref.TypeDescriptor);

            if (tref is ArrayType)
                result.Append((tref as ArrayType).ElementType);

            if (tref is ClassReference)
                result.Append(string.Concat((tref as ClassReference).Fullname.Replace(ClassReference.NamespaceSeparator, ClassReference.InternalNamespaceSeparator), ";"));

            return result.ToString();
        }

	}
}
