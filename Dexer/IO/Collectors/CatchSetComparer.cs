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

using System;
using System.Collections.Generic;

namespace Dexer.IO.Collector
{
    internal class CatchSetComparer : IComparer<CatchSet>
    {
        public List<int> CollectOffsets(CatchSet set)
        {
            var result = new List<int>();

            foreach (var @catch in set)
                result.Add(@catch.Instruction.Offset);

            if (set.CatchAll != null)
                result.Add(set.CatchAll.Offset);

            return result;
        }

        public int Compare(CatchSet x, CatchSet y)
        {
            var xOffsets = CollectOffsets(x);
            var yOffsets = CollectOffsets(y);

            int minp = Math.Min(xOffsets.Count, yOffsets.Count);
            for (int i = 0; i < minp; i++)
            {
                int cp = xOffsets[i].CompareTo(yOffsets[i]);
                if (cp != 0)
                    return cp;
            }

            return 0;
        }
    }
}
