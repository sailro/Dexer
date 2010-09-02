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

namespace Dexer.Core
{
    public class FieldDefinition : FieldReference, IAnnotationProvider
    {
        public bool IsStatic {
            get
            {
                return (AccessFlags & AccessFlags.Static) != 0;
            }
        }

        public AccessFlags AccessFlags { get; set; }
        public new ClassDefinition Owner {
            get
            {
                return base.Owner as ClassDefinition;
            }
            set
            {
                base.Owner = value;
            }
        }

        public IList<Annotation> Annotations { get; set; }
        public object Value { get; set; }

        internal FieldDefinition()
        {
            Annotations = new List<Annotation>();
        }

        internal FieldDefinition(FieldReference fref) : this()
        {
            this.Owner = fref.Owner as ClassDefinition;
            this.Type = fref.Type;
            this.Name = fref.Name;
        }
    }
}
