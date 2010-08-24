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
    public enum ValueFormats : byte
    {
        Byte = 0x00,
        Short = 0x02,
        Char = 0x03,
        Int = 0x04,
        Long = 0x06,
        Float = 0x10,
        Double = 0x11,
        String = 0x17,
        Type = 0x18,
        Field = 0x19,
        Method = 0x1a,
        Enum = 0x1b,
        Array = 0x1c,
        Annotation = 0x1d,
        Null = 0x1e,
        Boolean = 0x1f
    }
}
