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
using System.Collections.Generic;
using System.Text;

namespace Dexer.Instructions
{
    public class DebugInstruction
    {
        public DebugOpCodes OpCode { get; set; }
        public IList<object> Operands { get; set; }

        public DebugInstruction()
        {
            Operands = new List<object>();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(OpCode.ToString());
            for (int i = 0; i < Operands.Count; i++)
            {
                builder.Append(" ");
                builder.Append(Operands[i]);
            }
            return builder.ToString();
        }

    }
}
