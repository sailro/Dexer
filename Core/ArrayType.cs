namespace Dexer.Core
{
    public class ArrayType : ClassReference
    {
        public TypeReference ElementType { get; set; }

        public override string ToString()
        {
            return string.Concat("[", ElementType.ToString(), "]");
        }

        public override bool Equals(TypeReference other)
        {
            return (other is ArrayType) && (ElementType.Equals((other as ArrayType).ElementType));
        }
    }
}
