using System.Collections.Generic;
using System;
using System.Text;

namespace Dexer.Core
{
    public class MethodReference
    {
        public ClassReference Owner { get; set; }
        public string Name { get; set; }
        public Prototype Prototype { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Owner);
            builder.Append("::");
            builder.Append(Name);
            builder.Append(Prototype);
            return builder.ToString();
        }

    }
}
