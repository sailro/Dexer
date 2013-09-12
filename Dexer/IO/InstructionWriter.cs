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

using System.IO;
using Dexer.Core;
using Dexer.Instructions;
using System.Collections.Generic;
using System;

namespace Dexer.IO
{
    internal class InstructionWriter
    {
        private DexWriter DexWriter { get; set; }
        private MethodDefinition MethodDefinition { get; set; }
        internal ushort[] Codes { get; set; }
        private int _ip;
        private int _extraOffset;

        internal Dictionary<Instruction, int> LookupLast; // ending offsets by instruction

        public InstructionWriter(DexWriter dexWriter, MethodDefinition method)
        {
            DexWriter = dexWriter;
            MethodDefinition = method;
            LookupLast = new Dictionary<Instruction, int>();
            _ip = 0;
            _extraOffset = 0;
        }

        public void WriteTo(BinaryWriter writer)
        {
            OffsetStatistics stats = MethodDefinition.Body.UpdateInstructionOffsets();
            _extraOffset = stats.CodeUnits;
            Codes = new ushort[stats.CodeUnits + stats.ExtraCodeUnits];

	        foreach (var ins in MethodDefinition.Body.Instructions)
            {
                if (_ip != ins.Offset)
                    throw new InstructionException(ins, "Instruction pointer do not match");

                Codes[_ip] = (ushort)ins.OpCode;
	            int registerMask;
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
                        WritevAA(ins);
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
                        WritevA(ins);
                        WritevB(ins);
                        break;
                    case OpCodes.MoveWideFrom16:
                    case OpCodes.MoveFrom16:
                    case OpCodes.MoveObjectFrom16:
                        // vAA, vBBBB
                        WritevAA(ins);
                        WritevBBBB(ins);
                        break;
                    case OpCodes.Move16:
                    case OpCodes.MoveObject16:
                        // vAAAA, vBBBB
                        WritevAAAA(ins);
                        WritevBBBB(ins);
                        break;
                    case OpCodes.Const4:
                        // vA, #+B
                        WritevA(ins);
                        WriteNibble(ins);
                        break;
                    case OpCodes.Const16:
                    case OpCodes.ConstWide16:
                        // vAA, #+BBBB
                        WritevAA(ins);
                        WriteShort(ins);
                        break;
                    case OpCodes.Const:
                    case OpCodes.ConstWide32:
                        // vAA, #+BBBBBBBB
                        WritevAA(ins);
                        WriteInt(ins);
                        break;
                    case OpCodes.FillArrayData:
                        // vAA, #+BBBBBBBB
                        WritevAA(ins);
                        WriteInt(_extraOffset - ins.Offset, ref _ip);
                        WriteArrayData(ins);
                        break;
                    case OpCodes.ConstHigh16:
                        // vAA, #+BBBB0000
                        WritevAA(ins);
                        WriteShort(ins, 16);
                        break;
                    case OpCodes.ConstWide:
                        // vAA, #+BBBBBBBBBBBBBBBB
                        WritevAA(ins);
                        WriteLong(ins);
                        break;
                    case OpCodes.ConstWideHigh16:
                        // vAA, #+BBBB000000000000
                        WritevAA(ins);
                        WriteShort(ins, 48);
                        break;
                    case OpCodes.ConstString:
                        // vAA, string@BBBB
                        WritevAA(ins);
                        WriteShortStringIndex(ins);
                        break;
                    case OpCodes.ConstStringJumbo:
                        // vAA, string@BBBBBBBB
                        WritevAA(ins);
                        WriteIntStringIndex(ins);
                        break;
                    case OpCodes.ConstClass:
                    case OpCodes.NewInstance:
                    case OpCodes.CheckCast:
                        // vAA, type@BBBB
                        WritevAA(ins);
                        WriteShortTypeIndex(ins); 
                        break;
                    case OpCodes.InstanceOf:
                    case OpCodes.NewArray:
                        // vA, vB, type@CCCC
                        WritevA(ins);
                        WritevB(ins);
                        WriteShortTypeIndex(ins); 
                        break;
                    case OpCodes.FilledNewArray:
                        // {vD, vE, vF, vG, vA}, type@CCCC
                        registerMask = GetRegisterMask(ins);
                        Codes[_ip++] |= (ushort)(registerMask >> 16 << 8);
                        WriteShortTypeIndex(ins);
                        Codes[_ip++] |= (ushort)(registerMask << 12 >> 12);
                        break;
                    case OpCodes.FilledNewArrayRange:
                        // {vCCCC .. vNNNN}, type@BBBB
                        /*registerCount = Upper[Ip++] << 16;
                        ins.Operand = Dex.TypeReferences[ReadShort(ref Ip)];
                        ReadvBBBB(ins);
                        for (int i = 1; i < registerCount; i++)
                            ins.Registers.Add(registers[i + ins.Registers[0].Index]);*/
                        throw new NotImplementedException();
                    case OpCodes.Goto:
                        // +AA
                        WriteSbyteInstructionOffset(ins);
                        break;
                    case OpCodes.Goto16:
                        // +AAAA
                        _ip++;
                        WriteShortInstructionOffset(ins);
                        break;
                    case OpCodes.Goto32:
                        // +AAAAAAAA
                        _ip++;
                        WriteIntInstructionOffset(ins);
                        break;
                    case OpCodes.PackedSwitch:
                        // vAA, +BBBBBBBB
                        WritevAA(ins);
                        WriteInt(_extraOffset - ins.Offset, ref _ip);
                        WritePackedSwitch(ins);
                        break;
                    case OpCodes.SparseSwitch:
                        // vAA, +BBBBBBBB
                        WritevAA(ins);
                        WriteInt(_extraOffset - ins.Offset, ref _ip);
                        WriteSparseSwitch(ins);
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
                        WritevAA(ins);
                        WritevBB(ins);
                        WritevCC(ins);
                        break;
                    case OpCodes.IfEq:
                    case OpCodes.IfNe:
                    case OpCodes.IfLt:
                    case OpCodes.IfGe:
                    case OpCodes.IfGt:
                    case OpCodes.IfLe:
                        // vA, vB, +CCCC
                        WritevA(ins);
                        WritevB(ins);
                        WriteShortInstructionOffset(ins);
                        break;
                    case OpCodes.IfEqz:
                    case OpCodes.IfNez:
                    case OpCodes.IfLtz:
                    case OpCodes.IfGez:
                    case OpCodes.IfGtz:
                    case OpCodes.IfLez:
                        // vAA, +BBBB
                        WritevAA(ins);
                        WriteShortInstructionOffset(ins);
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
                        WritevA(ins);
                        WritevB(ins);
                        WriteShortFieldIndex(ins);
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
                        WritevAA(ins);
                        WriteShortFieldIndex(ins);
                        break;
                    case OpCodes.InvokeVirtual:
                    case OpCodes.InvokeSuper:
                    case OpCodes.InvokeDirect:
                    case OpCodes.InvokeStatic:
                    case OpCodes.InvokeInterface:
                        // {vD, vE, vF, vG, vA}, meth@CCCC
                        registerMask = GetRegisterMask(ins);
                        Codes[_ip++] |= (ushort) (registerMask >> 16 << 8);
                        WriteShortMethodIndex(ins);
                        Codes[_ip++] |= (ushort) (registerMask << 12 >> 12);
                        break;
                    case OpCodes.InvokeVirtualRange:
                    case OpCodes.InvokeSuperRange:
                    case OpCodes.InvokeDirectRange:
                    case OpCodes.InvokeStaticRange:
                    case OpCodes.InvokeInterfaceRange:
                        // {vCCCC .. vNNNN}, meth@BBBB
                        WriteSByte(ins.Registers.Count);
                        WriteShortMethodIndex(ins);
                        Codes[_ip++] |= (ushort)CheckRegister(ins, 0, 0xFFFF);
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
                        WritevA(ins);
                        WritevB(ins);
                        WriteShort(ins);
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
                        WritevAA(ins);
                        WritevBB(ins);
                        WriteSByte(ins);
                        break;

                    default:
                        throw new NotImplementedException(string.Concat("Unknown opcode:", ins.OpCode));
                }

                LookupLast.Add(ins, _ip-1);
            }

