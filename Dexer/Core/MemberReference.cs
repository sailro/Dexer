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

using System.Text;
using System;

namespace Dexer.Core
{
    public abstract class MemberReference: TypeReference
    {
        public ClassReference Owner { get; set; }
        public string Name { get; set; }

        internal MemberReference()
        {
        }

        internal MemberReference(ClassReference owner, string name)
        {
            Owner = owner;
            Name = name;
        }

        #region " IEquatable "
        public bool Equals(MemberReference other)
        {
            return base.Equals(other)
                && ((Owner == null && other.Owner == null) || Owner.Equals(other.Owner))
                && Name.Equals(other.Name);
        }

        public override bool Equals(TypeReference other)
        {
            return (other is MemberReference)
                && this.Equals(other as MemberReference);
        }
        #endregion

    }
}
