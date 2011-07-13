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

namespace Dexer.Core
{
    [Flags]
    public enum AccessFlags : uint
    {
        Public = 0x1,
        Private = 0x2,
        Protected = 0x4,
        Static = 0x8,
        Final = 0x10,
        Synchronized = 0x20,
        Volatile = 0x40,
        Bridge = 0x40,
        Transient = 0x80,
        VarArgs = 0x80,
        Native = 0x100,
        Interface = 0x200,
        Abstract = 0x400,
        StrictFp = 0x800,
        Synthetic = 0x1000,
        Annotation = 0x2000,
        Enum = 0x4000,
        Unused = 0x8000,
        Constructor = 0x10000,
        DeclaredSynchronized = 0x20000
    }
}
