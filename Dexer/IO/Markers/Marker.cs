/* Dexer Copyright (c) 2010-2011 Sebastien LEBRETON

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
using Dexer.Extensions;
using System.Collections.Generic;

namespace Dexer.IO.Markers
{

    internal abstract class Marker<T>
    {
        public BinaryWriter Writer { get; set; }
        public List<uint> Positions { get; set; }

        public abstract T Value { set; }

        public abstract void Allocate();

        public void CloneMarker()
        {
            uint position = (uint)Writer.BaseStream.Position;
            Positions.Add(position);
            Allocate();
        }

        public Marker(BinaryWriter writer)
        {
            Writer = writer;
            Positions = new List<uint>();
            CloneMarker();
        }
    }

}
