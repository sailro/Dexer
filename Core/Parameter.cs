using System.Collections.Generic;
using System;

namespace Dexer.Core
{
    public class Parameter : IAnnotationProvider
    {
        public TypeReference Type { get; set; }
        public IList<Annotation> Annotations { get; set; }

        public override string ToString()
        {
            return Type.ToString();
        }

        internal Parameter()
        {
            Annotations = new List<Annotation>();
        }

    }
}
