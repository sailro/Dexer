using System.Collections.Generic;
using System;

namespace Dexer.Core
{
    public class Parameter
    {
        public TypeReference Type { get; set; }


        internal List<Action<Parameter>> loaders = new List<Action<Parameter>>();
        internal void DelayLoad(Action<Parameter> action)
        {
            loaders.Add(action);
        }

        internal void FlushLoaders()
        {
            foreach (var loader in loaders)
                loader(this);
            loaders.Clear();
        }
    }
}
