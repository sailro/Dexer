/* Dexer Copyright (c) 2010-2013 Sebastien LEBRETON

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
using System.Linq;
using System.Text;
using System;
using Dexer.Metadata;

namespace Dexer.Core
{
    public class Prototype : ICloneable, IEquatable<Prototype>
	{
        public TypeReference ReturnType { get; set; }
        public List<Parameter> Parameters { get; set; }

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

        public bool ContainsAnnotation()
        {
	        return Parameters.Any(parameter => parameter.Annotations.Count > 0);
        }

	    public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("(");
            for (var i = 0; i < Parameters.Count; i++)
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

        #region " ICloneable "
        internal Prototype Clone()
        {
            return (Prototype)(this as ICloneable).Clone();
        }

        object ICloneable.Clone()
        {
            var result = new Prototype {ReturnType = ReturnType};

	        foreach (var p in Parameters)
            {
                result.Parameters.Add(p.Clone());
            }

            return result;
        }
        #endregion

        #region " IEquatable "
        public bool Equals(Prototype other)
        {
            var result = ReturnType.Equals(other.ReturnType) && Parameters.Count.Equals(other.Parameters.Count);
            if (result)
            {
                for (var i = 0; i < Parameters.Count; i++)
                    result = result && Parameters[i].Equals(other.Parameters[i]);
            }
            return result;
        }
        #endregion

        #region " Object "
        public override bool Equals(object obj)
        {
            if (obj is Prototype)
                return Equals(obj as Prototype);

            return false;
        }

        public override int GetHashCode()
        {
            var builder = new StringBuilder();
            builder.AppendLine(TypeDescriptor.Encode(ReturnType));

            foreach (var parameter in Parameters)
                builder.AppendLine(TypeDescriptor.Encode(parameter.Type));

            return builder.ToString().GetHashCode();
        }
        #endregion

    }
}
