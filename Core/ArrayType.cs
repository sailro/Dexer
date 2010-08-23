namespace Dexer.Core
{
    public class ArrayType : ClassReference
    {
        public TypeReference ElementType { get; set; }

        public override string ToString()
        {
            return string.Concat("[", ElementType.ToString(), "]");
        }
    }
}
