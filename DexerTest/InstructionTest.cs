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
using Dexer.IO.Collector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dexer.Instructions;

namespace Dexer.Test
{
    [TestClass]
    public class InstructionTest : BaseCollectorTest
    {
        [TestMethod]
        public void TestUpdateInstructionOffsets()
        {
            Dictionary<OpCodes, int> coverage = new Dictionary<OpCodes, int>();

            foreach (string file in Directory.GetFiles(FilesDirectory))
            {
                TestContext.WriteLine("Testing {0}", file);

                Dex dex = Dex.Read(file);

                foreach (ClassDefinition @class in dex.Classes)
                {
                    foreach (MethodDefinition method in @class.Methods)
                    {
                        if (method.Body != null)
                        {
                            List<int> Offsets = new List<int>();
                            foreach (Instruction ins in method.Body.Instructions)
                            {
                                if (!coverage.ContainsKey(ins.OpCode))
                                    coverage.Add(ins.OpCode, 0);
                                Offsets.Add(ins.Offset);
                                coverage[ins.OpCode]++;
                            }
                            
                            OffsetStatistics stats = method.Body.UpdateInstructionOffsets();
                            for (int i = 0; i < Offsets.Count; i++)
                                Assert.AreEqual(Offsets[i], method.Body.Instructions[i].Offset, "Check OpCode {0}", method.Body.Instructions[i==0?i:i-1].OpCode);
                        }
                    }
                }
            }

            bool isInconclusive = false;
            foreach (OpCodes opcode in System.Enum.GetValues(typeof(OpCodes)))
                if (!coverage.ContainsKey(opcode))
                {
                    isInconclusive = true;
                    TestContext.WriteLine("OpCode {0} was not covered", opcode);
                }

            if (isInconclusive)
                Assert.Inconclusive("Some OpCode(s) were not covered ({0:P} coverage) , see test details", ((double) coverage.Count) / (Enum.GetNames(typeof(OpCodes)).Length) );
        }

    }
}
