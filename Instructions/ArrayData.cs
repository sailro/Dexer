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

using Dexer.Core;

namespace Dexer.Instructions
{
    public class ArrayData
    {
        public int ElementSize { get; set; }
        public int ElementCount { get; set; }
        public byte[] Blob { get; set; }

        public ArrayData(int elementsize, int elementcount, byte[] blob)
        {
            this.ElementSize = elementsize;
            this.ElementCount = elementcount;
            this.Blob = blob;
        }

        public override string ToString()
        {
            return string.Concat("[", ElementSize, 'x', ElementCount, "]");
        }
    }
}
