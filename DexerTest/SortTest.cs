/* Dexer Copyright (c) 2010 Sebastien LEBRETON

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
using Dexer.IO.Collector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dexer.Test
{
    [TestClass]
    public class SortTest : BaseCollectorTest
    {
        public void TestSort<T>(Func<Dex, List<T>> provider, Func<Dex, IComparer<T>> comparer)
        {
            foreach (string file in Directory.GetFiles(filesDirectory))
            {
                testContextInstance.WriteLine("Testing {0}", file);

                Dex dex = Dex.Load(file);
                List<T> items = new List<T>(provider(dex));
                items.Reverse();
                items.Sort(comparer(dex));

                for (int i = 0; i < items.Count; i++)
                    Assert.AreEqual(items[i], provider(dex)[i]);

            }
        }

        [TestMethod]
        public void TestTypeSort()
        {
            TestSort<TypeReference>((dex) => dex.TypeReferences, (dex) => new TypeReferenceComparer());
        }

        [TestMethod]
        public void TestStringSort()
        {
            TestSort<string>((dex) => dex.Strings, (dex) => new Dexer.IO.Collector.StringComparer());
        }

        [TestMethod]
        public void TestPrototypeSort()
        {
            TestSort<Prototype>((dex) => dex.Prototypes, (dex) => {
                Dictionary<TypeReference, int> lookup = new Dictionary<TypeReference, int>();

                for (int i = 0; i < dex.TypeReferences.Count; i++)
                    lookup.Add(dex.TypeReferences[i], i);

                return new PrototypeComparer(lookup);
            });
        }
    }
}
