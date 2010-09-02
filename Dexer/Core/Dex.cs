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
using System.Diagnostics;
using System.IO;
using Dexer.Extensions;
using Dexer.IO;
using Dexer.Metadata;

namespace Dexer.Core
{
    public class Dex
    {
        public IList<ClassDefinition> Classes { get; internal set; }

        internal IList<TypeReference> TypeReferences { get; set; }
        internal IList<FieldReference> FieldReferences { get; set; }
        internal IList<MethodReference> MethodReferences { get; set; }

        internal DexHeader Header { get; set; }
        internal IList<string> Strings { get; set; }
        internal IList<Prototype> Prototypes { get; set; }
        internal Map Map { get; set; }

        public static Dex Load(string filename)
        {
            return Load(filename, true);
        }

        public static Dex Load(string filename, bool preload)
        {
            Dex result = new Dex();

            using (FileStream filestream = new FileStream(filename, FileMode.Open))
            {
                Stream sourcestream = filestream; 
                if (preload)
                {
                    MemoryStream memorystream = new MemoryStream();
                    filestream.CopyTo(memorystream);
                    memorystream.Position = 0;
                    sourcestream = memorystream;
                }

                using (BinaryReader binaryReader = new BinaryReader(sourcestream))
                {
                    DexReader reader = new DexReader(result);
                    reader.ReadFrom(binaryReader);
                    return result;
                }
            }
        }

        public Dex()
        {
            Classes = new List<ClassDefinition>();
            TypeReferences = new List<TypeReference>();
            FieldReferences = new List<FieldReference>();
            MethodReferences = new List<MethodReference>();

            Header = new DexHeader();
            Map = new Map();
            Prototypes = new List<Prototype>();
            Strings = new List<string>();
        }

        public ClassDefinition GetClass(string fullname)
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

        public TypeReference Import(TypeReference tref) {
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

        public MethodReference Import(MethodReference mref)
        {
            foreach (MethodReference item in MethodReferences)
            {
                if (mref.Equals(item))
                {
                    return item;
                }
            }
            MethodReferences.Add(mref);
            return mref;
        }

    }
}
