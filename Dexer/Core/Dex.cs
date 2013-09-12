/* Dexer Copyright (c) 2010-2013 Sebastien LEBRETON

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

using System.Collections.Generic;
using System.IO;
using Dexer.Extensions;
using Dexer.IO;

namespace Dexer.Core
{
    public class Dex
    {
        public List<ClassDefinition> Classes { get; internal set; }

        internal List<TypeReference> TypeReferences { get; set; }
        internal List<FieldReference> FieldReferences { get; set; }
        internal List<MethodReference> MethodReferences { get; set; }

        internal List<string> Strings { get; set; }
        internal List<Prototype> Prototypes { get; set; }

        public static Dex Read(string filename)
        {
            return Read(filename, true);
        }

        public void Write(string filename)
        {
            Write(filename, true);
        }

        public static Dex Read(string filename, bool bufferize)
        {
            var result = new Dex();

            using (var filestream = new FileStream(filename, FileMode.Open))
            {
                Stream sourcestream = filestream; 
                if (bufferize)
                {
                    var memorystream = new MemoryStream();
                    filestream.CopyTo(memorystream);
                    memorystream.Position = 0;
                    sourcestream = memorystream;
                }

                using (var binaryReader = new BinaryReader(sourcestream))
                {
                    var reader = new DexReader(result);
                    reader.ReadFrom(binaryReader);
                    return result;
                }
            }
        }

        public void Write(string filename, bool bufferize)
        {
            using (var filestream = new FileStream(filename, FileMode.Create))
            {
                Stream deststream = filestream;
                MemoryStream memorystream = null;

                if (bufferize)
                {
                    memorystream = new MemoryStream();
                    deststream = memorystream;
                }

                using (var binaryWriter = new BinaryWriter(deststream))
                {
                    var writer = new DexWriter(this);
                    writer.WriteTo(binaryWriter);

	                if (!bufferize)
						return;
	                
					memorystream.Position = 0;
	                memorystream.CopyTo(filestream);
                }

            }
        }

        public Dex()
        {
            Classes = new List<ClassDefinition>();
            TypeReferences = new List<TypeReference>();
            FieldReferences = new List<FieldReference>();
            MethodReferences = new List<MethodReference>();

            Prototypes = new List<Prototype>();
            Strings = new List<string>();
        }

        public ClassDefinition GetClass(string fullname)
        {
            return GetClass(fullname, Classes);
        }

        internal ClassDefinition GetClass(string fullname, List<ClassDefinition> container)
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

        internal TypeReference Import(TypeReference tref, bool add)
        {
            foreach (TypeReference item in TypeReferences)
            {
                if (tref.Equals(item))
                {
                    return item;
                }
            }
            if (add)
            {
                // if !add see TypeDescriptor comment 
                TypeReferences.Add(tref);
            }
            return tref;
        }

        public ClassReference Import(ClassReference cref)
        {
            return (ClassReference)Import(cref, true);
        }

        public TypeReference Import(TypeReference tref) {
            return Import(tref, true);
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
