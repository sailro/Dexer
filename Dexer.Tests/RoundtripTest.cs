﻿/* Dexer Copyright (c) 2010-2022 Sebastien Lebreton

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
using System.IO;
using System.Linq;
using Dexer.Core;
using Dexer.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dexer.IO;
using Dexer.Metadata;

namespace Dexer.Tests;

[TestClass]
public class RoundtripTest : BaseTest
{
	private void TestReadWrite(string file, out DexReader dexreader, out DexWriter dexwriter)
	{
		TestContext.WriteLine("Testing {0}", file);

		var dex = new Dex();
		dexreader = new DexReader(dex);

		using (Stream fs = new FileStream(file, FileMode.Open))
		using (var reader = new BinaryReader(fs))
			dexreader.ReadFrom(reader);

		dexwriter = new DexWriter(dex);

		if (Extralog)
		{
			using Stream fs = new FileStream(file + ".out", FileMode.Create);
			using var writer = new BinaryWriter(fs);
			
			dexwriter.WriteTo(writer);
		}
		else
		{
			using Stream fs = new MemoryStream();
			using var writer = new BinaryWriter(fs);
			
			dexwriter.WriteTo(writer);
		}
	}

	[TestMethod]
	public void TestMap()
	{
		foreach (var file in GetTestFiles())
		{
			TestReadWrite(file, out var dexreader, out var dexwriter);

			var checklist = new Dictionary<TypeCodes, string>();

			foreach (var tc in dexwriter.Map.Keys.Where(tc => dexreader.Map.ContainsKey(tc)))
			{
				if (dexreader.Map[tc].Count != dexwriter.Map[tc].Count)
				{
					TestContext.WriteLine("{0} Count differs expected={1}, actual={2}", tc, dexreader.Map[tc].Count, dexwriter.Map[tc].Count);
					if (!checklist.ContainsKey(tc))
						checklist.Add(tc, tc.ToString());
				}

				if (dexreader.Map[tc].Offset != dexwriter.Map[tc].Offset)
				{
					TestContext.WriteLine("{0} Offset differs : expected={1}, actual={2}", tc, dexreader.Map[tc].Offset, dexwriter.Map[tc].Offset);
					if (!checklist.ContainsKey(tc))
						checklist.Add(tc, tc.ToString());
				}
			}

			Assert.IsTrue(checklist.Count == 0, string.Concat("Check test report : ", string.Join(", ", checklist.Values)));
		}
	}

	[TestMethod]
	public void TestCheckSum()
	{
		foreach (var file in GetTestFiles())
		{
			TestReadWrite(file, out var dexreader, out var dexwriter);

			Assert.AreEqual(dexreader.Header.CheckSum, dexwriter.CheckSum);
		}
	}

	[TestMethod]
	public void TestSignature()
	{
		foreach (var file in GetTestFiles())
		{
			TestReadWrite(file, out var dexreader, out var dexwriter);

			Assert.IsTrue(dexreader.Header.Signature.Match(dexwriter.Signature, 0));
		}
	}
}
