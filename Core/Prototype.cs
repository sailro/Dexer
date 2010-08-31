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
using System.Text;
using System;

namespace Dexer.Core
{
    public class Prototype : ICloneable, IEquatable<Prototype>
	{
        public TypeReference ReturnType { get; set; }
        public IList<Parameter> Parameters { get; set; }

        public Prototype()
        {
            Parameters = new List<Parameter>();
        }

        public Prototype(TypeReference returntype, params Parameter[] parameters)
            : this()
        {
            ReturnType = returntype;
            Parameters = new List<Parameter>(parameters);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(");
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i>0)
                    builder.Append(", ");

                builder.Append(Parameters[i]);
            }
            builder.Append(")");
            builder.Append(" : ");
            builder.Append(ReturnType);
            return builder.ToString();
        }

        internal Prototype Clone()
        {
            return (Prototype)(this as ICloneable).Clone();
        }

        object ICloneable.Clone()
        {
            Prototype result = new Prototype();
            result.ReturnType = this.ReturnType;

            foreach (Parameter p in Parameters)
            {
                result.Parameters.Add(p.Clone());
            }

            return result;
        }

        public bool Equals(Prototype other)
        {
            bool result = ReturnType.Equals(other.ReturnType) && Parameters.Count.Equals(other.Parameters.Count);
            if (result) {
                for (int i = 0; i<Parameters.Count; i++)
                    result = result && Parameters[i].Equals(other.Parameters[i]);
            }
            return result;
        }
    }
}
