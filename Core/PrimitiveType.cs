using Dexer.Metadata;

namespace Dexer.Core
{
    public class PrimitiveType : TypeReference
    {
        public static readonly PrimitiveType Void = new PrimitiveType(TypeDescriptors.Void);
        public static readonly PrimitiveType Boolean = new PrimitiveType(TypeDescriptors.Boolean);
        public static readonly PrimitiveType Byte = new PrimitiveType(TypeDescriptors.Byte);
        public static readonly PrimitiveType Short = new PrimitiveType(TypeDescriptors.Short);
        public static readonly PrimitiveType Char = new PrimitiveType(TypeDescriptors.Char);
        public static readonly PrimitiveType Int = new PrimitiveType(TypeDescriptors.Int);
        public static readonly PrimitiveType Long = new PrimitiveType(TypeDescriptors.Long);
        public static readonly PrimitiveType Float = new PrimitiveType(TypeDescriptors.Float);
        public static readonly PrimitiveType Double = new PrimitiveType(TypeDescriptors.Double);

        private PrimitiveType(TypeDescriptors typeDescriptor)
        {
            this.TypeDescriptor = typeDescriptor;
        }

        public override string ToString()
        {
            return this.TypeDescriptor.ToString();
        }

    }
}
