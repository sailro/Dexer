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
using Dexer.Metadata;
using System.Linq;

namespace Dexer.Core
{
    public class ClassDefinition : ClassReference, IAnnotationProvider
	{
        public AccessFlags AccessFlag { get; set; }
        public ClassReference SuperClass { get; set; }
        public IList<ClassDefinition> InnerClasses { get; set; }
        public IList<ClassReference> Interfaces { get; set; }
        public string SourceFile { get; set; }
        public IList<Annotation> Annotations { get; set; }
        public IList<FieldDefinition> Fields { get; set; }
        public IList<MethodDefinition> Methods { get; set; }
        public ClassDefinition Owner { get; set; }

        internal ClassDefinition()
        {
            TypeDescriptor = TypeDescriptors.FullyQualifiedName;

            Interfaces = new List<ClassReference>();
            Annotations = new List<Annotation>();
            Fields = new List<FieldDefinition>();
            Methods = new List<MethodDefinition>();
            InnerClasses = new List<ClassDefinition>();
        }

        internal ClassDefinition(ClassReference cref)
            : this()
        {
            this.Fullname = cref.Fullname;
            this.Namespace = cref.Namespace;
            this.Name = cref.Name;
        }

        public override bool Equals(TypeReference other)
        {
            return (other is ClassDefinition) && (base.Equals(other));
        }

        public IEnumerable<MethodDefinition> GetMethods(string name)
        {
            foreach (var mdef in Methods)
                if (mdef.Name == name)
                    yield return mdef;
        }

        public MethodDefinition GetMethod(string name)
        {
            return Enumerable.First(GetMethods(name));
        }
    }
}
