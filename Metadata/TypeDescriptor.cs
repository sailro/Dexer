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
using Dexer.Core;

namespace Dexer.Metadata
{
	public class TypeDescriptor
	{
        internal static TypeReference Allocate(string typeDescriptor)
        {
            if (!string.IsNullOrEmpty(typeDescriptor))
            {
                char prefix = typeDescriptor[0];
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
                    case TypeDescriptors.ArrayOfDescriptor:
                        return new ArrayType();
                    case TypeDescriptors.FullyQualifiedName:
                        return new ClassReference();
                }
            }
            return null;
        }

        internal static void Fill(string typeDescriptor, TypeReference item, Dex context)
        {
            if (!string.IsNullOrEmpty(typeDescriptor))
            {
                char prefix = typeDescriptor[0];
                TypeDescriptors td = (TypeDescriptors)prefix;
                switch (td)
                {
                    case TypeDescriptors.ArrayOfDescriptor:
                        ArrayType atype = (ArrayType)item;

                        TypeReference elementType = Allocate(typeDescriptor.Substring(1));
                        Fill(typeDescriptor.Substring(1), elementType, context);

                        atype.ElementType = context.Import(elementType);
                        break;
                    case TypeDescriptors.FullyQualifiedName:
                        ClassReference cref = (ClassReference)item;
                        cref.Fullname = typeDescriptor.Substring(1, typeDescriptor.Length-2);
                        break;
                }
            }
        }

        public static bool IsPrimitive(TypeDescriptors td) {
            return (td != TypeDescriptors.ArrayOfDescriptor) && (td != TypeDescriptors.FullyQualifiedName);
        }

	}
}
