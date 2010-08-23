using System.Collections.Generic;
using System;
using Dexer.Core;

namespace Dexer.Core
{
    public class ClassDefinition : ClassReference
	{
        public AccessFlags AccessFlag { get; set; }
        public ClassReference SuperClass { get; set; }
        public IList<TypeReference> Interfaces { get; set; }
        public string SourceFile { get; set; }
        public IList<Annotation> Annotations { get; set; }
        public IList<FieldDefinition> Fields { get; set; }
        public IList<MethodDefinition> Methods { get; set; }

        public ClassDefinition()
        {
            Interfaces = new List<TypeReference>();
            Annotations = new List<Annotation>();
            Fields = new List<FieldDefinition>();
            Methods = new List<MethodDefinition>();

        }

        internal List<Action<ClassDefinition>> defloaders = new List<Action<ClassDefinition>>();
        internal void DelayLoad(Action<ClassDefinition> action)
        {
            defloaders.Add(action);
        }

        internal override void FlushLoaders()
        {
            foreach (var loader in defloaders)
                loader(this);
            defloaders.Clear();

            base.FlushLoaders();

            SuperClass.FlushLoaders();

            foreach (var field in Fields)
                field.FlushLoaders();

            foreach (var method in Methods)
                method.FlushLoaders();
        }

	}
}
