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
using System;

namespace Dexer.Core
{
    public class Parameter : ICloneable, IEquatable<Parameter>
    {
        public IList<Annotation> Annotations { get; set; }
        public TypeReference Type { get; set; }

        public Parameter()
        {
            Annotations = new List<Annotation>();
        }

        public Parameter(TypeReference type) : this()
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        internal Parameter Clone()
        {
            return (Parameter)(this as ICloneable).Clone();
        }

        object ICloneable.Clone()
        {
            Parameter result = new Parameter();
            result.Type = this.Type;

            if (Annotations.Count > 0)
            {
                // Cloning is here to annotate properly parameters
                throw new ArgumentException();
            }

            return result;
        }

        public bool Equals(Parameter other)
        {
            // do not check annotations at this time.
            return Type.Equals(other.Type);
        }
    }
}
