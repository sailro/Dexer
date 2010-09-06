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
