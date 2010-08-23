using System.Text;
using System;
using Dexer.Metadata;
using System.Collections.Generic;

namespace Dexer.Core
{
    public class ClassReference : TypeReference
    {
        internal ClassDefinition Promote() {
            ClassDefinition result = new ClassDefinition();
            result.Fullname = this.Fullname;
            result.Namespace = this.Namespace;
            result.Name = this.Name;
            result.TypeDescriptor = this.TypeDescriptor;
            result.loaders = this.loaders;
            return result;
        }

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

        internal List<Action<ClassReference>> loaders = new List<Action<ClassReference>>();
        internal void DelayLoad(Action<ClassReference> action)
        {
            loaders.Add(action);
        }

        internal virtual void FlushLoaders()
        {
            foreach (var loader in loaders)
                loader(this);
            loaders.Clear();
        }
    }
}
