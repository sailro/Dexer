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
using Dexer.Instructions;

namespace Dexer.IO
{
    internal class InstructionReader
    {
        private MethodDefinition MethodDefinition { get; set; }
        private List<Action> LazyInstructionsSetters { get; set; }
        private Dex Dex { get; set; }

        internal int[] Codes { get; set; }
        private int[] Lower { get; set; }
        private int[] Upper { get; set; }
        private int _ip;
        private uint InstructionsSize { get; set; }

        internal Dictionary<int, Instruction> Lookup;     // instructions by starting offset
        internal Dictionary<int, Instruction> LookupLast; // instructions by ending offset

        public InstructionReader(Dex dex, MethodDefinition mdef)
        {
            Dex = dex;
            MethodDefinition = mdef;
            Lookup = new Dictionary<int, Instruction>();
            LookupLast = new Dictionary<int, Instruction>();
            LazyInstructionsSetters = new List<Action>();
            _ip = 0;
        }

        private void ReadvA(Instruction ins)
        {
            ins.Registers.Add(MethodDefinition.Body.Registers[Upper[_ip] & 0xF]);
        }

        private void ReadvAA(Instruction ins) {
            ins.Registers.Add(MethodDefinition.Body.Registers[Upper[_ip++]]);
        }

        private void ReadvAAAA(Instruction ins) {
            _ip++;
            ins.Registers.Add(MethodDefinition.Body.Registers[Codes[_ip++]]);
        }

        private void ReadvB(Instruction ins)
        {
            ins.Registers.Add(MethodDefinition.Body.Registers[Upper[_ip++] >> 4]);
        }

        private void ReadvBB(Instruction ins)
        {
            ins.Registers.Add(MethodDefinition.Body.Registers[Lower[_ip]]);
        }

        private void ReadvBBBB(Instruction ins) {
            ins.Registers.Add(MethodDefinition.Body.Registers[Codes[_ip++]]);
        }

        private void ReadvCC(Instruction ins) {
            ReadvAA(ins);
        }

        private sbyte ReadNibble()
        {
            return (sbyte)((Upper[_ip++] << 24) >> 28);
        }

        private short ReadShort(ref int codeUnitOffset)
        {
            return (short)Codes[codeUnitOffset++];
        }

        private int ReadInt(ref int codeUnitOffset)
        {
            // don't reuse ReadShort to keep bit sign
            var result = Codes[codeUnitOffset++];
            result |= Codes[codeUnitOffset++] << 16;
            return result;
        }

        private long ReadLong(ref int codeUnitOffset)
        {
            // don't reuse ReadShort to keep bit sign
            long result = Codes[codeUnitOffset++];
            result |= ((long)Codes[codeUnitOffset++]) << 16;
            result |= ((long)Codes[codeUnitOffset++]) << 32;
            result |= ((long)Codes[codeUnitOffset++]) << 48;
            return result;
        }

        private sbyte ReadSByte()
        {
            return (sbyte)Upper[_ip++];
        }

