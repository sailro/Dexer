/* Dexer Copyright (c) 2010-2013 Sebastien LEBRETON

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

using System.Collections.Generic;
using Dexer.Core;
using Dexer.Instructions;

namespace Dexer.IO.Collector
{
    internal class ModelSorter : BaseCollector<object>
    {
        private ClassDefinitionComparer cdefc;
        private ClassReferenceComparer crefc;
        private MethodDefinitionComparer mdefc;
        private FieldDefinitionComparer fdefc;
        private AnnotationComparer ac;

        public ModelSorter()
        {
            cdefc = new ClassDefinitionComparer();
            crefc = new ClassReferenceComparer();
            mdefc = new MethodDefinitionComparer();
            fdefc = new FieldDefinitionComparer();
            ac = new AnnotationComparer();
        }

        public override void Collect(List<ClassDefinition> classes)
        {
            classes.Sort(cdefc);
            base.Collect(classes);
        }

        public override void Collect(List<ClassReference> classes)
        {
            classes.Sort(crefc);
            base.Collect(classes);
        }

        public override void Collect(List<MethodDefinition> methods)
        {
            methods.Sort(mdefc);
            base.Collect(methods);
        }

        public override void Collect(List<FieldDefinition> fields)
        {
            fields.Sort(fdefc);
            base.Collect(fields);
        }

        public override void Collect(List<Annotation> annotations)
        {
            annotations.Sort(ac);
            base.Collect(annotations);
        }
    }
}
