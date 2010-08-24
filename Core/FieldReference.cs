using System.Collections.Generic;
using System;
using System.Text;

namespace Dexer.Core
{
    public class FieldReference
    {
        public ClassReference Owner { get; set; }
        public TypeReference Type { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Owner);
            builder.Append("::");
            builder.Append(Name);
            builder.Append(" : ");
            builder.Append(Type);
            return builder.ToString();
        }
    }
}
