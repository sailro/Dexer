/*
    Dexer, open source framework for .DEX files (Dalvik Executable Format)
    Copyright (C) 2010 Sebastien LEBRETON

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
