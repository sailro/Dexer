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
using Dexer.Metadata;

namespace Dexer.IO
{
    internal class MapReader
    {

        private Dex Dex { get; set; }

        public MapReader(Dex dex)
        {
            Dex = dex;
        }

        public void ReadFrom(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(Dex.Header.MapOffset, () =>
            {
                uint mapsize = reader.ReadUInt32();
                for (int i = 0; i < mapsize; i++)
                {
                    MapItem item = new MapItem();
                    item.Type = (TypeCodes)reader.ReadUInt16();
                    reader.ReadUInt16(); // unused
                    item.Size = reader.ReadUInt32();
                    item.Offset = reader.ReadUInt32();
                    Dex.Map.Add(item.Type, item);
                }
            });
        }
    }
}
