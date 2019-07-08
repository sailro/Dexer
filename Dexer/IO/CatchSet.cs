/* Dexer Copyright (c) 2010-2019 Sebastien LEBRETON

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
using System.Globalization;
using System.Text;
using Dexer.Instructions;

namespace Dexer.IO
{
	internal class CatchSet : List<Catch>, IEquatable<CatchSet>
	{
		public Instruction CatchAll { get; set; }

		public CatchSet(ExceptionHandler handler)
		{
			AddRange(handler.Catches);
			CatchAll = handler.CatchAll;
		}

		public override bool Equals(object obj)
		{
			return obj is CatchSet set && Equals(set);
		}

		public override int GetHashCode()
		{
			var builder = new StringBuilder();
			builder.AppendLine(CatchAll == null ? "0" : CatchAll.Offset.ToString(CultureInfo.InvariantCulture));
			foreach (var @catch in this)
				builder.AppendLine(@catch.GetHashCode().ToString(CultureInfo.InvariantCulture));
			return builder.ToString().GetHashCode();
		}

		public bool Equals(CatchSet other)
		{
			if (other == null)
				return false;

			if (Count != other.Count || !Equals(CatchAll, other.CatchAll))
				return false;

			for (var i = 0; i < Count; i++)
			{
				if (!this[i].Equals(other[i]))
					return false;
			}

			return true;
		}
	}
}