            if (_ip != stats.CodeUnits)
                throw new MalformedException("Instruction pointer out of range");

            if (_extraOffset != stats.CodeUnits + stats.ExtraCodeUnits)
                throw new MalformedException("Data pointer out of range");

            writer.Write(_extraOffset);
            for (var i = 0; i < _extraOffset; i++)
                writer.Write(Codes[i]);
        }

        private int GetRegisterMask(Instruction ins)
        {
            var registerCount = ins.Registers.Count;
            var registerMask = registerCount << 20;

            for (var i = 0; i < registerCount; i++)
                registerMask |= CheckRegister(ins, i, 0xF) << (i * 4);

            return registerMask;
        }

        #region " Nibble "
        private void WriteNibble(Instruction ins)
        {
            Codes[_ip++] |= (ushort)((int)ins.Operand << 12);
        }
        #endregion

        #region " SByte "
        private void WriteSByte(object value)
        {
            Codes[_ip++] |= (ushort)(Convert.ToSByte(value) << 8);
        }

        private void WriteSbyteInstructionOffset(Instruction ins)
        {
            if (!(ins.Operand is Instruction))
                throw new InstructionException(ins, "Expecting Instruction");

            WriteSByte((ins.Operand as Instruction).Offset - ins.Offset);
        }

        private void WriteSByte(Instruction ins)
        {
            WriteSByte(ins.Operand);
        }
        #endregion

