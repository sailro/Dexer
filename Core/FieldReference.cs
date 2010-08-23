using System.Collections.Generic;
using System;

namespace Dexer.Core
{
    public class FieldReference
    {
        public ClassReference ClassReference { get; set; }
        public TypeReference Type { get; set; }
        public string Name { get; set; }

        internal List<Action<FieldReference>> loaders = new List<Action<FieldReference>>();
        internal void DelayLoad(Action<FieldReference> action)
        {
            loaders.Add(action);
        }

        internal void FlushLoaders()
        {
            foreach (var loader in loaders)
                loader(this);
            loaders.Clear();

            ClassReference.FlushLoaders();
        }
    }
}
