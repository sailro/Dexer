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
using System.IO;
using Dexer.Core;
using Dexer.Extensions;

namespace Dexer.IO
{
    public class DexWriter : IBinaryWriteable
    {
        private Dex Dex { get; set; }

        public UintMarker CheckSumMarker { get; set; }
        public UintMarker FileSizeMarker { get; set; }
        public SignatureMarker SignatureMarker { get; set; }
        public SizeOffsetMarker LinkMarker { get; set; }
        public UintMarker MapMarker { get; set; }
        public SizeOffsetMarker StringsMarker { get; set; }
        public SizeOffsetMarker TypeReferencesMarker { get; set; }
        public SizeOffsetMarker PrototypesMarker { get; set; }
        public SizeOffsetMarker FieldReferencesMarker { get; set; }
        public SizeOffsetMarker MethodReferencesMarker { get; set; }
        public SizeOffsetMarker ClassDefinitionsMarker { get; set; }
        public SizeOffsetMarker DataMarker { get; set; }

        public DexWriter(Dex dex)
        {
            Dex = dex;
        }

        public void WriteTo(BinaryWriter writer)
        {
            /* 
            Header
            StringId
            TypeId
            ProtoId 
            FieldId
            MethodId
            ClassDef
            Code
            TypeList
            StringData
            ClassData
            MapList
            */

            writer.Write(DexConsts.FileMagic);
            CheckSumMarker = writer.MarkUint(); 
            SignatureMarker = writer.MarkSignature(); 
            FileSizeMarker = writer.MarkUint(); 
            writer.Write(DexConsts.HeaderSize); 
            writer.Write(DexConsts.Endian); 
            LinkMarker = writer.MarkSizeOffset(); 
            MapMarker = writer.MarkUint(); 

            StringsMarker = writer.MarkSizeOffset(); 
            TypeReferencesMarker = writer.MarkSizeOffset(); 
            PrototypesMarker = writer.MarkSizeOffset(); 
            FieldReferencesMarker = writer.MarkSizeOffset(); 
            MethodReferencesMarker = writer.MarkSizeOffset(); 
            ClassDefinitionsMarker = writer.MarkSizeOffset(); 
            DataMarker = writer.MarkSizeOffset(); 
        }

    }
}
