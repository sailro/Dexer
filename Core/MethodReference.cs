using System.Collections.Generic;
using System;

namespace Dexer.Core
{
    public class MethodReference
    {
        public ClassReference ClassReference { get; set; }
        public string Name { get; set; }
        public Prototype Prototype { get; set; }

        internal List<Action<MethodReference>> loaders = new List<Action<MethodReference>>();
        internal void DelayLoad(Action<MethodReference> action)
        {
            loaders.Add(action);
        }

        internal void FlushLoaders()
        {
            foreach (var loader in loaders)
                loader(this);
            loaders.Clear();

            Prototype.FlushLoaders();
            ClassReference.FlushLoaders();
        }
    }
}
