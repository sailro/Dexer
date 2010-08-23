using System.Collections.Generic;

namespace Dexer.Core
{
    public class FieldDefinition : FieldReference
    {
        public bool IsInstance { get; set;  }
        public bool IsStatic {
            get { return !IsInstance; }
            set { IsInstance = !value;  }
        }

        public AccessFlags AccessFlag { get; set; }
        public ClassDefinition ClassDefinition { get; set; }
        public IEnumerable<Annotation> Annotations { get; set; }
    }
}
