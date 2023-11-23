﻿/* Dexer Copyright (c) 2010-2023 Sebastien Lebreton

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

using Dexer.IO.Markers;

namespace Dexer.Extensions;

public static class BinaryWriterExtensions
{
	public static void EnsureAlignmentWithSection(this BinaryWriter writer, uint sectionOffset, int alignment)
	{
		var position = writer.BaseStream.Position - sectionOffset;

		while (alignment > 0 && position % alignment != 0)
		{
			writer.Write((byte)0);
			position++;
		}
	}

	public static void EnsureSectionAlignment(this BinaryWriter writer, ref uint sectionOffset, int alignment)
	{
		var position = writer.BaseStream.Position;
		if (position != sectionOffset || alignment <= 0)
			return;

		while (sectionOffset % alignment != 0)
		{
			writer.Write((byte)0);
			sectionOffset++;
		}
	}

	public static void PreserveCurrentPosition(this BinaryWriter writer, uint newPosition, Action action)
	{
		var position = writer.BaseStream.Position;
		writer.BaseStream.Seek(newPosition, SeekOrigin.Begin);

		action();

		writer.BaseStream.Seek(position, SeekOrigin.Begin);
	}

	internal static UShortMarker MarkUShort(this BinaryWriter writer)
	{
		return new(writer);
	}

	internal static UIntMarker MarkUInt(this BinaryWriter writer)
	{
		return new(writer);
	}

	internal static SizeOffsetMarker MarkSizeOffset(this BinaryWriter writer)
	{
		return new(writer);
	}

	internal static SignatureMarker MarkSignature(this BinaryWriter writer)
	{
		return new(writer);
	}

	public static void WriteULEB128(this BinaryWriter writer, uint value)
	{
		do
		{
			var partial = (byte)(value & 0x7f);
			value >>= 7;
			if (value != 0)
				partial |= 0x80;
			writer.Write(partial);
		} while (value != 0);
	}

	public static void WriteULEB128P1(this BinaryWriter writer, long value)
	{
		WriteULEB128(writer, (uint)(value + 1));
	}

	public static void WriteSLEB128(this BinaryWriter writer, int value)
	{
		var negative = (value < 0);
		var next = true;

		while (next)
		{
			var partial = (byte)(value & 0x7f);
			value >>= 7;
			if (negative)
				value |= -(1 << 24);

			if ((value == 0 && ((partial & 0x40) == 0)) || (value == -1 && ((partial & 0x40) != 0)))
				next = false;
			else
				partial |= 0x80;

			writer.Write(partial);
		}
	}

	public static void WriteMUTF8String(this BinaryWriter writer, string value)
	{
		writer.WriteULEB128((uint)value.Length);

		foreach (var c in value)
		{
			if ((c != 0) && (c < 0x80))
			{
				writer.Write((byte)c);
			}
			else if (c < 0x800)
			{
				writer.Write((byte)(((c >> 6) & 0x1f) | 0xc0));
				writer.Write((byte)((c & 0x3f) | 0x80));
			}
			else
			{
				writer.Write((byte)(((c >> 12) & 0x0f) | 0xe0));
				writer.Write((byte)(((c >> 6) & 0x3f) | 0xc0));
				writer.Write((byte)((c & 0x3f) | 0x80));
			}
		}

		writer.Write((byte)0); // 0 padded;
	}

	private static int NumberOfLeadingZeros(long i)
	{
		if (i == 0)
			return 64;
		var n = 1;
		var x = (int)TripleShift(i, 32);

		if (x == 0)
		{
			n += 32;
			x = (int)i;
		}

		if (TripleShift(x, 16) == 0)
		{
			n += 16;
			x <<= 16;
		}

		if (TripleShift(x, 24) == 0)
		{
			n += 8;
			x <<= 8;
		}

		if (TripleShift(x, 28) == 0)
		{
			n += 4;
			x <<= 4;
		}

		if (TripleShift(x, 30) == 0)
		{
			n += 2;
			x <<= 2;
		}

		n -= (int)TripleShift(x, 31);
		return n;
	}

	private static long TripleShift(long n, int s)
	{
		if (n >= 0)
			return n >> s;
		return (n >> s) + (2 << ~s);
	}

#pragma warning disable IDE0060
	public static int GetByteCountForSignedPackedNumber(this BinaryWriter writer, long value)
	{
		var requiredBits = 65 - NumberOfLeadingZeros(value ^ (value >> 63));
		int result = (byte)((requiredBits + 0x07) >> 3);

		return result;
	}

	public static int GetByteCountForUnsignedPackedNumber(this BinaryWriter writer, long value)
	{
		var requiredBits = 64 - NumberOfLeadingZeros(value);
		if (requiredBits == 0)
			requiredBits = 1;

		int result = (byte)((requiredBits + 0x07) >> 3);

		return result;
	}
#pragma warning restore IDE0060

	public static void WritePackedSignedNumber(this BinaryWriter writer, long value)
	{
		var requiredBytes = GetByteCountForSignedPackedNumber(writer, value);

		for (var i = 0; i < requiredBytes; i++)
		{
			writer.Write((byte)value);
			value >>= 8;
		}
	}

	public static void WriteUnsignedPackedNumber(this BinaryWriter writer, long value)
	{
		var requiredBytes = GetByteCountForUnsignedPackedNumber(writer, value);

		for (var i = 0; i < requiredBytes; i++)
		{
			writer.Write((byte)value);
			value >>= 8;
		}
	}
}
