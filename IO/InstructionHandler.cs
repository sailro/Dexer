/*
    Dexer, open source framework for .DEX files (Dalvik Executable Format)
    Copyright (C) 2010 Sebastien LEBRETON

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using Dexer.Core;
using Dexer.Instructions;

namespace Dexer.IO
{
    internal class InstructionHandler : IBinaryReadable
    {
        private MethodDefinition MethodDefinition { get; set; }
        private List<Action> LazyInstructionsSetters { get; set; }
        private Dex Dex { get; set; }

        internal int[] Codes { get; set; }
        private int[] Lower { get; set; }
        private int[] Upper { get; set; }
        private int Ip { get; set; }
        private uint InstructionsSize { get; set; }

        internal Dictionary<int, Instruction> Lookup;

        public InstructionHandler(Dex dex, MethodDefinition methodDefinition)
        {
            Dex = dex;
            MethodDefinition = methodDefinition;
            Lookup = new Dictionary<int, Instruction>();
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

        private int ReadNibble() {
            return (Upper[Ip++] << 24) >> 28;
        }

        private int ReadShort()
        {
            return Codes[Ip++];
        }

        private int ReadInt()
        {
            int result = ReadShort();
            result |= ((int)ReadShort()) << 16;
            return result;
        }

        private long ReadLong()
        {
            long result = ReadShort();
            result |= ((long)ReadShort()) << 16;
            result |= ((long)ReadShort()) << 32;
            result |= ((long)ReadShort()) << 48;
            return result;
        }

        private int ReadSByte()
        {
            return Upper[Ip++];
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
                int data;
                int offset;

                Instruction ins = new Instruction();
                ins.OpCode = (OpCodes)Lower[Ip];
                ins.Offset = Ip;

                Lookup.Add(ins.Offset, ins);
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
                        ins.Operand = (int) ReadShort();
                        break;
                    case OpCodes.Const_wide_16:
                        // vAA, #+BBBB
                        ReadvAA(ins);
                        ins.Operand = (long) ReadShort();
                        break;
                    case OpCodes.Const:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = ReadInt();
                        break;
                    case OpCodes.Const_wide_32:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = (long) ReadInt();
                        break;
                    case OpCodes.Fill_array_data:
                        // vAA, #+BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt();
                        ins.Operand = ExtractArrayData(ins.Offset + offset);
                        break;
                    case OpCodes.Const_high16:
                        // vAA, #+BBBB0000
                        ReadvAA(ins);
                        ins.Operand = ((long)ReadShort()) << 16;
                        break;
                    case OpCodes.Const_wide:
                        // vAA, #+BBBBBBBBBBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = ReadLong();
                        break;
                    case OpCodes.Const_wide_high16:
                        // vAA, #+BBBB000000000000
                        ReadvAA(ins);
                        ins.Operand = ((long)ReadShort()) << 48;
                        break;
                    case OpCodes.Const_string:
                        // vAA, string@BBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.Strings[ReadShort()];
                        break;
                    case OpCodes.Const_string_jumbo:
                        // vAA, string@BBBBBBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.Strings[ReadInt()];
                        break;
                    case OpCodes.Const_class:
                    case OpCodes.New_instance:
                    case OpCodes.Check_cast:
                        // vAA, type@BBBB
                        ReadvAA(ins);
                        ins.Operand = Dex.TypeReferences[ReadShort()];
                        break;
                    case OpCodes.Instance_of:
                    case OpCodes.New_array:
                        // vA, vB, type@CCCC
                        ReadvA(ins);
                        ReadvB(ins);
                        ins.Operand = Dex.TypeReferences[ReadShort()];
                        break;
                    case OpCodes.Filled_new_array:
                        // {vD, vE, vF, vG, vA}, type@CCCC
                        throw new NotImplementedException(); // TODO Implement
                    case OpCodes.Filled_new_array_range:
                        // {vCCCC .. vNNNN}, type@BBBB
                        throw new NotImplementedException(); // TODO Implement
                    case OpCodes.Goto:
                        // +AA
                        offset = (sbyte) ReadSByte();
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Goto_16:
                        // +AAAA
                        Ip++;
                        offset = (short) ReadShort();
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Goto_32:
                        // +AAAAAAAA
                        Ip++;
                        offset = ReadInt();
                        LazyInstructionsSetters.Add(() => ins.Operand = Lookup[ins.Offset + offset]);
                        break;
                    case OpCodes.Packed_switch:
                        // vAA, +BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt();
                        ins.Operand = ExtractPackedSwitch(ins, ins.Offset + offset);
                        break;
                    case OpCodes.Sparse_switch:
                        // vAA, +BBBBBBBB
                        ReadvAA(ins);
                        offset = ReadInt();
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
                        offset = (short) ReadShort();
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
                        offset = (short) ReadShort();
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
                        ins.Operand = Dex.FieldReferences[ReadShort()];
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
                        ins.Operand = Dex.FieldReferences[ReadShort()];
                        break;
                    case OpCodes.Invoke_virtual:
                    case OpCodes.Invoke_super:
                    case OpCodes.Invoke_direct:
                    case OpCodes.Invoke_static:
                    case OpCodes.Invoke_interface:
                        // {vD, vE, vF, vG, vA}, meth@CCCC
                        data = Upper[Ip++] << 16;
                        ins.Operand = Dex.MethodReferences[ReadShort()];
                        data |= Codes[Ip++];
                        // TODO: handle registers
                        break;
                    case OpCodes.Invoke_virtual_range:
                    case OpCodes.Invoke_super_range:
                    case OpCodes.Invoke_direct_range:
                    case OpCodes.Invoke_static_range:
                    case OpCodes.Invoke_interface_range:
                        // {vCCCC .. vNNNN}, meth@BBBB
                        data = Upper[Ip++];
                        ins.Operand = Dex.MethodReferences[ReadShort()];
                        Ip++; //a1ri = Codes[Ip++];
                        // TODO: handle registers
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
                        ins.Operand = (int) ReadShort();
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
                        ins.Operand = ReadSByte();
                        break;

                    default:
                        throw new NotImplementedException(string.Concat("Unknown opcode:", ins.OpCode));
                }
            }
            // Check overhead
            FormatChecker.CheckExpression(() => Ip == InstructionsSize);

            foreach (Action action in LazyInstructionsSetters)
                action();
        }

        private void ProcessPseudoCode(PseudoOpCodes expected, ref int offset) {
            // auto reduce scope (PseudoCode data at the end)
            InstructionsSize = (uint)Math.Min(InstructionsSize, offset);
            PseudoOpCodes poc = (PseudoOpCodes)ReadRawShort(ref offset);
            FormatChecker.CheckExpression(() => poc == expected);
        }

        private byte ReadRawByte(ref int offset, bool next)
        {
            if (next)
                return (byte)((Codes[offset++] >> 8) & 0xff);
            else
                return (byte)(Codes[offset] & 0xff);
        }

        private int ReadRawInt(ref int offset)
        {
            int result = ReadRawShort(ref offset);
            result |= ((int)ReadRawShort(ref offset)) << 16;
            return result;
        }

        private short ReadRawShort(ref int offset)
        {
            return (short) Codes[offset++];
        }

        private long ReadRawLong(ref int offset)
        {
            long result = ReadRawShort(ref offset);
            result |= ((long)ReadRawShort(ref offset)) << 16;
            result |= ((long)ReadRawShort(ref offset)) << 32;
            result |= ((long)ReadRawShort(ref offset)) << 48;
            return result;
        }

        private SparseSwitch ExtractSparseSwitch(Instruction ins, int offset)
        {
            int baseOffset = offset;
            SparseSwitch result = new SparseSwitch();
            ProcessPseudoCode(PseudoOpCodes.Sparse_switch, ref offset);

            int targetcount = ReadRawShort(ref offset);

            int[] keys = new int[targetcount];
            for (int i = 0; i < targetcount; i++)
                keys[i] = ReadRawInt(ref offset);

            for (int i = 0; i < targetcount; i++)
            {
                int index = i; // used for closure
                int target = ReadRawInt(ref offset);
                LazyInstructionsSetters.Add(() => result.Targets.Add(keys[index], Lookup[ins.Offset + target]));
            }

            FormatChecker.CheckExpression(() => offset - baseOffset == targetcount * 4 + 2);
            return result;
        }

        private PackedSwitch ExtractPackedSwitch(Instruction ins, int offset)
        {
            int baseOffset = offset;
            PackedSwitch result = new PackedSwitch();
            ProcessPseudoCode(PseudoOpCodes.Packed_switch, ref offset);

            int targetcount = ReadRawShort(ref offset);
            result.FirstKey = ReadRawInt(ref offset);

            for (int i=0; i<targetcount; i++) {
                int target = ReadRawInt(ref offset);
                LazyInstructionsSetters.Add( () => result.Targets.Add(Lookup[ins.Offset + target]));
            }

            FormatChecker.CheckExpression(() => offset - baseOffset == targetcount * 2 + 4);
            return result;
        }

        private object[] ExtractArrayData(int offset)
        {
            int baseOffset = offset;
            ProcessPseudoCode(PseudoOpCodes.Fill_array_data, ref offset);

            int elementsize = ReadRawShort(ref offset);
            int elementcount = ReadRawInt(ref offset);
            List<object> items = new List<object>();

            bool next = false;
            for (int i = 0; i < elementcount; i++)
            {
                switch (elementsize)
                {
                    case 1:
                        items.Add(ReadRawByte(ref offset, next));
                        next = !next;
                        break;
                    case 2: 
                        items.Add(ReadRawShort(ref offset));
                        break;
                    case 4: 
                        items.Add(ReadRawInt(ref offset));
                        break;
                    case 8: 
                        items.Add(ReadRawLong(ref offset));
                        break;
                    default:
                        throw new NotImplementedException("Unknown Fill_array_data element size");
                }
            }

            if ((elementcount % 2 != 0) && (elementsize == 1))
                offset++;

            FormatChecker.CheckExpression(() => offset - baseOffset == (elementsize * elementcount + 1) / 2 + 4);
            return items.ToArray();
        }

    }

}
