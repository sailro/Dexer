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
using System;

namespace Dexer.Instructions
{
    public class Catch : ICloneable
    {
        public TypeReference Type { get; set; }
        public Instruction Instruction { get; set; }

        internal Catch Clone()
        {
            return (Catch)(this as ICloneable).Clone();
        }

        object ICloneable.Clone()
        {
            Catch result = new Catch();
            
            result.Type = this.Type;
            result.Instruction = this.Instruction;

            return result;
        }

    }
}
