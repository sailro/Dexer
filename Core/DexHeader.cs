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
