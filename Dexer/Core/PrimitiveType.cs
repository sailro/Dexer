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

using Dexer.Metadata;

namespace Dexer.Core
{
    public class PrimitiveType : TypeReference
    {
        public static readonly PrimitiveType Void = new PrimitiveType(TypeDescriptors.Void);
        public static readonly PrimitiveType Boolean = new PrimitiveType(TypeDescriptors.Boolean);
        public static readonly PrimitiveType Byte = new PrimitiveType(TypeDescriptors.Byte);
        public static readonly PrimitiveType Short = new PrimitiveType(TypeDescriptors.Short);
        public static readonly PrimitiveType Char = new PrimitiveType(TypeDescriptors.Char);
        public static readonly PrimitiveType Int = new PrimitiveType(TypeDescriptors.Int);
        public static readonly PrimitiveType Long = new PrimitiveType(TypeDescriptors.Long);
        public static readonly PrimitiveType Float = new PrimitiveType(TypeDescriptors.Float);
        public static readonly PrimitiveType Double = new PrimitiveType(TypeDescriptors.Double);

        private PrimitiveType(TypeDescriptors typeDescriptor)
        {
            this.TypeDescriptor = typeDescriptor;
        }

        public override string ToString()
        {
            return this.TypeDescriptor.ToString();
        }

        #region " IEquatable "
        public bool Equals(PrimitiveType other)
        {
            return base.Equals(other)
                && this.TypeDescriptor == other.TypeDescriptor;
        }

        public override bool Equals(TypeReference other)
        {
            return (other is PrimitiveType)
                && this.Equals(other as PrimitiveType);
        }
        #endregion

    }
}
