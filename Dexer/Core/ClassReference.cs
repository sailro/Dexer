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

using System;
using System.Text;
using Dexer.Metadata;

namespace Dexer.Core
{
    public class ClassReference : CompositeType, IMemberReference
    {
        public const char NamespaceSeparator = '.';
        public const char InternalNamespaceSeparator = '/';

        public string Namespace { get; set; }
        public string Name { get; set; }

        public string Fullname
        {
            get
            {
                StringBuilder result = new StringBuilder(Namespace);
                if (result.Length > 0)
                    result.Append(NamespaceSeparator);
                result.Append(Name);
                return result.ToString();
            }
            set
            {
                value = value.Replace(InternalNamespaceSeparator, NamespaceSeparator);
                string[] items = value.Split(NamespaceSeparator);
                if (items.Length > 0)
                {
                    Name = items[items.Length - 1];
                    Array.Resize(ref items, items.Length - 1);
                    Namespace = string.Join(NamespaceSeparator.ToString(), items);
                }
                else
                {
                    Name = string.Empty;
                    Namespace = string.Empty;
                }
            }
        }

        public override string ToString()
        {
            return Fullname;
        }

        public ClassReference()
        {
            TypeDescriptor = TypeDescriptors.FullyQualifiedName;
        }

        public ClassReference(string fullname) : this()
        {
            Fullname = fullname;
        }

        #region " IEquatable "
        public bool Equals(ClassReference other)
        {
            return base.Equals(other)
                && this.Fullname == other.Fullname;
        }

        public override bool Equals(TypeReference other)
        {
            return (other is ClassReference)
                && this.Equals(other as ClassReference);
        }

        public bool Equals(IMemberReference other)
        {
            return (other is ClassReference)
                && this.Equals(other as ClassReference);
        }
        #endregion
    }
}
