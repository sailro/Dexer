using System.Collections.Generic;
using System;
using System.Text;

namespace Dexer.Core
{
	public class Prototype
	{
        public TypeReference ReturnType { get; set; }
        public IList<Parameter> Parameters { get; set; }

        public Prototype()
        {
            Parameters = new List<Parameter>();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(");
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (i>0)
                    builder.Append(",");

                builder.Append(Parameters[i]);
            }
            builder.Append(")");
            builder.Append(" : ");
            builder.Append(ReturnType);
            return builder.ToString();
        }
	}
}
