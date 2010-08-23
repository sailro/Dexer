using Dexer.IO;
using System.IO;
using System.Collections.Generic;

namespace Dexer.Core
{
    public class Dex
    {
        public IList<ClassDefinition> ClassDefinitions { get; internal set; }

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
            ClassDefinitions = new List<ClassDefinition>();
        }

        public static void Main(string[] args)
        {
            Dex dex = Dex.Load("classes.dex");

        }

    }
}
