/* Dexer Copyright (c) 2010 Sebastien LEBRETON

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

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
