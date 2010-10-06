﻿/*
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