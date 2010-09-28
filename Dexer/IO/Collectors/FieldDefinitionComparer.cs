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

using System.Collections.Generic;
using Dexer.Core;
using Dexer.Metadata;

namespace Dexer.IO.Collector
{
    internal class FieldDefinitionComparer : FieldReferenceComparer, IComparer<FieldDefinition>
    {
        public int Compare(FieldDefinition x, FieldDefinition y)
        {
            int result = y.IsStatic.CompareTo(x.IsStatic);

            if (result == 0)
                result = base.Compare(x, y);

            return result;
        }
    }
}
