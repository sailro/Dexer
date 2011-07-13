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

namespace Dexer.Core
{
    internal class DexHeader
    {
        internal byte[] Magic { get; set; }
        internal uint CheckSum { get; set; }
        internal byte[] Signature { get; set; }

        internal uint FileSize { get; set; }
        internal uint HeaderSize { get; set; }
        internal uint EndianTag { get; set; }

        internal uint LinkSize { get; set; }
        internal uint LinkOffset { get; set; }

        internal uint MapOffset { get; set; }

        internal uint StringsSize { get; set; }
        internal uint StringsOffset { get; set; }

        internal uint TypeReferencesSize { get; set; }
        internal uint TypeReferencesOffset { get; set; }

        internal uint PrototypesSize { get; set; }
        internal uint PrototypesOffset { get; set; }

        internal uint FieldReferencesSize { get; set; }
        internal uint FieldReferencesOffset { get; set; }

        internal uint MethodReferencesSize { get; set; }
        internal uint MethodReferencesOffset { get; set; }

        internal uint ClassDefinitionsSize { get; set; }
        internal uint ClassDefinitionsOffset { get; set; }

        internal uint DataSize { get; set; }
        internal uint DataOffset { get; set; }
    }
}
