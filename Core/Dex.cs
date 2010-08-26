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

using System.Collections.Generic;
using System.IO;
using Dexer.IO;

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

        internal ClassDefinition GetClass(string fullname)
        {
            return GetClass(fullname, Classes);
        }

        internal ClassDefinition GetClass(string fullname, IList<ClassDefinition> container)
        {
            foreach (ClassDefinition item in container)
            {
                if (fullname.Equals(item.Fullname))
                    return item;

                var inner = GetClass(fullname, item.InnerClasses);
                if (inner != null)
                    return inner;
            }
            return null;
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
            Dex dex = Dex.Load(@"E:\Devl\Java\Budroid\bin\classes.dex");
            //Dex dex = Dex.Load(@"E:\Devl\Dexer\bin\Debug\classes.dex");
        }

    }
}
