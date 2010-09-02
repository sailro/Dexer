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
