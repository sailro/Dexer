using System.Collections.Generic;

namespace Dexer.Core
{
    interface IAnnotationProvider
    {
        IList<Annotation> Annotations { get; set; }
    }
}
