using System.Text;
using System;
using Dexer.Metadata;
using System.Collections.Generic;

namespace Dexer.Core
{
    public class ClassReference : TypeReference
    {
        public string Name { get; set; }

        public string Namespace { get; set; }

        public string Fullname
        {
            get
            {
                StringBuilder result = new StringBuilder(Namespace);
                if (result.Length > 0)
                    result.Append(".");
                result.Append(Name);
                return result.ToString();
            }
            set
            {
                value = value.Replace("/", ".");
                string[] items = value.Split('.');
                if (items.Length > 0)
                {
                    Name = items[items.Length - 1];
                    Array.Resize(ref items, items.Length - 1);
                    Namespace = string.Join(".", items);
                }
                else
                {
                    Name = string.Empty;
                    Namespace = string.Empty;
                }
            }
        }

        public override string ToString()
        {
            return Fullname;
        }

        public ClassReference()
        {
            TypeDescriptor = TypeDescriptors.FullyQualifiedName;
        }

        public override bool Equals(TypeReference other)
        {
            return (other is ClassReference) && (this.Fullname == (other as ClassReference).Fullname);
        }
    }
}
