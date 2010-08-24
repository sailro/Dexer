using System.Collections.Generic;

namespace Dexer.Core
{
    public class MethodDefinition : MethodReference, IAnnotationProvider
    {
        public bool IsDirect { get; set; }
        public bool IsVirtual
        {
            get { return !IsDirect; }
            set { IsDirect = !value; }
        }

        public AccessFlags AccessFlags { get; set; }
        public new ClassDefinition Owner
        {
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

        internal MethodDefinition()
        {
            Annotations = new List<Annotation>();
        }

        internal MethodDefinition(MethodReference mref) : this()
        {
            this.Owner = mref.Owner as ClassDefinition;
            this.Name = mref.Name;
            this.Prototype = mref.Prototype;
        }
    }
}
