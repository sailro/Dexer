using System.Collections.Generic;

namespace Dexer.Core
{
    public class Annotation
    {
        public TypeReference Type { get; set; }
        public ICollection<AnnotationArgument> Arguments { get; set; }
        public AnnotationVisibility Visibility { get; set; }
    }
}
