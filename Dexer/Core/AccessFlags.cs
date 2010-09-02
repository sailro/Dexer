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
        Strict = 0x800,
        Synthetic = 0x1000,
        Annotation = 0x2000,
        Enum = 0x4000,
        Unused = 0x8000,
        Constructor = 0x10000,
        DeclaredSynchronized = 0x20000
    }
}
