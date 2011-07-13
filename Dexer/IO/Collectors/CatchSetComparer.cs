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
