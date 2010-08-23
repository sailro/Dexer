using System.Collections.Generic;
using System;

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

        internal List<Action<Prototype>> loaders = new List<Action<Prototype>>();
        internal void DelayLoad(Action<Prototype> action)
        {
            loaders.Add(action);
        }

        internal void FlushLoaders()
        {
            foreach (var loader in loaders)
                loader(this);
            loaders.Clear();

            foreach (var parameter in Parameters)
                parameter.FlushLoaders();
        }
	}
}
