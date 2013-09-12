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

using System;
using System.Collections.Generic;
using System.IO;
using Dexer.Core;
using Dexer.Extensions;
using Dexer.IO.Collectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StringComparer = Dexer.IO.Collectors.StringComparer;

namespace Dexer.Test
{
    [TestClass]
    public class SortTest : BaseCollectorTest
    {

        public void TestGlobalSort<T>(Func<Dex, List<T>> provider, IComparer<T> comparer)
        {
            foreach (var file in Directory.GetFiles(FilesDirectory))
            {
                TestContext.WriteLine("Testing {0}", file);

                var dex = Dex.Read(file);
                var items = new List<T>(provider(dex));
                items.Shuffle();
                items.Sort(comparer);

                if (comparer is IPartialComparer<T>)
                {
                    var tsorter = new TopologicalSorter();
                    items = new List<T>(tsorter.TopologicalSort(items, comparer as IPartialComparer<T>));
                }

                for (var i = 0; i < items.Count; i++)
                    Assert.AreEqual(items[i], provider(dex)[i]);

            }
        }

        [TestMethod]
        public void TestMethodReferenceSort()
        {
            TestGlobalSort(dex => dex.MethodReferences, new MethodReferenceComparer());
        }

        [TestMethod]
        public void TestFieldReferenceSort()
        {
            TestGlobalSort(dex => dex.FieldReferences, new FieldReferenceComparer());
        }

        private void SortAndCheck<T>(List<T> source, IComparer<T> comparer)
        {
            var items = new List<T>(source);
            items.Shuffle();
            items.Sort(comparer);

            for (var i = 0; i < items.Count; i++)
                Assert.AreEqual(items[i], source[i]);
        }

        [TestMethod]
        public void TestMethodDefinitionSort()
        {
            foreach (var file in Directory.GetFiles(FilesDirectory))
            {
                TestContext.WriteLine("Testing {0}", file);
                var dex = Dex.Read(file);

                foreach (var @class in dex.Classes)
                    SortAndCheck(@class.Methods, new MethodDefinitionComparer());
            }
        }

        [TestMethod]
        public void TestAnnotationSort()
        {
            foreach (var file in Directory.GetFiles(FilesDirectory))
            {
                TestContext.WriteLine("Testing {0}", file);
                var dex = Dex.Read(file);

                foreach (var @class in dex.Classes)
                {
                    SortAndCheck(@class.Annotations, new AnnotationComparer());

                    foreach (var field in @class.Fields)
                        SortAndCheck(field.Annotations, new AnnotationComparer());

                    foreach (var method in @class.Methods)
                    {
                        SortAndCheck(method.Annotations, new AnnotationComparer());

                        foreach (var parameter in method.Prototype.Parameters)
                            SortAndCheck(parameter.Annotations, new AnnotationComparer());
                    }
                }
            }
        }
        

        [TestMethod]
        public void TestFieldDefinitionSort()
        {
            foreach (var file in Directory.GetFiles(FilesDirectory))
            {
                TestContext.WriteLine("Testing {0}", file);
                var dex = Dex.Read(file);

                foreach (var @class in dex.Classes)
                    SortAndCheck(@class.Fields, new FieldDefinitionComparer());
            }
        }

        [TestMethod]
        public void TestTypeReferenceSort()
        {
            TestGlobalSort(dex => dex.TypeReferences, new TypeReferenceComparer());
        }

        [TestMethod]
        public void TestClassDefinitionTopologicalSort()
        {
            TestGlobalSort(dex => dex.Classes, new ClassDefinitionComparer());
        }

        [TestMethod]
        public void TestStringSort()
        {
            TestGlobalSort(dex => dex.Strings, new StringComparer());
        }

        [TestMethod]
        public void TestPrototypeSort()
        {
            TestGlobalSort(dex => dex.Prototypes, new PrototypeComparer());
        }

    }
}