        public void ReadFrom(BinaryReader reader)
        {
            var registers = MethodDefinition.Body.Registers;
            InstructionsSize = reader.ReadUInt32();

            Codes = new int[InstructionsSize];
            Lower = new int[InstructionsSize];
            Upper = new int[InstructionsSize];

            for (var i = 0; i < InstructionsSize; i++)
            {
                Codes[i] = reader.ReadUInt16();
                Lower[i] = Codes[i] & 0xFF;
                Upper[i] = Codes[i] >> 8;
            }

            while (_ip < InstructionsSize)
            {
                int offset;
                int registerCount;
                int registerMask;

                var ins = new Instruction {OpCode = (OpCodes) Lower[_ip], Offset = _ip};

	            Lookup.Add(_ip, ins);
                MethodDefinition.Body.Instructions.Add(ins);

                switch (ins.OpCode)
                {

                    case OpCodes.Nop:
                    case OpCodes.ReturnVoid:
                        _ip++;
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
                        ReadvAA(ins);
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
                        ReadvA(ins);
                        ReadvB(ins);
                        break;
                    case OpCodes.MoveWideFrom16:
                    case OpCodes.MoveFrom16:
                    case OpCodes.MoveObjectFrom16:
                        // vAA, vBBBB
                        ReadvAA(ins);
                        ReadvBBBB(ins);
                        break;
                    case OpCodes.Move16:
                    case OpCodes.MoveObject16:
                        // vAAAA, vBBBB
                        ReadvAAAA(ins);
                        ReadvBBBB(ins);
                        break;
                    case OpCodes.Const4:
                        // vA, #+B
                        ReadvA(ins);
                        ins.Operand = (int) ReadNibble();
                        break;
                    case OpCodes.Const16:
                        // vAA, #+BBBB
                        ReadvAA(ins);
                        ins.Operand = (int)ReadShort(ref _ip);
                        break;
                    case OpCodes.ConstWide16:
                        // vAA, #+BBBB
                        ReadvAA(ins);
                        ins.Operand = (long)ReadShort(ref _ip);
                        break;
                    case OpCodes.Const:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = ReadInt(ref _ip);
                        break;
                    case OpCodes.ConstWide32:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = (long)ReadInt(ref _ip);
                        break;
                    case OpCodes.FillArrayData:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt(ref _ip);
                        ins.Operand = ExtractArrayData(ins.Offset + offset);
                        break;
                    case OpCodes.ConstHigh16:
                        // vAA, #+BBBB0000
                        ReadvAA(ins);
                        ins.Operand = ((long)ReadShort(ref _ip)) << 16;
                        break;
                    case OpCodes.ConstWide:
                        // vAA, #+BBBBBBBBBBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = ReadLong(ref _ip);
                        break;
                    case OpCodes.ConstWideHigh16:
                        // vAA, #+BBBB000000000000
                        ReadvAA(ins);
                        ins.Operand = ((long)ReadShort(ref _ip)) << 48;
                        break;
                    case OpCodes.ConstString:
                        // vAA, string@BBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.Strings[ReadShort(ref _ip)];
                        break;
                    case OpCodes.ConstStringJumbo:
                        // vAA, string@BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.Strings[ReadInt(ref _ip)];
                        break;
                    case OpCodes.ConstClass:
                    case OpCodes.NewInstance:
                    case OpCodes.CheckCast:
                        // vAA, type@BBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.TypeReferences[ReadShort(ref _ip)];
                        break;
                    case OpCodes.InstanceOf:
                    case OpCodes.NewArray:
                        // vA, vB, type@CCCC
                        ReadvA(ins);
                        ReadvB(ins);
                        ins.Operand = Dex.TypeReferences[ReadShort(ref _ip)];
                        break;
                    case OpCodes.FilledNewArray:
                        // {vD, vE, vF, vG, vA}, type@CCCC
                        registerMask = Upper[_ip++] << 16;
                        ins.Operand = Dex.TypeReferences[ReadShort(ref _ip)];
                        registerMask |= Codes[_ip++];
                        SetRegistersByMask(ins, registerMask);
                        break;
                    case OpCodes.FilledNewArrayRange:
                        // {vCCCC .. vNNNN}, type@BBBB
                        registerCount = Upper[_ip++] << 16;
                        ins.Operand = Dex.TypeReferences[ReadShort(ref _ip)];
                        ReadvBBBB(ins);
                        for (var i = 1; i < registerCount; i++)
                            ins.Registers.Add(registers[i + ins.Registers[0].Index]);
                        break;
                    case OpCodes.Goto:
                        // +AA
                        offset = ReadSByte();
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Goto16:
                        // +AAAA
                        _ip++;
                        offset = ReadShort(ref _ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Goto32:
                        // +AAAAAAAA
                        _ip++;
                        offset = ReadInt(ref _ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.PackedSwitch:
                        // vAA, +BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt(ref _ip);
                        ins.Operand = ExtractPackedSwitch(ins, ins.Offset + offset);
                        break;
                    case OpCodes.SparseSwitch:
                        // vAA, +BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt(ref _ip);
                        ins.Operand = ExtractSparseSwitch(ins, ins.Offset + offset);
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
                        ReadvAA(ins);
                        ReadvBB(ins);
                        ReadvCC(ins);
                        break;
                    case OpCodes.IfEq:
                    case OpCodes.IfNe:
                    case OpCodes.IfLt:
                    case OpCodes.IfGe:
                    case OpCodes.IfGt:
                    case OpCodes.IfLe:
                        // vA, vB, +CCCC
                        ReadvA(ins);
                        ReadvB(ins);
                        offset = ReadShort(ref _ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.IfEqz:
                    case OpCodes.IfNez:
                    case OpCodes.IfLtz:
                    case OpCodes.IfGez:
                    case OpCodes.IfGtz:
                    case OpCodes.IfLez:
                        // vAA, +BBBB
                        ReadvAA(ins);
                        offset = ReadShort(ref _ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
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
                        ReadvA(ins);
                        ReadvB(ins);
                        ins.Operand = Dex.FieldReferences[ReadShort(ref _ip)];
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
                        ReadvAA(ins);
                        ins.Operand = Dex.FieldReferences[ReadShort(ref _ip)];
                        break;
                    case OpCodes.InvokeVirtual:
                    case OpCodes.InvokeSuper:
                    case OpCodes.InvokeDirect:
                    case OpCodes.InvokeStatic:
                    case OpCodes.InvokeInterface:
                        // {vD, vE, vF, vG, vA}, meth@CCCC
                        registerMask = Upper[_ip++] << 16;
                        ins.Operand = Dex.MethodReferences[ReadShort(ref _ip)];
                        registerMask |= Codes[_ip++];
                        SetRegistersByMask(ins, registerMask);
                        break;
                    case OpCodes.InvokeVirtualRange:
                    case OpCodes.InvokeSuperRange:
                    case OpCodes.InvokeDirectRange:
                    case OpCodes.InvokeStaticRange:
                    case OpCodes.InvokeInterfaceRange:
                        // {vCCCC .. vNNNN}, meth@BBBB
                        registerCount = ReadSByte();
                        ins.Operand = Dex.MethodReferences[ReadShort(ref _ip)];
                        ReadvBBBB(ins);
                        for (var i = 1; i < registerCount; i++)
                            ins.Registers.Add(registers[i + ins.Registers[0].Index]);
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
                        ReadvA(ins);
                        ReadvB(ins);
                        ins.Operand = (int)ReadShort(ref _ip);
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
                        ReadvAA(ins);
                        ReadvBB(ins);
                        ins.Operand = ReadSByte();
                        break;

                    default:
                        throw new NotImplementedException(string.Concat("Unknown opcode:", ins.OpCode));
                }

                LookupLast.Add(_ip-1, ins);
            }
            
            if (_ip != InstructionsSize)
                throw new MalformedException("Instruction pointer out of range");

            foreach (var action in LazyInstructionsSetters)
                action();
        }

        private void SetRegistersByMask(Instruction ins, int registerMask)
        {
            var registerCount = registerMask >> 20;
            for (var i = 0; i < registerCount; i++)
                ins.Registers.Add(MethodDefinition.Body.Registers[(registerMask >> (i * 4)) & 0xF]);
        }

		// ReSharper disable UnusedParameter.Local
	    private void ProcessPseudoCode(PseudoOpCodes expected, ref int offset)
        {
            // auto reduce scope (PseudoCode data at the end)
            InstructionsSize = (uint)Math.Min(InstructionsSize, offset);
            var poc = (PseudoOpCodes)ReadShort(ref offset);
            
            if (poc != expected)
                throw new MalformedException("Unexpected Pseudo-code identifier");
        }
		// ReSharper restore UnusedParameter.Local

        private SparseSwitchData ExtractSparseSwitch(Instruction ins, int offset)
        {
            int baseOffset = offset;
            var result = new SparseSwitchData();
            ProcessPseudoCode(PseudoOpCodes.SparseSwitch, ref offset);

            int targetcount = ReadShort(ref offset);

            var keys = new int[targetcount];
            for (var i = 0; i < targetcount; i++)
                keys[i] = ReadInt(ref offset);

            for (var i = 0; i < targetcount; i++)
            {
                var index = i; // used for closure
                var target = ReadInt(ref offset);
                LazyInstructionsSetters.Add(() => result.Targets.Add(keys[index], Lookup[ins.Offset + target]));
            }

            if (offset - baseOffset != targetcount * 4 + 2)
                throw new MalformedException("Unexpected Sparse switch blocksize");

            return result;
        }

        private PackedSwitchData ExtractPackedSwitch(Instruction ins, int offset)
        {
            int baseOffset = offset;
            var result = new PackedSwitchData();
            ProcessPseudoCode(PseudoOpCodes.PackedSwitch, ref offset);

            int targetcount = ReadShort(ref offset);
            result.FirstKey = ReadInt(ref offset);

            for (var i=0; i<targetcount; i++) {
                var target = ReadInt(ref offset);
                LazyInstructionsSetters.Add( () => result.Targets.Add(Lookup[ins.Offset + target]));
            }

            if (offset - baseOffset != targetcount * 2 + 4)
                throw new MalformedException("Unexpected Packed switch blocksize");

            return result;
        }

        private object[] ExtractArrayData(int offset)
        {
            var baseOffset = offset;
            ProcessPseudoCode(PseudoOpCodes.FillArrayData, ref offset);

            int elementsize = ReadShort(ref offset);
            var elementcount = ReadInt(ref offset);
            var items = new List<object>();

            var next = false;
            for (var i = 0; i < elementcount; i++)
            {
                switch (elementsize)
                {
                    case 1:
                        items.Add(next ? (sbyte)((Codes[offset++] >> 8) & 0xff) : (sbyte)(Codes[offset] & 0xff));
                        next = !next;
                        break;
                    case 2:
                        items.Add(ReadShort(ref offset));
                        break;
                    case 4:
                        items.Add(ReadInt(ref offset));
                        break;
                    case 8:
                        items.Add(ReadLong(ref offset));
                        break;
                    default:
                        throw new MalformedException("Unexpected Fill-array-data element size");
                }
            }

            if ((elementcount % 2 != 0) && (elementsize == 1))
                offset++;

            if (offset - baseOffset != (elementsize * elementcount + 1) / 2 + 4)
                throw new MalformedException("Unexpected Fill-array-data blocksize");

            return items.ToArray();
        }

    }

}