        #region " Short "
        private void WriteShortInstructionOffset(Instruction ins)
        {
            if (!(ins.Operand is Instruction))
                throw new InstructionException(ins, "Expecting Instruction");

            WriteShort((ins.Operand as Instruction).Offset - ins.Offset, ref _ip);
        }

        private void WriteShortFieldIndex(Instruction ins)
        {
            if (!(ins.Operand is FieldReference))
                throw new InstructionException(ins, "Expecting FieldReference");

            WriteShort(DexWriter.FieldLookup[ins.Operand as FieldReference], ref _ip);
        }

        private void WriteShortMethodIndex(Instruction ins)
        {
            if (!(ins.Operand is MethodReference))
                throw new InstructionException(ins, "Expecting MethodReference");

            WriteShort(DexWriter.MethodLookup[ins.Operand as MethodReference], ref _ip);
        }

        private void WriteShortStringIndex(Instruction ins)
        {
            if (!(ins.Operand is String))
                throw new InstructionException(ins, "Expecting String");

            WriteShort(DexWriter.StringLookup[ins.Operand as String], ref _ip);
        }

        private void WriteShortTypeIndex(Instruction ins)
        {
            if (!(ins.Operand is TypeReference))
                throw new InstructionException(ins, "Expecting TypeReference");

            WriteShort(DexWriter.TypeLookup[ins.Operand as TypeReference], ref _ip);
        }

        private void WriteShort(Instruction ins, int shift)
        {
            long value = Convert.ToInt64(ins.Operand) >> shift;
            WriteShort(value, ref _ip);
        }

        private void WriteShort(Instruction ins)
        {
            WriteShort(ins.Operand, ref _ip);
        }

        private void WriteShort(object value, ref int codeUnitOffset)
        {
            Codes[codeUnitOffset++] = (ushort) Convert.ToInt16(value);
        }
        #endregion

        #region " Int "
		// ReSharper disable UnusedParameter.Local
        private static void WriteIntInstructionOffset(Instruction ins)
        {
            throw new NotImplementedException();
        }

        private static void WriteIntStringIndex(Instruction ins)
        {
            throw new NotImplementedException();
        }
		// ReSharper restore UnusedParameter.Local

        private void WriteInt(object value, ref int codeUnitOffset)
        {
            int result = Convert.ToInt32(value);
            Codes[codeUnitOffset++] = (ushort) (result & 0xffff); 
            Codes[codeUnitOffset++] = (ushort) (result >> 16);
        }

        private void WriteInt(Instruction ins)
        {
            WriteInt(ins.Operand, ref _ip);
        }
        #endregion

        #region " Long "
        private void WriteLong(object value, ref int codeUnitOffset)
        {
            var result = Convert.ToInt64(value);
            Codes[codeUnitOffset++] = (ushort)(result & 0xffff);
            Codes[codeUnitOffset++] = (ushort)((result >> 16) & 0xffff);
            Codes[codeUnitOffset++] = (ushort)((result >> 32) & 0xffff);
            Codes[codeUnitOffset++] = (ushort)(result >> 48);
        }

