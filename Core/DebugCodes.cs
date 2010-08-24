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

namespace Dexer.Core
{
    public enum DebugCodes
    {
        EndSequence = 0x00,
        AdvancePc = 0x01,
        AdvanceLine = 0x02,
        StartLocal = 0x03,
        StartLocalExtended = 0x04,
        EndLocal = 0x05,
        RestartLocal = 0x06,
        SetPrologueEnd = 0x07,
        SetEpilogueBegin = 0x08,
        SetFile = 0x09,
        Firstpecial = 0x0a
    }
}
