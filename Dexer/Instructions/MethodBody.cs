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

using System.Collections.Generic;
using System;
using Dexer.IO;
using System.Runtime.InteropServices;
using Dexer.Core;

namespace Dexer.Instructions
{
	public class MethodBody
	{
        public DebugInfo DebugInfo { get; set; }
        public List<Register> Registers { get; set; }
        public List<Instruction> Instructions { get; set; }
        public List<ExceptionHandler> Exceptions { get; set; }
        public ushort IncomingArguments { get; set; }
        public ushort OutgoingArguments { get; set; }
        public MethodDefinition Owner { get; set; }

        public MethodBody(MethodDefinition method, int registersSize)
        {
            Owner = method;
            Registers = new List<Register>();
            for (var i = 0; i < registersSize; i++)
            {
                Registers.Add(new Register(i));
            }
            Instructions = new List<Instruction>();
            Exceptions = new List<ExceptionHandler>();
        }

        internal static void CheckArrayData(Instruction ins, out Array elements, out Type elementtype, out int elementsize)
        {
            if (!(ins.Operand is Array) || (ins.Operand as Array).Length == 0)
                throw new InstructionException(ins, "Expecting non empty Array");

            elements = ins.Operand as Array;
            elementtype = elements.GetValue(0).GetType();
            elementsize = Marshal.SizeOf(elementtype);

            if (!(elementtype == typeof(sbyte)
                || elementtype == typeof(short)
                || elementtype == typeof(int)
                || elementtype == typeof(long)))
            {
                throw new InstructionException(ins, "Expecting sbyte/short/int/long element type");
            }
        }

