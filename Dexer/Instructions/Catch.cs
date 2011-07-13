/* Dexer Copyright (c) 2010-2011 Sebastien LEBRETON

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

using Dexer.Core;
using System;
using System.Text;
using Dexer.Metadata;

namespace Dexer.Instructions
{
    public class Catch : ICloneable, IEquatable<Catch>
    {
        public TypeReference Type { get; set; }
        public Instruction Instruction { get; set; }

        #region " ICloneable "
        internal Catch Clone()
        {
            return (Catch)(this as ICloneable).Clone();
        }

        object ICloneable.Clone()
        {
            Catch result = new Catch();
            
            result.Type = this.Type;
            result.Instruction = this.Instruction;

            return result;
        }
        #endregion

        #region " IEquatable "
        public bool Equals(Catch other)
        {
            return Type.Equals(other.Type)
                && Instruction.Equals(other.Instruction);
        }
        #endregion

        #region " Object "
        public override bool Equals(object obj)
        {
            if (obj is Catch)
                return Equals(obj as Catch);

            return false;
        }

        public override int GetHashCode()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(TypeDescriptor.Encode(Type));
            builder.AppendLine(Instruction.GetHashCode().ToString());

            return builder.ToString().GetHashCode();
        }
        #endregion

    }
}