        private void WriteLong(Instruction ins)
        {
            WriteLong(ins.Operand, ref _ip);
        }
        #endregion

        #region " Pseudo OpCodes "
        private void WriteSparseSwitch(Instruction ins)
        {
            if (!(ins.Operand is SparseSwitchData))
                throw new InstructionException(ins, "Expecting SparseSwitchData");
            var data = ins.Operand as SparseSwitchData;

            WriteShort((short)PseudoOpCodes.SparseSwitch, ref _extraOffset);
            WriteShort(data.Targets.Count, ref _extraOffset);

            foreach(var key in data.Targets.Keys)
                WriteInt(key, ref _extraOffset);

            foreach(var key in data.Targets.Keys)
                WriteInt(data.Targets[key].Offset - ins.Offset, ref _extraOffset);
        }

        private void WritePackedSwitch(Instruction ins)
        {
            if (!(ins.Operand is PackedSwitchData))
                throw new InstructionException(ins, "Expecting PackedSwitchData");
            var data = ins.Operand as PackedSwitchData;
 
            WriteShort((short)PseudoOpCodes.PackedSwitch, ref _extraOffset);
            WriteShort(data.Targets.Count, ref _extraOffset);
            WriteInt(data.FirstKey, ref _extraOffset);

            foreach(var target in data.Targets)
                WriteInt(target.Offset - ins.Offset, ref _extraOffset);
        }

        private void WriteArrayData(Instruction ins)
        {
            Array elements;
            Type elementtype;
            int elementsize;
            MethodBody.CheckArrayData(ins, out elements, out elementtype, out elementsize);

            WriteShort(PseudoOpCodes.FillArrayData, ref _extraOffset);
            WriteShort(elementsize, ref _extraOffset);
            WriteInt(elements.Length, ref _extraOffset);

            var next = false;
            foreach (var element in elements)
            {
                switch (elementsize)
                {
                    case 1:
                        if (next)
                            Codes[_extraOffset++] |= (ushort)((byte)(Convert.ToSByte(element)) << 8);
                        else
                            Codes[_extraOffset] |= (byte)Convert.ToSByte(element);
                        next = !next;
                        break;
                    case 2:
                        WriteShort(element, ref _extraOffset);
                        break;
                    case 4:
                        WriteInt(element, ref _extraOffset);
                        break;
                    case 8:
                        WriteLong(element, ref _extraOffset);
                        break;
                    default:
                        throw new InstructionException(ins, "Unexpected Fill-array-data element size");
                }
            }

            if ((elements.Length % 2 != 0) && (elementsize == 1))
                _extraOffset++;
        }
        #endregion
        
        #region " Registers "
        private void WritevA(Instruction ins)
        {
            Codes[_ip] |= (ushort)(CheckRegister(ins, 0, 0xF) << 8);
        }

        private void WritevAA(Instruction ins)
        {
            Codes[_ip++] |= (ushort)(CheckRegister(ins, 0, 0xFF) << 8);
        }

		// ReSharper disable UnusedParameter.Local
        private static void WritevAAAA(Instruction ins)
        {
            throw new NotImplementedException();
        }
		// ReSharper restore UnusedParameter.Local

        private void WritevB(Instruction ins)
        {
            Codes[_ip++] |= (ushort)(CheckRegister(ins, 1, 0xF) << 12);
        }

        private void WritevBB(Instruction ins)
        {
            Codes[_ip] |= (ushort) CheckRegister(ins, 1, 0xFF);
        }

        private void WritevBBBB(Instruction ins)
        {
            Codes[_ip++] |= (ushort)CheckRegister(ins, 1, 0xFFFF);
        }

        private void WritevCC(Instruction ins)
        {
            Codes[_ip++] |= (ushort)(CheckRegister(ins, 2, 0xFF) << 8);
        }

        private static int CheckRegister(Instruction ins, int position, int maxIndex)
        {
            if (ins.Registers.Count <= position)
                throw new InstructionException(ins, string.Format("Expecting register at position {0}", position));

            int index = ins.Registers[position].Index;
            if (index < 0 || index > maxIndex)
                throw new InstructionException(ins, string.Format("Register index out of range [0..{0}]", maxIndex));

            return index;
        }
        #endregion

    }
}
