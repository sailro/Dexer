/* Dexer Copyright (c) 2010-2011 Sebastien LEBRETON

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
using Dexer.Metadata;

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
        private int Ip;
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
            Ip = 0;
        }

        private void ReadvA(Instruction ins)
        {
            ins.Registers.Add(MethodDefinition.Body.Registers[Upper[Ip] & 0xF]);
        }

        private void ReadvAA(Instruction ins) {
            ins.Registers.Add(MethodDefinition.Body.Registers[Upper[Ip++]]);
        }

        private void ReadvAAAA(Instruction ins) {
            Ip++;
            ins.Registers.Add(MethodDefinition.Body.Registers[Codes[Ip++]]);
        }

        private void ReadvB(Instruction ins)
        {
            ins.Registers.Add(MethodDefinition.Body.Registers[Upper[Ip++] >> 4]);
        }

        private void ReadvBB(Instruction ins)
        {
            ins.Registers.Add(MethodDefinition.Body.Registers[Lower[Ip]]);
        }

        private void ReadvBBBB(Instruction ins) {
            ins.Registers.Add(MethodDefinition.Body.Registers[Codes[Ip++]]);
        }

        private void ReadvCC(Instruction ins) {
            ReadvAA(ins);
        }

        private sbyte ReadNibble()
        {
            return (sbyte)((Upper[Ip++] << 24) >> 28);
        }

        private short ReadShort(ref int codeUnitOffset)
        {
            return (short)Codes[codeUnitOffset++];
        }

        private int ReadInt(ref int codeUnitOffset)
        {
            // don't reuse ReadShort to keep bit sign
            int result = Codes[codeUnitOffset++];
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

        private sbyte ReadSByte(ref int codeUnitOffset)
        {
            return (sbyte)Upper[Ip++];
        }

        public void ReadFrom(BinaryReader reader)
        {
            var registers = MethodDefinition.Body.Registers;
            InstructionsSize = reader.ReadUInt32();

            Codes = new int[InstructionsSize];
            Lower = new int[InstructionsSize];
            Upper = new int[InstructionsSize];

            for (int i = 0; i < InstructionsSize; i++)
            {
                Codes[i] = reader.ReadUInt16();
                Lower[i] = Codes[i] & 0xFF;
                Upper[i] = Codes[i] >> 8;
            }

            while (Ip < InstructionsSize)
            {
                int offset;
                int registerCount;
                int registerMask;

                Instruction ins = new Instruction();
                ins.OpCode = (OpCodes)Lower[Ip];
                ins.Offset = Ip;

                Lookup.Add(Ip, ins);
                MethodDefinition.Body.Instructions.Add(ins);

                switch (ins.OpCode)
                {

                    case OpCodes.Nop:
                    case OpCodes.Return_void:
                        Ip++;
                        break;
                    case OpCodes.Move_result:
                    case OpCodes.Move_result_wide:
                    case OpCodes.Move_result_object:
                    case OpCodes.Move_exception:
                    case OpCodes.Return:
                    case OpCodes.Return_wide:
                    case OpCodes.Return_object:
                    case OpCodes.Monitor_enter:
                    case OpCodes.Monitor_exit:
                    case OpCodes.Throw:
                        // vAA
                        ReadvAA(ins);
                        break;
                    case OpCodes.Move_object:
                    case OpCodes.Move_wide:
                    case OpCodes.Move:
                    case OpCodes.Array_length:
                    case OpCodes.Neg_int:
                    case OpCodes.Not_int:
                    case OpCodes.Neg_long:
                    case OpCodes.Not_long:
                    case OpCodes.Neg_float:
                    case OpCodes.Neg_double:
                    case OpCodes.Int_to_long:
                    case OpCodes.Int_to_float:
                    case OpCodes.Int_to_double:
                    case OpCodes.Long_to_int:
                    case OpCodes.Long_to_float:
                    case OpCodes.Long_to_double:
                    case OpCodes.Float_to_int:
                    case OpCodes.Float_to_long:
                    case OpCodes.Float_to_double:
                    case OpCodes.Double_to_int:
                    case OpCodes.Double_to_long:
                    case OpCodes.Double_to_float:
                    case OpCodes.Int_to_byte:
                    case OpCodes.Int_to_char:
                    case OpCodes.Int_to_short:
                    case OpCodes.Add_int_2addr:
                    case OpCodes.Sub_int_2addr:
                    case OpCodes.Mul_int_2addr:
                    case OpCodes.Div_int_2addr:
                    case OpCodes.Rem_int_2addr:
                    case OpCodes.And_int_2addr:
                    case OpCodes.Or_int_2addr:
                    case OpCodes.Xor_int_2addr:
                    case OpCodes.Shl_int_2addr:
                    case OpCodes.Shr_int_2addr:
                    case OpCodes.Ushr_int_2addr:
                    case OpCodes.Add_long_2addr:
                    case OpCodes.Sub_long_2addr:
                    case OpCodes.Mul_long_2addr:
                    case OpCodes.Div_long_2addr:
                    case OpCodes.Rem_long_2addr:
                    case OpCodes.And_long_2addr:
                    case OpCodes.Or_long_2addr:
                    case OpCodes.Xor_long_2addr:
                    case OpCodes.Shl_long_2addr:
                    case OpCodes.Shr_long_2addr:
                    case OpCodes.Ushr_long_2addr:
                    case OpCodes.Add_float_2addr:
                    case OpCodes.Sub_float_2addr:
                    case OpCodes.Mul_float_2addr:
                    case OpCodes.Div_float_2addr:
                    case OpCodes.Rem_float_2addr:
                    case OpCodes.Add_double_2addr:
                    case OpCodes.Sub_double_2addr:
                    case OpCodes.Mul_double_2addr:
                    case OpCodes.Div_double_2addr:
                    case OpCodes.Rem_double_2addr:
                        // vA, vB
                        ReadvA(ins);
                        ReadvB(ins);
                        break;
                    case OpCodes.Move_wide_from16:
                    case OpCodes.Move_from16:
                    case OpCodes.Move_object_from16:
                        // vAA, vBBBB
                        ReadvAA(ins);
                        ReadvBBBB(ins);
                        break;
                    case OpCodes.Move_16:
                    case OpCodes.Move_object_16:
                        // vAAAA, vBBBB
                        ReadvAAAA(ins);
                        ReadvBBBB(ins);
                        break;
                    case OpCodes.Const_4:
                        // vA, #+B
                        ReadvA(ins);
                        ins.Operand = (int) ReadNibble();
                        break;
                    case OpCodes.Const_16:
                        // vAA, #+BBBB
                        ReadvAA(ins);
                        ins.Operand = (int)ReadShort(ref Ip);
                        break;
                    case OpCodes.Const_wide_16:
                        // vAA, #+BBBB
                        ReadvAA(ins);
                        ins.Operand = (long)ReadShort(ref Ip);
                        break;
                    case OpCodes.Const:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = (int)ReadInt(ref Ip);
                        break;
                    case OpCodes.Const_wide_32:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = (long)ReadInt(ref Ip);
                        break;
                    case OpCodes.Fill_array_data:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt(ref Ip);
                        ins.Operand = ExtractArrayData(ins.Offset + offset);
                        break;
                    case OpCodes.Const_high16:
                        // vAA, #+BBBB0000
                        ReadvAA(ins);
                        ins.Operand = ((long)ReadShort(ref Ip)) << 16;
                        break;
                    case OpCodes.Const_wide:
                        // vAA, #+BBBBBBBBBBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = ReadLong(ref Ip);
                        break;
                    case OpCodes.Const_wide_high16:
                        // vAA, #+BBBB000000000000
                        ReadvAA(ins);
                        ins.Operand = ((long)ReadShort(ref Ip)) << 48;
                        break;
                    case OpCodes.Const_string:
                        // vAA, string@BBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.Strings[ReadShort(ref Ip)];
                        break;
                    case OpCodes.Const_string_jumbo:
                        // vAA, string@BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.Strings[ReadInt(ref Ip)];
                        break;
                    case OpCodes.Const_class:
                    case OpCodes.New_instance:
                    case OpCodes.Check_cast:
                        // vAA, type@BBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.TypeReferences[ReadShort(ref Ip)];
                        break;
                    case OpCodes.Instance_of:
                    case OpCodes.New_array:
                        // vA, vB, type@CCCC
                        ReadvA(ins);
                        ReadvB(ins);
                        ins.Operand = Dex.TypeReferences[ReadShort(ref Ip)];
                        break;
                    case OpCodes.Filled_new_array:
                        // {vD, vE, vF, vG, vA}, type@CCCC
                        registerMask = Upper[Ip++] << 16;
                        ins.Operand = Dex.TypeReferences[ReadShort(ref Ip)];
                        registerMask |= Codes[Ip++];
                        SetRegistersByMask(ins, registerMask);
                        break;
                    case OpCodes.Filled_new_array_range:
                        // {vCCCC .. vNNNN}, type@BBBB
                        registerCount = Upper[Ip++] << 16;
                        ins.Operand = Dex.TypeReferences[ReadShort(ref Ip)];
                        ReadvBBBB(ins);
                        for (int i = 1; i < registerCount; i++)
                            ins.Registers.Add(registers[i + ins.Registers[0].Index]);
                        break;
                    case OpCodes.Goto:
                        // +AA
                        offset = (sbyte)ReadSByte(ref Ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Goto_16:
                        // +AAAA
                        Ip++;
                        offset = (short)ReadShort(ref Ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Goto_32:
                        // +AAAAAAAA
                        Ip++;
                        offset = ReadInt(ref Ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Packed_switch:
                        // vAA, +BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt(ref Ip);
                        ins.Operand = ExtractPackedSwitch(ins, ins.Offset + offset);
                        break;
                    case OpCodes.Sparse_switch:
                        // vAA, +BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt(ref Ip);
                        ins.Operand = ExtractSparseSwitch(ins, ins.Offset + offset);
                        break;
                    case OpCodes.Cmpl_float:
                    case OpCodes.Cmpg_float:
                    case OpCodes.Cmpl_double:
                    case OpCodes.Cmpg_double:
                    case OpCodes.Cmp_long:
                    case OpCodes.Aget:
                    case OpCodes.Aget_wide:
                    case OpCodes.Aget_object:
                    case OpCodes.Aget_boolean:
                    case OpCodes.Aget_byte:
                    case OpCodes.Aget_char:
                    case OpCodes.Aget_short:
                    case OpCodes.Aput:
                    case OpCodes.Aput_wide:
                    case OpCodes.Aput_object:
                    case OpCodes.Aput_boolean:
                    case OpCodes.Aput_byte:
                    case OpCodes.Aput_char:
                    case OpCodes.Aput_short:
                    case OpCodes.Add_int:
                    case OpCodes.Sub_int:
                    case OpCodes.Mul_int:
                    case OpCodes.Div_int:
                    case OpCodes.Rem_int:
                    case OpCodes.And_int:
                    case OpCodes.Or_int:
                    case OpCodes.Xor_int:
                    case OpCodes.Shl_int:
                    case OpCodes.Shr_int:
                    case OpCodes.Ushr_int:
                    case OpCodes.Add_long:
                    case OpCodes.Sub_long:
                    case OpCodes.Mul_long:
                    case OpCodes.Div_long:
                    case OpCodes.Rem_long:
                    case OpCodes.And_long:
                    case OpCodes.Or_long:
                    case OpCodes.Xor_long:
                    case OpCodes.Shl_long:
                    case OpCodes.Shr_long:
                    case OpCodes.Ushr_long:
                    case OpCodes.Add_float:
                    case OpCodes.Sub_float:
                    case OpCodes.Mul_float:
                    case OpCodes.Div_float:
                    case OpCodes.Rem_float:
                    case OpCodes.Add_double:
                    case OpCodes.Sub_double:
                    case OpCodes.Mul_double:
                    case OpCodes.Div_double:
                    case OpCodes.Rem_double:
                        // vAA, vBB, vCC
                        ReadvAA(ins);
                        ReadvBB(ins);
                        ReadvCC(ins);
                        break;
                    case OpCodes.If_eq:
                    case OpCodes.If_ne:
                    case OpCodes.If_lt:
                    case OpCodes.If_ge:
                    case OpCodes.If_gt:
                    case OpCodes.If_le:
                        // vA, vB, +CCCC
                        ReadvA(ins);
                        ReadvB(ins);
                        offset = (short)ReadShort(ref Ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.If_eqz:
                    case OpCodes.If_nez:
                    case OpCodes.If_ltz:
                    case OpCodes.If_gez:
                    case OpCodes.If_gtz:
                    case OpCodes.If_lez:
                        // vAA, +BBBB
                        ReadvAA(ins);
                        offset = (short)ReadShort(ref Ip);
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Iget:
                    case OpCodes.Iget_wide:
                    case OpCodes.Iget_object:
                    case OpCodes.Iget_boolean:
                    case OpCodes.Iget_byte:
                    case OpCodes.Iget_char:
                    case OpCodes.Iget_short:
                    case OpCodes.Iput:
                    case OpCodes.Iput_wide:
                    case OpCodes.Iput_object:
                    case OpCodes.Iput_boolean:
                    case OpCodes.Iput_byte:
                    case OpCodes.Iput_char:
                    case OpCodes.Iput_short:
                        // vA, vB, field@CCCC
                        ReadvA(ins);
                        ReadvB(ins);
                        ins.Operand = Dex.FieldReferences[ReadShort(ref Ip)];
                        break;
                    case OpCodes.Sget:
                    case OpCodes.Sget_wide:
                    case OpCodes.Sget_object:
                    case OpCodes.Sget_boolean:
                    case OpCodes.Sget_byte:
                    case OpCodes.Sget_char:
                    case OpCodes.Sget_short:
                    case OpCodes.Sput:
                    case OpCodes.Sput_wide:
                    case OpCodes.Sput_object:
                    case OpCodes.Sput_boolean:
                    case OpCodes.Sput_byte:
                    case OpCodes.Sput_char:
                    case OpCodes.Sput_short:
                        // vAA, field@BBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.FieldReferences[ReadShort(ref Ip)];
                        break;
                    case OpCodes.Invoke_virtual:
                    case OpCodes.Invoke_super:
                    case OpCodes.Invoke_direct:
                    case OpCodes.Invoke_static:
                    case OpCodes.Invoke_interface:
                        // {vD, vE, vF, vG, vA}, meth@CCCC
                        registerMask = Upper[Ip++] << 16;
                        ins.Operand = Dex.MethodReferences[ReadShort(ref Ip)];
                        registerMask |= Codes[Ip++];
                        SetRegistersByMask(ins, registerMask);
                        break;
                    case OpCodes.Invoke_virtual_range:
                    case OpCodes.Invoke_super_range:
                    case OpCodes.Invoke_direct_range:
                    case OpCodes.Invoke_static_range:
                    case OpCodes.Invoke_interface_range:
                        // {vCCCC .. vNNNN}, meth@BBBB
                        registerCount = ReadSByte(ref Ip);
                        ins.Operand = Dex.MethodReferences[ReadShort(ref Ip)];
                        ReadvBBBB(ins);
                        for (int i = 1; i < registerCount; i++)
                            ins.Registers.Add(registers[i + ins.Registers[0].Index]);
                        break;
                    case OpCodes.Add_int_lit16:
                    case OpCodes.Rsub_int:
                    case OpCodes.Mul_int_lit16:
                    case OpCodes.Div_int_lit16:
                    case OpCodes.Rem_int_lit16:
                    case OpCodes.And_int_lit16:
                    case OpCodes.Or_int_lit16:
                    case OpCodes.Xor_int_lit16:
                        // vA, vB, #+CCCC
                        ReadvA(ins);
                        ReadvB(ins);
                        ins.Operand = (int)ReadShort(ref Ip);
                        break;
                    case OpCodes.Add_int_lit8:
                    case OpCodes.Rsub_int_lit8:
                    case OpCodes.Mul_int_lit8:
                    case OpCodes.Div_int_lit8:
                    case OpCodes.Rem_int_lit8:
                    case OpCodes.And_int_lit8:
                    case OpCodes.Or_int_lit8:
                    case OpCodes.Xor_int_lit8:
                    case OpCodes.Shl_int_lit8:
                    case OpCodes.Shr_int_lit8:
                    case OpCodes.Ushr_int_lit8:
                        // vAA, vBB, #+CC
                        ReadvAA(ins);
                        ReadvBB(ins);
                        ins.Operand = ReadSByte(ref Ip);
                        break;

                    default:
                        throw new NotImplementedException(string.Concat("Unknown opcode:", ins.OpCode));
                }

                LookupLast.Add(Ip-1, ins);
            }
            
            if (Ip != InstructionsSize)
                throw new MalformedException("Instruction pointer out of range");

            foreach (Action action in LazyInstructionsSetters)
                action();
        }

        private void SetRegistersByMask(Instruction ins, int registerMask)
        {
            int registerCount = registerMask >> 20;
            for (int i = 0; i < registerCount; i++)
                ins.Registers.Add(MethodDefinition.Body.Registers[(registerMask >> (i * 4)) & 0xF]);
        }

        private void ProcessPseudoCode(PseudoOpCodes expected, ref int offset)
        {
            // auto reduce scope (PseudoCode data at the end)
            InstructionsSize = (uint)Math.Min(InstructionsSize, offset);
            PseudoOpCodes poc = (PseudoOpCodes)ReadShort(ref offset);
            
            if (poc != expected)
                throw new MalformedException("Unexpected Pseudo-code identifier");
        }

        private SparseSwitchData ExtractSparseSwitch(Instruction ins, int offset)
        {
            int baseOffset = offset;
            SparseSwitchData result = new SparseSwitchData();
            ProcessPseudoCode(PseudoOpCodes.Sparse_switch, ref offset);

            int targetcount = ReadShort(ref offset);

            int[] keys = new int[targetcount];
            for (int i = 0; i < targetcount; i++)
                keys[i] = ReadInt(ref offset);

            for (int i = 0; i < targetcount; i++)
            {
                int index = i; // used for closure
                int target = ReadInt(ref offset);
                LazyInstructionsSetters.Add(() => result.Targets.Add(keys[index], Lookup[ins.Offset + target]));
            }

            if (offset - baseOffset != targetcount * 4 + 2)
                throw new MalformedException("Unexpected Sparse switch blocksize");

            return result;
        }

        private PackedSwitchData ExtractPackedSwitch(Instruction ins, int offset)
        {
            int baseOffset = offset;
            PackedSwitchData result = new PackedSwitchData();
            ProcessPseudoCode(PseudoOpCodes.Packed_switch, ref offset);

            int targetcount = ReadShort(ref offset);
            result.FirstKey = ReadInt(ref offset);

            for (int i=0; i<targetcount; i++) {
                int target = ReadInt(ref offset);
                LazyInstructionsSetters.Add( () => result.Targets.Add(Lookup[ins.Offset + target]));
            }

            if (offset - baseOffset != targetcount * 2 + 4)
                throw new MalformedException("Unexpected Packed switch blocksize");

            return result;
        }

        private object[] ExtractArrayData(int offset)
        {
            int baseOffset = offset;
            ProcessPseudoCode(PseudoOpCodes.Fill_array_data, ref offset);

            int elementsize = ReadShort(ref offset);
            int elementcount = ReadInt(ref offset);
            List<object> items = new List<object>();

            bool next = false;
            for (int i = 0; i < elementcount; i++)
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
