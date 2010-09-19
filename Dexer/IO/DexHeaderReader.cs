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

using System.IO;
using Dexer.Core;
using Dexer.Extensions;

namespace Dexer.IO
{
    internal class DexHeaderReader
    {

        private DexHeader Item { get; set; }

        public DexHeaderReader(DexHeader item)
        {
            Item = item;
        }

        public void ReadFrom(BinaryReader reader)
        {
            Item.Magic = reader.ReadBytes(DexConsts.FileMagic.Length);

            if (!Item.Magic.Match(DexConsts.FileMagic, 0))
                throw new MalformedException("Unexpected Magic number");


            Item.CheckSum = reader.ReadUInt32();
            Item.Signature = reader.ReadBytes(DexConsts.SignatureSize);

            Item.FileSize = reader.ReadUInt32();
            Item.HeaderSize = reader.ReadUInt32();
            Item.EndianTag = reader.ReadUInt32();
            
            if (Item.EndianTag != DexConsts.Endian)
                throw new MalformedException("Only Endian-encoded files are supported");

            Item.LinkSize = reader.ReadUInt32();
            Item.LinkOffset = reader.ReadUInt32();

            Item.MapOffset = reader.ReadUInt32();

            Item.StringsSize = reader.ReadUInt32();
            Item.StringsOffset = reader.ReadUInt32();

            Item.TypeReferencesSize = reader.ReadUInt32();
            Item.TypeReferencesOffset = reader.ReadUInt32();

            Item.PrototypesSize = reader.ReadUInt32();
            Item.PrototypesOffset = reader.ReadUInt32();

            Item.FieldReferencesSize = reader.ReadUInt32();
            Item.FieldReferencesOffset = reader.ReadUInt32();

            Item.MethodReferencesSize = reader.ReadUInt32();
            Item.MethodReferencesOffset = reader.ReadUInt32();

            Item.ClassDefinitionsSize = reader.ReadUInt32();
            Item.ClassDefinitionsOffset = reader.ReadUInt32();

            Item.DataSize = reader.ReadUInt32();
            Item.DataOffset = reader.ReadUInt32();
        }
    }
}
