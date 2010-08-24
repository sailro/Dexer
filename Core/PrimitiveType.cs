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

        public override bool Equals(TypeReference other)
        {
            return (other is PrimitiveType) && (this.TypeDescriptor == other.TypeDescriptor);
        }
    }
}
