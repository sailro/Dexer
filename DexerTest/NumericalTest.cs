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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Dexer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dexer.Test
{
    [TestClass]
    public class NumericalTest : BaseTest
    {
        public void TestNumber<T>(Func<BinaryReader, T> readProvider, Action<BinaryWriter, T> writeProvider, IEnumerable<T> values)
        {
            var stream = new MemoryStream();
            var reader = new BinaryReader(stream);
            var writer = new BinaryWriter(stream);

            foreach (var expected in values.Select(item => (T)Convert.ChangeType(item, typeof(T))))
            {
	            TestContext.WriteLine("{0}, {0:x}", expected);

	            stream.Position = 0;
	            writeProvider(writer, expected);

	            stream.Position = 0;
	            var actual = readProvider(reader);

	            Assert.AreEqual(expected, actual);
            }

        }
        
        [TestMethod]
        public void TestULEB128()
        {
            TestNumber(reader => reader.ReadULEB128(),
                             (writer, value) => writer.WriteULEB128(value),
                             GenerateUIntValues());
        }

        [TestMethod]
        public void TestSLEB128()
        {
            TestNumber(reader => reader.ReadSLEB128(),
                            (writer, value) => writer.WriteSLEB128(value),
                            GenerateSIntValues());
        }

        private int _bytelength;
        
        private void VBLSIntWriter(BinaryWriter writer, long value) {
            _bytelength = writer.GetByteCountForSignedPackedNumber(value);
            writer.WritePackedSignedNumber(value);
        }

        private void VBLUIntWriter(BinaryWriter writer, long value)
        {
            _bytelength = writer.GetByteCountForUnsignedPackedNumber(value);
            writer.WriteUnsignedPackedNumber(value);
        }

        [TestMethod]
        public void TestUnsignedPackedNumbers()
        {
            TestNumber(reader => reader.ReadUnsignedPackedNumber(_bytelength), 
                              VBLUIntWriter,
                              GenerateULongValues());
        }

        [TestMethod]
        public void TestSignedPackedNumbers()
        {
            TestNumber(reader => reader.ReadSignedPackedNumber(_bytelength),
                              VBLSIntWriter,
                              GenerateSLongValues());
        }

        private IEnumerable<long> GenerateULongValues()
        {
	        return GenerateUIntValues().Select(Convert.ToInt64);
        }

	    private IEnumerable<long> GenerateSLongValues()
	    {
		    return GenerateSIntValues().Select(Convert.ToInt64);
	    }

	    private static IEnumerable<uint> GenerateUIntValues()
        {
            long value = 1;

            while (value <= uint.MaxValue)
            {
                yield return (uint)value - 1;
                yield return (uint)value;
                yield return (uint)value + 1;
                value = value * 2;
            }
        }

        private static IEnumerable<int> GenerateSIntValues()
        {
            foreach (var item in GenerateUIntValues())
            {
                yield return (int)item;
                yield return -(int)item;
            }
        }

    }
}
