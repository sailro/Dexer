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