        internal OffsetStatistics UpdateInstructionOffsets() {
            var ip = 0;
            var extra = 0;

            foreach (var ins in Instructions)
            {
                ins.Offset = ip;
                switch (ins.OpCode)
                {
                    case OpCodes.Nop:
                    case OpCodes.ReturnVoid:
                        ip++;
                        break;
                    case OpCodes.MoveResult:
                    case OpCodes.MoveResultWide:
                    case OpCodes.MoveResultObject:
                    case OpCodes.MoveException:
                    case OpCodes.Return:
                    case OpCodes.ReturnWide:
                    case OpCodes.ReturnObject:
                    case OpCodes.MonitorEnter:
                    case OpCodes.MonitorExit:
                    case OpCodes.Throw:
                        // vAA
                        ip+=1;
                        break;
                    case OpCodes.MoveObject:
                    case OpCodes.MoveWide:
                    case OpCodes.Move:
                    case OpCodes.ArrayLength:
                    case OpCodes.NegInt:
                    case OpCodes.NotInt:
                    case OpCodes.NegLong:
                    case OpCodes.NotLong:
                    case OpCodes.NegFloat:
                    case OpCodes.NegDouble:
                    case OpCodes.IntToLong:
                    case OpCodes.IntToFloat:
                    case OpCodes.IntToDouble:
                    case OpCodes.LongToInt:
                    case OpCodes.LongToFloat:
                    case OpCodes.LongToDouble:
                    case OpCodes.FloatToInt:
                    case OpCodes.FloatToLong:
                    case OpCodes.FloatToDouble:
                    case OpCodes.DoubleToInt:
                    case OpCodes.DoubleToLong:
                    case OpCodes.DoubleToFloat:
                    case OpCodes.IntToByte:
                    case OpCodes.IntToChar:
                    case OpCodes.IntToShort:
                    case OpCodes.AddInt2Addr:
                    case OpCodes.SubInt2Addr:
                    case OpCodes.MulInt2Addr:
                    case OpCodes.DivInt2Addr:
                    case OpCodes.RemInt2Addr:
                    case OpCodes.AndInt2Addr:
                    case OpCodes.OrInt2Addr:
                    case OpCodes.XorInt2Addr:
                    case OpCodes.ShlInt2Addr:
                    case OpCodes.ShrInt2Addr:
                    case OpCodes.UshrInt2Addr:
                    case OpCodes.AddLong2Addr:
                    case OpCodes.SubLong2Addr:
                    case OpCodes.MulLong2Addr:
                    case OpCodes.DivLong2Addr:
                    case OpCodes.RemLong2Addr:
                    case OpCodes.AndLong2Addr:
                    case OpCodes.OrLong2Addr:
                    case OpCodes.XorLong2Addr:
                    case OpCodes.ShlLong2Addr:
                    case OpCodes.ShrLong2Addr:
                    case OpCodes.UshrLong2Addr:
                    case OpCodes.AddFloat2Addr:
                    case OpCodes.SubFloat2Addr:
                    case OpCodes.MulFloat2Addr:
                    case OpCodes.DivFloat2Addr:
                    case OpCodes.RemFloat2Addr:
                    case OpCodes.AddDouble2Addr:
                    case OpCodes.SubDouble2Addr:
                    case OpCodes.MulDouble2Addr:
                    case OpCodes.DivDouble2Addr:
                    case OpCodes.RemDouble2Addr:
                        // vA, vB
                        ip+=1;
                        break;
                    case OpCodes.MoveWideFrom16:
                    case OpCodes.MoveFrom16:
                    case OpCodes.MoveObjectFrom16:
                        // vAA, vBBBB
                        ip += 2;
                        break;
                    case OpCodes.Move16:
                    case OpCodes.MoveObject16:
                        // vAAAA, vBBBB
                        ip += 3;
                        break;
                    case OpCodes.Const4:
                        // vA, #+B
                        ip++;
                        break;
                    case OpCodes.Const16:
                    case OpCodes.ConstWide16:
                        // vAA, #+BBBB
                        ip += 2;
                        break;
                    case OpCodes.Const:
                    case OpCodes.ConstWide32:
                        // vAA, #+BBBBBBBB
                        ip += 3;
                        break;
                    case OpCodes.FillArrayData:
                        // vAA, #+BBBBBBBB
                        ip += 3;

                        Array elements;
                        Type elementtype;
                        int elementsize;
                        CheckArrayData(ins, out elements, out elementtype, out elementsize);

                        extra += (elements.Length * elementsize + 1) / 2 + 4;
                        break;
                    case OpCodes.ConstHigh16:
                        // vAA, #+BBBB0000
                        ip += 2;
                        break;
                    case OpCodes.ConstWide:
                        // vAA, #+BBBBBBBBBBBBBBBB
                        ip += 5;
                        break;
                    case OpCodes.ConstWideHigh16:
                        // vAA, #+BBBB000000000000
                        ip += 2;
                        break;
                    case OpCodes.ConstString:
                        // vAA, string@BBBB
                        ip += 2;
                        break;
                    case OpCodes.ConstStringJumbo:
                        // vAA, string@BBBBBBBB
                        ip += 3;
                        break;
                    case OpCodes.ConstClass:
                    case OpCodes.NewInstance:
                    case OpCodes.CheckCast:
                        // vAA, type@BBBB
                        ip += 2;
                        break;
                    case OpCodes.InstanceOf:
                    case OpCodes.NewArray:
                        // vA, vB, type@CCCC
                        ip += 2;
                        break;
                    case OpCodes.FilledNewArray:
                        // {vD, vE, vF, vG, vA}, type@CCCC
                        ip += 3;
                        break;
                    case OpCodes.FilledNewArrayRange:
                        // {vCCCC .. vNNNN}, type@BBBB
                        ip += 4;
                        break;
                    case OpCodes.Goto:
                        // +AA
                        ip += 1;
                        break;
                    case OpCodes.Goto16:
                        // +AAAA
                        ip += 2;
                        break;
                    case OpCodes.Goto32:
                        // +AAAAAAAA
                        ip += 3;
                        break;
                    case OpCodes.PackedSwitch:
                        // vAA, +BBBBBBBB
                        if (!(ins.Operand is PackedSwitchData))
                            throw new InstructionException(ins, "Expecting PackedSwitchData");
                        var pdata = ins.Operand as PackedSwitchData;

                        ip += 3;
                        extra += (pdata.Targets.Count * 2) + 4;
                        break;
                    case OpCodes.SparseSwitch:
                        // vAA, +BBBBBBBB
                        if (!(ins.Operand is SparseSwitchData))
                            throw new InstructionException(ins, "Expecting SparseSwitchData");
                        var sdata = ins.Operand as SparseSwitchData;

                        ip += 3;
                        extra += (sdata.Targets.Count * 4) + 2;
                        break;
                    case OpCodes.CmplFloat:
                    case OpCodes.CmpgFloat:
                    case OpCodes.CmplDouble:
                    case OpCodes.CmpgDouble:
                    case OpCodes.CmpLong:
                    case OpCodes.Aget:
                    case OpCodes.AgetWide:
                    case OpCodes.AgetObject:
                    case OpCodes.AgetBoolean:
                    case OpCodes.AgetByte:
                    case OpCodes.AgetChar:
                    case OpCodes.AgetShort:
                    case OpCodes.Aput:
                    case OpCodes.AputWide:
                    case OpCodes.AputObject:
                    case OpCodes.AputBoolean:
                    case OpCodes.AputByte:
                    case OpCodes.AputChar:
                    case OpCodes.AputShort:
                    case OpCodes.AddInt:
                    case OpCodes.SubInt:
                    case OpCodes.MulInt:
                    case OpCodes.DivInt:
                    case OpCodes.RemInt:
                    case OpCodes.AndInt:
                    case OpCodes.OrInt:
                    case OpCodes.XorInt:
                    case OpCodes.ShlInt:
                    case OpCodes.ShrInt:
                    case OpCodes.UshrInt:
                    case OpCodes.AddLong:
                    case OpCodes.SubLong:
                    case OpCodes.MulLong:
                    case OpCodes.DivLong:
                    case OpCodes.RemLong:
                    case OpCodes.AndLong:
                    case OpCodes.OrLong:
                    case OpCodes.XorLong:
                    case OpCodes.ShlLong:
                    case OpCodes.ShrLong:
                    case OpCodes.UshrLong:
                    case OpCodes.AddFloat:
                    case OpCodes.SubFloat:
                    case OpCodes.MulFloat:
                    case OpCodes.DivFloat:
                    case OpCodes.RemFloat:
                    case OpCodes.AddDouble:
                    case OpCodes.SubDouble:
                    case OpCodes.MulDouble:
                    case OpCodes.DivDouble:
                    case OpCodes.RemDouble:
                        // vAA, vBB, vCC
                        ip += 2;
                        break;
                    case OpCodes.IfEq:
                    case OpCodes.IfNe:
                    case OpCodes.IfLt:
                    case OpCodes.IfGe:
                    case OpCodes.IfGt:
                    case OpCodes.IfLe:
                        // vA, vB, +CCCC
                        ip += 2;
                        break;
                    case OpCodes.IfEqz:
                    case OpCodes.IfNez:
                    case OpCodes.IfLtz:
                    case OpCodes.IfGez:
                    case OpCodes.IfGtz:
                    case OpCodes.IfLez:
                        // vAA, +BBBB
                        ip += 2;
                        break;
                    case OpCodes.Iget:
                    case OpCodes.IgetWide:
                    case OpCodes.IgetObject:
                    case OpCodes.IgetBoolean:
                    case OpCodes.IgetByte:
                    case OpCodes.IgetChar:
                    case OpCodes.IgetShort:
                    case OpCodes.Iput:
                    case OpCodes.IputWide:
                    case OpCodes.IputObject:
                    case OpCodes.IputBoolean:
                    case OpCodes.IputByte:
                    case OpCodes.IputChar:
                    case OpCodes.IputShort:
                        // vA, vB, field@CCCC
                        ip += 2;
                        break;
                    case OpCodes.Sget:
                    case OpCodes.SgetWide:
                    case OpCodes.SgetObject:
                    case OpCodes.SgetBoolean:
                    case OpCodes.SgetByte:
                    case OpCodes.SgetChar:
                    case OpCodes.SgetShort:
                    case OpCodes.Sput:
                    case OpCodes.SputWide:
                    case OpCodes.SputObject:
                    case OpCodes.SputBoolean:
                    case OpCodes.SputByte:
                    case OpCodes.SputChar:
                    case OpCodes.SputShort:
                        // vAA, field@BBBB
                        ip += 2;
                        break;
                    case OpCodes.InvokeVirtual:
                    case OpCodes.InvokeSuper:
                    case OpCodes.InvokeDirect:
                    case OpCodes.InvokeStatic:
                    case OpCodes.InvokeInterface:
                        // {vD, vE, vF, vG, vA}, meth@CCCC
                        ip += 3;
                        break;
                    case OpCodes.InvokeVirtualRange:
                    case OpCodes.InvokeSuperRange:
                    case OpCodes.InvokeDirectRange:
                    case OpCodes.InvokeStaticRange:
                    case OpCodes.InvokeInterfaceRange:
                        // {vCCCC .. vNNNN}, meth@BBBB
                        ip += 3;
                        break;
                    case OpCodes.AddIntLit16:
                    case OpCodes.RsubInt:
                    case OpCodes.MulIntLit16:
                    case OpCodes.DivIntLit16:
                    case OpCodes.RemIntLit16:
                    case OpCodes.AndIntLit16:
                    case OpCodes.OrIntLit16:
                    case OpCodes.XorIntLit16:
                        // vA, vB, #+CCCC
                        ip += 2;
                        break;
                    case OpCodes.AddIntLit8:
                    case OpCodes.RsubIntLit8:
                    case OpCodes.MulIntLit8:
                    case OpCodes.DivIntLit8:
                    case OpCodes.RemIntLit8:
                    case OpCodes.AndIntLit8:
                    case OpCodes.OrIntLit8:
                    case OpCodes.XorIntLit8:
                    case OpCodes.ShlIntLit8:
                    case OpCodes.ShrIntLit8:
                    case OpCodes.UshrIntLit8:
                        // vAA, vBB, #+CC
                        ip += 2;
                        break;

                    default:
                        throw new NotImplementedException(string.Concat("Unknown opcode:", ins.OpCode));
                }
            }
            return new OffsetStatistics { CodeUnits=ip, ExtraCodeUnits=extra };
        }
    }
}
