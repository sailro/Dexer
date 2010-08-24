using System.Collections.Generic;

namespace Dexer.Core
{
    public class FieldDefinition : FieldReference, IAnnotationProvider
    {
        public bool IsInstance { get; set;  }
        public bool IsStatic {
            get { return !IsInstance; }
            set { IsInstance = !value;  }
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
