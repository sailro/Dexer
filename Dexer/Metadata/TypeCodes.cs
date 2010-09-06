/* Dexer Copyright (c) 2010 Sebastien LEBRETON

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

namespace Dexer.Metadata
{
    public enum TypeCodes
    {
        Header = 0x0000,
        StringId = 0x0001,
        TypeId = 0x0002,
        ProtoId = 0x0003,
        FieldId = 0x0004,
        MethodId = 0x0005,
        ClassDef = 0x0006,
        MapList = 0x1000,
        TypeList = 0x1001,
        AnnotationSetRefList = 0x1002,
        AnnotationSet = 0x1003,
        ClassData = 0x2000,
        Code = 0x2001,
        StringData = 0x2002,
        DebugInfo = 0x2003,
        Annotation = 0x2004,
        EncodedArray = 0x2005,
        AnnotationDirectory = 0x2006
    }
}
