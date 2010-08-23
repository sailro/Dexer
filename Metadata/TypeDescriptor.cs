using System;
using Dexer.Core;

namespace Dexer.Metadata
{
	public class TypeDescriptor
	{
        public static TypeReference Parse(string typeDescriptor) {
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
                        // TODO : lookup !
                        ArrayType atype = new ArrayType();
                        atype.ElementType = TypeDescriptor.Parse(typeDescriptor.Substring(1));
                        return atype;
                    case TypeDescriptors.FullyQualifiedName:
                        ClassReference cref = new ClassReference();
                        cref.Fullname = typeDescriptor.Substring(1, typeDescriptor.Length-2);
                        return cref;
                }
            }
            return null;
        }

        public static bool IsPrimitive(TypeDescriptors td) {
            return (td != TypeDescriptors.ArrayOfDescriptor) && (td != TypeDescriptors.FullyQualifiedName);
        }

	}
}
