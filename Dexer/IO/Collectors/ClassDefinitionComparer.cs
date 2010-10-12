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

namespace Dexer.IO.Collector
{
    internal class ClassDefinitionComparer : ClassReferenceComparer, IPartialComparer<ClassDefinition>, IComparer<ClassDefinition>
    {
        public int Compare(ClassDefinition x, ClassDefinition y)
        {
            return base.Compare(x, y);
        }

        public List<ClassDefinition> CollectDependencies(ClassDefinition cdef)
        {
            DependencyCollector collector = new DependencyCollector();
            collector.Collect(cdef);
            var result = collector.ToList();
            result.Remove(cdef);
            return result;
        }

        public int? PartialCompare(ClassDefinition x, ClassDefinition y)
        {
            var xdependencies = CollectDependencies(x);
            var ydependencies = CollectDependencies(y);

            if (ydependencies.Contains(x))
            {
                if (xdependencies.Contains(y))
                    return 0;
                return -1;
            }

            if (xdependencies.Contains(y))
                return 1;

            return null;
        }
    }
}
