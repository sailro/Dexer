using Dexer.IO;
using System.IO;
using System.Collections.Generic;

namespace Dexer.Core
{
    public class Dex
    {
        public IList<ClassDefinition> Classes { get; internal set; }

        internal IList<Prototype> Prototypes { get; set; }
        internal IList<TypeReference> TypeReferences { get; set; }
        internal IList<FieldReference> FieldReferences { get; set; }
        internal IList<MethodReference> MethodReferences { get; set; }

        public static Dex Load(string filename)
        {
            Dex result = new Dex();
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                using (BinaryReader binaryReader = new BinaryReader(stream))
                {
                    DexHandler reader = new DexHandler(result);
                    reader.ReadFrom(binaryReader);
                    return result;
                }
            }
        }

        public Dex()
        {
            Classes = new List<ClassDefinition>();
            TypeReferences = new List<TypeReference>();
            Prototypes = new List<Prototype>();
            FieldReferences = new List<FieldReference>();
            MethodReferences = new List<MethodReference>();
        }

        internal TypeReference Import(TypeReference tref) {
            foreach (TypeReference item in TypeReferences)
            {
                if (tref.Equals(item))
                {
                    return item;
                }
            }
            TypeReferences.Add(tref);
            return tref;
        }

        public static void Main(string[] args)
        {
            Dex dex = Dex.Load("classes.dex");
        }

    }
}
