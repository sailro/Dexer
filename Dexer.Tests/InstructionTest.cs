/* Dexer Copyright (c) 2010-2023 Sebastien Lebreton

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

using Dexer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dexer.Instructions;

namespace Dexer.Tests;

[TestClass]
public class InstructionTest : BaseCollectorTest
{
	[TestMethod]
	public void TestUpdateInstructionOffsets()
	{
		var coverage = new Dictionary<OpCodes, int>();

		foreach (var file in GetTestFiles())
		{
			TestContext.WriteLine("Testing {0}", file);

			var dex = Dex.Read(file);

			foreach (var @class in dex.Classes)
			{
				foreach (var method in @class.Methods)
				{
					if (method.Body == null)
						continue;

					var offsets = new List<int>();
					foreach (var ins in method.Body.Instructions)
					{
						coverage.TryAdd(ins.OpCode, 0);
						offsets.Add(ins.Offset);
						coverage[ins.OpCode]++;
					}

					method.Body.UpdateInstructionOffsets();
					for (var i = 0; i < offsets.Count; i++)
						Assert.AreEqual(offsets[i], method.Body.Instructions[i].Offset, "Check OpCode {0}", method.Body.Instructions[i == 0 ? i : i - 1].OpCode);
				}
			}
		}

		var isPartial = false;
		foreach (var opcode in Enum.GetValues<OpCodes>())
			if (!coverage.ContainsKey(opcode))
			{
				isPartial = true;
				TestContext.WriteLine("OpCode {0} was not covered", opcode);
			}

		if (isPartial)
			TestContext.WriteLine("Some OpCode(s) were not covered ({0:P} coverage)", (double)coverage.Count / Enum.GetNames<OpCodes>().Length);
	}
}
