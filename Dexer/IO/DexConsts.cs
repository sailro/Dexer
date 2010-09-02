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

namespace Dexer.IO
{
    internal static class DexConsts
    {
        internal static readonly byte[] DexFileMagic = { 0x64, 0x65, 0x78, 0x0a, 0x30, 0x33, 0x35, 0x00 };
        internal const uint Endian = 0x12345678;
        internal const uint ReverseEndian = 0x78563412;
        internal const uint NoIndex = 0xffffffff;
        internal const char InnerClassMarker = '$';
    }
}
