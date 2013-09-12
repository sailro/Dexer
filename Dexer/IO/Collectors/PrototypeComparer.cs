/* Dexer Copyright (c) 2010-2013 Sebastien LEBRETON

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

using System.Collections.Generic;
using Dexer.Core;
using System;

namespace Dexer.IO.Collectors
{
    internal class PrototypeComparer : IComparer<Prototype>
    {
        private readonly TypeReferenceComparer _typeReferenceComparer = new TypeReferenceComparer();

        public int Compare(Prototype x, Prototype y)
        {
            var crt = _typeReferenceComparer.Compare(x.ReturnType, y.ReturnType);
            if (crt == 0)
            {
                if (x.Parameters.Count == 0 && y.Parameters.Count != 0)
                    return -1;

                if (y.Parameters.Count == 0 && x.Parameters.Count != 0)
                    return 1;

                var minp = Math.Min(x.Parameters.Count, y.Parameters.Count);
                for (var i = 0; i < minp; i++)
                {
                    var cp = _typeReferenceComparer.Compare(x.Parameters[i].Type,y.Parameters[i].Type);
                    if (cp != 0)
                        return cp;
                }
                return x.Parameters.Count.CompareTo(y.Parameters.Count);
            }
            return crt;
        }
    }
}
