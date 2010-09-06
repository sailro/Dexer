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
