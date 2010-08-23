using System.Collections.Generic;

namespace Dexer.Core
{
    public class MethodDefinition : MethodReference
    {
        public bool IsDirect { get; set; }
        public bool IsVirtual
        {
            get { return !IsDirect; }
            set { IsDirect = !value; }
        }

        public AccessFlags AccessFlag { get; set; }
        public ClassDefinition ClassDefinition { get; set; }
        public IEnumerable<Annotation> Annotations { get; set; }
    }
}
