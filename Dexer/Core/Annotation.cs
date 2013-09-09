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
using System.Text;
using Dexer.Metadata;
using System;

namespace Dexer.Core
{
    public class Annotation : IEquatable<Annotation>
    {
        public ClassReference Type { get; set; }
        public List<AnnotationArgument> Arguments { get; set; }
        public AnnotationVisibility Visibility { get; set; }

        public Annotation()
        {
            Arguments = new List<AnnotationArgument>();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Type);
            builder.Append("(");
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(Arguments[i]);
            }
            builder.Append(")");
            return builder.ToString();
        }

        #region " IEquatable "
        public bool Equals(Annotation other)
        {
            bool result = Type.Equals(other.Type) && Arguments.Count.Equals(other.Arguments.Count);
            if (result)
            {
                for (int i = 0; i < Arguments.Count; i++)
                    result = result && Arguments[i].Equals(other.Arguments[i]);
            }
            return result;
        }
        #endregion

        #region " Object "
        public override bool Equals(object obj)
        {
            if (obj is Annotation)
                return Equals(obj as Annotation);

            return false;
        }

        public override int GetHashCode()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(TypeDescriptor.Encode(Type));

            foreach (AnnotationArgument argument in Arguments)
            {
                builder.Append(String.Format("{0}=", argument.Name));
                if (ValueFormat.GetFormat(argument.Value) == ValueFormats.Array)
                {
                    Array array = argument.Value as Array;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (i > 0)
                            builder.Append(",");
                        builder.Append(array.GetValue(i));
                    }
                } else
                    builder.Append(argument.Value);
                builder.AppendLine();
            }

            return builder.ToString().GetHashCode();
        }
        #endregion

    }
}
