using System.Collections.Generic;

namespace Dexer.Core
{
    public class Annotation
    {
        public ClassReference Type { get; set; }
        public IList<AnnotationArgument> Arguments { get; set; }
        public AnnotationVisibility Visibility { get; set; }

        internal Annotation()
        {
            Arguments = new List<AnnotationArgument>();
        }
    }
}
