using System.Collections.Generic;
using System;
using Dexer.Core;
using Dexer.Metadata;

namespace Dexer.Core
{
    public class ClassDefinition : ClassReference, IAnnotationProvider
	{
        public AccessFlags AccessFlag { get; set; }
        public ClassReference SuperClass { get; set; }
        public IList<ClassReference> Interfaces { get; set; }
        public string SourceFile { get; set; }
        public IList<Annotation> Annotations { get; set; }
        public IList<FieldDefinition> Fields { get; set; }
        public IList<MethodDefinition> Methods { get; set; }

        internal ClassDefinition()
        {
            TypeDescriptor = TypeDescriptors.FullyQualifiedName;

            Interfaces = new List<ClassReference>();
            Annotations = new List<Annotation>();
            Fields = new List<FieldDefinition>();
            Methods = new List<MethodDefinition>();
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
    }
}
