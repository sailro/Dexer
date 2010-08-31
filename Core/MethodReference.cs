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

using System.Text;
using System;

namespace Dexer.Core
{
    public class MethodReference : MemberReference, IEquatable<MethodReference>
    {
        public Prototype Prototype { get; set; }

        public MethodReference() : base()
        {
        }

        public MethodReference(ClassReference owner, string name, Prototype prototype) : base(owner, name)
        {
            Prototype = prototype;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Owner);
            builder.Append("::");
            builder.Append(Name);
            builder.Append(Prototype);
            return builder.ToString();
        }

        public bool Equals(MethodReference other)
        {
            return other.Prototype.Equals(Prototype) && base.Equals(other);
        }
    }
}
