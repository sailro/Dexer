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
using System.Collections.Generic;
using Dexer.Core;
using Dexer.Metadata;

namespace Dexer.IO.Collector
{
    internal class AnnotationComparer : IComparer<Annotation>
    {
        private TypeReferenceComparer typeReferenceComparer = new TypeReferenceComparer();
        private ArgumentComparer argumentComparer = new ArgumentComparer();

        public int Compare(Annotation x, Annotation y)
        {
            int result = typeReferenceComparer.Compare(x.Type, y.Type);

            if (result == 0)
                result = x.Visibility.CompareTo(y.Visibility);

            if (result != 0)
                return result;

            for (int i = 0; i < Math.Min(x.Arguments.Count, y.Arguments.Count); i++)
            {
                result = argumentComparer.Compare(x.Arguments[i], y.Arguments[i]);
                if (result != 0)
                    return result;
            }

            if (x.Arguments.Count > y.Arguments.Count)
                return 1;

            if (y.Arguments.Count > x.Arguments.Count)
                return -1;
            
            return result;
        }
    }
}
