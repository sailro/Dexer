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

namespace Dexer.Instructions;

public enum OpCodes
{
	Nop = 0x00,
	Move = 0x01,
	MoveFrom16 = 0x02,
	Move16 = 0x03,
	MoveWide = 0x04,
	MoveWideFrom16 = 0x05,
	MoveWide16 = 0x06,
	MoveObject = 0x07,
	MoveObjectFrom16 = 0x08,
	MoveObject16 = 0x09,
	MoveResult = 0x0a,
	MoveResultWide = 0x0b,
	MoveResultObject = 0x0c,
	MoveException = 0x0d,
	ReturnVoid = 0x0e,
	Return = 0x0f,
	ReturnWide = 0x10,
	ReturnObject = 0x11,
	Const4 = 0x12,
	Const16 = 0x13,
	Const = 0x14,
	ConstHigh16 = 0x15,
	ConstWide16 = 0x16,
	ConstWide32 = 0x17,
	ConstWide = 0x18,
	ConstWideHigh16 = 0x19,
	ConstString = 0x1a,
	ConstStringJumbo = 0x1b,
	ConstClass = 0x1c,
	MonitorEnter = 0x1d,
	MonitorExit = 0x1e,
	CheckCast = 0x1f,
	InstanceOf = 0x20,
	ArrayLength = 0x21,
	NewInstance = 0x22,
	NewArray = 0x23,
	FilledNewArray = 0x24,
	FilledNewArrayRange = 0x25,
	FillArrayData = 0x26,
	Throw = 0x27,
	Goto = 0x28,
	Goto16 = 0x29,
	Goto32 = 0x2a,
	PackedSwitch = 0x2b,
	SparseSwitch = 0x2c,
	CmplFloat = 0x2d,
	CmpgFloat = 0x2e,
	CmplDouble = 0x2f,
	CmpgDouble = 0x30,
	CmpLong = 0x31,
	IfEq = 0x32,
	IfNe = 0x33,
	IfLt = 0x34,
	IfGe = 0x35,
	IfGt = 0x36,
	IfLe = 0x37,
	IfEqz = 0x38,
	IfNez = 0x39,
	IfLtz = 0x3a,
	IfGez = 0x3b,
	IfGtz = 0x3c,
	IfLez = 0x3d,
	Aget = 0x44,
	AgetWide = 0x45,
	AgetObject = 0x46,
	AgetBoolean = 0x47,
	AgetByte = 0x48,
	AgetChar = 0x49,
	AgetShort = 0x4a,
	Aput = 0x4b,
	AputWide = 0x4c,
	AputObject = 0x4d,
	AputBoolean = 0x4e,
	AputByte = 0x4f,
	AputChar = 0x50,
	AputShort = 0x51,
	Iget = 0x52,
	IgetWide = 0x53,
	IgetObject = 0x54,
	IgetBoolean = 0x55,
	IgetByte = 0x56,
	IgetChar = 0x57,
	IgetShort = 0x58,
	Iput = 0x59,
	IputWide = 0x5a,
	IputObject = 0x5b,
	IputBoolean = 0x5c,
	IputByte = 0x5d,
	IputChar = 0x5e,
	IputShort = 0x5f,
	Sget = 0x60,
	SgetWide = 0x61,
	SgetObject = 0x62,
	SgetBoolean = 0x63,
	SgetByte = 0x64,
	SgetChar = 0x65,
	SgetShort = 0x66,
	Sput = 0x67,
	SputWide = 0x68,
	SputObject = 0x69,
	SputBoolean = 0x6a,
	SputByte = 0x6b,
	SputChar = 0x6c,
	SputShort = 0x6d,
	InvokeVirtual = 0x6e,
	InvokeSuper = 0x6f,
	InvokeDirect = 0x70,
	InvokeStatic = 0x71,
	InvokeInterface = 0x72,
	InvokeVirtualRange = 0x74,
	InvokeSuperRange = 0x75,
	InvokeDirectRange = 0x76,
	InvokeStaticRange = 0x77,
	InvokeInterfaceRange = 0x78,
	NegInt = 0x7b,
	NotInt = 0x7c,
	NegLong = 0x7d,
	NotLong = 0x7e,
	NegFloat = 0x7f,
	NegDouble = 0x80,
	IntToLong = 0x81,
	IntToFloat = 0x82,
	IntToDouble = 0x83,
	LongToInt = 0x84,
	LongToFloat = 0x85,
	LongToDouble = 0x86,
	FloatToInt = 0x87,
	FloatToLong = 0x88,
	FloatToDouble = 0x89,
	DoubleToInt = 0x8a,
	DoubleToLong = 0x8b,
	DoubleToFloat = 0x8c,
	IntToByte = 0x8d,
	IntToChar = 0x8e,
	IntToShort = 0x8f,
	AddInt = 0x90,
	SubInt = 0x91,
	MulInt = 0x92,
	DivInt = 0x93,
	RemInt = 0x94,
	AndInt = 0x95,
	OrInt = 0x96,
	XorInt = 0x97,
	ShlInt = 0x98,
	ShrInt = 0x99,
	UshrInt = 0x9a,
	AddLong = 0x9b,
	SubLong = 0x9c,
	MulLong = 0x9d,
	DivLong = 0x9e,
	RemLong = 0x9f,
	AndLong = 0xa0,
	OrLong = 0xa1,
	XorLong = 0xa2,
	ShlLong = 0xa3,
	ShrLong = 0xa4,
	UshrLong = 0xa5,
	AddFloat = 0xa6,
	SubFloat = 0xa7,
	MulFloat = 0xa8,
	DivFloat = 0xa9,
	RemFloat = 0xaa,
	AddDouble = 0xab,
	SubDouble = 0xac,
	MulDouble = 0xad,
	DivDouble = 0xae,
	RemDouble = 0xaf,
	AddInt2Addr = 0xb0,
	SubInt2Addr = 0xb1,
	MulInt2Addr = 0xb2,
	DivInt2Addr = 0xb3,
	RemInt2Addr = 0xb4,
	AndInt2Addr = 0xb5,
	OrInt2Addr = 0xb6,
	XorInt2Addr = 0xb7,
	ShlInt2Addr = 0xb8,
	ShrInt2Addr = 0xb9,
	UshrInt2Addr = 0xba,
	AddLong2Addr = 0xbb,
	SubLong2Addr = 0xbc,
	MulLong2Addr = 0xbd,
	DivLong2Addr = 0xbe,
	RemLong2Addr = 0xbf,
	AndLong2Addr = 0xc0,
	OrLong2Addr = 0xc1,
	XorLong2Addr = 0xc2,
	ShlLong2Addr = 0xc3,
	ShrLong2Addr = 0xc4,
	UshrLong2Addr = 0xc5,
	AddFloat2Addr = 0xc6,
	SubFloat2Addr = 0xc7,
	MulFloat2Addr = 0xc8,
	DivFloat2Addr = 0xc9,
	RemFloat2Addr = 0xca,
	AddDouble2Addr = 0xcb,
	SubDouble2Addr = 0xcc,
	MulDouble2Addr = 0xcd,
	DivDouble2Addr = 0xce,
	RemDouble2Addr = 0xcf,
	AddIntLit16 = 0xd0,
	RsubInt = 0xd1,
	MulIntLit16 = 0xd2,
	DivIntLit16 = 0xd3,
	RemIntLit16 = 0xd4,
	AndIntLit16 = 0xd5,
	OrIntLit16 = 0xd6,
	XorIntLit16 = 0xd7,
	AddIntLit8 = 0xd8,
	RsubIntLit8 = 0xd9,
	MulIntLit8 = 0xda,
	DivIntLit8 = 0xdb,
	RemIntLit8 = 0xdc,
	AndIntLit8 = 0xdd,
	OrIntLit8 = 0xde,
	XorIntLit8 = 0xdf,
	ShlIntLit8 = 0xe0,
	ShrIntLit8 = 0xe1,
	UshrIntLit8 = 0xe2,
}
